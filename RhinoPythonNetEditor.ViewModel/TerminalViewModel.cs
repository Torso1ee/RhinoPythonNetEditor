using CommunityToolkit.Mvvm.ComponentModel;
using RhinoPythonNetEditor.DataModels.Business;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhinoPythonNetEditor.ViewModel
{
    public class TerminalViewModel : ObservableRecipient
    {

        public ObservableCollection<ScriptLine> OutputContent { get; set; } = new ObservableCollection<ScriptLine>() { 
        new ScriptLine{ State= ScriptLineState.Normal, Text="D:\\Anaconda\\envs\\PythonNet\\python.exe"},
        new ScriptLine{ State= ScriptLineState.Normal, Text="D:\\Anaconda\\envs\\PythonNet\\python.exe"},
        new ScriptLine{ State= ScriptLineState.Normal, Text="D:\\Anaconda\\envs\\PythonNet\\python.exe"},
        new ScriptLine{ State= ScriptLineState.Normal, Text="D:\\Anaconda\\envs\\PythonNet\\python.exe"},
        };

        private string script;
        public string Script
        {
            get { return script; }
            set { SetProperty(ref script, value); }
        }

    }
}
