using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 客户端连接
    /// </summary>
    public class ClientConnection
    {
        /// <summary>
        /// 客户端连接套接字
        /// </summary>
        public Socket ConnSocket { get; set; }
        /// <summary>
        /// 由IP地址和端口号组成的标识符，若未连接则为默认值，可通过DefaultIdentifier来设置默认值
        /// </summary>
        public string Identifier { get; set; } = DefaultIdentifier;
        /// <summary>
        /// 默认的标识符Identifier
        /// </summary>
        public static string DefaultIdentifier { get; set; } = "Unconnected";
        /// <summary>
        /// 读缓冲区
        /// </summary>
        internal ByteBuffer ReadBuff { get; set; } = new ByteBuffer();
        /// <summary>
        /// 是否正在连接中
        /// </summary>
        public bool IsConnecting { get; set; } = false;
        /// <summary>
        /// 是否正在关闭连接
        /// </summary>
        public bool IsClosing { get; set; } = false;
        /// <summary>
        /// 是否已关闭连接
        /// </summary>
        public bool IsClosed { get; set; } = true;
        /// <summary>
        /// 是否能够连接
        /// </summary>
        internal bool CanConnect { get { return (ConnSocket == null || !ConnSocket.Connected) && !IsConnecting && !IsClosing; } }
        /// <summary>
        /// 是否能够收发消息
        /// </summary>
        internal bool CanSendOrReceive { get { return ConnSocket != null && ConnSocket.Connected && !IsConnecting && !IsClosing; } }
        /// <summary>
        /// 是否能够发送剩下的数据
        /// </summary>
        internal bool CanSendRest { get { return ConnSocket != null && !IsConnecting; } }
        /// <summary>
        /// 是否能够关闭连接
        /// </summary>
        internal bool CanClose { get { return ConnSocket != null && !IsClosed && !IsClosing && !IsConnecting; } }
        /// <summary>
        /// 写队列
        /// </summary>
        internal Queue<ByteBuffer> WriteQueue { get; private set; } = new Queue<ByteBuffer>();
        /// <summary>
        /// 用户数据，用于自行存储需要的数据
        /// </summary>
        public Hashtable CustomData { get; private set; } = new Hashtable();

        /// <summary>
        /// 创建一个客户端连接，空套接字，需要先给连接套接字ConnSocket赋值才能正常使用
        /// </summary>
        internal ClientConnection()
        {

        }

        /// <summary>
        /// 创建一个客户端连接
        /// </summary>
        /// <param name="socket">客户端的连接套接字</param>
        internal ClientConnection(Socket socket)
        {
            ConnSocket = socket;
        }

        /// <summary>
        /// 设置标识符，值为远程连接对象的IP地址和端口号
        /// </summary>
        internal void SetIdentifier()
        {
            if (ConnSocket.Connected)
            {
                Identifier = ConnSocket.RemoteEndPoint.ToString();
            }
        }

        /// <summary>
        /// 立即关闭连接
        /// </summary>
        internal void Close()
        {
            IsClosing = true;
            if (ConnSocket != null)
            {
                ConnSocket.Close();
                Identifier = DefaultIdentifier;
            }
            IsClosing = false;
        }
    }
}
