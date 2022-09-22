using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Grasshopper.Kernel;
using RhinoPythonNetEditor.Managers;
using RhinoPythonNetEditor.ViewModel.Messages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RhinoPythonNetEditor.ViewModel
{
    public class DebugViewModel : ObservableRecipient
    {
        private WeakReferenceMessenger Messenger => Locator?.Messenger;
        private ViewModelLocator Locator { get; set; }
        public DebugViewModel( ViewModelLocator locator)
        {
            Locator = locator;
            Locator.ConfigureFinished += (s,e) => Messenger.Register<AllBreakPointInformationsMessage>(this, Receive);
            currentDir = Directory.GetCurrentDirectory();
            IsActive = true;
        }

        private string PythonPath { get; set; } = @"D:\Anaconda\envs\PythonNet\python.exe";

        private bool isDebuging;

        public bool IsDebuging
        {
            get { return isDebuging; }
            set
            {
                SetProperty(ref isDebuging, value);
            }
        }
        private string currentDir { get; set; }

        public ICommand StartDebug => new RelayCommand(() => StartDebugCore(), () => !IsDebuging);

        private bool stopped;

        public bool Stopped
        {
            get { return stopped; }
            set { SetProperty(ref stopped, value); }
        }
        private bool configDone = false;

        public bool ConfigDone
        {
            get { return configDone; }
            set { SetProperty(ref configDone, value); }
        }

        private bool restart;

        public bool Restarting
        {
            get { return restart; }
            set { SetProperty(ref restart, value); }
        }


        public ICommand Stop => new RelayCommand(() => debugManager?.Stop());
        public ICommand Continue => new RelayCommand(() => { debugManager?.Continue(); ConfigDone = false; });
        public ICommand Next => new RelayCommand(() => debugManager?.Next());

        public ICommand StepIn => new RelayCommand(() => debugManager?.StepIn());
        public ICommand StepOut => new RelayCommand(() => debugManager?.StepOut());

        public ICommand Restart => new RelayCommand(() => { Restarting = true; debugManager?.Terminate(); }, () => !Restarting);

        private List<int> Indicis { get; set; } = new List<int>();
        public ICommand Terminate => new RelayCommand(() => { debugManager?.Terminate(); restart = false; });

        private DebugManager debugManager;
        private int CurrentLine { get; set; } = -1;

        private Reason CurrentStopReason { get; set; } = Reason.Unset;
        private async void StartDebugCore()
        {
            debugManager = new DebugManager();
            var port = debugManager.NextPort();
            var infos = Indicis;
            var code = Messenger.Send<CodeRequestMessage>();
            if (!Directory.Exists($@"temp\")) Directory.CreateDirectory($@"temp\");
            using (var fs = new FileStream(@"temp\temp.py", FileMode.Create))
            {
                var bytes = Encoding.UTF8.GetBytes(code.Response);
                await fs.WriteAsync(bytes, 0, bytes.Length);
            }
            var file = currentDir + $@"\temp\temp.py";
            var script = $@"{PythonPath} -u -m debugpy --listen localhost:{port} --wait-for-client --log-to ~/logs ""{file}""";
            debugManager.DebugEnd += DebugManager_DebugEnd;
            debugManager.Stopped += DebugManager_Stopped;
            debugManager.ConfigDone += (s, e) => ConfigDone = true;
            debugManager.Continued += DebugManager_Continued;
            if (Messenger.Send(new DebugRequestMessage { Port = port, Script = script }))
            {
                IsDebuging = true;
                debugManager.Start(infos.ToList(), file);
            }
        }

        private async void DebugManager_DebugEnd(object sender, EventArgs e)
        {
            var start = restart;
            ConfigDone = false;
            Stopped = false;
            Messenger.Send(new StepMessage(false) { Line = -1 });
            if (start)
            {
                await Task.Delay(1000);
                Application.Current.Dispatcher.Invoke(() => StartDebugCore());
            }
            else
            {
                IsDebuging = false;
            }
            Restarting = false;
        }

        private void DebugManager_Continued(object sender, EventArgs e)
        {
            Messenger.Send(new StepMessage(false) { Line = CurrentLine });
            Stopped = false;
            ConfigDone = true;
        }

        private void DebugManager_Stopped(object sender, StoppedArgs e)
        {
            CurrentLine = e.Line;
            Stopped = true;
            CurrentStopReason = e.Reason;
            Messenger.Send(new StepMessage(true) { Line = e.Line });
        }

        public void Receive(object recipient, AllBreakPointInformationsMessage message)
        {
            Indicis.Clear();
            Indicis.AddRange(message.Value);
            if (IsDebuging) debugManager.SendBreakPointRequest(Indicis);
        }

        private void SerializeParams()
        {
        }
    }
}
