using Eto.Forms;
using GH_IO.Serialization;
using Grasshopper;
using Grasshopper.GUI.Script;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Grasshopper.Kernel.Types;
using Microsoft.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;
using Python.Runtime;
using Rhino;
using Rhino.Geometry;
using Rhino.Resources;
using Rhino.Runtime;
using Rhino.Runtime.InProcess;
using RhinoPythonNetEditor.Interface;
using RhinoPythonNetEditor.ViewModel;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Configuration.Assemblies;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RhinoPythonNetEditor.Component
{
    public class PythonNetScriptComponent : GH_Component, IGH_VariableParameterComponent, IScriptComponent
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public PythonNetScriptComponent()
          : base("PythonNet Script", "PythonNet Script",
            "PythonNetScriptComponent provides editing and debugging cpython code in Rhino. PythonNet Script also supports interoperating with .Net library.",
            "Maths", "Script")
        {
            ScriptSource = new ScriptSource(this);
            ScriptSource.References.Add(AssemblyPath + @"\Python.Runtime.dll");
        }



        private void Document_ObjectsDeleted(object sender, GH_DocObjectEventArgs e)
        {
            Editor?.Hide();
        }

        public override void RemovedFromDocument(GH_Document document)
        {
            Editor?.Hide();
            base.RemovedFromDocument(document);
        }

        static PythonNetScriptComponent()
        {
            AssemblyPath = Path.GetDirectoryName(typeof(PythonNetScriptComponent).Assembly.Location);
        }
        public static bool IsPythonInitialized { get; set; } = false;


        public static void PythonInitialized()
        {
            var p = AssemblyPath + @"\compiled";
            if (!Directory.Exists(p)) Directory.CreateDirectory(p);
            CompiledPath = p;
            var pathToBaseEnv = AssemblyPath + @"\python_env\";
            Runtime.PythonDLL = pathToBaseEnv + @"\python38.dll";
            PythonEngine.PythonHome = pathToBaseEnv;
            //Environment.SetEnvironmentVariable("PATH", $@"{pathToBaseEnv}\Library\bin", EnvironmentVariableTarget.Process);
            //Environment.SetEnvironmentVariable("PATH", p, EnvironmentVariableTarget.Process);
            PythonEngine.Initialize();
            Rhino.RhinoApp.Closing += RhinoApp_Closing;
            var paths = Environment.GetEnvironmentVariable("PATH");
            //Environment.SetEnvironmentVariable("PATH", paths + @";D:\Anaconda\envs\PythonNet\Library\bin", EnvironmentVariableTarget.Process);
            using (Py.GIL())
            {
                dynamic sys = Py.Import("sys");
                sys.path.append(CompiledPath);
            }
            IsPythonInitialized = true;
        }

        private static void RhinoApp_Closing(object sender, EventArgs e)
        {
            RhinoApp.InvokeOnUiThread(new Action(() => PythonEngine.Shutdown()));
        }

        internal static string CompiledPath { get; set; }
        private static string AssemblyPath { get; set; }

        internal void SetWindow()
        {
            Editor = new PythonNetScriptEditor(this);
            Editor.Loaded += Editor_Loaded;
        }

        internal ViewModelLocator Locator { get; set; }
        private void Editor_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsInjected)
            {
                Locator = Editor.DataContext as ViewModelLocator;
                Locator.ComponentHost = this;
            }
            else
            {
                Editor.Loaded -= Editor_Loaded;
            }
        }


        public bool IsInjected { get; set; } = false;


        public bool HasOutParameter => ((this.Params.Output.Count != 0) ? (this.Params.Output[0] is Param_String) : false);
        internal PythonNetScriptEditor Editor { get; set; }
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            IGH_Param param = this.CreateParameter(GH_ParameterSide.Input, 0);
            IGH_Param param2 = this.CreateParameter(GH_ParameterSide.Input, 1);
            param.NickName = "x";
            param2.NickName = "y";
            pManager.AddParameter(param);
            pManager.AddParameter(param2);
        }

        public override void CreateAttributes()
        {
            m_attributes = new ScriptComponentAttribute(this);
        }
        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("out", "out", "Print, Reflect and Error streams", GH_ParamAccess.list);
            IGH_Param param = this.CreateParameter(GH_ParameterSide.Output, 1);
            pManager.AddParameter(param);
            this.VariableParameterMaintenance();
        }

        protected override void BeforeSolveInstance()
        {
            if (!IsPythonInitialized) PythonInitialized();
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            if (HasOutParameter) DA.DisableGapLogic(0);
            IGH_ScriptInstance scriptInstance = GetScriptInstance();
            if (DA.Iteration == 0 && compilerErrors.Count > 0)
            {
                foreach (string str in compilerErrors)
                {
                    if (str.StartsWith("Error", StringComparison.OrdinalIgnoreCase))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Error, str);
                    }
                    if (str.StartsWith("Warning", StringComparison.OrdinalIgnoreCase))
                    {
                        AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, str);
                    }
                }
                if (HasOutParameter)
                {
                    DA.SetDataList(0, compilerErrors);
                }
            }
            if (scriptInstance != null)
            {
                List<object> inputs = new List<object>();
                var count = Params.Input.Count;
                for (int i = 0; i < count; i++)
                {
                    GH_ParamAccess access = this.Params.Input[i].Access;
                    switch (access)
                    {
                        case GH_ParamAccess.item:
                            inputs.Add(this.GetItemFromParameter(DA, i));
                            break;

                        case GH_ParamAccess.list:
                            inputs.Add(this.GetListFromParameter(DA, i));
                            break;

                        case GH_ParamAccess.tree:
                            inputs.Add(this.GetTreeFromParameter(DA, i));
                            break;
                        default:
                            break;
                    }
                }
                try
                {
                    scriptInstance.InvokeRunScript(this, RhinoDoc.ActiveDoc, DA.Iteration, inputs, DA);
                }
                catch (Exception exception)
                {
                    Exception ex = exception;
                    ProjectData.SetProjectError(ex);
                    Exception e = ex;
                    if (ex.Message.Contains("EOL"))
                    {
                        var ma = Regex.Match(ex.Message, @"line (\d+)");
                        if (ma.Groups.Count == 2)
                        {
                            var eLine = ma.Groups[0].Value.Replace(ma.Groups[1].Value, (int.Parse(ma.Groups[1].Value) - 2).ToString());
                            eLine = ex.Message.Replace(ma.Groups[0].Value, eLine);
                            DA.SetData(0, string.Format("error: {0})", eLine));
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, eLine);
                        }
                    }
                    else if (HasOutParameter)
                    {
                        StackTrace trace = new StackTrace(e, true);
                        if (trace.FrameCount == 0)
                        {
                            DA.SetData(0, string.Format("error: {0} (no line number available, sorry)", e.Message));
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, e.Message);
                        }
                        else
                        {
                            StackFrame frame = trace.GetFrame(trace.FrameCount - 1);
                            var errorLines = exception.StackTrace.Split('\n');
                            var lCount = errorLines.Length;
                            var pyError = new List<string>();
                            for (int i = 0; i < lCount;)
                            {
                                if (errorLines[i].StartsWith("  File") && i + 1 < lCount)
                                {
                                    pyError.Add(errorLines[i] + "\n" + errorLines[i + 1]);
                                    i += 2;
                                }
                                else
                                {
                                    i++;
                                }
                            }
                            var ls = new List<string>();
                            foreach (var l in pyError)
                            {
                                var ma = Regex.Match(l, @"line (\d+), in");
                                if (ma.Groups.Count == 2)
                                {
                                    var eLine = ma.Groups[0].Value.Replace(ma.Groups[1].Value, (int.Parse(ma.Groups[1].Value) - 2).ToString());
                                    ls.Add(l.Replace(ma.Groups[0].Value, eLine));
                                }
                            }
                            var error = string.Join("\n", ls);
                            DA.SetData(0, string.Format("error: {0})", error));
                            AddRuntimeMessage(GH_RuntimeMessageLevel.Error, error);
                        }
                    }
                    HostUtils.ExceptionReport(e);
                    ProjectData.ClearProjectError();
                }
            }

        }

        private object GetTreeFromParameter(IGH_DataAccess access, int index)
        {
            GH_Structure<IGH_Goo> structure = new GH_Structure<IGH_Goo>();
            access.GetDataTree<IGH_Goo>(index, out structure);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)this.Params.Input[index]).TypeHint;
            DataTree<object> tree = new DataTree<object>();
            int num = structure.PathCount - 1;
            int num2 = 0;
            while (num2 <= num)
            {
                GH_Path path = structure.get_Path(num2);
                List<IGH_Goo> list = structure.Branches[num2];
                List<object> data = new List<object>();
                int num3 = list.Count - 1;
                int num4 = 0;
                while (true)
                {
                    if (num4 > num3)
                    {
                        tree.AddRange(data, path);
                        num2++;
                        break;
                    }
                    data.Add(this.TypeCast(list[num4], typeHint));
                    num4++;
                }
            }
            return tree;
        }

        private object GetListFromParameter(IGH_DataAccess access, int index)
        {
            List<IGH_Goo> list = new List<IGH_Goo>();
            access.GetDataList<IGH_Goo>(index, list);
            IGH_TypeHint typeHint = ((Param_ScriptVariable)this.Params.Input[index]).TypeHint;
            List<object> list2 = new List<object>();
            int num = list.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                list2.Add(this.TypeCast(list[i], typeHint));
            }
            return list2;
        }


        private object GetItemFromParameter(IGH_DataAccess access, int index)
        {
            IGH_Goo destination = null;
            access.GetData<IGH_Goo>(index, ref destination);
            return this.TypeCast(destination, index);
        }

        private object TypeCast(IGH_Goo data, int index)
        {
            Param_ScriptVariable variable = (Param_ScriptVariable)this.Params.Input[index];
            return this.TypeCast(data, variable.TypeHint);
        }

        private object TypeCast(IGH_Goo data, IGH_TypeHint hint)
        {
            object obj;
            if (data == null)
            {
                obj = null;
            }
            else if (hint == null)
            {
                obj = data.ScriptVariable();
            }
            else
            {
                object obj1 = data.ScriptVariable();
                object target = null;
                hint.Cast(obj1, out target);
                obj = target;
            }
            return obj;
        }


        public bool CanInsertParameter(GH_ParameterSide side, int index)
        {
            return ((side != GH_ParameterSide.Output) || (!this.HasOutParameter) || (index != 0));
        }

        public bool CanRemoveParameter(GH_ParameterSide side, int index) => true;


        public IGH_Param CreateParameter(GH_ParameterSide side, int index)
        {
            IGH_Param param;
            if (side == GH_ParameterSide.Input)
            {
                Param_ScriptVariable variable1 = new Param_ScriptVariable();
                variable1.NickName = GH_ComponentParamServer.InventUniqueNickname("xyzuvw", this.Params.Input);
                param = variable1;
            }
            else if (side != GH_ParameterSide.Output)
            {
                param = null;
            }
            else
            {
                Param_GenericObject obj1 = new Param_GenericObject();
                obj1.NickName = GH_ComponentParamServer.InventUniqueNickname("ABCDEF", this.Params.Output);
                param = obj1;
            }
            return param;
        }


        public bool DestroyParameter(GH_ParameterSide side, int index)
        {
            ScriptAssembly = null;
            return true;
        }

        public void VariableParameterMaintenance()
        {
            int num = this.Params.Input.Count - 1;
            for (int i = 0; i <= num; i++)
            {
                IGH_Param param = this.Params.Input[i];
                param.Name = param.NickName;
                param.Description = string.Format("Script Variable {0}", param.NickName);
                Param_ScriptVariable variable = param as Param_ScriptVariable;
                if (variable != null)
                {
                    variable.AllowTreeAccess = true;
                    variable.Optional = true;
                    variable.ShowHints = true;
                }
            }
            int num3 = this.Params.Output.Count - 1;
            for (int j = this.FirstOutputIndex; j <= num3; j++)
            {
                IGH_Param param2 = this.Params.Output[j];
                param2.Name = param2.NickName;
                param2.Description = string.Format("Output parameter {0}", param2.NickName);
            }
        }


        internal int FirstOutputIndex
        {
            get
            {
                return (!this.HasOutParameter ? 0 : 1);
            }
        }
        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// You can add image files to your project resources and access them like this:
        /// return Resources.IconForThisComponent;
        /// </summary>
        protected override System.Drawing.Bitmap Icon => Resources.PythonIcon;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5B694A39-598E-4FCA-880A-0326F854E3E6");

        public string TooltipText { get; internal set; }
        public string TooltipDesc { get; internal set; }

        internal CompiledScript ScriptAssembly { get; set; }

        protected override void AppendAdditionalComponentMenuItems(ToolStripDropDown menu)
        {
            if (this.HasOutParameter)
            {
                Menu_AppendItem(menu, "Remove out", new EventHandler(this.Menu_ReinstateOutClicked)).ToolTipText = "Hide the [out] parameter";
            }
            else
            {
                Menu_AppendItem(menu, "Reinstate out", new EventHandler(this.Menu_ReinstateOutClicked)).ToolTipText = "Display the [out] parameter again";
            }

        }

        private void Menu_ReinstateOutClicked(object sender, EventArgs e)
        {
            if (this.HasOutParameter)
            {
                this.RecordUndoEvent("Remove out");
                this.Params.UnregisterOutputParameter(this.Params.Output[0], true);
                this.ExpireSolution(true);
            }
            else
            {
                this.RecordUndoEvent("Reinstate out");
                Param_String str = new Param_String
                {
                    Access = GH_ParamAccess.list,
                    Name = "out",
                    NickName = "out",
                    Description = "Print, Reflect and Error streams"
                };
                this.Params.RegisterOutputParam(str, 0);
                this.ExpireSolution(true);
            }

        }

        private IGH_ScriptInstance GetScriptInstance()
        {
            IGH_ScriptInstance instance;
            if (this.ScriptAssembly != null)
            {
                if (this.ScriptAssembly.Instance == null)
                {
                    if (this.ScriptAssembly.Type == null)
                    {
                        this.ScriptAssembly = null;
                    }
                    else
                    {
                        object obj2 = Activator.CreateInstance(this.ScriptAssembly.Type);
                        if (obj2 != null)
                        {
                            this.ScriptAssembly.Instance = (IGH_ScriptInstance)obj2;
                            instance = this.ScriptAssembly.Instance;
                        }
                        else
                        {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Script instance cannot be constructed.");
                            this.ScriptAssembly = null;
                            instance = null;
                        }
                        return instance;
                    }
                }
                else
                {
                    return this.ScriptAssembly.Instance;
                }
            }
            if (this.ScriptSource == null)
            {
                this.ScriptAssembly = null;
                instance = null;
            }
            else if (this.ScriptSource.IsEmpty)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "No code supplied");
                this.ScriptAssembly = null;
                instance = null;
            }
            else
            {
                Type type = this.CreateScriptType(this.ScriptSource);
                if (type == null)
                {
                    instance = null;
                }
                else
                {
                    this.ScriptAssembly = new CompiledScript(type);
                    if (this.ScriptAssembly.Instance != null)
                    {
                        instance = this.ScriptAssembly.Instance;
                    }
                    else
                    {
                        this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Script instance is a null reference");
                        this.ScriptAssembly = null;
                        instance = null;
                    }
                }
            }
            return instance;
        }

        private static readonly SortedDictionary<Guid, List<string>> CachedFailures = new SortedDictionary<Guid, List<string>>();
        private readonly List<string> compilerErrors = new List<string>();
        private static readonly SortedDictionary<Guid, Type> CachedAssemblies = new SortedDictionary<Guid, Type>();
        public ScriptSource ScriptSource { get; }
        private Type CreateScriptType(ScriptSource source)
        {
            Type type;
            this.compilerErrors.Clear();
            if (source.IsEmpty)
            {
                type = null;
            }
            else
            {
                Guid key = this.ComputeScriptHash(source);
                if (CachedAssemblies.ContainsKey(key))
                {
                    type = CachedAssemblies[key];
                }
                else if (CachedFailures.ContainsKey(key))
                {
                    List<string> collection = CachedFailures[key];
                    if (collection != null)
                    {
                        this.compilerErrors.AddRange(collection);
                    }
                    type = null;
                }
                else
                {
                    foreach (var path in source.References) AssemblyResolver.AddSearchFile(path);
                    string str = this.CreateSourceForCompile(source, out Guid id);
                    CompilerResults results = this.CompileSource(str, id);
                    foreach (var error in results.Errors)
                    {
                        var cError = error as CompilerError;
                        if (!IgnoreWarning(cError))
                        {
                            List<string> list2 = GH_CodeBlocks.StringSplit(Environment.NewLine, cError.ErrorText);
                            string format = "Error ({0}): {1}";
                            if (cError.IsWarning)
                            {
                                format = "Warning ({0}): {1}";
                            }
                            if (cError.Line > 0)
                            {
                                format = format + string.Format(" (line {0})", cError.Line);
                            }

                            foreach (var er in list2)
                                this.compilerErrors.Add(string.Format(format, cError.ErrorNumber, er));

                        }
                    }

                    if (results.Errors.HasErrors)
                    {
                        if (!CachedFailures.ContainsKey(key))
                        {
                            CachedFailures.Add(key, new List<string>(this.compilerErrors));
                        }
                        type = null;
                    }
                    else
                    {
                        Assembly compiledAssembly = results.CompiledAssembly;
                        if (compiledAssembly == null)
                        {
                            this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Assembly was not properly compiled");
                            type = null;
                        }
                        else
                        {
                            Type type2 = compiledAssembly.GetType("PythonScriptInstance");
                            if (type2 == null)
                            {
                                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Custom type could not be found in assembly");
                                type = null;
                            }
                            else
                            {
                                if (!CachedAssemblies.ContainsKey(key))
                                {
                                    CachedAssemblies.Add(key, type2);
                                }
                                type = type2;
                            }
                        }
                    }
                }
            }
            return type;
        }

        private CompilerResults CompileSource(string str, Guid id)
        {
            string[] sources = new string[] { str };
            return new CSharpCodeProvider().CompileAssemblyFromSource(ScriptAssemblyCompilerParameters(id), sources);

        }

        private CompilerParameters ScriptAssemblyCompilerParameters(Guid id)
        {
            CompilerParameters parameters = new CompilerParameters
            {
                OutputAssembly = CompiledPath + $@"\{id}.dll",
                GenerateExecutable = false,
                GenerateInMemory = false,
                IncludeDebugInformation = true,
                TreatWarningsAsErrors = false,
                WarningLevel = 4
            };
            foreach (string str in GH_ScriptEditor.DefaultAssemblyLocations())
            {
                parameters.ReferencedAssemblies.Add(str);
            }
            Assembly assembly = null, assembly1 = null;
            try
            {
                assembly = Assembly.Load("Microsoft.CSharp");
                assembly1 = Assembly.Load("netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51");

            }
            catch (Exception exception1)
            {
                Exception ex = exception1;
                ProjectData.SetProjectError(ex);
                HostUtils.ExceptionReport(ex);
                ProjectData.ClearProjectError();
            }
            if (assembly != null)
            {
                parameters.ReferencedAssemblies.Add(assembly.Location);
            }
            if (assembly1 != null)
            {
                parameters.ReferencedAssemblies.Add(assembly1.Location);
            }
            if ((this.ScriptSource != null) && (this.ScriptSource.References != null))
            {
                foreach (var reference in ScriptSource.References)
                {
                    if (File.Exists(reference))
                    {
                        try
                        {
                            Assembly.LoadFile(reference);
                            parameters.ReferencedAssemblies.Add(reference);
                        }
                        catch (Exception exception3)
                        {
                            Exception ex = exception3;
                            ProjectData.SetProjectError(ex);
                            Exception exception2 = ex;
                            Tracing.Assert(new Guid("{800AF1E3-DFA1-420a-BA06-A7E1A5CEF8F4}"), "Referenced Assembly failed to load: " + reference);
                            ProjectData.ClearProjectError();
                        }
                    }
                }
            }
            return parameters;
        }



        private string CreateSourceForCompile(ScriptSource source, out Guid id)
        {
            id = Guid.NewGuid();
            return source.GenerateCode(id);
        }

        private Guid ComputeScriptHash(ScriptSource source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            if (this.Params == null)
            {
                throw new NullReferenceException("Params field is unset");
            }
            MemoryStream output = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(output);
            source.WriteHashData(writer);
            foreach (IGH_Param param in this.Params.Input)
            {
                GH_ComponentParamServer.WriteParamHashData(writer, param, GH_ParamHashFields.TypeHint | GH_ParamHashFields.Expression | GH_ParamHashFields.PersistentData | GH_ParamHashFields.TypeId | GH_ParamHashFields.Access | GH_ParamHashFields.NickName);
            }
            int num = this.Params.Output.Count - 1;
            for (int i = 1; i <= num; i++)
            {
                GH_ComponentParamServer.WriteParamHashData(writer, this.Params.Output[i], GH_ParamHashFields.NickName);
            }
            var id = GH_Convert.ToSHA_Hash(output);
            writer.Close();
            output.Dispose();
            return id;
        }

        public override void AddedToDocument(GH_Document document)
        {
            Params.ParameterChanged -= ParameterChanged;
            Params.ParameterChanged += ParameterChanged;
        }


        private void ParameterChanged(object sender, GH_ParamServerEventArgs e)
        {
            switch (e.OriginalArguments.Type)
            {
                case GH_ObjectEventType.NickName:
                case GH_ObjectEventType.Icon:
                case GH_ObjectEventType.IconDisplayMode:
                case GH_ObjectEventType.Sources:
                case GH_ObjectEventType.Selected:
                case GH_ObjectEventType.Enabled:
                case GH_ObjectEventType.Preview:
                case GH_ObjectEventType.PersistentData:
                case GH_ObjectEventType.DataMapping:
                    return;
                case GH_ObjectEventType.NickNameAccepted:
                    this.ScriptAssembly = null;
                    this.ExpireSolution(true);
                    return;
            }
            this.ScriptAssembly = null;
            this.ExpireSolution(true);
        }

        private static readonly SortedDictionary<string, bool> IgnoreWarnings = new SortedDictionary<string, bool>();
        public static bool IgnoreWarning(string warning)
        {
            if (IgnoreWarnings.Count == 0)
            {
                GH_SettingsServer server = new GH_SettingsServer("grasshopper_ignorewarnings");
                if (server.Count == 0)
                {
                    server.SetValue("CS1702");
                    server.WritePersistentSettings();
                }
                foreach (var name in server.EntryNames())
                    IgnoreWarnings.Add(name, true);
            }
            return IgnoreWarnings.ContainsKey(warning);
        }

        public static bool IgnoreWarning(CompilerError warning)
        {
            return (warning.IsWarning ? IgnoreWarning(warning.ErrorNumber) : false);
        }

        public override bool Read(GH_IReader reader)
        {
            ScriptSource.PythonCode = reader.GetString("code");
            ScriptSource.References.Clear();
            ScriptSource.References.AddRange(reader.GetString("reference").Split('\n'));
            ScriptAssembly = null;
            return base.Read(reader);
        }

        public override bool Write(GH_IWriter writer)
        {
            writer.SetString("code", ScriptSource.PythonCode);
            writer.SetString("reference", String.Join("\n", ScriptSource.References));
            return base.Write(writer);
        }

        public void SetSource(string code)
        {
            if (ScriptSource.PythonCode != code)
            {
                RecordUndoEvent("CodeChanged");
                ScriptSource.PythonCode = code;
            }
            ScriptAssembly = null;
        }

        public void CloseEditor()
        {
            Editor?.Hide();
        }

        public string GetCode()
        {
            return ScriptSource.PythonCode;
        }
    }
}