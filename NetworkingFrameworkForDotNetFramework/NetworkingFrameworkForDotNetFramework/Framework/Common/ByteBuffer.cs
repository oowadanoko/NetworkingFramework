using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oowada.NetworkingFramework.Common
{
    /// <summary>
    /// 缓冲区
    /// </summary>
    internal class ByteBuffer
    {
        /// <summary>
        /// 缓冲区数据数组
        /// </summary>
        internal byte[] Buff { get; private set; }
        /// <summary>
        /// 读索引
        /// </summary>
        internal int ReadIndex { get; private set; } = 0;
        /// <summary>
        /// 写索引
        /// </summary>
        internal int WriteIndex { get; private set; } = 0;
        /// <summary>
        /// 缓冲区总容量
        /// </summary>
        internal int Capacity { get { return Buff.Length; } }
        /// <summary>
        /// 缓冲区剩余容量
        /// </summary>
        internal int RemainCapacity { get { return Buff.Length - WriteIndex; } }
        /// <summary>
        /// 实际存储的数据的大小
        /// </summary>
        internal int DataSize { get { return WriteIndex - ReadIndex; } }
        /// <summary>
        /// 自动检查时，当剩余容量小于数组总容量Capacity的该倍率时，会尝试移动或扩容
        /// </summary>
        internal float AutoMoveExpendLimits { get; set; } = 0.2f;
        /// <summary>
        /// 自动检查时，当数据容量小于数组总容量Capacity的该倍率一定次数时，会尝试缩容，可以通过设置AutoReduceLimitsCount改变次数
        /// </summary>
        internal float AutoReduceLimits { get; set; } = 0.3f;
        /// <summary>
        /// 自动检查时，当数据容量小于数组总容量Capacity的一定倍率该次数时，会尝试缩容，可以通过设置AutoReduceLimits改变倍率
        /// </summary>
        internal int AutoReduceLimitsCount { get; set; } = 10;
        /// <summary>
        /// 自动检查时，判定符合缩容条件的实际次数，可以通过设置AutoReduceLimits和AutoReduceLimitsCount修改条件
        /// </summary>
        internal int CurrentAutoReduceLimitsCount { get; private set; } = 0;

        /// <summary>
        /// 构建一个缓冲区
        /// </summary>
        /// <param name="size">缓冲区初始大小</param>
        internal ByteBuffer(int size = 1024)
        {
            Buff = new byte[size];
        }

        /// <summary>
        /// 移动写索引，异步操作数据时需要自己根据情况修改写索引
        /// </summary>
        /// <param name="offset">偏移量</param>
        internal void MoveWriteIndex(int offset)
        {
            WriteIndex += offset;
        }

        /// <summary>
        /// 移动读索引，异步操作数据时需要自己根据情况修改读索引
        /// </summary>
        /// <param name="offset">偏移量</param>
        internal void MoveReadIndex(int offset)
        {
            ReadIndex += offset;
        }

        /// <summary>
        /// 重新设置大小
        /// </summary>
        /// <param name="size">要设置的大小</param>
        /// <returns>如果传入的大小比实际数据大小DataSize小则返回false；否则返回true</returns>
        internal bool Resize(int size)
        {
            if (size < DataSize)
            {
                return false;
            }
            byte[] bytes = new byte[size];
            Array.Copy(Buff, ReadIndex, bytes, 0, DataSize);
            WriteIndex = DataSize;
            ReadIndex = 0;
            Buff = bytes;
            return true;
        }

        /// <summary>
        /// 扩展容量，将容量扩充至原先的scale倍
        /// </summary>
        /// <param name="scale">扩充倍数</param>
        internal void ExpandCapacity(int scale)
        {
            Resize(scale * Capacity);
        }

        /// <summary>
        /// 尝试移动数组中的数据到数组开头，只有当数组中前面一半的空间都是空的，并且剩余容量较少时，尝试移动数据到数组开头才会成功
        /// </summary>
        /// <returns>是否移动了数组</returns>
        internal bool TryMoveBytes()
        {
            // 前面有一半是空的 并且 剩余容量较少
            if (ReadIndex > Capacity / 2 && RemainCapacity < Capacity * AutoMoveExpendLimits)  
            {
                Array.Copy(Buff, ReadIndex, Buff, 0, DataSize);
                WriteIndex = DataSize;
                ReadIndex = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 自动检查容量，若发现容量即将不足，会尝试移动或扩容；若发现剩余容量一直太多，会尝试缩容
        /// </summary>
        internal void AutoCheckCapacity()
        {
            // 连续多次实际存储的数据比较少
            if (DataSize < Capacity * AutoReduceLimits)
            {
                if (++CurrentAutoReduceLimitsCount > AutoReduceLimitsCount)
                {
                    Resize(Capacity / 2);
                    CurrentAutoReduceLimitsCount = 0;  // 缩容之后重置
                }
            }
            TryMoveBytes();
            // 尝试移动之后剩余容量仍然较少
            if (RemainCapacity < Capacity * AutoMoveExpendLimits)
            {
                ExpandCapacity(2);
            }
        }

        /// <summary>
        /// 以大端模式读取一个16位无符号整数
        /// </summary>
        /// <returns>16位无符号整数，如果不能读取返回0</returns>
        internal UInt16 ReadUInt16()
        {
            AutoCheckCapacity();
            if (DataSize < 2)  // 不足2个字节，无法凑成一个16位无符号整数
            {
                return 0;
            }
            UInt16 high = (UInt16)(Buff[ReadIndex] << 8);
            UInt16 low = Buff[ReadIndex + 1];
            ReadIndex += 2;
            return (UInt16)(high | low);
        }

        /// <summary>
        /// 以大端模式写入一个16位无符号整数
        /// </summary>
        /// <param name="num">16位无符号整数</param>
        internal void WriteUInt16(UInt16 num)
        {
            AutoCheckCapacity();
            Buff[WriteIndex] = (byte)(num >> 8);
            Buff[WriteIndex + 1] = (byte)num;
            WriteIndex += 2;
        }
        
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="size">要读取的数据大小，若超过数据实际大小则只会读取实际大小的数据</param>
        /// <returns>读取的数据，若没有数据会返回长度为0的数组</returns>
        internal byte[] ReadData(int size)
        {
            AutoCheckCapacity();
            size = size > DataSize ? DataSize : size;
            byte[] bytes = new byte[size];
            Array.Copy(Buff, ReadIndex, bytes, 0, size);
            ReadIndex += size;
            return bytes;
        }


        /// <summary>
        /// 写入数据，如果数据超出缓冲区总容量，会扩容到足够装下所有数据，可以通过设置CapacityLackResizeRate调整扩容的比率
        /// </summary>
        /// <param name="data">要写入的数据</param>
        internal void WriteData(byte[] data)
        {
            AutoCheckCapacity();
            Array.Copy(data, 0, Buff, WriteIndex, data.Length);
            WriteIndex += data.Length;
        }
    }
}
