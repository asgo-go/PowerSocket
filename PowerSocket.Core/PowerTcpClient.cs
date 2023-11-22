using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerSocket.Core
{
    public class PowerTcpClient
    {
        private TcpClient tcpClient;
        TcpLinkEventArgs tlEvent;
        private IPEndPoint serverEP;
        #region 事件
        /// <summary>
        /// 有客户端连接时触发
        /// </summary>
        public event CbGeneric<TcpLinkEventArgs>? EventConnected;

        public event CbGeneric<TcpLinkEventArgs>? EventDisConnected;

        public event CbGeneric<TcpDataEventArgs>? EventReceived;

        #endregion
        public PowerTcpClient(string serverIP, int port)
        {
            serverEP = new IPEndPoint(IPAddress.Parse(serverIP), port);
            tcpClient = new TcpClient();
        }

        public void Start()
        {
            tcpClient.Connect(serverEP);
            tlEvent = new TcpLinkEventArgs(tcpClient.Client.RemoteEndPoint, tcpClient.Client.LocalEndPoint);
            EventConnected?.Invoke(tlEvent);
            NetworkStream netStream = tcpClient.GetStream();

            Task.Run(() =>
            {
                Thread.Sleep(100);
                while (true)
                {
                    if (tcpClient.Client is null) break;//客户端主动关闭 

                    if (!tcpClient.Client.Connected)
                    {
                        EventDisConnected?.Invoke(tlEvent);//被动 掉线监测点1  
                        break;
                    }

                    if (netStream == null)
                    {
                        break;
                    }

                    try
                    {
                        if (netStream.DataAvailable)
                        {
                            byte[] data = new byte[1024];
                            int len = netStream.Read(data, 0, 1024);
                            if (len > 0)
                            {
                                var rdata = new byte[len];
                                Buffer.BlockCopy(data, 0, rdata, 0, len);
                                var tdEvent = new TcpDataEventArgs(rdata, tcpClient.Client.RemoteEndPoint, tcpClient.Client.LocalEndPoint);
                                EventReceived?.Invoke(tdEvent);
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }

                    Thread.Sleep(10);
                }
            });

            //掉线监测 2s一次, 考虑非阻塞 是否会影响正常的数据发送，最后收到的数据夹杂了一个空字节
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(2000);
            //        if (!IsConnect())
            //        {
            //            EventDisConnected?.Invoke(tlEvent);//被动掉线
            //            break;
            //        }
            //    }

            //});

        }
        public void Dispose()
        {
            var netStream = tcpClient.GetStream();
            netStream.Close();
            netStream = null;
            tcpClient.Close();
        }
        /// <summary>
        /// 服务端调用scoket.close能检测到
        /// </summary>
        /// <param name="data"></param>
        public void SendData(byte[] data)
        {
            Task.Run(() =>
            {
                try
                {
                    tcpClient.Client.Send(data);
                }
                catch
                {
                    EventDisConnected?.Invoke(tlEvent);//掉线监测点2
                }

                //NetworkStream netStream = tcpClient.GetStream();
                //for (int i = 0; i < 100; i++)
                //{
                //    int Len = data.Length;
                //    netStream.Write(data, 0, Len);
                //    Thread.Sleep(1000);
                //}
            });
        }

        public bool IsConnect()
        {
            var _SocketClient = tcpClient.Client;
            //非阻塞模式：无论connect是否成功都立即返回
            if (_SocketClient == null)
                return false;
            //先看看态
            if (_SocketClient.Connected == false || _SocketClient.RemoteEndPoint == null)
                return false;
            //尝试发送以非阻塞模式发送一个消 注意这个非阻塞模式不会影响异步发送bool blockingstate = client.Client.Blocking:try
            bool blockingstate = _SocketClient.Blocking;
            try
            {
                byte[] tmp = new byte[1];
                _SocketClient.Blocking = false;
                _SocketClient.Send(tmp, 1, 0);
                return true;
            }
            catch (SocketException e)
            {
                //产生 10035 == WSAEWOULDBLOCK 误，说明被阳止了，但是还是连接的 这个误是说发送缓中区已满或if (e.NativeErrorCode.Equals(10835))
                if (e.NativeErrorCode.Equals(10035))
                    return true;
                else
                    return false;
            }
            finally
            {
                _SocketClient.Blocking = blockingstate; //恢复状态
            }
        }

    }


}


