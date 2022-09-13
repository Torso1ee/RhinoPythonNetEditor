using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace RhinoPythonNetEditor.View.Tools
{

    public class LineAnalize
    {
        public LineAnalize(TextDocument document, DocumentLine s)
        {
            Line = s;
            var txt = document.GetText(s.Offset, s.Length);
            CheckIsSpaceOrTipAndKeyword(txt);
            var space = txt.TakeWhile(Char.IsWhiteSpace);
            foreach (var c in space)
            {
                if (c == ' ') Space += 1;
                else if (c == '\t') Space += 4;
            }
        }
        public DocumentLine Line { get; set; }
        public bool HasKeyword { get; set; }
        public bool Empty { get; set; }

        public bool InNote { get; set; }
        public bool HasNote { get; set; }

        public bool HasNoteEnd { get; set; }

        public int Space { get; set; }

        private static List<string> keywords = new List<string> { "class", "for", "if", "else", "def", "elif", "except", "finally", "while" };

        private static List<string> noteWords = new List<string> { "'''", "\"\"\"" };

        private void CheckIsSpaceOrTipAndKeyword(string s)
        {
            var seg = s.SkipWhile(Char.IsWhiteSpace).ToArray();
            var sr = new string(seg);
            Empty = seg.Length == 0 || seg[0] == '#';
            HasKeyword = false;
            HasNote = false;
            HasNoteEnd = false;
            foreach (var w in noteWords)
            {
                if (sr.StartsWith(w))
                {
                    HasNote = true;
                    break;
                }
            }
            if (!HasNote)
            {
                foreach (var w in keywords)
                {
                    if (sr.StartsWith(w))
                    {
                        HasKeyword = true;
                        break;
                    }
                }
            }
            if (sr.Length > 5)
            {
                foreach (var w in noteWords)
                {
                    if (sr.EndsWith(w))
                    {
                        HasNoteEnd = true;
                        break;
                    }
                }
            }
        }
    }
    internal class PythonFoldingStrategy
    {
        public void UpdateFoldings(FoldingManager manager, TextDocument document)
        {
            var foldings = CreateNewFoldings(document, out var firstErrorOffset);
            manager.UpdateFoldings(foldings, firstErrorOffset);
        }



        public IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
        {
            var foldings = new List<NewFolding>();
            var lineAnalizes = document.Lines.Select(l => new LineAnalize(document, l)).ToList();

            var noting = false;
            for (int i = 0; i < lineAnalizes.Count; i++)
            {
                if (noting) lineAnalizes[i].InNote = true;
                if (lineAnalizes[i].HasNote)
                {
                    noting = !noting;
                }
                if (noting) lineAnalizes[i].InNote = true;
                if (lineAnalizes[i].HasNoteEnd)
                {
                    noting = !noting;
                }
                if (noting) lineAnalizes[i].InNote = true;
            }

            var lookupItems = lineAnalizes.ToLookup(l => !l.Empty && !l.InNote);
            var keyLines = lookupItems[true].ToList();
            var emptyLines = lineAnalizes.Where(l => l.InNote).ToList();
            var spaces = new Stack<LineAnalize>();
            var count = 0;
            for (int i = 0; i < keyLines.Count; i++)
            {
                count = keyLines[i].Space;
                if (spaces.Count == 0) spaces.Push(keyLines[i]);
                else
                {
                    if (spaces.Peek().Space < count) spaces.Push(keyLines[i]);
                    else if (spaces.Peek().Space >= count)
                    {
                        while (spaces.Count > 0 && spaces.Peek().Space >= count)
                        {
                            var lz = spaces.Pop();
                            if (lz.Line != keyLines[i - 1].Line && lz.HasKeyword)
                                foldings.Add(new NewFolding(lz.Line.Offset, keyLines[i - 1].Line.EndOffset));
                        }
                        spaces.Push(keyLines[i]);
                    }
                }
            }

            var totalCount = keyLines.Count;
            while (spaces.Count > 0)
            {
                var lz = spaces.Pop();
                if (lz.Line != keyLines[totalCount - 1].Line && lz.HasKeyword)
                    foldings.Add(new NewFolding(lz.Line.Offset, keyLines[totalCount - 1].Line.EndOffset));
            }

            if (emptyLines.Count > 1)
            {
                var numbers = new List<int>() { 0 };
                for (int i = 1; i < emptyLines.Count; i++)
                {
                    if (emptyLines[i].Line.LineNumber - emptyLines[i - 1].Line.LineNumber > 1)
                    {
                        numbers.Add(i - 1);
                        numbers.Add(i);
                    }
                }
                numbers.Add(emptyLines.Count - 1);
                var eCount = numbers.Count / 2;
                for (int i = 0; i < eCount; i++)
                {
                    if (numbers[i * 2] < numbers[i * 2 + 1])
                    {
                        foldings.Add(new NewFolding(emptyLines[numbers[i * 2]].Line.Offset, emptyLines[numbers[i * 2 + 1]].Line.EndOffset));
                    }
                }
            }
            firstErrorOffset = 0;
            return foldings.OrderBy(f => f.StartOffset);
        }
    }
}
