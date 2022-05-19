using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oowada.NetworkingFramework.Server;
using System.Timers;
using Oowada.NetworkingFramework.Common;

/// <summary>
/// 心跳机制示例服务器
/// </summary>
class HeartbeatServer
{
    public static void Start()
    {
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += Update;  // 驱动Update，每10毫秒执行一次Update

        // 添加Ping消息监听，每当收到客户端发来的Ping消息就更新上次收到Ping消息的时间，并回复Pong消息
        ServerNetManager.AddMessageListener(typeof(PingMessage), (PendingMessage m) =>
        {
            ServerNetManager.GetCustomData(m.Client)["lastPingTime"] = DateTime.Now;
            ServerNetManager.Send(m.Client, new PongMessage());
        });

        // 当一个客户端连接成功，添加上次收到Ping消息的时间
        ServerNetManager.AddNetEventListener(NetEvent.OnAcceptSuccess, (socket) =>
        {
            ServerNetManager.GetCustomData(socket).Add("lastPingTime", DateTime.Now);
        });

        // 添加检查是否超时的回调函数
        ServerNetManager.AddUpdateCallback(CheckPingPong);

        // 注册消息，服务器与客户端都需要注册相同的消息才能保证能够正常通信
        ServerNetManager.RegisterMessage<PingMessage>();
        ServerNetManager.RegisterMessage<PongMessage>();

        ServerNetManager.StartServer("0.0.0.0", 12345);  // 开启服务器
        updateTimer.Start();  // 开启Update

        Console.ReadLine();  // 阻塞控制台
        // 模拟主动关闭连接，这里选择关闭第一个客户端连接
        try
        {
            ServerNetManager.CloseClientConnection(ServerNetManager.Clients.First().Value);
            Console.WriteLine("你关闭了客户端连接");
        }
        catch
        {
            ServerNetManager.Log("当前没有正在连接的客户端");
        }
        Console.ReadLine();  // 阻塞控制台，如果客户端开启了断线重连，可以看到客户端又重新连接
        ServerNetManager.CloseServer();  // 关闭服务器
        Console.WriteLine("你关闭了服务器");
        Console.ReadLine();  // 阻塞控制台，此时的所有客户端都会断开连接
    }

    private static void CheckPingPong()
    {
        // 检查所有的客户端是否超时
        foreach (ClientConnection client in ServerNetManager.Clients.Values)
        {
            if (client.CustomData.ContainsKey("lastPingTime"))
            {
                DateTime last = (DateTime)client.CustomData["lastPingTime"];
                // 超过10秒没收到Ping消息认为客户端已经失去联系，就断开连接，释放资源
                if (DateTime.Now - last > TimeSpan.FromSeconds(10))
                {
                    ServerNetManager.CloseClientConnection(client);
                    ServerNetManager.Log("断开连接");
                }
            }
        }
    }

    private static void Update(object sender, ElapsedEventArgs e)
    {
        ServerNetManager.Update();  // 驱动Update
    }
}

