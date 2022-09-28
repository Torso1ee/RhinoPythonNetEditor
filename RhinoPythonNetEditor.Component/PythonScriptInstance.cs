using Grasshopper.Kernel;
using Python.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using Rhino;
using Rhino.Geometry;
using Grasshopper;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Parameters;

namespace RhinoPythonNetEditor.Component
{
    public class PythonScriptInstance : GH_ScriptInstance
    {
        private List<string> __err = new List<string>();
        private List<string> __out = new List<string>();

        public override void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
        {
            __out.Clear();
            __err.Clear();
            {tadokorokouji114514}
            using (Py.GIL())
            {
                {bokusyu1919810}
            }
            try
            {
                {1145141919810}
            }
            catch (Exception exception)
            {
                __err.Add(string.Format("Script exception: {0}", exception.Message));
            }
            finally
            {
                if ((owner.Params.Output.Count > 0) && (owner.Params.Output[0] is Param_String))
                {
                    List<string> data = new List<string>();
                    if (!ReferenceEquals(this.__err, null))
                    {
                        data.AddRange(this.__err);
                    }
                    if (!ReferenceEquals(this.__out, null))
                    {
                        data.AddRange(this.__out);
                    }
                    if (data.Count > 0)
                    {
                        DA.SetDataList(0, data);
                    }
                }
            }

        }
    }
}


