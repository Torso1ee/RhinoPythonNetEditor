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
        DispatcherTimer timer;
        int time = 0;
        public Editor()
        {
            InitializeComponent();
            Id = Guid.NewGuid();
            CachePath = Path.GetDirectoryName(typeof(Editor).Assembly.Location) + @"\cache";
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += Timer_Tick;
        }

        private string CachePath { get; set; }
        private string CacheFile { get; set; }

        private Guid Id { get; set; }


        private void Document_TextChanged(object sender, EventArgs e)
        {
            time = 0;
            if (!timer.IsEnabled) timer.Start();
        }

        private async void Timer_Tick(object sender, EventArgs e)
        {
            time += 100;
            if (time >= 1000)
            {
                timer.Stop();
                File.WriteAllText(CacheFile, textEditor.Document.Text);
                var l = textEditor.Document.GetLineByOffset(textEditor.CaretOffset);
                await LintManager.RequestCompletionAsync(CacheFile, (l.LineNumber - 1, textEditor.CaretOffset - l.Offset - 1));
            }
        }


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
                textEditor.Document.TextChanged += Document_TextChanged;
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
    }
}
