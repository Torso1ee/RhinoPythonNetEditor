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

namespace RhinoPythonNetEditor.ViewModel
{
    public class MenuBarViewModel : ObservableRecipient
    {
        public MenuBarViewModel()
        {
            IsActive = true;
        }
        public ICommand StartDebug => new RelayCommand(() => StartDebugCore(), () => !IsDebuging);


        private bool isDebuging;

        public bool IsDebuging
        {
            get { return isDebuging; }
            set { SetProperty(ref isDebuging, value); }
        }

        private void StartDebugCore()
        {
            var debugManager = new DebugManager();
            var port = debugManager.NextPort();
            var file = @"D:\Source\VsCodeRepos\pythonTest\ttt.py";
            var script = $@"python -u -m debugpy --listen localhost:{port} --wait-for-client ""{file}""";
            debugManager.DebugEnd += (s, e) => IsDebuging = false;
            if (WeakReferenceMessenger.Default.Send(new DebugRequestMessage { Port = port, Script = script }))
            {
                IsDebuging = true;
                debugManager.Start();
            }
        }

    }
}
