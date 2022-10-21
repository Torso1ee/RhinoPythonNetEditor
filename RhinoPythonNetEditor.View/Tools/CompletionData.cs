using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using RhinoPythonNetEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RhinoPythonNetEditor.View.Tools
{
    public class CompletionData : ICompletionData
    {
        public CompletionData(CompletionItem item)
        {
            Item = item;
        }
        public ImageSource Image => null;

        public CompletionItem Item { get; set; }
        public string Text => Item.InsertText;

        public object Content => Text;

        public object Description => Item.Detail;

        public double Priority => 0;

        public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
        {
            Item = LintManager.ResolveCompletionItem(Item);
            var tryResult = int.TryParse(Item.FilterText, out int likeTextLegth);
            if (!tryResult) likeTextLegth = 0;
            var segment = new CompletionSegment { Offset = completionSegment.Offset - likeTextLegth, EndOffset = completionSegment.EndOffset, Length = completionSegment.Length + likeTextLegth };
            textArea.Document.Replace(segment, Text);
        }
    }
}
