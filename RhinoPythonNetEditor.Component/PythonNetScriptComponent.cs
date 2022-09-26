using Grasshopper;
using Grasshopper.GUI.Script;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Microsoft.Extensions.DependencyInjection;
using Rhino.Geometry;
using Rhino.Runtime;
using RhinoPythonNetEditor.ViewModel;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace RhinoPythonNetEditor.Component
{
    public class PythonNetScriptComponent : GH_Component, IGH_VariableParameterComponent
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

        }

        internal void SetWindow()
        {
            Editor = new PythonNetScriptEditor();
            Editor.Loaded += Editor_Loaded;
        }

        private void Editor_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!IsInjected)
            {
                var locator = Editor.DataContext as ViewModelLocator;
                locator.ComponentHost = this;
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


        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

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


        public bool DestroyParameter(GH_ParameterSide side, int index) => true;

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
                    variable.Hints = this.AvailableTypeHints;
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

        protected virtual List<IGH_TypeHint> AvailableTypeHints
        {
            get
            {
                List<IGH_TypeHint> list1 = new List<IGH_TypeHint>();
                list1.Add(new GH_DynamicHint());
                list1.Add(new GH_HintSeparator());
                list1.Add(new GH_BooleanHint_CS());
                list1.Add(new GH_IntegerHint_CS());
                list1.Add(new GH_DoubleHint_CS());
                list1.Add(new GH_ComplexHint());
                list1.Add(new GH_StringHint_CS());
                list1.Add(new GH_DateTimeHint());
                list1.Add(new GH_ColorHint());
                list1.Add(new GH_GuidHint());
                list1.Add(new GH_HintSeparator());
                list1.Add(new GH_Point3dHint());
                list1.Add(new GH_Vector3dHint());
                list1.Add(new GH_PlaneHint());
                list1.Add(new GH_IntervalHint());
                list1.Add(new GH_UVIntervalHint());
                list1.Add(new GH_Rectangle3dHint());
                list1.Add(new GH_BoxHint());
                list1.Add(new GH_TransformHint());
                list1.Add(new GH_HintSeparator());
                list1.Add(new GH_LineHint());
                list1.Add(new GH_CircleHint());
                list1.Add(new GH_ArcHint());
                list1.Add(new GH_PolylineHint());
                list1.Add(new GH_CurveHint());
                list1.Add(new GH_SurfaceHint());
                list1.Add(new GH_BrepHint());
                list1.Add(new GH_SubDHint());
                list1.Add(new GH_MeshHint());
                list1.Add(new GH_GeometryBaseHint());
                return list1;
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
        protected override System.Drawing.Bitmap Icon => null;

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid => new Guid("5B694A39-598E-4FCA-880A-0326F854E3E6");

        public string TooltipText { get; internal set; }
        public string TooltipDesc { get; internal set; }

        internal CompiledScript ScriptAssembly { get; set; }
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
        private readonly List<string> _compilerErrors = new List<string>();
        private static readonly SortedDictionary<Guid, Type> CachedAssemblies;
        private ScriptSource ScriptSource { get; } = new ScriptSource();
        private Type CreateScriptType(ScriptSource source)
        {
            Type type;
            this._compilerErrors.Clear();
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
                        this._compilerErrors.AddRange(collection);
                    }
                    type = null;
                }
                else
                {
                    IEnumerator enumerator;
                    if (source.References != null)
                    {
                        List<string>.Enumerator enumerator;
                        try
                        {
                            enumerator = source.References.GetEnumerator();
                            while (enumerator.MoveNext())
                            {
                                AssemblyResolver.AddSearchFile(enumerator.Current);
                            }
                        }
                        finally
                        {
                            enumerator.Dispose();
                        }
                    }
                    string str = this.CreateSourceForCompile(source);
                    str = this.ClearTemplateTags(str);
                    CompilerResults results = this.CompileSource(str);
                    try
                    {
                        enumerator = results.Errors.GetEnumerator();
                        while (enumerator.MoveNext())
                        {
                            CompilerError current = (CompilerError)enumerator.Current;
                            if (!IgnoreWarning(current))
                            {
                                List<string>.Enumerator enumerator;
                                List<string> list2 = GH_CodeBlocks.StringSplit(Environment.NewLine, current.ErrorText);
                                string format = "Error ({0}): {1}";
                                if (current.IsWarning)
                                {
                                    format = "Warning ({0}): {1}";
                                }
                                if (current.Line > 0)
                                {
                                    format = format + string.Format(" (line {0})", current.Line);
                                }
                                try
                                {
                                    enumerator = list2.GetEnumerator();
                                    while (enumerator.MoveNext())
                                    {
                                        string str3 = enumerator.Current;
                                        this._compilerErrors.Add(string.Format(format, current.ErrorNumber, str3));
                                    }
                                }
                                finally
                                {
                                    enumerator.Dispose();
                                }
                            }
                        }
                    }
                    finally
                    {
                        if (enumerator is IDisposable)
                        {
                            (enumerator as IDisposable).Dispose();
                        }
                    }
                    if (results.Errors.HasErrors)
                    {
                        if (!CachedFailures.ContainsKey(key))
                        {
                            CachedFailures.Add(key, new List<string>(this._compilerErrors));
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
                            Type type2 = compiledAssembly.GetType("Script_Instance");
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
            writer.Close();
            output.Dispose();
            return GH_Convert.ToSHA_Hash(output);
        }


    }
}