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

        private int Index = -1;

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
            Index = -1;
            OutputContent.Add(new ScriptLine { State = ScriptLineState.Input, Text = Script });
            ScriptRecorder.Add(new ScriptLine { State = ScriptLineState.Input, Text = Script });
            Manager?.RunScript(Script);
            Script = "";
        }

        private void LastScriptCore()
        {
            if (Index == -1) Index = ScriptRecorder.Count;
            if (Index > 0)
            {
                Script = ScriptRecorder[--Index].Text;
            }
        }

        private void NextScriptCore()
        {
            if (Index != -1 && Index < ScriptRecorder.Count - 1)
            {
                Script = ScriptRecorder[++Index].Text;
            }
        }

        public ICommand LastScript => new RelayCommand(() => LastScriptCore());

        public ICommand NextScript => new RelayCommand(() => NextScriptCore());

        public ICommand ResetIndex => new RelayCommand(() => Index = -1);

        public ICommand RunScript => new RelayCommand(() => RunScriptCore());

    }
}
