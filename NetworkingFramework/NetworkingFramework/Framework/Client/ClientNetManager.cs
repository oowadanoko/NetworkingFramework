using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Oowada.NetworkingFramework.Common;
using System.Collections;
using System.Timers;

namespace Oowada.NetworkingFramework.Client
{
    /// <summary>
    /// 客户端网络管理器
    /// </summary>
    public class ClientNetManager : BaseNetManager
    {
        /// <summary>
        /// 本地客户端连接
        /// </summary>
        private static readonly ClientConnection conn = new ClientConnection();
        /// <summary>
        /// 客户端的用户数据
        /// </summary>
        public static Hashtable CustomData { get { return conn.CustomData; } }
        /// <summary>
        /// 是否正在连接
        /// </summary>
        public static bool IsConnecting { get { return conn.IsConnecting; } }
        /// <summary>
        /// 是否已经连接
        /// </summary>
        public static bool IsConnected { get { return conn.ConnSocket != null && conn.ConnSocket.Connected; } }
        /// <summary>
        /// 是否正在关闭连接
        /// </summary>
        public static bool IsClosing { get { return conn.IsClosing; } }
        /// <summary>
        /// 是否已关闭连接
        /// </summary>
        public bool IsClosed { get { return conn.IsClosed; } }

        /// <summary>
        /// 发起连接
        /// </summary>
        /// <param name="ip">要连接的IP地址</param>
        /// <param name="port">端口号</param>
        public static void Connect(string ip, int port)
        {
            if(!conn.CanConnect)
            {
                return;
            }
            try
            {
                conn.ConnSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress ipAddress = IPAddress.Parse(ip);
                IPEndPoint iPEndPoint = new IPEndPoint(ipAddress, port);
                conn.IsConnecting = true;
                conn.ConnSocket.NoDelay = true;  // 不使用Nagle算法
                InvokeNetEventListener(NetEvent.OnConnecting, conn.ConnSocket);
                conn.ConnSocket.BeginConnect(iPEndPoint, ConnectCallback, conn);
            }
            catch (Exception e)
            {
                Log("[Client] Connect Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetEventListener(NetEvent.OnConnectFail, conn.ConnSocket);
            }
        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            ClientConnection conn = ar.AsyncState as ClientConnection;
            try
            {
                conn.ConnSocket.EndConnect(ar);
                conn.SetIdentifier();
                Log("[Client] Connect To " + conn.Identifier);
                conn.IsConnecting = false;
                conn.IsClosed = false;
                InvokeNetEventListener(NetEvent.OnConnectSuccess, conn.ConnSocket);
                BeginReceive(conn);
            }
            catch (Exception e)
            {
                Log("[Client] ConnectCallback Exception, " + e.ToString() + Environment.NewLine);
                InvokeNetEventListener(NetEvent.OnConnectFail, conn.ConnSocket);
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="message">要发送的消息</param>
        public static void Send(BaseMessage message)
        {
            BaseNetManager.Send(conn, message);
        }

        /// <summary>
        /// 立即关闭连接
        /// </summary>
        public static void Close()
        {
            BaseNetManager.Close(conn);
        }
    }
}
