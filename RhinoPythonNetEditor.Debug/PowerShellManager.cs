using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;

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
    }
    public class PowerShellManager
    {

        private PowerShell PSInstance { get; set; }

        public event EventHandler<PowerShellDataAddedEventArgs> PowerShellDataAdded = delegate { };

        public event EventHandler<PowerShellRunScriptEventArgs> PowerShellRunScript = delegate { };

        public event EventHandler<PowerShellRunScriptEndEventArgs> PowerShellRunScriptEnd = delegate { };

        public PowerShellManager()
        {
            PSInstance = PowerShell.Create();
        }

        public void RunScript(string scriptText)
        {
            if (PSInstance != null)
            {
                var state = new object();
                var start = DateTime.Now;
                PSInstance.AddScript(scriptText);
                var inCol = new PSDataCollection<PSObject>();
                var col = new PSDataCollection<PSObject>();
                col.DataAdded += Col_DataAdded;
                PowerShellRunScript?.Invoke(this, new PowerShellRunScriptEventArgs { Script = scriptText });
                PSInstance.BeginInvoke(inCol, col, new PSInvocationSettings(), asyncResult => PowerShellRunScriptEnd?.Invoke(this, new PowerShellRunScriptEndEventArgs { Time = DateTime.Now - start }), state);
            }
        }

        void Col_DataAdded(object sender, DataAddedEventArgs e)
        {
            PowerShellDataAdded?.Invoke(this, new PowerShellDataAddedEventArgs { Message = (sender as PSDataCollection<PSObject>)[e.Index].ToString() });
        }
    }
}
