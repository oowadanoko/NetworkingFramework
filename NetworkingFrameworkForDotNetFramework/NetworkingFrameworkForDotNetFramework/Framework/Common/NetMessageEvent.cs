using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 网络消息事件
    /// </summary>
    public enum NetMessageEvent
    {
        /// <summary>
        /// 正在发送
        /// </summary>
        OnSending,
        /// <summary>
        /// 发送成功
        /// </summary>
        OnSendSuccess,
        /// <summary>
        /// 发送失败
        /// </summary>
        OnSendFail,
        /// <summary>
        /// 正在接收
        /// </summary>
        OnReceiving,
        /// <summary>
        /// 接收成功
        /// </summary>
        OnReceiveSuccess,
        /// <summary>
        /// 接收失败
        /// </summary>
        OnReceiveFail,
    }
}
