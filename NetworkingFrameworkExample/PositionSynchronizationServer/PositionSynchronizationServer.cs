using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Oowada.NetworkingFramework.Server;

/// <summary>
/// 位置同步示例服务器  
/// </summary>
class PositionSynchronizationServer
{
    public static void Start()
    {
        // 设置日志输出目录
        ServerNetManager.SetLogOutput("Log.txt");

        // 注册消息
        ServerNetManager.RegisterMessage<PositionSynchronizationMessage>();

        // 添加消息监听
        ServerNetManager.AddMessageListener(typeof(PositionSynchronizationMessage), 
            (pendingMessage) =>
            {
                ServerNetManager.Broadcast(pendingMessage.Message);
            });

        ServerNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnReceiveSuccess, (pendingMessage) =>
        {
            ServerNetManager.Log("OnReceiveSuccess " + pendingMessage.Message.ToString());
        });

        ServerNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnSendSuccess, (pendingMessage) =>
        {
            ServerNetManager.Log("OnSendSuccess " + pendingMessage.Message.ToString());
        });

        // 驱动Update
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
        {
            ServerNetManager.Update();
        };
        updateTimer.Start();

        // 开启服务器
        ServerNetManager.StartServer("0.0.0.0", 12345);

        // 阻塞线程
        Console.ReadLine();
    }
}

