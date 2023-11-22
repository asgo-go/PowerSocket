using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PowerSocket.Core;
using PowerSocket.BusShare;
using System.Diagnostics;
using System.Net.Sockets;

namespace PowerSocket.BusServer
{
    public class TcpBusServer : IBusBase
    {
        private PowerTcpServer server;
        public List<Action<byte[], Socket>> listAction = new List<Action<byte[], Socket>>();
        #region 事件
        /// <summary>
        /// 有客户端连接时触发
        /// </summary>
        public event CbGeneric<TcpLinkEventArgs>? EventConnected;

        public event CbGeneric<TcpLinkEventArgs>? EventDisConnected;

        public event CbGeneric<TcpDataEventArgs>? EventReceived;
        //public event Action call;
        #endregion
        public TcpBusServer(int port, string locateIP = "any")
        {
            server = new PowerTcpServer(port, locateIP);
            server.EventConnected += (e => EventConnected?.Invoke(e));
            server.EventDisConnected += (e => EventDisConnected?.Invoke(e));
            server.EventReceived += (e => EventReceived?.Invoke(e));
            server.EventReceived += Server_EventReceived;  //Action 两边都要加载，自动回应
            // server.dicTcpClient
        }

        private void Server_EventReceived(TcpDataEventArgs e)
        {
            if (!server.dicTcpClient.ContainsKey(e.RemoteEP.ToString()))
            {
                return;
            }
            var curSocket = server.dicTcpClient[e.RemoteEP.ToString()].Client;
            foreach (var action in listAction)
            {
                action?.Invoke(e.Data, curSocket);
            }
 
        }
  
        public void Start(int backlog = -1)
        {
            server.Start(backlog);
        }

        public void SendAll(byte[] buffer)
        {
            server.SendAll(buffer);
        }
        public void Send(string remoteEndPoint, byte[] buffer)
        {
            server.Send(remoteEndPoint, buffer);
        }

        public void SendAll(string text)
        {
            server.SendAll(Encoding.UTF8.GetBytes(text));
        }
        public void Send(string remoteEndPoint, string text)
        {
            server.Send(remoteEndPoint, Encoding.UTF8.GetBytes(text));
        }


    }
}
