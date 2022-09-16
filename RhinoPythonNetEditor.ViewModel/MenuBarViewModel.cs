using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RhinoPythonNetEditor.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RhinoPythonNetEditor.ViewModel
{
    public class MenuBarViewModel : ObservableRecipient
    {
        public ICommand StartDebug => new RelayCommand(() => StartDebugCore(),()=>!IsDebuging);


        private bool isDebuging;

        public bool IsDebuging
        {
            get { return isDebuging; }
            set { SetProperty(ref isDebuging, value); }
        }

        private void StartDebugCore()
        {
            var debugManager = new DebugManager();
            debugManager.OnDebugEnded += (s, e) => IsDebuging = false;
            IsDebuging = true;
            debugManager.Start(@"D:\Source\VsCodeRepos\pythonTest\ttt.py");
        }
    }
}
