using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PowerSocket.Core
{

    public class UdpStateEventArgs : EventArgs
    {
        public IPEndPoint? remoteEndPoint;
        public byte[] buffer = null;
    }

    public delegate void UDPReceivedEventHandler(UdpStateEventArgs args);

    public class PowerUdpClient
    {
        private UdpClient udpClient;
        public event UDPReceivedEventHandler? UDPMessageReceived;

        public PowerUdpClient(int locatePort, string locateIP = "any")
        {
            IPAddress locAddress;
            if (locateIP.ToLower() == "any")
                locAddress = IPAddress.Any;//本机IP，内网ip为192.168.0.1，外网ip为120.210.1.1，服务器可以同时监听192.168.0.1:80和120.210.1.1:80。
            else
            {
                locAddress = IPAddress.Parse(locateIP);
            }

            IPEndPoint locatePoint = new IPEndPoint(locAddress, locatePort);
            udpClient = new UdpClient(locatePoint);

            //监听创建好后，创建一个线程，开始接收信息
            Task.Run(() =>
            {
                while (true)
                {
                    UdpStateEventArgs udpReceiveState = new UdpStateEventArgs();

                    if (udpClient != null)
                    {
                        IPEndPoint remotePoint = new IPEndPoint(IPAddress.Parse("1.1.1.1"), 1);
                        var received = udpClient.Receive(ref remotePoint);
                        udpReceiveState.remoteEndPoint = remotePoint;
                        udpReceiveState.buffer = received;
                        UDPMessageReceived?.Invoke(udpReceiveState);
                    }
                    else
                    {
                        break;
                    }
                }
            });
        }

        public void Send(string remoteIP, int remotePort, string msg)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(msg);
            Send(remoteIP, remotePort, buffer);

        }
        public void Send(string remoteIP, int remotePort, byte[] buffer)
        {
            if (udpClient != null)
            {
                IPAddress remoteIp = IPAddress.Parse(remoteIP);
                IPEndPoint remotePoint = new IPEndPoint(remoteIp, remotePort);
                udpClient.Send(buffer, buffer.Length, remotePoint);
            }
        }
    }
}
