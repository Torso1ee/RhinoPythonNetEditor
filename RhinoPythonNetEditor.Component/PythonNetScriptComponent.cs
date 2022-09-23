using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Parameters.Hints;
using Microsoft.Extensions.DependencyInjection;
using Rhino.Geometry;
using RhinoPythonNetEditor.ViewModel;
using System;
using System.Collections.Generic;

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

    }
}