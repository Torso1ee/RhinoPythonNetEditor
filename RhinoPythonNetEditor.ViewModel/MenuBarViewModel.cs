using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using RhinoPythonNetEditor.ViewModel.Messages;
using System.Diagnostics;
using System.Windows.Documents;
using System.IO;
using RhinoPythonNetEditor.Interface;
using Microsoft.Win32;
using System.Security.Policy;


namespace RhinoPythonNetEditor.ViewModel
{

    public class MenuBarViewModel : ObservableRecipient
    {
        public MenuBarViewModel(ViewModelLocator locator)
        {
            IsActive = true;
            Locator = locator;
        }

        private WeakReferenceMessenger Messenger => Locator?.Messenger;

        private ViewModelLocator Locator { get; set; }

        public ICommand OpenWebsite => new RelayCommand<string>(uri =>
        {
            Process.Start(new ProcessStartInfo("cmd", $"/c start {uri}"));
        });

        private int port = 1080;

        public int Port
        {
            get { return port; }
            set { SetProperty(ref port, value); }
        }

        private string address = "127.0.0.1";

        public string Address
        {
            get { return address = "127.0.0.1"; }
            set { SetProperty(ref address, value); }
        }

        private string libName;

        public string LibName
        {
            get { return libName; }
            set { SetProperty(ref libName, value); }
        }

        public ICommand Pip => new RelayCommand<string>(txt =>
        {
            if (txt != "install" && txt != "uninstall") return;
            var pythonPath = Locator.DebugViewModel.PythonPath;
            var pipPath = Locator.DebugViewModel.CurrentDir + @"\python_env\Scripts\pip.exe";
            var proxyCmd = "";
            if (useProxy) proxyCmd = $"--proxy {Address}:{Port}";
            var cmd = $"{pipPath} {txt} {libName} {proxyCmd}";
            Process.Start(new ProcessStartInfo(pythonPath, cmd));
        });

        public ICommand PipDialog => new RelayCommand(() =>
        {
            Messenger.Send(new PipMessage { DataContext = this });
        });

        private bool useProxy;

        public bool UseProxy
        {
            get { return useProxy; }
            set { SetProperty(ref useProxy, value); }
        }


        public ICommand Run => new RelayCommand(() =>
        {
            if (Locator.ComponentHost != null && Locator.ComponentHost is IScriptComponent sc) sc.SetSource(Locator.TextEditorViewModel.Document.Text);
            Locator.ComponentHost.ExpireSolution(true);
        });

        public ICommand Confirm => new RelayCommand(() =>
        {
            if (Locator.ComponentHost != null && Locator.ComponentHost is IScriptComponent sc)
            {
                sc.SetSource(Locator.TextEditorViewModel.Document.Text);
                sc.CloseEditor();
            }
            Locator.ComponentHost.ExpireSolution(true);

        });

        public ICommand ImportFrom => new RelayCommand(() =>
        {
            OpenFileDialog dialog = new OpenFileDialog
            {
                Title = "Please select file",
                Filter = "Python Code File(*.py)|*py"
            };
            if (dialog.ShowDialog() == true)
            {
                var text = File.ReadAllText(dialog.FileName);
                Messenger.Send(new SetCodeMessage(text));
            }
        });

        public ICommand ExportAs => new RelayCommand(() =>
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Title = "Save As",
                Filter = "Python Code File(*.py)|*py"
            };
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName + ".py", Locator.TextEditorViewModel.Document.Text);
            }
        });

        public ICommand Edit => new RelayCommand<string>(txt =>
        {
            if (Enum.TryParse(txt, out EditBehaviors behavior))
                Messenger.Send(new EditorEditMessage(behavior));

        });

        public ICommand Note => new RelayCommand(() =>
        {
            Messenger.Send(new NoteRequestMessage());
        });

        public ICommand Mark => new RelayCommand<string>(txt =>
        {
            if (Enum.TryParse(txt, out MarkBehaviors behavior))
                Messenger.Send(new MarkMessage(behavior));
        });

        public ICommand Search => new RelayCommand(() =>
        {
            Locator.TextEditorViewModel.IsSearch = true;
        });

    }
}
