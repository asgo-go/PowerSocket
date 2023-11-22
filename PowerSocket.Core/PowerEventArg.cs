using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket.Core
{
    public class TcpLinkEventArgs : EventArgs
    {
        public TcpLinkEventArgs(EndPoint remoteEP, EndPoint localEP)
        {
            RemoteEP = (IPEndPoint)remoteEP;
            LocalEP = (IPEndPoint)localEP;

        }
        public IPEndPoint RemoteEP { get; }
        public IPEndPoint LocalEP { get; }
    }
    public class TcpDataEventArgs : EventArgs
    {
        public TcpDataEventArgs(byte[] data, EndPoint remoteEP, EndPoint localEP)
        {
            RemoteEP = (IPEndPoint)remoteEP;
            LocalEP = (IPEndPoint)localEP;
            Data = data;


        }
        public IPEndPoint RemoteEP { get; }
        public IPEndPoint LocalEP { get; }
        public byte[] Data { get; }
    }
}
