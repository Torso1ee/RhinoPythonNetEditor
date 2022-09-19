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
using ICSharpCode.AvalonEdit;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.ViewModel;
using RhinoPythonNetEditor.ViewModel.Messages;

namespace RhinoPythonNetEditor.View.Controls
{
    /// <summary>
    /// Editor.xaml 的交互逻辑
    /// </summary>
    public partial class Editor : UserControl
    {

        BreakPointMargin breakPointMargin;
        IHighlightingDefinition defaultHighlighting;

        public Editor()
        {
            InitializeComponent();
            InstallHighlightDefinition();
            InstallBreakPoint();
            InstallFolding();
            WeakReferenceMessenger.Default.Register<CodeRequestMessage>(this, (r, m) =>
            {
                m.Reply(textEditor.Document.Text);
            });
            breakPointMargin.BreakPointChanged += BreakPointMargin_BreakPointChanged;
        }

        private void BreakPointMargin_BreakPointChanged(object sender, BreakPointEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new BreakPointValueChangedMessage(e.IsAddOrRemove) { Line = e.Information.Row });
        }

        private void InstallHighlightDefinition()
        {
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
        }

        private void InstallBreakPoint()
        {
            breakPointMargin = new BreakPointMargin();
            var lm = textEditor.TextArea.LeftMargins;
            lm.Insert(0, breakPointMargin);
            var nm = lm[1] as LineNumberMargin;
            nm.Margin = new Thickness(0, 0, 5, 0);
            lm.RemoveAt(2);
        }

        private void InstallFolding()
        {
            var manager = FoldingManager.Install(textEditor.TextArea);
            var strategy = new PythonFoldingStrategy();
            textEditor.Document.Changed += (s, e) => strategy.UpdateFoldings(manager, textEditor.Document);
        }


    }
}
