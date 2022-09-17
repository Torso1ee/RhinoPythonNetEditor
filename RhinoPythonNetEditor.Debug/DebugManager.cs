using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SuperSimpleTcp;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace RhinoPythonNetEditor.Debug
{
    public class DebugManager
    {


        public DebugManager(PowerShellManager powerShellManager)
        {
            PowerShellManager = powerShellManager;
        }

        public PowerShellManager PowerShellManager { get; set; }

        public event EventHandler OnDebugEnded = delegate { };

        TcpClient tcpClient { get; set; }

        DebugProtocolHost Client { get; set; }
        int AdapterPort { get; set; }


        public void Start(string file)
        {
            AdapterPort = NextFreePort();
            InitializeHost();
            Client.SendRequest(new InitializeRequest() { }, e => { });
            Client.SendRequest(new AttachRequest() { _Restart = false }, e => { });
            Client.SendRequest(new ConfigurationDoneRequest() { }, e => { });
        }

        private void InitializeHost()
        {
            tcpClient = new TcpClient();
            tcpClient.Connect(IPAddress.Loopback.ToString(), AdapterPort);
            var stream = new NetworkStream(tcpClient.Client);
            Client = new DebugProtocolHost(stream, stream);
            Client.Run();
        }


      
        private void RunAdapter(string file)
        {
            python - m debugpy--listen localhost:{ AdapterPort}
            --wait -for-client ""{ file}
            ""
        }

        private void AdapterProcess_Exited(object sender, EventArgs e)
        {
            Client.Stop();
            tcpClient.Close();
            tcpClient.Dispose();
            OnDebugEnded?.Invoke(this, EventArgs.Empty);
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
