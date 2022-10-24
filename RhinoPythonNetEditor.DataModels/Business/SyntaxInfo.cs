using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.DataModels.Business
{
    public class SyntaxInfo
    {
        public string Message { get; set; }
        public Servity Servity { get; set; }

        public string Range { get; set; }

        public (int,int) Start { get; set; }
        public (int,int) End { get; set; }

        public string Source { get; set; }
    }

    public enum Servity
    {
        Error,
        Warning,
        Information,
        Hint
    }
}
