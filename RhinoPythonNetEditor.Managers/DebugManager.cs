﻿using Microsoft.VisualStudio.Shared.VSCodeDebugProtocol.Messages;
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

namespace RhinoPythonNetEditor.Managers
{
    public class DebugManager
    {
        public event EventHandler DebugEnd = delegate { };

        TcpClient TcpClient { get; set; }

        DebugProtocolHost Client { get; set; }
        public int AdapterPort { get; set; }


        public void Start()
        {
            InitializeHost();
            Client.SendRequest(new InitializeRequest() { }, e => { });
            Client.SendRequest(new AttachRequest() { _Restart = false }, e => { });
            Client.SendRequest(new ConfigurationDoneRequest() { }, e => { });
        }

        private void InitializeHost()
        {
            TcpClient = new TcpClient();
            TcpClient.Connect(IPAddress.Loopback.ToString(), AdapterPort);
            var stream = new NetworkStream(TcpClient.Client);
            Client = new DebugProtocolHost(stream, stream);
            Client.EventReceived += Client_EventReceived;
            Client.Run();
        }

        private void Client_EventReceived(object sender, EventReceivedEventArgs e)
        {
            if (e.EventType == "terminated")
            {
                End();
                DebugEnd?.Invoke(this, EventArgs.Empty);
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
