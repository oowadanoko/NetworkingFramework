using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 消息基类，所有的消息都从该类派生
    /// </summary>
    public abstract class BaseMessage
    {
        /// <summary>
        /// 消息名和消息类型的映射字典
        /// </summary>
        private static Dictionary<string, Type> types = new Dictionary<string, Type>();

        /// <summary>
        /// 注册当前消息类
        /// </summary>
        /// <typeparam name="T">要注册的消息类，该类必须从BaseMessage派生</typeparam>
        internal static void RegisterMessage<T>() where T : BaseMessage
        {
            lock (types)
            {
                types[typeof(T).Name] = typeof(T);
            }
        }

        /// <summary>
        /// 注销当前消息类
        /// </summary>
        /// <typeparam name="T">要注销的消息类，该类必须从BaseMessage派生</typeparam>
        internal static void UnregisterMessage<T>() where T : BaseMessage
        {
            lock(types)
            {
                if (types.ContainsKey(typeof(T).Name))
                {
                    types.Remove(typeof(T).Name);
                }
            }
        }

        /// <summary>
        /// 获取该消息写入缓冲区需要占用的字节数
        /// </summary>
        /// <returns>该消息写入缓冲区需要占用的字节数</returns>
        internal int GetMessageByteLength()
        {
            return GetMessageByteLength(this);
        }

        /// <summary>
        /// 获取消息写入缓冲区需要占用的字节数
        /// </summary>
        /// <param name="message">要写如的消息</param>
        /// <returns>消息写入缓冲区需要占用的字节数</returns>
        internal static int GetMessageByteLength(BaseMessage message)
        {
            string name = message.GetType().Name;
            string data = JsonSerializer.Serialize(message, message.GetType());
            return 2 + 2 + name.Length + data.Length;
        }

        /// <summary>
        /// 将消息写入缓冲区，包括写入消息名长度、消息名、消息数据，并在开头加上整体消息的长度
        /// </summary>
        /// <param name="buff">要写入的缓冲区</param>
        internal void WriteToByteBuffer(ByteBuffer buff)
        {
            WriteToByteBuffer(buff, this);
        }

        /// <summary>
        /// 将消息写入缓冲区，包括写入消息名长度、消息名、消息数据，并在开头加上整体消息的长度
        /// </summary>
        /// <param name="buff">要写入的缓冲区</param>
        /// <param name="message">要写入的消息</param>
        internal static void WriteToByteBuffer(ByteBuffer buff, BaseMessage message)
        {
            string name = message.GetType().Name;
            string data = JsonSerializer.Serialize(message, types[name]);
            // 指示名字长度的16位无符号整数占2位 + 名字 + 数据
            buff.WriteUInt16((UInt16)(2 + name.Length + data.Length));
            buff.WriteUInt16((UInt16)name.Length);
            buff.WriteData(Encoding.UTF8.GetBytes(name));
            buff.WriteData(Encoding.UTF8.GetBytes(data));
        }

        /// <summary>
        /// 从缓冲区中读取消息
        /// </summary>
        /// <param name="buff">要读取的缓冲区</param>
        /// <returns>缓冲区中读取出的消息对象，若读取失败返回null</returns>
        internal static BaseMessage ReadFromByteBuff(ByteBuffer buff)
        {
            UInt16 messageSize = buff.ReadUInt16();
            if (messageSize == 0)  // 读取16位无符号整数失败
            {
                return null;
            }
            if (messageSize > buff.DataSize)  // 数据不完整
            {
                buff.MoveReadIndex(-2);  // 回退读到的UInt16
                return null;
            }
            UInt16 nameSize = buff.ReadUInt16();
            string name = Encoding.UTF8.GetString(buff.ReadData((UInt16)nameSize));
            string messageString =  // 消息长度 - UInt16占的字节数（指示名字的长度） - 名字的长度
                Encoding.UTF8.GetString(buff.ReadData(messageSize - 2 - nameSize));
            return JsonSerializer.Deserialize(messageString, types[name]) as BaseMessage;
        }

        /// <summary>
        /// 转化为消息名加JSON字符串格式输出
        /// </summary>
        /// <returns>消息名加JSON字符串</returns>
        public override string ToString()
        {
            string name = this.GetType().Name;
            string data = JsonSerializer.Serialize(this, types[name]);
            return name + data;
        }
    }
}
