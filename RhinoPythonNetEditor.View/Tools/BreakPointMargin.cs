﻿using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Rendering;
using RhinoPythonNetEditor.View.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using ICSharpCode.AvalonEdit.Document;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using ICSharpCode.AvalonEdit.Utils;
using ICSharpCode.AvalonEdit;
using System.Runtime.Remoting.Contexts;
using System.Windows.Media.Media3D;

namespace RhinoPythonNetEditor.View.Tools
{
    public class BreakPointInformation
    {
        public int Row { get; set; }
    }

    public class BreakPointEventArgs:EventArgs
    {
        public BreakPointInformation Information { get; set; }

        public bool IsAddOrRemove { get; set; }
    }

    public class BreakPointMargin : AbstractMargin, IWeakEventListener
    {
        //private BreakpointStore _manager;
        TextArea textArea;
        private double radius;
        private int previewLine;
        private bool previewPointVisible;
        protected int maxLineNumberLength = 1;
        private List<BreakPointInformation> storedLines = new List<BreakPointInformation>();


        static BreakPointMargin()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BreakPointMargin),
                                                       new FrameworkPropertyMetadata(typeof(BreakPointMargin)));
        }

        public event EventHandler<BreakPointEventArgs> BreakPointChanged = delegate { };
        protected override Size MeasureOverride(Size availableSize)
        {
            radius = (double)GetValue(TextBlock.FontSizeProperty);
            return new Size(radius, 0);
        }


        public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                OnDocumentLineCountChanged();
                return true;
            }
            return false;
        }

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            return ReceiveWeakEvent(managerType, sender, e);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            TextView textView = this.TextView;
            if (textView != null && textView.VisualLinesValid)
            {
                foreach (VisualLine line in textView.VisualLines)
                {
                    double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                    drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(0, y - textView.VerticalOffset, radius, radius));

                }
                foreach (var i in storedLines)
                {
                    var line = TextView.VisualLines.FirstOrDefault(vl => vl.FirstDocumentLine.LineNumber == i.Row);
                    if (line != null)
                    {
                        double y = line.GetTextLineVisualYPosition(line.TextLines[0], VisualYPosition.TextTop);
                        drawingContext.DrawEllipse(Brushes.Red, null, new Point(0 + radius / 2, y - textView.VerticalOffset + radius / 2 + 1), radius / 2 - 2, radius / 2 - 2);
                    }
                }
                if (previewPointVisible)
                {
                    var item = storedLines.FirstOrDefault(l => l.Row == previewLine);
                    if (item == null)
                    {
                        var visualLine = TextView.VisualLines.FirstOrDefault(vl => vl.FirstDocumentLine.LineNumber == previewLine);
                        if (visualLine != null)
                        {
                            double y = visualLine.GetTextLineVisualYPosition(visualLine.TextLines[0], VisualYPosition.TextTop);
                            drawingContext.DrawEllipse(Brushes.DarkRed, null, new Point(0 + radius / 2, y - textView.VerticalOffset + radius / 2 + 1), radius / 2 - 2, radius / 2 - 2);
                        }
                    }
                }
            }
        }

        protected override void OnTextViewChanged(TextView oldTextView, TextView newTextView)
        {
            if (oldTextView != null)
            {
                oldTextView.VisualLinesChanged -= TextViewVisualLinesChanged;
            }
            base.OnTextViewChanged(oldTextView, newTextView);
            if (newTextView != null)
            {
                newTextView.VisualLinesChanged += TextViewVisualLinesChanged;
                // find the text area belonging to the new text view
                textArea = newTextView.GetService(typeof(TextArea)) as TextArea;
            }
            else
            {
                textArea = null;
            }
            InvalidateVisual();
        }
        void TextViewVisualLinesChanged(object sender, EventArgs e)
        {
            InvalidateVisual();
        }


        protected override void OnDocumentChanged(TextDocument oldDocument, TextDocument newDocument)
        {
            if (oldDocument != null)
            {
                PropertyChangedEventManager.RemoveListener(oldDocument, this, "LineCount");
            }
            base.OnDocumentChanged(oldDocument, newDocument);
            if (newDocument != null)
            {
                PropertyChangedEventManager.AddListener(newDocument, this, "LineCount");
            }
            OnDocumentLineCountChanged();
        }

        void OnDocumentLineCountChanged()
        {
             int documentLineCount = Document != null ? Document.LineCount : 1;
            int newLength = documentLineCount.ToString(CultureInfo.CurrentCulture).Length;
            storedLines = storedLines.Where(l => l.Row <= Document.LineCount).ToList();
            // The margin looks too small when there is only one digit, so always reserve space for
            // at least two digits
            if (newLength < 2)
                newLength = 2;

            if (newLength != maxLineNumberLength)
            {
                maxLineNumberLength = newLength;
                InvalidateMeasure();
            }
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (previewLine != -1)
            {
                var info = storedLines.FirstOrDefault(l => l.Row == previewLine);
                if (info == null)
                {
                    var infor = new BreakPointInformation { Row = previewLine };
                    storedLines.Add(infor);
                    BreakPointChanged?.Invoke(this, new BreakPointEventArgs { Information = infor, IsAddOrRemove = true });
                }
                else
                {
                    storedLines.Remove(info);
                    BreakPointChanged?.Invoke(this, new BreakPointEventArgs { Information = info, IsAddOrRemove = false });
                }
            }
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            previewPointVisible = true;
            var textView = TextView;
            var offset = GetOffsetFromPoint(e);
            if (offset != -1)
            {
                previewLine = textView.Document.GetLineByOffset(offset).LineNumber; // convert from text line to visual line.
            }
            InvalidateVisual();
        }

      


        int GetOffsetFromPoint(MouseEventArgs e)
        {
            Point pos = e.GetPosition(TextView);
            pos.X = 0;
            pos.Y = Math.Min(pos.Y, TextView.ActualHeight);
            pos.Y = Math.Max(pos.Y, 0);
            pos.Y += TextView.VerticalOffset;
            VisualLine vl = TextView.GetVisualLineFromVisualTop(pos.Y);
            TextLine tl = vl.GetTextLineByVisualYPosition(pos.Y);
            int relStart = vl.FirstDocumentLine.Offset;
            return relStart;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            previewLine = -1;
            previewPointVisible = false;
            InvalidateVisual();
        }
    }
}
