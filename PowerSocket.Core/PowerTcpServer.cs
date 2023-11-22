
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PowerSocket.Core
{
    public class PowerTcpServer
    {
        public Dictionary<string, TcpClient> dicTcpClient = new Dictionary<string, TcpClient>();
        private TcpListener? tcpListener;
        private IPEndPoint serverEP;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        #region 事件
        /// <summary>
        /// 有客户端连接时触发
        /// </summary>
        public event CbGeneric<TcpLinkEventArgs>? EventConnected;

        public event CbGeneric<TcpLinkEventArgs>? EventDisConnected;

        public event CbGeneric<TcpDataEventArgs>? EventReceived;
        #endregion
        public PowerTcpServer(int port, string locateIP = "any")
        {
            IPAddress locAddress;
            if (locateIP.ToLower() == "any")
                locAddress = IPAddress.Any;//本机IP，内网ip为192.168.0.1，外网ip为120.210.1.1，服务器可以同时监听192.168.0.1:80和120.210.1.1:80。
            else
            {
                locAddress = IPAddress.Parse(locateIP);
            }
            serverEP = new IPEndPoint(locAddress, port);

        }
        /// <summary>
        /// 启动前先绑定事件
        /// </summary>
        /// <param name="backlog"></param>
        public void Start(int backlog = -1)
        {
            tcpListener = new TcpListener(serverEP);
            if (backlog == -1)
            {
                tcpListener.Start();
            }
            else
            {
                tcpListener.Start(backlog);
            }
            CancellationToken cancellationToken = tokenSource.Token;
            Task.Run(() =>
             {
                 while (true)//listener的监听
                 {
                     //任务是否已经被取消
                     if (tokenSource.IsCancellationRequested)
                     {
                         Trace.WriteLine("监听任务取消");
                         break;
                     }

                     if (tcpListener.Pending())//stop后这里报错
                     {

                         TcpClient client = tcpListener.AcceptTcpClient();
                         var remote = client.Client.RemoteEndPoint;
                         //连接成功事件 
                         dicTcpClient.Add(remote.ToString(), client);
                         var tlEvent = new TcpLinkEventArgs(client.Client.RemoteEndPoint, client.Client.LocalEndPoint);
                         EventConnected?.Invoke(tlEvent);
                         Task.Run(() =>
                         {
                             NetworkStream stream = client.GetStream();
                             while (true)//socket接收数据   
                             {
                                 if (tokenSource.IsCancellationRequested)
                                 {
                                     Trace.WriteLine("接收数据线程取消");
                                     break;
                                 }
                                 if (!client.IsOnline())
                                 {
                                     // 掉线事件
                                     if (dicTcpClient.ContainsKey(remote.ToString()))
                                     {
                                         dicTcpClient.Remove(remote.ToString());
                                     }

                                     EventDisConnected?.Invoke(tlEvent);//接收出现异常就认为掉线
                                     //client.Client.Close();
                                     break;
                                 }

                                 if (stream.DataAvailable)
                                 {
                                     byte[] data = new byte[1024];// new byte[1024];
                                     int len = stream.Read(data, 0, 1024);
                                     if (len > 0)
                                     {
                                         var rdata = new byte[len];
                                         Buffer.BlockCopy(data, 0, rdata, 0, len);
                                         //接收数据事件 长时间无数据接收到主动关闭-不处理
                                         var tdEvent = new TcpDataEventArgs(rdata, remote, client.Client.LocalEndPoint);
                                         EventReceived?.Invoke(tdEvent);
                                     }
                                 }
                                 Thread.Sleep(1);
                             }
                         }, cancellationToken);
                     }

                     Thread.Sleep(1);
                 }
             }, cancellationToken);


        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="remoteEndPoint">ip:port</param>
        /// <param name="buffer"></param>
        /// <exception cref="Exception"></exception>
        public void Send(string remoteEndPoint, byte[] buffer)
        {
            Task.Run(() =>
            {
                if (dicTcpClient.ContainsKey(remoteEndPoint))
                {
                    dicTcpClient[remoteEndPoint].Client.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }
                else
                {
                    throw new Exception("不存在此连接！");
                }
            });
        }
        public void SendAll(byte[] buffer)
        {
            Task.Run(() =>
            {
                foreach (var item in dicTcpClient.Values)
                {
                    item.Client.Send(buffer, 0, buffer.Length, SocketFlags.None);
                }

            });


        }
        public void Close()
        {
            foreach (var item in dicTcpClient)
            {
                item.Value.Client.Close();//会中止 接收数据线程
            }

            dicTcpClient.Clear();
            tokenSource.Cancel();//中止监听线程
            tcpListener.Stop();
            //tcpListener.Server.Close();//会引发IsOnline异常
            //GC.Collect();
        }

    }
    internal static class TcpClientEx
    {
        public static bool IsOnline(this TcpClient client)
        {
            try
            {
                return !((client.Client.Poll(15000, SelectMode.SelectRead) && (client.Client.Available == 0)) || !client.Client.Connected);
            }
            catch (Exception ex)
            {
                client.Client.Close();
                return false;
            }

        }
    }
}
