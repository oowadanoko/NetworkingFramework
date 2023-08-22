using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Diagnostics;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 网络管理器基类
    /// </summary>
    public class BaseNetManager
    {
        /// <summary>
        /// 消息处理函数
        /// </summary>
        private static readonly Dictionary<Type, MessageListener> messageListeners
            = new Dictionary<Type, MessageListener>();
        /// <summary>
        /// 网络事件处理函数
        /// </summary>
        private static readonly Dictionary<NetEvent, NetEventListener> netEventListeners
            = new Dictionary<NetEvent, NetEventListener>();
        /// <summary>
        /// 网络消息事件处理函数
        /// </summary>
        private static readonly Dictionary<NetMessageEvent, NetMessageEventListener> 
            netMessageEventListeners = new Dictionary<NetMessageEvent, NetMessageEventListener>();
        /// <summary>
        /// 消息队列
        /// </summary>
        private static readonly Queue<PendingMessage> messageQueue = new Queue<PendingMessage>();
        /// <summary>
        /// 一次处理消息的最大数量
        /// </summary>
        public static int MaxHandleMessageCount { get; set; } = 10;
        /// <summary>
        /// 在Update之前调用的回调函数
        /// </summary>
        private static readonly List<Action> beforeUpdateCallbacks = new List<Action>();
        /// <summary>
        /// Update中调用的回调函数
        /// </summary>
        private static readonly List<Action> updateCallbacks = new List<Action>();
        /// <summary>
        /// 在Update之后调用的回调函数
        /// </summary>
        private static readonly List<Action> afterUpdateCallbacks = new List<Action>();
        /// <summary>
        /// 当前时间
        /// </summary>
        private static string CurTime { get { return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff"); } }
        /// <summary>
        /// 标准输出
        /// </summary>
        private static readonly TextWriter stdOut = Console.Out;
        /// <summary>
        /// 当前输出流
        /// </summary>
        private static TextWriter curOut = Console.Out;

        /// <summary>
        /// 添加消息监听，建议使用泛型方法代替
        /// </summary>
        /// <param name="messageType">要监听的消息类型</param>
        /// <param name="onMessage">监听器</param>
        public static void AddMessageListener(Type messageType, MessageListener onMessage)
        {
            lock(messageListeners)
            {
                if (messageListeners.ContainsKey(messageType))
                {
                    messageListeners[messageType] += onMessage;
                }
                else
                {
                    messageListeners[messageType] = onMessage;
                }
            }
        }

        /// <summary>
        /// 移除消息监听，建议使用泛型方法代替
        /// </summary>
        /// <param name="messageType">要移除监听器的消息类型</param>
        /// <param name="onMessage">监听器</param>
        public static void RemoveMessageListener(Type messageType, MessageListener onMessage)
        {
            lock(messageListeners)
            {
                if (messageListeners.ContainsKey(messageType))
                {
                    messageListeners[messageType] -= onMessage;
                    if (messageListeners[messageType] == null)  // 防止调用null导致错误
                    {
                        messageListeners.Remove(messageType);
                    }
                }
            }
        }

        /// <summary>
        /// 执行消息监听器，建议使用泛型方法代替
        /// </summary>
        /// <param name="messageType">消息类型</param>
        /// <param name="pendingMessage">待处理的消息</param>
        protected static void InvokeMessageListener(Type messageType, PendingMessage pendingMessage)
        {
            MessageListener listener = null;
            lock(messageListeners)
            {
                if (messageListeners.ContainsKey(messageType))
                {
                    listener = messageListeners[messageType];
                }
            }
            if (listener != null)
            {
                listener(pendingMessage);
            }
        }

        /// <summary>
        /// 添加消息监听
        /// </summary>
        /// <typeparam name="T">要监听的消息类型，必须从BaseMessage派生</typeparam>
        /// <param name="onMessage">监听器</param>
        public static void AddMessageListener<T>(MessageListener onMessage) where T : BaseMessage
        {
            AddMessageListener(typeof(T), onMessage);
        }

        /// <summary>
        /// 移除消息监听
        /// </summary>
        /// <typeparam name="T">要移除监听器的消息类型，必须从BaseMessage派生</typeparam>
        /// <param name="onMessage">监听器</param>
        public static void RemoveMessageListener<T>(MessageListener onMessage) where T : BaseMessage
        {
            RemoveMessageListener(typeof(T), onMessage);
        }

        /// <summary>
        /// 执行消息监听器
        /// </summary>
        /// <typeparam name="T">消息类型，必须从BaseMessage派生</typeparam>
        /// <param name="pendingMessage">待处理的消息</param>
        protected static void InvokeMessageListener<T>(PendingMessage pendingMessage) where T : BaseMessage
        {
            InvokeMessageListener(typeof(T), pendingMessage);
        }

        /// <summary>
        /// 添加网络事件监听
        /// </summary>
        /// <param name="e">要监听的网络事件</param>
        /// <param name="onEvent">监听器</param>
        public static void AddNetEventListener(NetEvent e, NetEventListener onEvent)
        {
            lock(netEventListeners)
            {
                if (netEventListeners.ContainsKey(e))
                {
                    netEventListeners[e] += onEvent;
                }
                else
                {
                    netEventListeners[e] = onEvent;
                }
            }
        }

        /// <summary>
        /// 移除网络事件监听
        /// </summary>
        /// <param name="e">要移除监听器的网络事件</param>
        /// <param name="onEvent">监听器</param>
        public static void RemoveNetEventListener(NetEvent e, NetEventListener onEvent)
        {
            lock(netEventListeners)
            {
                if (netEventListeners.ContainsKey(e))
                {
                    netEventListeners[e] -= onEvent;
                    if (netEventListeners[e] == null)  // 防止调用null导致错误
                    {
                        netEventListeners.Remove(e);
                    }
                }
            }
        }

        /// <summary>
        /// 执行网络事件监听器
        /// </summary>
        /// <param name="e">要执行监听器的网络事件</param>
        /// <param name="socket">引发网络事件的Socket对象</param>
        protected static void InvokeNetEventListener(NetEvent e, Socket socket)
        {
            NetEventListener listener = null;
            lock(netEventListeners)
            {
                if (netEventListeners.ContainsKey(e))
                {
                    listener = netEventListeners[e];
                }
            }
            if (listener != null)
            {
                listener(socket);
            }
        }

        /// <summary>
        /// 添加网络消息事件监听
        /// </summary>
        /// <param name="e">要监听的网络消息事件</param>
        /// <param name="onEvent">监听器</param>
        public static void AddNetMessageEventListener(NetMessageEvent e, NetMessageEventListener onEvent)
        {
            lock(netMessageEventListeners)
            {
                if (netMessageEventListeners.ContainsKey(e))
                {
                    netMessageEventListeners[e] += onEvent;
                }
                else
                {
                    netMessageEventListeners[e] = onEvent;
                }
            }
        }

        /// <summary>
        /// 移除网络消息事件监听
        /// </summary>
        /// <param name="e">要移除监听器的网络消息事件</param>
        /// <param name="onEvent">监听器</param>
        public static void RemoveNetMessageEventListener(NetMessageEvent e, NetMessageEventListener onEvent)
        {
            lock(netMessageEventListeners)
            {
                if (netMessageEventListeners.ContainsKey(e))
                {
                    netMessageEventListeners[e] -= onEvent;
                    if (netMessageEventListeners[e] == null)  // 防止调用null导致错误
                    {
                        netMessageEventListeners.Remove(e);
                    }
                }
            }
        }

        /// <summary>
        /// 执行网络消息事件监听器
        /// </summary>
        /// <param name="e">要执行监听器的网络消息事件</param>
        /// <param name="pendingMessage">引发网络消息事件的待处理消息</param>
        protected static void InvokeNetMessageEventListener(NetMessageEvent e, PendingMessage pendingMessage)
        {
            NetMessageEventListener listener = null;
            lock(netMessageEventListeners)
            {
                if (netMessageEventListeners.ContainsKey(e))
                {
                    listener = netMessageEventListeners[e];
                }
            }
            if (listener != null)
            {
                listener(pendingMessage);
            }
        }

        /// <summary>
        /// 添加要在Update之前执行的回调方法
        /// </summary>
        /// <param name="callback">要在Update之前执行的回调方法</param>
        public static void AddBeforeUpdateCallback(Action callback)
        {
            lock (beforeUpdateCallbacks)
            {
                beforeUpdateCallbacks.Add(callback);
            }
        }

        /// <summary>
        /// 移除要在Update之前执行的回调方法
        /// </summary>
        /// <param name="callback">要移除的在Update之前执行的回调方法</param>
        public static void RemoveBeforeUpdateCallback(Action callback)
        {
            lock (beforeUpdateCallbacks)
            {
                beforeUpdateCallbacks.Remove(callback);
            }
        }

        /// <summary>
        /// 添加要在Update中执行的回调方法
        /// </summary>
        /// <param name="callback">要在Update中执行的回调方法</param>
        public static void AddUpdateCallback(Action callback)
        {
            lock(updateCallbacks)
            {
                updateCallbacks.Add(callback);
            }
        }

        /// <summary>
        /// 移除要在Update中执行的回调方法
        /// </summary>
        /// <param name="callback">要移除的在Update中执行的回调方法</param>
        public static void RemoveUpdateCallback(Action callback)
        {
            lock(updateCallbacks)
            {
                updateCallbacks.Remove(callback);
            }
        }

        /// <summary>
        /// 添加要在Update之后执行的回调方法
        /// </summary>
        /// <param name="callback">要在Update之后执行的回调方法</param>
        public static void AddAfterUpdateCallback(Action callback)
        {
            lock (afterUpdateCallbacks)
            {
                afterUpdateCallbacks.Add(callback);
            }
        }

        /// <summary>
        /// 移除要在Update之后执行的回调方法
        /// </summary>
        /// <param name="callback">要移除的在Update之后执行的回调方法</param>
        public static void RemoveAfterUpdateCallback(Action callback)
        {
            lock (afterUpdateCallbacks)
            {
                afterUpdateCallbacks.Remove(callback);
            }
        }

        /// <summary>
        /// 处理消息队列中的第一条消息
        /// </summary>
        /// <returns>是否处理了消息，若未处理任何消息则返回false</returns>
        private static bool HandleMessage()
        {
            PendingMessage pendingMessage = null;
            lock (messageQueue)
            {
                if (messageQueue.Count > 0)
                {
                    pendingMessage = messageQueue.Dequeue();
                }
            }
            if (pendingMessage != null)
            {
                InvokeMessageListener(pendingMessage.Message.GetType(), pendingMessage);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 处理消息队列中的多条消息
        /// </summary>
        private static void HandleMessages()
        {
            int count = MaxHandleMessageCount;
            bool hasMessage = true;
            //如果无消息则直接退出，减少内部加锁次数
            while (hasMessage && count-- > 0)
            {
                hasMessage = HandleMessage();
            }
        }

        /// <summary>
        /// 在Update之前执行
        /// </summary>
        private static void BeforeUpdate()
        {
            lock(beforeUpdateCallbacks)
            {
                foreach (Action callback in beforeUpdateCallbacks)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// <para>此方法需要放在一段能够循环执行的代码中，此方法将会不断处理消息队列里的消息，可以通过设置MaxHandleMessageCount来改变一次处理的最大消息数</para>
        /// <para>执行顺序为BeforeUpdate -> HandleMessages -> Update -> AfterUpdate</para>
        /// <para>可以通过AddBeforeUpdateCallback和RemoveBeforeUpdateCallback自行添加和移除要在此方法之前BeforeUpdate重复调用执行的方法</para>
        /// <para>可以通过AddUpdateCallback和RemoveUpdateCallback自行添加和移除要在此方法Update中重复调用执行的方法</para>
        /// <para>可以通过AddAfterUpdateCallback和RemoveAfterUpdateCallback自行添加和移除要在此方法之后AfterUpdate重复调用执行的方法</para>
        /// </summary>
        public static void Update()
        {
            BeforeUpdate();
            HandleMessages();
            lock(updateCallbacks)
            {
                foreach (Action callback in updateCallbacks)
                {
                    callback();
                }
            }
            AfterUpdate();
        }

        /// <summary>
        /// 在Update之后执行
        /// </summary>
        private static void AfterUpdate()
        {
            lock(afterUpdateCallbacks)
            {
                foreach (Action callback in afterUpdateCallbacks)
                {
                    callback();
                }
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="conn">要发送给的客户端连接</param>
        /// <param name="message">要发送的消息</param>
        protected static void Send(ClientConnection conn, BaseMessage message)
        {
            if (!conn.CanSendOrReceive)
            {
                Log("Can Not Send Message While Unconnected");
                return;
            }
            ByteBuffer writeBuff = new ByteBuffer(message.GetMessageByteLength());
            message.WriteToByteBuffer(writeBuff);
            int count = 0;
            lock (conn.WriteQueue)
            {
                conn.WriteQueue.Enqueue(writeBuff);
                count = conn.WriteQueue.Count;
            }
            if (count == 1)  // count为1说明当前只有新增的这一条数据，即当前没有在发送数据，可以开始发送数据
            {
                PendingMessage pendingMessage = new PendingMessage(conn, message);
                try
                {
                    InvokeNetMessageEventListener(NetMessageEvent.OnSending, pendingMessage);
                    conn.ConnSocket.BeginSend(
                        writeBuff.Buff,
                        writeBuff.ReadIndex,
                        writeBuff.DataSize,
                        SocketFlags.None,
                        SendCallback,
                        pendingMessage);
                    Log("Send " + message.GetType().Name + " To " + conn.Identifier);
                }
                catch (Exception e)
                {
                    Log("Send Exception, " + e.ToString() + Environment.NewLine);
                    InvokeNetMessageEventListener(NetMessageEvent.OnSendFail, pendingMessage);
                    Close(conn);
                }
            }
        }

        private static void SendCallback(IAsyncResult ar)
        {
            PendingMessage pendingMessage = ar.AsyncState as PendingMessage;
            ClientConnection conn = pendingMessage.Client;
            BaseMessage message = pendingMessage.Message;
            if (!conn.CanSendRest)
            {
                return;
            }
            try
            {
                int size = conn.ConnSocket.EndSend(ar);
                InvokeNetMessageEventListener(NetMessageEvent.OnSendSuccess, pendingMessage);
                ByteBuffer writeBuff = null;
                lock (conn.WriteQueue)
                {
                    writeBuff = conn.WriteQueue.Peek();
                }
                writeBuff.MoveReadIndex(size);  // 发送数据后，手动调整读索引
                if (writeBuff.DataSize <= 0)  // 缓冲区所有数据发送完了
                {
                    writeBuff = null;
                    lock (conn.WriteQueue)  // 获取下一条数据
                    {
                        conn.WriteQueue.Dequeue();
                        if (conn.WriteQueue.Count > 0)
                        {
                            writeBuff = conn.WriteQueue.Peek();
                        }
                    }
                }
                if (writeBuff != null)  // 发送队列还有待发送的数据则继续发送
                {
                    InvokeNetMessageEventListener(NetMessageEvent.OnSending, pendingMessage);
                    conn.ConnSocket.BeginSend(
                        writeBuff.Buff,
                        writeBuff.ReadIndex,
                        writeBuff.DataSize,
                        SocketFlags.None,
                        SendCallback,
                        pendingMessage);
                }
            }
            catch (Exception e)
            {
                Log("SendCallback Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetMessageEventListener(NetMessageEvent.OnSendFail, pendingMessage);
                Close(conn);
            }
        }

        /// <summary>
        /// 开始接收客户端数据
        /// </summary>
        /// <param name="conn">客户端</param>
        protected static void BeginReceive(ClientConnection conn)
        {
            PendingMessage pendingMessage = new PendingMessage(conn, null);
            try
            {
                InvokeNetMessageEventListener(NetMessageEvent.OnReceiving, pendingMessage);
                conn.ConnSocket.BeginReceive(
                    conn.ReadBuff.Buff,
                    conn.ReadBuff.WriteIndex,
                    conn.ReadBuff.RemainCapacity,
                    SocketFlags.None,
                    ReceiveCallback,
                    pendingMessage);
            }
            catch (Exception e)
            {
                Log("BeginReceive Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetMessageEventListener(NetMessageEvent.OnReceiveFail, null);
                Close(conn);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            PendingMessage pendingMessage = ar.AsyncState as PendingMessage;
            ClientConnection conn = pendingMessage.Client;
            if (!conn.CanSendOrReceive)
            {
                return;
            }
            try
            {
                int size = conn.ConnSocket.EndReceive(ar);
                if (size > 0)
                {
                    conn.ReadBuff.MoveWriteIndex(size);  // 读取了size字节，需要手动移动写索引
                    conn.ReadBuff.AutoCheckCapacity();  // 检查容量，自动判定是否需要移动或扩容
                    InvokeNetMessageEventListener(NetMessageEvent.OnReceiving, pendingMessage);
                    ReceiveMessage(pendingMessage);
                    conn.ConnSocket.BeginReceive(
                    conn.ReadBuff.Buff,
                    conn.ReadBuff.WriteIndex,
                    conn.ReadBuff.RemainCapacity,
                    SocketFlags.None,
                    ReceiveCallback,
                    pendingMessage);
                }
                else  //消息长度为0，代表服务器断开连接
                {
                    Log("Receive fail, disconnected");
                    InvokeNetMessageEventListener(NetMessageEvent.OnReceiveFail, null);
                    Close(conn);
                }
            }
            catch (Exception e)
            {
                Log("ReceiveCallback Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetMessageEventListener(NetMessageEvent.OnReceiveFail, null);
                Close(conn);
            }
        }

        /// <summary>
        /// 接收消息
        /// </summary>
        /// <param name="pendingMessage">待处理消息</param>
        protected static void ReceiveMessage(PendingMessage pendingMessage)
        {
            ClientConnection client = pendingMessage.Client;
            BaseMessage message = BaseMessage.ReadFromByteBuff(client.ReadBuff);
            while (message != null)
            {
                // 用客户端套接字新建一个待处理消息，防止新消息覆盖原来的消息
                PendingMessage newPendingMessage = new PendingMessage(client, message);
                lock (messageQueue)
                {
                    messageQueue.Enqueue(newPendingMessage);
                }
                Log("Receive " + newPendingMessage.Message.GetType().Name + " From " + client.Identifier);
                InvokeNetMessageEventListener(NetMessageEvent.OnReceiveSuccess, newPendingMessage);
                message = BaseMessage.ReadFromByteBuff(client.ReadBuff);  // 尝试继续读取消息
            }
        }

        /// <summary>
        /// 立即关闭连接
        /// </summary>
        /// <param name="client">要关闭的客户端连接</param>
        protected static void Close(ClientConnection client)
        {
            if (!client.CanClose)
            {
                return;
            }
            string identifier = client.Identifier;
            InvokeNetEventListener(NetEvent.OnDisconnecting, client.ConnSocket);
            Log(identifier + " Disconnecting");
            client.Close();
            client.IsClosed = true;
            InvokeNetEventListener(NetEvent.OnDisconnected, client.ConnSocket);
            Log(identifier + " Disconnected");
        }

        /// <summary>
        /// 设置日志文件的输出位置
        /// </summary>
        /// <param name="logFile">日志文件的输出位置</param>
        public static void SetLogOutput(string logFile)
        {
            FileStream fs = new FileStream(logFile, FileMode.Append);
            curOut = new StreamWriter(fs);
            Console.SetOut(curOut);
        }

        /// <summary>
        /// 重置日志文件的输出位置到标准输出
        /// </summary>
        public static void ResetLogOutput()
        {
            curOut = stdOut;
            Console.SetOut(curOut);
        }

        /// <summary>
        /// 输出日志，可以通过SetLogOutput和ResetLogOutput设置和重置日志的输出位置
        /// </summary>
        /// <param name="log">日志内容</param>
        public static void Log(string log)
        {
            Console.WriteLine("[" + CurTime + "] " + log + " [In " + CurrentSourceCodeInfo() + "]");
            curOut.Flush();
        }

        /// <summary>
        /// 获取当前代码信息，包括文件名，方法名，行号
        /// </summary>
        /// <returns>当前代码信息字符串</returns>
        private static string CurrentSourceCodeInfo()
        {
            StackTrace st = new StackTrace(new StackFrame(2, true));
            StackFrame sf = st.GetFrame(0);
            string fileName = sf.GetFileName()?.Split(Path.DirectorySeparatorChar)?.Last();
            return "File: " + fileName + ", Method: " + sf.GetMethod().Name + ", Line: " + sf.GetFileLineNumber();
        }

        /// <summary>
        /// 注册当前消息类
        /// </summary>
        /// <typeparam name="T">要注册的消息类，该类必须从BaseMessage派生</typeparam>
        public static void RegisterMessage<T>() where T : BaseMessage
        {
            BaseMessage.RegisterMessage<T>();
        }

        /// <summary>
        /// 注销当前消息类
        /// </summary>
        /// <typeparam name="T">要注销的消息类，该类必须从BaseMessage派生</typeparam>
        public static void UnregisterMessage<T>() where T : BaseMessage
        {
            BaseMessage.UnregisterMessage<T>();
        }
    }
}
