using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Runtime.Remoting.Messaging;
using System.Collections;

namespace RhinoPythonNetEditor.Managers
{
    public class PowerShellDataAddedEventArgs : EventArgs
    {
        public string Message { get; set; }
    }

    public class PowerShellRunScriptEventArgs : EventArgs
    {
        public string Script { get; set; }
    }

    public class PowerShellRunScriptEndEventArgs : EventArgs
    {
        public TimeSpan Time { get; set; }

        public bool Error { get; set; }

        public string ErrorMessage { get; set; }
    }
    public class PowerShellManager
    {
        private DateTime Start { get; set; }
        private PowerShell PSInstance { get; set; }


        public event EventHandler<PowerShellDataAddedEventArgs> PowerShellDataAdded = delegate { };

        public event EventHandler<PowerShellRunScriptEventArgs> PowerShellRunScript = delegate { };

        public event EventHandler<PowerShellRunScriptEndEventArgs> PowerShellRunScriptEnd = delegate { };

        public IAsyncResult RunScript(string scriptText)
        {
            PSInstance = PowerShell.Create();
            var state = new object();
            Start = DateTime.Now;
            PSInstance.AddScript(scriptText);
            PSInstance.Streams.ClearStreams();
            var inCol = new PSDataCollection<PSObject>();
            var col = new PSDataCollection<PSObject>();
            col.DataAdded += Col_DataAdded;
            inCol.DataAdded += Col_DataAdded;
            PowerShellRunScript?.Invoke(this, new PowerShellRunScriptEventArgs { Script = scriptText });
            return PSInstance.BeginInvoke(inCol, col, new PSInvocationSettings(), OnExcuteEnd, state);
        }

        void OnExcuteEnd(IAsyncResult asyncResult)
        {
            var msg = string.Join("\n", PSInstance.Streams.Error.Select(d => d.ToString()));
            PowerShellRunScriptEnd?.Invoke(this, new PowerShellRunScriptEndEventArgs
            {
                Time = DateTime.Now - Start,
                Error = PSInstance.HadErrors,
                ErrorMessage = msg
            });
            PSInstance.Stop();
        }


        void Col_DataAdded(object sender, DataAddedEventArgs e)
        {
            PowerShellDataAdded?.Invoke(this, new PowerShellDataAddedEventArgs { Message = (sender as PSDataCollection<PSObject>)[e.Index].ToString() });
        }

    }
}
