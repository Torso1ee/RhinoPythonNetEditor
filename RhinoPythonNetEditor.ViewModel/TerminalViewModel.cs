using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RhinoPythonNetEditor.DataModels.Business;
using RhinoPythonNetEditor.Debug;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
namespace RhinoPythonNetEditor.ViewModel
{
    public class TerminalViewModel : ObservableRecipient
    {

        private readonly PowerShellManager Manager;


        public TerminalViewModel()
        {
            Manager = new PowerShellManager();
            Manager.PowerShellRunScript += (s, e) => Free = false;
            Manager.PowerShellRunScriptEnd += OnExcuteEnd;
            Manager.PowerShellDataAdded += (s, e) => UpdateLine(new ScriptLine { State = ScriptLineState.Normal, Text = e.Message });
        }

        public ObservableCollection<ScriptLine> OutputContent { get; set; } = new ObservableCollection<ScriptLine>();
        public ObservableCollection<ScriptLine> ScriptRecorder { get; set; } = new ObservableCollection<ScriptLine>();


        private string script;
        public string Script
        {
            get { return script; }
            set { SetProperty(ref script, value); }
        }

        private bool free = true;

        public bool Free
        {
            get { return free; }
            set { SetProperty(ref free, value); }
        }

        private void OnExcuteEnd(object sender, PowerShellRunScriptEndEventArgs e)
        {
            if (e.Error) UpdateLine(new ScriptLine { State = ScriptLineState.Error, Text = e.ErrorMessage });
            Free = true;
        }

        private void UpdateLine(ScriptLine line)
        {
            Application.Current.Dispatcher.Invoke(() => OutputContent.Add(line));
        }

        private void RunScriptCore()
        {
            OutputContent.Add(new ScriptLine { State = ScriptLineState.Input, Text = Script });
            Manager?.RunScript(Script);
        }
        public ICommand RunScript => new RelayCommand(() => RunScriptCore());

    }
}
