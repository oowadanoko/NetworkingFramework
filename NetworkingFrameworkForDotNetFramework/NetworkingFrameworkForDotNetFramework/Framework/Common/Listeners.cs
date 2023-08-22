using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 消息监听器
    /// </summary>
    /// <param name="pendingMessage">待处理的消息</param>
    public delegate void MessageListener(PendingMessage pendingMessage);
    /// <summary>
    /// <para>网络事件监听器</para>
    /// <para>对于不同的事件传入的参数会有所不同</para>
    /// <para>NetEvent.OnConnecting - 连接套接字，此时未连接（只有客户端会调用）</para>
    /// <para>NetEvent.OnConnectSuccess - 连接套接字（只有客户端会调用）</para>
    /// <para>NetEvent.OnConnectFail - 连接套接字，此时未连接（只有客户端会调用）</para>
    /// <para>NetEvent.OnDisconnecting - 客户端连接套接字，只有主动关闭方才会调用</para>
    /// <para>NetEvent.OnDisconnected - 客户端连接套接字，此时未连接</para>
    /// <para>NetEvent.OnAccepting - 监听套接字（只有服务器会调用）</para>
    /// <para>NetEvent.OnAcceptSuccess - 成功接收到的客户端连接套接字（只有服务器会调用）</para>
    /// <para>NetEvent.OnAcceptFail - 监听套接字（只有服务器会调用）</para>
    /// </summary>
    /// <param name="socket">发生网络事件的Socket对象</param>
    public delegate void NetEventListener(Socket socket);
    /// <summary>
    /// <para>网络消息事件监听器</para>
    /// <para>对于不同的事件传入的参数的Message值会有所不同，Client值均为收发消息的客户端连接</para>
    /// <para>NetMessageEvent.OnSending - 将要发送的消息</para>
    /// <para>NetMessageEvent.OnSendSuccess - 成功发送的消息</para>
    /// <para>NetMessageEvent.OnSendFail - 发送失败的消息</para>
    /// <para>NetMessageEvent.OnReceiving - null</para>
    /// <para>NetMessageEvent.OnReceiveSuccess - 接收到的消息</para>
    /// <para>NetMessageEvent.OnReceiveFail - null</para>
    /// </summary>
    /// <param name="pendingMessage">引发网络消息事件的待处理消息</param>
    public delegate void NetMessageEventListener(PendingMessage pendingMessage);
}
