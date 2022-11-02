using ICSharpCode.AvalonEdit.Document;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.View.Tools
{
    internal class CompletionSegment : ISegment
    {
        public int Offset{get; set;}

        public int Length { get; set; }

        public int EndOffset { get; set; }
    }
}
