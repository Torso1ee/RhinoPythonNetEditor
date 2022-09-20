using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Dynamic;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Utilities;
using System.Threading;
using Thread = System.Threading.Thread;

namespace RhinoPythonNetEditor.Managers
{
    public enum Reason
    {
        Breakpoint,
        Step,
        Pause,
        Unset,
    }
    public class StoppedArgs : EventArgs
    {
        public int Line { get; set; }

        public Reason Reason { get; set; }
    }
    public class DebugManager
    {
        public event EventHandler DebugEnd = delegate { };
        public event EventHandler<StoppedArgs> Stopped = delegate { };
        public event EventHandler ConfigDone = delegate { };
        public event EventHandler Continued = delegate { };

        private List<int> Indicis { get; set; }
        private string FilePath { get; set; }

        private int StoppedThreadId { get; set; }
        TcpClient TcpClient { get; set; }

        DebugProtocolHost Client { get; set; }
        public int AdapterPort { get; set; }


        public void Start(List<int> indicis, string file)
        {
            Indicis = indicis.ToList();
            FilePath = file;
            InitializeHost();
            Client.SendRequest(new InitializeRequest() { }, e => { });
            Client.SendRequest(new AttachRequest() { _Restart = false }, arg => { });

        }

        public void Continue()
        {
            if (Client.IsRunning) Client.SendRequest(new ContinueRequest(), e => { });
        }

        public void Stop()
        {
            if (Client.IsRunning) Client.SendRequest(new PauseRequest(), e => { });
        }

        public void Next()
        {
            if (Client.IsRunning) Client.SendRequest(new NextRequest() { ThreadId = StoppedThreadId }, e => { });
        }

        public void StepIn()
        {
            if (Client.IsRunning) Client.SendRequest(new StepInRequest() { ThreadId = StoppedThreadId }, e => { });
        }

        public void StepOut()
        {
            if (Client.IsRunning) Client.SendRequest(new StepOutRequest() { ThreadId = StoppedThreadId }, e => { });
        }

        public void Restart()
        {
            if (Client.IsRunning) Client.SendRequest(new RestartRequest(), e => { });
        }

        public void Terminate()
        {
            if (Client.IsRunning) Client.SendRequest(new TerminateRequest(), e => { });
        }

        public void SendBreakPointRequest(List<int> indicis)
        {
            Indicis = indicis.ToList();
            var req = new SetBreakpointsRequest();
            for (int i = 0; i < Indicis.Count; i++) req.Breakpoints.Add(new SourceBreakpoint(Indicis[i]));
            req.Source = new Source() { Path = FilePath };
            Client.SendRequest(req, (a, e) => { });
        }

        private void InitializeHost()
        {
            TcpClient = new TcpClient();
            TcpClient.Connect(IPAddress.Loopback.ToString(), AdapterPort);
            var stream = new NetworkStream(TcpClient.Client);
            Client = new DebugProtocolHost(stream, stream);
            Client.EventReceived += Client_EventReceived;
            Client.LogMessage += Client_LogMessage;
            Client.Run();
        }

        private void Client_LogMessage(object sender, LogEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        private void Client_EventReceived(object sender, EventReceivedEventArgs e)
        {
            if (e.EventType == "terminated")
            {
                End();
                DebugEnd?.Invoke(this, EventArgs.Empty);
            }
            else if (e.EventType == "initialized")
            {
                SendBreakPointRequest(Indicis);
                Client.SendRequest(new ConfigurationDoneRequest() { }, arg =>
                {
                    ConfigDone?.Invoke(this, EventArgs.Empty);
                });
            }
            else if (e.EventType == "stopped")
            {
                var se = e.Body as StoppedEvent;
                StoppedThreadId = se.ThreadId.Value;
                var reason = Reason.Unset;
                Enum.TryParse(se.Reason.ToString(), false, out reason);
                Client.SendRequest(new StackTraceRequest(se.ThreadId.Value), (args, resp) =>
                {
                    Stopped?.Invoke(this, new StoppedArgs { Line = resp.StackFrames[0].Line, Reason = reason });
                });
            }
            else if (e.EventType == "continued")
            {
                Continued?.Invoke(this, EventArgs.Empty);
            }

        }


        public void End()
        {
            Client.Stop();
            TcpClient.Close();
        }


        public int NextPort()
        {
            AdapterPort = NextFreePort();
            return AdapterPort;
        }

        int NextFreePort(int port = 0)
        {
            port = (port > 0) ? port : new Random().Next(1, 65535);
            while (!IsFree(port))
            {
                port += 1;
            }
            return port;
        }

        static bool IsFree(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] listeners = properties.GetActiveTcpListeners();
            int[] openPorts = listeners.Select(item => item.Port).ToArray<int>();
            return openPorts.All(openPort => openPort != port);
        }
    }
}
