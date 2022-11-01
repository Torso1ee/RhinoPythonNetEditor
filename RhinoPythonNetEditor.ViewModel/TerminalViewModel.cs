using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RhinoPythonNetEditor.DataModels.Business;
using RhinoPythonNetEditor.Managers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using RhinoPythonNetEditor.ViewModel.Messages;
using System.Text.RegularExpressions;

namespace RhinoPythonNetEditor.ViewModel
{
    public class TerminalViewModel : ObservableRecipient
    {

        private readonly PowerShellManager Manager;



        public TerminalViewModel(ViewModelLocator locator)
        {
            Manager = new PowerShellManager();
            Manager.PowerShellRunScript += (s, e) => Free = false;
            Manager.PowerShellRunScriptEnd += OnExcuteEnd;
            Manager.PowerShellDataAdded += (s, e) => UpdateLine(new ScriptLine { State = ScriptLineState.Normal, Text = e.Message });
            Locator = locator;
            Locator.ConfigureFinished +=(s,e) => Messenger.Register<DebugRequestMessage>(this, Receive);
            IsActive = true;
        }

        private WeakReferenceMessenger Messenger => Locator?.Messenger;
        private ViewModelLocator Locator { get; set; }

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
            string time = "";
            if (e.Error) UpdateLine(new ScriptLine { State = ScriptLineState.Error, Text = e.ErrorMessage });
            if (e.Time.TotalSeconds <= 60) time = $"{Math.Round(e.Time.TotalSeconds, 2)} sec";
            else if (e.Time.TotalSeconds <= 3600) time = $"{e.Time.Minutes} min {e.Time.Seconds} sec";
            else time = $"{Math.Round(e.Time.TotalSeconds / 60)} min";
            UpdateLine(new ScriptLine { State = ScriptLineState.Normal, Text = $"End, run time: {time}" });
            Free = true;
        }

        private void UpdateLine(ScriptLine line)
        {
            if(line.State == ScriptLineState.Error)
            {
                var txt = line.Text.Split('\n');
                var ls = new List<string>();
                foreach(var l in txt)
                {
                    if (l.Contains("temp.py"))
                    {
                        var ma = Regex.Match(l, @"\\temp.py"", line (\d+)");
                        if (ma.Groups.Count == 2)
                        {
                            var eLine = ma.Groups[0].Value.Replace(ma.Groups[1].Value, (Math.Max(0,int.Parse(ma.Groups[1].Value) - 5)).ToString());
                            ls.Add(l.Replace(ma.Groups[0].Value, eLine));
                        }
                        else ls.Add(l);
                    }
                    else if (!l.Contains("0.00s - ")) ls.Add(l);
                }
                line.Text = string.Join("\n", ls);
            }
            Application.Current.Dispatcher.Invoke(() => OutputContent.Add(line));
        }

        public void RunScriptCore()
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

        public void Receive(object recipient, DebugRequestMessage message)
        {
            OutputContent.Clear();
            Script = message.Script;
            RunScriptCore();
            message.Reply(true);
        }



        public ICommand LastScript => new RelayCommand(() => LastScriptCore());

        public ICommand NextScript => new RelayCommand(() => NextScriptCore());

        public ICommand ResetIndex => new RelayCommand(() => Index = -1);

        public ICommand RunScript => new RelayCommand(() => RunScriptCore());

    }
}
