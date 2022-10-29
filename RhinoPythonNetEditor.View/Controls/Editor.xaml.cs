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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.AxHost;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;


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
        TextMarkerService textMarkerService;
        private CompletionWindow completionWindow;
        private OverloadInsightWindow insightWindow;
        public LintManager LintManager = LintManager.Instance;
        private static Dictionary<string, WeakReferenceMessenger> MessengerRecord = new Dictionary<string, WeakReferenceMessenger>();
        private bool canReDraw = true;
        private DispatcherTimer timer;
        private int time = 0;

        public Editor()
        {
            InitializeComponent();
            Id = Guid.NewGuid();
            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(30) };
            timer.Tick += Timer_Tick;
            CachePath = Path.GetDirectoryName(typeof(Editor).Assembly.Location) + $@"\cache\{Id}";
            textEditor.TextArea.TextEntered += TextArea_TextEntered;
            textEditor.TextArea.PreviewKeyDown += TextArea_PreviewKeyDown;
            textEditor.TextArea.PreviewKeyUp += TextArea_PreviewKeyUp;
            textEditor.TextArea.TextEntering += TextArea_TextEntering;
            textEditor.TextArea.TextView.NonPrintableCharacterBrush = new SolidColorBrush(Color.FromRgb(0xD4, 0xD4, 0xD4)) { Opacity = 0.3 };
            IsVisibleChanged += Editor_IsVisibleChanged;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            time += 10;
            if (time > 100 && canReDraw)
            {
                canReDraw = false;
                time = 0;
                timer.Stop();
                var t = textEditor.Document.Text;
                LintManager.DidChange(CacheFile, t);
            }
        }

        private void TextArea_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            time = 0;
            if (!timer.IsEnabled) timer.Start();
        }

        private async void Editor_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue && textEditor.Document != null)
            {
                await Task.Delay(100);

                var t = textEditor.Document.Text;
                LintManager.DidChange(CacheFile, t);
            }
            else
            {
                insightWindow?.Close();
                completionWindow?.Close();
            }
        }

        private void AddHighLight(int start, int offset, Color color)
        {
            ITextMarker marker = textMarkerService.Create(start, offset);
            marker.ForegroundColor = color;
            marker.Reason = MarkerReason.HighLight;
        }

        private void AddMark(int start, int offset, Color color)
        {
            ITextMarker marker = textMarkerService.Create(start, offset);
            marker.ForegroundColor = color;
            marker.Reason = MarkerReason.Mark;
        }


        private void AddErrorHint(int start, int offset, Color color)
        {
            ITextMarker marker = textMarkerService.Create(start, offset);
            marker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker.MarkerColor = color;
            marker.Reason = MarkerReason.Hint;
        }
        private void InstallTextMarkerService()
        {
            textMarkerService = new TextMarkerService(textEditor.Document);
            textEditor.TextArea.TextView.BackgroundRenderers.Add(textMarkerService);
            textEditor.TextArea.TextView.LineTransformers.Add(textMarkerService);
        }
        private void TextArea_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            time = 0;
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
                LintManager.DidChange(CacheFile, t);
                var items = await LintManager.RequestCompletionAsync(CacheFile, (textEditor.TextArea.Caret.Line - 1, textEditor.TextArea.Caret.Column - 1));
                if (items != null && items.Count() > 0)
                {
                    completionWindow = new CompletionWindow(textEditor.TextArea);
                    completionWindow.Style = FindResource("CompletionWindowStyle") as Style;
                    completionWindow.CompletionList.Style = FindResource("CompletionListStyle") as Style;
                    completionWindow.Closed += (o, args) => completionWindow = null;
                    var data = completionWindow.CompletionList.CompletionData;
                    foreach (var item in items) data.Add(new CompletionData(item, messenger));
                    completionWindow.CompletionList.ListBox.SelectedIndex = 0;
                    completionWindow.Show();
                }
                else
                {
                    if (completionWindow != null) completionWindow.Close();
                }
            }
            else if (e.Text == "(" || e.Text == "," || e.Text == "=")
            {
                var t = textEditor.Document.Text;
                LintManager.DidChange(CacheFile, t);
                if (completionWindow != null) completionWindow.Close();
                var help = await LintManager.RequestSignatureAsync(CacheFile, (textEditor.TextArea.Caret.Line - 1, textEditor.TextArea.Caret.Column - 1));
                if (help != null && help.Signatures.Count() > 0)
                {
                    insightWindow = new OverloadInsightWindow(textEditor.TextArea);
                    insightWindow.Style = FindResource("InsightWindowStyle") as Style;
                    insightWindow.Closed += (o, args) => insightWindow = null;
                    insightWindow.Provider = new OverloadProvider(help, messenger);
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
            IsEnabled = false;
            if (!Installed)
            {
                InstallHighlightDefinition();
                InstallBreakPoint();
                InstallFolding();
                InstallTextMarkerService();
                if (!LintManager.IsInitialized)
                {
                    await LintManager.InitialzeClientAsync();
                    LintManager.OnDiagnosticPublished += (s, arg) =>
                    {
                        MessengerRecord[arg.File].Send(new SyntaxHintChangedMessage(arg.PublishDiagnostics));
                    };
                }
                CacheFile = $@"{CachePath}\{Id}.py";
                LintManager.DidOpen(CacheFile);
                Installed = true;
            }
            if (messenger == null)
            {
                messenger = (DataContext as ViewModelLocator).Messenger;
                MessengerRecord[Id.ToString()] = messenger;
                messenger.Register<StepMessage>(this, (r, m) =>
                {
                    Application.Current.Dispatcher.Invoke(() => breakPointMargin.Step(m.Line, m.Value));
                });
                messenger.Register<SetCodeMessage>(this, (r, m) => Application.Current.Dispatcher.Invoke(() =>
                {
                    textEditor.Text = m.Value;
                    textMarkerService.RemoveAll(t => true);
                    LintManager.DidChange(CacheFile, m.Value);
                }));
                messenger.Register<EditorEditMessage>(this, (r, m) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (m.Value)
                        {
                            case EditBehaviors.Copy:
                                textEditor.Copy();
                                break;
                            case EditBehaviors.Paste:
                                textEditor.Paste();
                                break;
                            case EditBehaviors.Cut:
                                textEditor.Cut();
                                break;
                            case EditBehaviors.Redo:
                                textEditor.Redo();
                                break;
                            case EditBehaviors.Undo:
                                textEditor.Undo();
                                break;
                            case EditBehaviors.SelectAll:
                                textEditor.SelectAll();
                                break;
                        }

                    });
                });
                messenger.Register<MarkMessage>(this, (r, m) =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        switch (m.Value)
                        {
                            case MarkBehaviors.Mark:
                                AddMark(textEditor.SelectionStart, textEditor.SelectionLength, Colors.Yellow);
                                break;
                            case MarkBehaviors.Unmark:
                                textMarkerService.RemoveAll(t => t.StartOffset >= textEditor.SelectionStart && t.EndOffset <= textEditor.SelectionStart + textEditor.SelectionLength && t.Reason == MarkerReason.Mark);
                                break;
                            case MarkBehaviors.UnmarkAll:
                                textMarkerService.RemoveAll(t => t.Reason == MarkerReason.Mark);
                                break;
                        }
                    });
                });
                messenger.Register<SyntaxHintChangedMessage>(this, (r, m) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        textMarkerService.RemoveAll(t => t.Reason == MarkerReason.Hint);
                        foreach (var s in m.Value)
                        {
                            var color = Colors.Transparent;
                            switch (s.Servity)
                            {
                                case Servity.Error:
                                    color = Colors.DarkRed;
                                    break;
                                case Servity.Warning:
                                    color = Colors.DarkOrange;
                                    break;
                                default:
                                    color = Colors.DarkBlue;
                                    break;
                            }
                            if (s.Start.Item1 < textEditor.Document.Lines.Count)
                            {
                                var start = Math.Min(textEditor.Document.Lines[s.Start.Item1].EndOffset, textEditor.Document.Lines[s.Start.Item1].Offset + s.Start.Item2);
                                var end = Math.Min(textEditor.Document.Lines[s.End.Item1].EndOffset, textEditor.Document.Lines[s.End.Item1].Offset + s.End.Item2);
                                AddErrorHint(start, Math.Max(0, end - start), color);
                            }
                            else
                            {
                                AddErrorHint(textEditor.Document.TextLength - 1, 0, color);
                            }
                        }
                    });
                    canReDraw = true;
                });
                breakPointMargin.BreakPointChanged += BreakPointMargin_BreakPointChanged;
            }
            IsEnabled = true;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            LintManager.DidClose(CacheFile);
        }
    }
}
