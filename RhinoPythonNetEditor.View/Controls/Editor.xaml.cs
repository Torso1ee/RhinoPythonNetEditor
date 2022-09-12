using ICSharpCode.AvalonEdit.Highlighting;
using RhinoPythonNetEditor.Resources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using System.Xml;
using System.Resources;
using System.Collections;
using ICSharpCode.AvalonEdit.Folding;
using RhinoPythonNetEditor.View.Pages;
using ICSharpCode.AvalonEdit.Editing;
using RhinoPythonNetEditor.View.Tools;

namespace RhinoPythonNetEditor.View.Controls
{
    /// <summary>
    /// Editor.xaml 的交互逻辑
    /// </summary>
    public partial class Editor : UserControl
    {

        public Editor()
        {
            InitializeComponent();
            IHighlightingDefinition defaultHighlighting;
            using (Stream s = new MemoryStream(RhinoPythonNetEditor.Resources.Properties.Resources.Default))
            {
                using (XmlReader reader = new XmlTextReader(s))
                {
                    defaultHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                        HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
            HighlightingManager.Instance.RegisterHighlighting("DefaultHighlighting", new string[] { ".cool" }, defaultHighlighting);
            textEditor.SyntaxHighlighting = defaultHighlighting;
            var bm = new BreakPointMargin();
            var lm = textEditor.TextArea.LeftMargins;
            lm.Insert(0, bm);
            var nm = lm[1] as LineNumberMargin;
            nm.Margin = new Thickness(0, 0, 20, 0);
            lm.RemoveAt(2);
        }

    }
}
