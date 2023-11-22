using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using PowerSocket.BusShare;
using PowerSocket.Core;

namespace PowerSocket.BusClient
{
    public class TcpBusClient : IBusBase
    {
        private PowerTcpClient client;
        public List<Action<byte[]>> listAction = new List<Action<byte[]>>();
        #region 事件
        /// <summary>
        /// 有客户端连接时触发
        /// </summary>
        public event CbGeneric<TcpLinkEventArgs>? EventConnected;

        public event CbGeneric<TcpLinkEventArgs>? EventDisConnected;

        public event CbGeneric<TcpDataEventArgs>? EventReceived;
        #endregion
        public TcpBusClient(string IPAddress, int port)
        {
            client = new PowerTcpClient(IPAddress, port);
            client.EventConnected += (e => EventConnected?.Invoke(e)); ;
            client.EventDisConnected += (e => EventDisConnected?.Invoke(e)); ;
            client.EventReceived += (e => EventReceived?.Invoke(e)); ;
            client.EventReceived += Client_EventReceived;
        }

        private void Client_EventReceived(TcpDataEventArgs e)
        {
            foreach (var action in listAction)
            {
                action?.Invoke(e.Data);
            }
        }

        public void Start()
        {
            client.Start();//start后再加事件 绑定无效
        }

        public void SendData(byte[] data)
        {
            client.SendData(data);
        }

        public void SendData(string text)
        {
            SendData(Encoding.UTF8.GetBytes(text));
        }
    

    }

}
