using Grasshopper.GUI;
using Grasshopper.GUI.Canvas;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Attributes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.Component
{
    public class ScriptComponentAttribute : GH_ComponentAttributes
    {
        public ScriptComponentAttribute(IGH_Component comp) : base(comp)
        {
        }
        public override GH_ObjectResponse RespondToMouseDoubleClick(GH_Canvas sender, GH_CanvasMouseEvent e)
        {
            if (Owner is PythonNetScriptComponent comp)
            {
                if (comp.Editor == null) comp.SetWindow();
                comp.Editor.Show();
                comp.Editor.Focus();
                return GH_ObjectResponse.Handled;
            }
            else
            {
                return base.RespondToMouseDoubleClick(sender, e);
            }
        }

        public override void SetupTooltip(PointF canvasPoint, GH_TooltipDisplayEventArgs e)
        {
            base.SetupTooltip(canvasPoint, e);
            PythonNetScriptComponent owner = base.Owner as PythonNetScriptComponent;
            if (owner != null)
            {
                if (!string.IsNullOrEmpty(owner.TooltipText))
                {
                    e.Text = owner.TooltipText;
                }
                if (!string.IsNullOrEmpty(owner.TooltipDesc))
                {
                    e.Description = owner.TooltipDesc;
                }
            }
        }
    }
}
