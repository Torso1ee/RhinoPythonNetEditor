using Grasshopper.Kernel;
using Python.Runtime;
using Rhino.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Component
{
    public class PythonScriptInstance : IGH_ScriptInstance
    {
        public bool Hidden { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public bool IsPreviewCapable => throw new NotImplementedException();

        public BoundingBox ClippingBox => throw new NotImplementedException();

        public void AfterRunScript()
        {
            throw new NotImplementedException();
        }

        public void BeforeRunScript()
        {
            throw new NotImplementedException();
        }

        public void DrawViewportMeshes(IGH_PreviewArgs args)
        {
            throw new NotImplementedException();
        }

        public void DrawViewportWires(IGH_PreviewArgs args)
        {
            throw new NotImplementedException();
        }

        public void InvokeRunScript(IGH_Component owner, object rhinoDocument, int iteration, List<object> inputs, IGH_DataAccess DA)
        {
            using (Py.GIL())
            {
                dynamic np = Py.Import("numpy");
                Console.WriteLine(np.cos(np.pi * 2));

                dynamic sin = np.sin;
                Console.WriteLine(sin(5));

                double c = (double)(np.cos(5) + sin(5));
                Console.WriteLine(c);

                dynamic a = np.array(new List<float> { 1, 2, 3 });
                Console.WriteLine(a.dtype);

                dynamic b = np.array(new List<float> { 6, 5, 4 }, dtype: np.int32);
                Console.WriteLine(b.dtype);

                Console.WriteLine(a * b);
            }

        }
    }
}
