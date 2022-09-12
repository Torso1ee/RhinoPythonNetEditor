using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RhinoPythonNetEditor.View.Tools
{

    internal class PythonFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var foldings = CreateNewFoldings(document, out var firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }

        private List<string> keywords = new List<string> { "class", "for", "if", "else", "def", "elif", "except", "finally", "while" };


        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            var foldings = new List<NewFolding>();
            var spaces = new Stack<(int, int, bool, bool)>();
            for (int i = 0; i < document.LineCount; i++)
            {
                var txt = document.GetText(document.Lines[i].Offset, document.Lines[i].Length);
                var space = txt.TakeWhile(Char.IsWhiteSpace);
                var count = 0;
                var (empty, hasKeyword) = CheckIsSpaceOrTipAndKeyword(txt);
                if (!empty)
                    foreach (var c in space)
                    {
                        if (c == ' ') count += 1;
                        else if (c == '\t') count += 4;
                    }
                if (spaces.Count == 0) spaces.Push((i, count, empty, hasKeyword));
                else if (empty) spaces.Push((i, spaces.Peek().Item2, empty, hasKeyword));
                else
                {
                    if (spaces.Peek().Item2 < count) spaces.Push((i, count, empty, hasKeyword));
                    else if (spaces.Peek().Item2 >= count)
                    {
                        while (spaces.Count > 0 && spaces.Peek().Item2 >= count)
                        {
                            var pair = spaces.Pop();
                            if (pair.Item1 != i - 1 && !pair.Item3 && pair.Item4)
                                foldings.Add(new NewFolding(document.Lines[pair.Item1].Offset, document.Lines[i - 1].EndOffset));
                        }
                        spaces.Push((i, count, empty, hasKeyword));
                    }
                }
            }
            while (spaces.Count > 0)
            {
                var pair = spaces.Pop();
                if (pair.Item1 != document.LineCount - 1 && !pair.Item3 && pair.Item4)
                    foldings.Add(new NewFolding(document.Lines[pair.Item1].Offset, document.Lines[document.LineCount - 1].EndOffset));
            }
            firstErrorOffset = 0;
            return foldings.OrderBy(f => f.StartOffset);
        }

        private (bool, bool) CheckIsSpaceOrTipAndKeyword(string s)
        {
            var seg = s.SkipWhile(Char.IsWhiteSpace).ToArray();
            var sr = new string(seg);
            var empty = seg.Length == 0 || seg[0] == '#';
            var hasKeyword = false;
            foreach (var w in keywords)
            {
                if (sr.StartsWith(w))
                {
                    hasKeyword = true;
                    break;
                }
            }
            return (empty, hasKeyword);
        }

    }
}
