using Grasshopper.Kernel.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Component
{
    public class GH_DynamicHint : IGH_TypeHint
    {
        public Guid HintID => new Guid("{6A184B65-BAA3-42d1-1234-3915B401DE53}");

        public string TypeName => "Dynamic";

        public bool Cast(object data, out object target)
        {
            target = RuntimeHelpers.GetObjectValue(data);
            return true;
        }

        bool IGH_TypeHint.Cast(object data, out object target)
        {
            //ILSpy generated this explicit interface implementation from .override directive in Cast
            return this.Cast(data, out target);
        }
    }
}
