using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Component
{
    public class ScriptComponentAttribute : GH_Attributes<PythonNetScriptComponent>
    {
        public ScriptComponentAttribute(PythonNetScriptComponent owner) : base(owner)
        {
        }

        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            Owner.Editor.Value.Show();
            return base.RespondToMouseDoubleClick(sender, e);
        }
    }
}
