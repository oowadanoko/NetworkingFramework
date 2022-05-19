using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using Oowada.NetworkingFramework.Client;

/// <summary>
/// 位置同步示例客户端
/// </summary>
class PositionSynchronizationClientXiaoHong
{
    public static void Start()
    {
        // 假设两个玩家
        Player me = new Player();
        me.Name = "XiaoHong";
        Player you = new Player();
        you.Name = "XiaoMing";

        // 设置日志输出目录
        ClientNetManager.SetLogOutput("Log.txt");

        // 注册位置同步消息
        ClientNetManager.RegisterMessage<PositionSynchronizationMessage>();

        // 添加消息监听器
        ClientNetManager.AddMessageListener(typeof(PositionSynchronizationMessage),
            (pendingMessage) =>
            {
                PositionSynchronizationMessage message =
                    pendingMessage.Message as PositionSynchronizationMessage;
                // 同步其他玩家的位置
                if (message.Name != me.Name)
                {
                    you.X = message.X;
                    you.Z = message.Z;
                    ClientNetManager.Log(message.Name + " Has Moved To " + you.ToString());
                }
            });

        ClientNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnReceiveSuccess, (pendingMessage) =>
        {
            ClientNetManager.Log("OnReceiveSuccess " + pendingMessage.Message.ToString());
        });

        ClientNetManager.AddNetMessageEventListener(Oowada.NetworkingFramework.Common.NetMessageEvent.OnSendSuccess, (pendingMessage) =>
        {
            ClientNetManager.Log("OnSendSuccess " + pendingMessage.Message.ToString());
        });

        // 驱动Update
        Timer updateTimer = new Timer(10);
        updateTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
        {
            ClientNetManager.Update();
        };
        updateTimer.Start();

        // 每5秒同步一次自己的消息给服务器
        Timer syncTimer = new Timer(5000);
        syncTimer.Elapsed += (object sender, ElapsedEventArgs e) =>
        {
            ClientNetManager.Send(new PositionSynchronizationMessage(me.Name, me.X, me.Z));
        };
        syncTimer.Start();

        // 开始连接
        ClientNetManager.Connect("127.0.0.1", 12345);

        // wasd移动，q退出
        while (true)
        {
            switch (Console.Read())
            {
                case 'W':
                case 'w':
                    me.MoveForwad();
                    break;
                case 'S':
                case 's':
                    me.MoveBack();
                    break;
                case 'A':
                case 'a':
                    me.MoveLeft();
                    break;
                case 'D':
                case 'd':
                    me.MoveRight();
                    break;
                case 'Q':
                case 'q':
                    ClientNetManager.Close();
                    return;
            }
        }
    }
}

