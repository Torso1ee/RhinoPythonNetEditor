using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Runtime.Remoting.Messaging;
using System.Collections;

namespace RhinoPythonNetEditor.Debug
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

        private string PythonPath { get; set; } = "python";

        public event EventHandler<PowerShellDataAddedEventArgs> PowerShellDataAdded = delegate { };

        public event EventHandler<PowerShellRunScriptEventArgs> PowerShellRunScript = delegate { };

        public event EventHandler<PowerShellRunScriptEndEventArgs> PowerShellRunScriptEnd = delegate { };

        public PowerShellManager()
        {
            PSInstance = PowerShell.Create();
        }

        public IAsyncResult RunScript(string scriptText)
        {
            if (PSInstance != null)
            {
                var state = new object();
                Start = DateTime.Now;
                PSInstance.AddScript(scriptText);
                var inCol = new PSDataCollection<PSObject>();
                var col = new PSDataCollection<PSObject>();
                col.DataAdded += Col_DataAdded;
                PowerShellRunScript?.Invoke(this, new PowerShellRunScriptEventArgs { Script = scriptText });
                return PSInstance.BeginInvoke(inCol, col, new PSInvocationSettings(), OnExcuteEnd, state);
            }
            return null;
        }

        void OnExcuteEnd(IAsyncResult asyncResult)
        {
            var msg =string.Join("\n", PSInstance.Streams.Error.Select(d=>d.ToString()));
            PowerShellRunScriptEnd?.Invoke(this, new PowerShellRunScriptEndEventArgs { 
                Time = DateTime.Now - Start ,
                Error = PSInstance.HadErrors,
                ErrorMessage = msg
            });
        }

      
        void Col_DataAdded(object sender, DataAddedEventArgs e)
        {
            PowerShellDataAdded?.Invoke(this, new PowerShellDataAddedEventArgs { Message = (sender as PSDataCollection<PSObject>)[e.Index].ToString() });
        }

    }
}
