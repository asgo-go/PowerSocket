// See https://aka.ms/new-Trace-template for more information
using PowerSocket.BusClient;
using PowerSocket.BusServer;
using PowerSocket.Core;
using System;
using System.Diagnostics;
using System.Net;
using System.Text;
using Xunit;
using PowerSocket.BusAction;

if (Debugger.IsAttached)
    Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
Trace.WriteLine("Hello, World!");

//UdpTest();
//TcpCoreTest();
TcpBusTest();

Console.ReadLine();
#region Tcp.Bus
//功能需求 Action
//1 能发送信息，服务器转发，用户管理，多tcpserver管理
//2 能发送文件
//3 能上传下载文件
//4 离线文件
//5 大文件断点续传
//6 自定义对象传输
//7 P2P能穿墙直连 2个公网ip更好 猜测（一个ip客户端猜测）
//8 自定义广播 组播=群
//9 远程桌面 键鼠捕获 界面捕获（必须要显示器)
//10 登录验证
//11 及时等候回应，同步需求。
void TcpBusTest()
{
    int port = 5410;
    var aBusServer = new TcpBusServer(port);
    aBusServer.EventConnected += ABusServer_EventConnected;
    aBusServer.EventReceived += ABusServer_EventReceived;
    aBusServer.Start();

    var aBusClient = new TcpBusClient("127.0.0.1", port);
    aBusClient.EventConnected += ABusClient_EventConnected;
    aBusClient.EventReceived += ABusClient_EventReceived;
    aBusClient.Start();
    //1 发送信息
    aBusClient.SendData("I am BusClient A");
    aBusServer.SendAll("I am BusServer");
    //2 action简单示例。自然语言
    var action = new ReplyTime();
    aBusServer.listAction.Add(action.ReTime);
    aBusClient.SendData("I need this time");
    //3 文件发送



}

void ABusClient_EventReceived(TcpDataEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Data);
    Trace.WriteLine("客户端" + e.RemoteEP.ToString() + "接收到消息：" + message);
}

void ABusClient_EventConnected(TcpLinkEventArgs e)
{
    Trace.WriteLine("连接服务器成功：" + e.RemoteEP.ToString());
}

void ABusServer_EventReceived(TcpDataEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Data);
    Trace.WriteLine("服务端接收到" + e.RemoteEP.ToString() + "的消息：" + message);
}

void ABusServer_EventConnected(TcpLinkEventArgs e)
{
    Trace.WriteLine("收到客户端连接：" + e.RemoteEP.ToString());

}

#endregion

#region TCP.core
void TcpCoreTest()
{
    //var message = Encoding.UTF8.GetString(data, 0, len);
    var aServer = new PowerTcpServer(5409);
    aServer.EventConnected += AServer_EventConnected;
    aServer.EventReceived += AServer_EventReceived;
    aServer.EventDisConnected += AServer_EventDisConnected;
    aServer.Start();

    var aClient = new PowerTcpClient("127.0.0.1", 5409);
    aClient.EventReceived += AClient_EventReceived;
    aClient.Start();
    aClient.SendData(Encoding.UTF8.GetBytes("Hello Server!I am Client A"));

    var bClient = new PowerTcpClient("127.0.0.1", 5409);
    bClient.EventReceived += AClient_EventReceived;
    bClient.EventDisConnected += BClient_EventDisConnected;
    bClient.Start();
    bClient.SendData(Encoding.UTF8.GetBytes("Hello Server!I am Client B"));

    Thread.Sleep(500);
    aServer.SendAll(Encoding.UTF8.GetBytes("Hello Client!I am Server."));
    aServer.Send(aServer.dicTcpClient.FirstOrDefault().Key, Encoding.UTF8.GetBytes("Hello Client 1! Send to you only."));
    Thread.Sleep(500);
    //客户端主动关闭
    //bClient.tcpClient.Close();
    //测试服务端掉线后发数据
    aServer.Close();
    try
    {
        aClient.SendData(Encoding.UTF8.GetBytes("关闭服务端后发数据"));//此条记录应无法收到
        var cClient = new PowerTcpClient("127.0.0.1", 5409);
        cClient.Start();//报错 ok
    }
    catch
    {
        Trace.WriteLine("服务端掉线测试通过");
    }
    // Assert.Equal(1, 1);

}

void BClient_EventDisConnected(TcpLinkEventArgs e)
{

    Trace.WriteLine("服务端：" + e.RemoteEP.ToString() + "无法连接！");
}

void AServer_EventDisConnected(TcpLinkEventArgs e)
{
    Trace.WriteLine("客户端：" + e.RemoteEP.ToString() + "已取消连接！");
}

void AServer_EventConnected(TcpLinkEventArgs e)
{
    Trace.WriteLine("连接成功：" + e.RemoteEP.ToString());
}

void AClient_EventReceived(TcpDataEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Data);
    Trace.WriteLine("客户端" + e.RemoteEP.ToString() + "接收到消息：" + message);
}

void AServer_EventReceived(TcpDataEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.Data);
    Trace.WriteLine("服务端接收到" + e.RemoteEP.ToString() + "的消息：" + message);

}

#endregion

#region UDP
void UdpTest()
{
    var bClient = new PowerUdpClient(6490);
    var aClient = new PowerUdpClient(5490);
    aClient.UDPMessageReceived += UdpReceive;

    bClient.Send("127.0.0.1", 5490, "好好学习");

}
void UdpReceive(UdpStateEventArgs e)
{
    var message = Encoding.UTF8.GetString(e.buffer, 0, e.buffer.Length);
    Trace.WriteLine(e.remoteEndPoint.ToString());
    Trace.WriteLine(message);
    //Trace.ReadLine();
}

#endregion

