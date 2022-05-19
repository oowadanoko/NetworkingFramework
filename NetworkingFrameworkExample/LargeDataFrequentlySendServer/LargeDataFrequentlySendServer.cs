using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Oowada.NetworkingFramework.Server;

/// <summary>
/// 大量数据频繁发送示例服务器
/// </summary>
class LargeDataFrequentlySendServer
{
    public static void Start()
    {
        // 注册消息
        ServerNetManager.RegisterMessage<AnotherTestUsingNamespace.EchoMessage>();

        // 添加消息监听
        ServerNetManager.AddMessageListener<AnotherTestUsingNamespace.EchoMessage>((pendingMessage) =>
        {
            ServerNetManager.Log("Receive EchoMessage! " + pendingMessage.Message.ToString());
            ServerNetManager.Broadcast(pendingMessage.Message);
        });

        // 添加事件监听
        ServerNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnReceiveFail, (pendingMessage) =>
        {
            ServerNetManager.Log("Reveive FAIL");
        });

        // 驱动Update
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += (obj, arg) =>
        {
            ServerNetManager.Update();
        };
        updateTimer.Start();

        // 开启服务器
        ServerNetManager.StartServer("0.0.0.0", 12345);

        // 阻塞控制台
        Console.ReadLine();
    }
}

