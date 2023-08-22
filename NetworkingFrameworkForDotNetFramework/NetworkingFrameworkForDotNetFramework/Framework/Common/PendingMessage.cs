using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 待处理的消息
    /// </summary>
    public class PendingMessage
    {
        /// <summary>
        /// 收发消息的客户端
        /// </summary>
        public ClientConnection Client { get; set; }
        /// <summary>
        /// 收发的消息
        /// </summary>
        public BaseMessage Message { get; set; }

        /// <summary>
        /// 构建一个待处理的消息
        /// </summary>
        /// <param name="client">收发消息的客户端</param>
        /// <param name="message">收发的消息</param>
        public PendingMessage(ClientConnection client, BaseMessage message)
        {
            Client = client;
            Message = message;
        }
    }
}
