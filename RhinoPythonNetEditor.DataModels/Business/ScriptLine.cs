using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.DataModels.Business
{
    public class ScriptLine
    {

        public ScriptLineState State { get; set; }

        public string Text { get; set; }
    }

    public enum ScriptLineState
    {
        Error,
        Normal,
        Input
    }

}
