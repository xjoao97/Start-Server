#region

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Oblivion.Messages.Net;

#endregion

namespace Oblivion.Net
{
    public class MusSocket
    {
        private readonly List<string> _allowedIPs;
        private readonly int _musPort;
        private readonly Socket _musSocket;

        private string _musIP;

        public MusSocket(string MusIP, int MusPort, string[] AllowdIPs, int backlog)
        {
            _musIP = MusIP;
            _musPort = MusPort;

            _allowedIPs = new List<string>();
            foreach (var ip in AllowdIPs)
                _allowedIPs.Add(ip);

            try
            {
                _musSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _musSocket.Bind(new IPEndPoint(IPAddress.Any, _musPort));
                _musSocket.Listen(backlog);
                _musSocket.BeginAccept(OnEvent_NewConnection, _musSocket);
            }

            catch (Exception e)
            {
                throw new ArgumentException("Could not set up MUS socket:\n" + e);
            }
        }

        private void OnEvent_NewConnection(IAsyncResult iAr)
        {
            try
            {
                var socket = ((Socket) iAr.AsyncState).EndAccept(iAr);
                var ip = socket.RemoteEndPoint.ToString().Split(':')[0];
                if (_allowedIPs.Contains(ip) || ip == "127.0.0.1")
                {
                    var nC = new MusConnection(socket);
                }
                else
                {
                    socket.Close();
                }
            }
            catch (Exception)
            {
            }

            _musSocket.BeginAccept(OnEvent_NewConnection, _musSocket);
        }
    }
}