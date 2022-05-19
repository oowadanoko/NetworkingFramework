using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oowada.NetworkingFramework.Client;
using System.Timers;

/// <summary>
/// 大量数据频繁发送示例客户端
/// </summary>
class LargeDataFrequentlySendClient
{
    public static void Start()
    {
        // 注册消息
        ClientNetManager.RegisterMessage<TestUsingNamespace.EchoMessage>();

        // 添加事件监听
        ClientNetManager.AddMessageListener<TestUsingNamespace.EchoMessage>((pendingMessage) =>
        {
            ClientNetManager.Log("Receive EchoMessage! " + pendingMessage.Message.ToString());
        });

        // 添加事件监听
        ClientNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnSendFail, (pendingMessage) =>
        {
            ClientNetManager.Log("Send FAIL");
        });

        // 驱动Update
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += (obj, arg) =>
        {
            ClientNetManager.Update();
        };
        updateTimer.Start();

        // 每10毫秒发送大量数据
        Timer sendTimer = new Timer(10);
        sendTimer.Elapsed += (sender, arg) =>
        {
            if (ClientNetManager.IsConnected)
            {
                // 随机生成3000-5000个字符的字符串
                Random random = new Random(DateTime.Now.Millisecond);
                string data = RandomString(random.Next(3000, 5000));

                ClientNetManager.Log("Send EchoMessage! " + data);
                ClientNetManager.Send(new TestUsingNamespace.EchoMessage(data));
            }
        };
        sendTimer.Start();

        // 连接服务器
        ClientNetManager.Connect("127.0.0.1", 12345);

        // 阻塞控制台
        Console.ReadLine();
    }

    public static string alpha = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>
    /// 随机生成指定个数字符的字符串
    /// </summary>
    /// <param name="size">字符个数</param>
    /// <returns>随机生成的字符串</returns>
    public static string RandomString(int size)
    {
        Random random = new Random(DateTime.Now.Millisecond); ;
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < size; ++i)
        {
            builder.Append(alpha[random.Next(0, alpha.Length)]);
        }
        return builder.ToString();
    }
}
