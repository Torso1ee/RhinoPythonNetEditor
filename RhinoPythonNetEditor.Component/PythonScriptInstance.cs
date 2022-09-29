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
            object x = default(object);
            if (inputs[0] != null)
            {
                x = (object)(inputs[0]);
            }
            object y = default(object);
            if (inputs[1] != null)
            {
                y = (object)(inputs[1]);
            }

            object A = null;

            using (Py.GIL())
            {
                dynamic module = Py.Import("25b9c48b-1f28-4fe2-8156-cb8f0992e37a");
                dynamic func = module.func;
                func(x, y, ref A);
            }
            try
            {
                if (A != null)
                {
                    if (GH_Format.TreatAsCollection(A))
                    {
                        IEnumerable __enum_A = (IEnumerable)(A);
                        DA.SetDataList(1, __enum_A);
                    }
                    else
                    {
                        if (A is Grasshopper.Kernel.Data.IGH_DataTree)
                        {
                            //merge tree
                            DA.SetDataTree(1, (Grasshopper.Kernel.Data.IGH_DataTree)(A));
                        }
                        else
                        {
                            //assign direct
                            DA.SetData(1, A);
                        }
                    }
                }
                else
                {
                    DA.SetData(1, null);
                }
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