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

namespace RhinoPythonNetEditor.ViewModel
{
    public class MenuBarViewModel : ObservableRecipient
    {
        public MenuBarViewModel()
        {
            IsActive = true;
            currentDir = Directory.GetCurrentDirectory();
        }
        public ICommand StartDebug => new RelayCommand(() => StartDebugCore(), () => !IsDebuging);


        private string currentDir { get; set; }

        private bool isDebuging;

        public bool IsDebuging
        {
            get { return isDebuging; }
            set { SetProperty(ref isDebuging, value); }
        }

        private async void StartDebugCore()
        {
            var debugManager = new DebugManager();
            var port = debugManager.NextPort();
            var infos = WeakReferenceMessenger.Default.Send<AllBreakPointInformationsRequestMessage>();
            var code = WeakReferenceMessenger.Default.Send<CodeRequestMessage>();
            if (!Directory.Exists($@"temp\")) Directory.CreateDirectory($@"temp\");
            using (var fs = new FileStream(@"temp\temp.py", FileMode.Truncate))
            {
                var bytes = Encoding.UTF8.GetBytes(code.Response);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
            var file = currentDir+ $@"\temp\temp.py";
            var script = $@"python -u -m debugpy --listen localhost:{port} --wait-for-client --log-to ~/logs ""{file}""";
            debugManager.DebugEnd += (s, e) => IsDebuging = false;
            if (WeakReferenceMessenger.Default.Send(new DebugRequestMessage { Port = port, Script = script }))
            {
                IsDebuging = true;
                debugManager.Start(infos.Response,file);
            }
        }

    }
}
