using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Oowada.NetworkingFramework.Common;

namespace Oowada.NetworkingFramework.Server
{
    /// <summary>
    /// 服务器网络控制器
    /// </summary>
    public class ServerNetManager : BaseNetManager
    {
        /// <summary>
        /// 监听套接字
        /// </summary>
        private static Socket listening;
        /// <summary>
        /// 客户端连接
        /// </summary>
        public static Dictionary<Socket, ClientConnection> Clients { get; private set; }
            = new Dictionary<Socket, ClientConnection>();
        /// <summary>
        /// 服务器是否正在运行
        /// </summary>
        public static bool IsRunning { get; private set; } = false;
        /// <summary>
        /// 关闭服务器时，检查是否所有客户端均已成功断开连接的微秒时间间隔
        /// </summary>
        public static int CheckCloseIntervalMacroSeconds { get; set; } = 1000;

        static ServerNetManager()
        {
            // 移除断开连接的客户端
            AddNetEventListener(NetEvent.OnDisconnected, RemoveClientConnection);
        }

        /// <summary>
        /// 从当前客户端中移除指定的客户端
        /// </summary>
        /// <param name="socket">要移除的客户端套接字</param>
        private static void RemoveClientConnection(Socket socket)
        {
            lock (Clients)
            {
                if (Clients.ContainsKey(socket))
                {
                    Clients.Remove(socket);
                }
            }
        }

        /// <summary>
        /// 获取指定客户端的用户数据
        /// </summary>
        /// <param name="client">指定的客户端</param>
        /// <returns>指定客户端的用户数据，未找到则返回null</returns>
        public static Hashtable GetCustomData(ClientConnection client)
        {
            return GetCustomData(client.ConnSocket);
        }

        /// <summary>
        /// 获取指定客户端套接字的用户数据
        /// </summary>
        /// <param name="clientSocket">指定客户端套接字</param>
        /// <returns>指定客户端套接字的用户数据，未找到则返回null</returns>
        public static Hashtable GetCustomData(Socket clientSocket)
        {
            Hashtable data = null;
            lock(Clients)
            {
                if (Clients.ContainsKey(clientSocket))
                {
                    data = Clients[clientSocket].CustomData;
                }
            }
            return data;
        }

        /// <summary>
        /// 开启服务器
        /// </summary>
        /// <param name="ip">要监听的IP地址</param>
        /// <param name="port">要监听的端口号</param>
        public static void StartServer(string ip, int port)
        {
            if (IsRunning)
            {
                Log("[Server] Server Already Running");
                return;
            }
            listening = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPAddress ipAddress = IPAddress.Parse(ip);
            IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
            try
            {
                listening.Bind(iPEndPoint);
                listening.Listen(0);
            }
            catch (Exception e)
            {
                Log("[Server] Start Fail Exception, " + e.ToString() + Environment.NewLine);
            }
            IsRunning = true;
            Log("[Server] Start");

            try
            {
                Log("[Server] Begin Accept");
                listening.BeginAccept(AcceptCallback, listening);
            }
            catch (Exception e)
            {
                Log("[Server] BeginAccept Fail Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetEventListener(NetEvent.OnAcceptFail, listening);
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            if (!IsRunning)
            {
                return;
            }
            Socket listening = ar.AsyncState as Socket;
            InvokeNetEventListener(NetEvent.OnAccepting, listening);
            try
            {
                Socket client = listening.EndAccept(ar);
                ClientConnection conn = new ClientConnection(client);
                conn.SetIdentifier();
                lock (Clients)
                {
                    Clients.Add(client, conn);
                }
                conn.IsClosed = false;
                InvokeNetEventListener(NetEvent.OnAcceptSuccess, client);
                Log("[Server] Accept " + conn.Identifier);
                BeginReceive(conn);
                listening.BeginAccept(AcceptCallback, listening);
            }
            catch (Exception e)
            {
                Log("[Server] AcceptCallback Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetEventListener(NetEvent.OnAcceptFail, listening);
            }
        }

        /// <summary>
        /// 立即关闭客户端连接
        /// </summary>
        /// <param name="client">要关闭的客户端</param>
        public static void CloseClientConnection(ClientConnection client)
        {
            BaseNetManager.Close(client);
        }

        /// <summary>
        /// 立即关闭服务器，会先立即关闭所有客户端连接
        /// </summary>
        public static void CloseServer()
        {
            if (!IsRunning)
            {
                Log("[Server] Not Is Running");
                return;
            }
            IsRunning = false;

            lock (Clients)
            {
                foreach (ClientConnection client in Clients.Values)  // 立即关闭所有客户端
                {
                    CloseClientConnection(client);
                }
            }
            listening.Close();
            Log("[Server] Closed");
        }

        /// <summary>
        /// 向客户端发送消息
        /// </summary>
        /// <param name="client">要发送给的客户端</param>
        /// <param name="message">要发送的消息</param>
        public new static void Send(ClientConnection client, BaseMessage message)
        {
            if (!IsRunning)
            {
                Log("[Server] Server Not Is Running, Send Fail");
                return;
            }
            BaseNetManager.Send(client, message);
        }

        /// <summary>
        /// 向所有客户端广播消息
        /// </summary>
        /// <param name="message">要广播的消息</param>
        public static void Broadcast(BaseMessage message)
        {
            if (message == null)
            {
                return;
            }
            Log("[Server] Broadcast " + message.GetType().Name);
            lock (Clients)
            {
                foreach (ClientConnection client in Clients.Values)
                {
                    Send(client, message);
                }
            }
        }
    }
}
