using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Oowada.NetworkingFramework.Client;
using Oowada.NetworkingFramework.Server;
using Oowada.NetworkingFramework.Common;

/// <summary>
/// 心跳机制示例客户端
/// </summary>
class HeartbeatClient
{
    public static void Start()
    {
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += Update;  // 驱动Update，每10毫秒执行一次Update

        Timer pingTimer = new Timer(3000);
        pingTimer.Elapsed += Ping;  // 每3秒发送一个Ping消息

        // 添加用户自定义数据，上一次收到Pong消息的时间
        ClientNetManager.CustomData.Add("lastPongTime", DateTime.Now);

        // Pong消息的监听，每当收到服务器发来的Pong消息则跟新上次收到Pong消息的时间
        ClientNetManager.AddMessageListener(typeof(PongMessage), (PendingMessage m) =>
        {
            ClientNetManager.CustomData["lastPongTime"] = DateTime.Now;
        });

        // 连接失败的监听，连接失败时移除检查Ping消息的回调函数
        ClientNetManager.AddNetEventListener(NetEvent.OnConnectFail, (socket) =>
        {
            ClientNetManager.Log("连接失败");
            ClientNetManager.RemoveUpdateCallback(CheckPingPong);
        });

        // 当断开连接时，尝试重新连接
        ClientNetManager.AddNetEventListener(NetEvent.OnDisconnected, (socket) =>
        {
            ClientNetManager.Log("已断开连接");
            ClientNetManager.Log("尝试重新连接");
            ClientNetManager.CustomData["lastPongTime"] = DateTime.Now;
            ClientNetManager.Connect("127.0.0.1", 12345);
        });

        // 添加Update回调函数，循环检查是否超时
        ClientNetManager.AddUpdateCallback(CheckPingPong);

        // 注册消息，服务器与客户端都需要注册相同的消息才能保证能够正常通信
        ClientNetManager.RegisterMessage<PingMessage>();
        ClientNetManager.RegisterMessage<PongMessage>();

        // 开始连接
        ClientNetManager.Connect("127.0.0.1", 12345);

        updateTimer.Start();  // 启动Update驱动
        pingTimer.Start();  // 开始定时发送Ping消息

        Console.ReadLine();  // 阻塞控制台，当按下回车键模拟主动关闭连接
        ClientNetManager.Close();  // 主动关闭连接
        Console.WriteLine("你关闭了连接");
        Console.ReadLine();  // 阻塞控制台，由于前面注册了断开连接时的回调，所以此时可以看到尝试重新连接
    }

    private static void CheckPingPong()
    {
        DateTime last = (DateTime)ClientNetManager.CustomData["lastPongTime"];
        // 超过10秒没收到Pong消息认为已经与服务器失去联系，就断开连接，释放资源
        if (DateTime.Now - last > TimeSpan.FromSeconds(10))
        {
            ClientNetManager.Close();
            ClientNetManager.Log("断开连接");
        }
    }

    private static void Ping(object sender, ElapsedEventArgs e)
    {
        ClientNetManager.Send(new PingMessage());  // 发送Ping消息
    }

    private static void Update(object sender, ElapsedEventArgs e)
    {
        ClientNetManager.Update();  // 驱动Update
    }
}

