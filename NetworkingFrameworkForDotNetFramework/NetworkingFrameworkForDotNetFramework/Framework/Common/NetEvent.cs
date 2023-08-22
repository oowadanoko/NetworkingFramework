using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 网络事件
    /// </summary>
    public enum NetEvent
    {
        /// <summary>
        /// 正在连接
        /// </summary>
        OnConnecting,
        /// <summary>
        /// 连接成功
        /// </summary>
        OnConnectSuccess,
        /// <summary>
        /// 连接失败
        /// </summary>
        OnConnectFail,
        /// <summary>
        /// 正在断开连接
        /// </summary>
        OnDisconnecting,
        /// <summary>
        /// 已断开连接
        /// </summary>
        OnDisconnected,
        /// <summary>
        /// 正在接收
        /// </summary>
        OnAccepting,
        /// <summary>
        /// 接收成功
        /// </summary>
        OnAcceptSuccess,
        /// <summary>
        /// 接收失败
        /// </summary>
        OnAcceptFail,
    }
}
