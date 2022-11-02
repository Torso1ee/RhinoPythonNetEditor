using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace RhinoPythonNetEditor.Component
{
    public class RhinoPythonNetEditor_ComponentInfo : GH_AssemblyInfo
    {
        public override string Name => "RhinoPythonNetEditor.Component";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("2892F182-F6B5-430C-8E69-1327FC392403");

        //Return a string identifying you or your company.
        public override string AuthorName => "";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}