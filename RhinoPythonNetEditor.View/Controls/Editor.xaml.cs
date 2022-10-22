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
using System.Text.RegularExpressions;
using RhinoPythonNetEditor.DataModels.Business;
using System.Windows.Threading;
using System.Configuration.Assemblies;
using Path = System.IO.Path;
using RhinoPythonNetEditor.Managers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using ICSharpCode.AvalonEdit.CodeCompletion;


namespace RhinoPythonNetEditor.View.Controls
{
    /// <summary>
    /// Editor.xaml 的交互逻辑
    /// </summary>
    public partial class Editor : UserControl
    {
        BreakPointMargin breakPointMargin;
        IHighlightingDefinition defaultHighlighting;
        WeakReferenceMessenger messenger;
        private CompletionWindow completionWindow;
        private OverloadInsightWindow insightWindow;


        public Editor()
        {
            InitializeComponent();
            Id = Guid.NewGuid();
            CachePath = Path.GetDirectoryName(typeof(Editor).Assembly.Location) + $@"\cache\{Id}";
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
        }

        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {

            if ((e.Key == Key.Enter || e.Key == Key.Tab) && completionWindow != null)
            {
                completionWindow.CompletionList.RequestInsertion(e);
            }
            insightWindow?.Hide();
        }

        private void TextArea_TextEntering(object sender, TextCompositionEventArgs e)
        {
            insightWindow?.Hide();
        }

        private async void TextArea_TextEntered(object sender, TextCompositionEventArgs e)
        {
            if (char.IsLetter(e.Text[0]) || e.Text == ".")
            {
                var t = textEditor.Document.Text;
                await Task.Run(() => File.WriteAllText(CacheFile, t));
                var items = await LintManager.RequestCompletionAsync(CacheFile, (textEditor.TextArea.Caret.Line - 1, textEditor.TextArea.Caret.Column - 1));
                var itemList = items.ToArray();
                if (itemList.Length > 0)
                {
                    completionWindow = new CompletionWindow(textEditor.TextArea);
                    completionWindow.Style = FindResource("CompletionWindowStyle") as Style;
                    completionWindow.CompletionList.Style = FindResource("CompletionListStyle") as Style;
                    completionWindow.Closed += (o, args) => completionWindow = null;
                    var data = completionWindow.CompletionList.CompletionData;
                    foreach (var item in items) data.Add(new CompletionData(item,messenger));
                    completionWindow.CompletionList.ListBox.SelectedIndex = 0;
                    completionWindow.Show();
                }
                else
                {
                    if (completionWindow != null) completionWindow.Close();
                }
            }
            else if (e.Text == "(" || e.Text == "," || e.Text=="=")
            {
                if (completionWindow != null) completionWindow.Close();
                var t = textEditor.Document.Text;
                await Task.Run(() => File.WriteAllText(CacheFile, t));
                var help = LintManager.RequestSignature(CacheFile, (textEditor.TextArea.Caret.Line - 1, textEditor.TextArea.Caret.Column - 1));
                if (help != null && help.Signatures.Count() > 0)
                {
                    insightWindow = new OverloadInsightWindow(textEditor.TextArea);
                    insightWindow.Style = FindResource("InsightWindowStyle") as Style;
                    insightWindow.Closed += (o, args) => insightWindow = null;
                    insightWindow.Provider = new OverloadProvider(help,messenger);
                    insightWindow.Show();
                }
            }
            else
            {
                if (completionWindow != null) completionWindow.Close();
            }
        }

        private string CachePath { get; set; }
        private string CacheFile { get; set; }

        private Guid Id { get; set; }


        private bool Installed { get; set; }


        private void BreakPointMargin_BreakPointChanged(object sender, BreakPointEventArgs e)
        {
            messenger.Send(new AllBreakPointInformationsMessage(e.Indicis));
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

        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!Installed)
            {
                InstallHighlightDefinition();
                InstallBreakPoint();
                InstallFolding();
                if (!LintManager.IsInitialized) await LintManager.InitialzeClientAsync();
                if (!Directory.Exists(CachePath)) Directory.CreateDirectory(CachePath);
                CacheFile = $@"{CachePath}\{Id}.py";
                File.WriteAllText(CacheFile, textEditor.Document.Text);
                LintManager.DidOpen(CacheFile);
                Installed = true;
            }
            if (messenger == null)
            {
                messenger = (DataContext as ViewModelLocator).Messenger;
                messenger.Register<StepMessage>(this, (r, m) =>
                {
                    Application.Current.Dispatcher.Invoke(() => breakPointMargin.Step(m.Line, m.Value));
                });
                breakPointMargin.BreakPointChanged += BreakPointMargin_BreakPointChanged;
            }

        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LintManager.DidClose(CacheFile);
        }
    }
}
