using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WGController32_CSharp
{
    /// <summary>
    /// 短报文
    /// </summary>
    public class WGPacketShort
        : IDisposable
    {

        /// <summary>
        /// 释放资源1
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)  //2017-09-12 09:42:30 增加释放
        {
            if (disposing)
            {
                // dispose managed resources
                // newFile.Close();
                if (controller != null)
                {
                    controller.Dispose();
                }
            }
            // free native resources
        }
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()  //2017-09-12 09:42:37
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public const int WGPacketSize = 64;			    //报文长度
        //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//类型
        public const int Type = 0x17;		//2015-04-29 22:22:50			//类型
        public const int ControllerPort = 60000;        //控制器端口
        public const long SpecialFlag = 0x55AAAA55;     //特殊标识 防止误操作

        public int functionID;		                     //功能号
        public long iDevSn;                              //设备序列号 4字节, 9位数
        public string IP;                                //控制器的IP地址

        public byte[] data = new byte[56];               //56字节的数据 [含流水号]
        public byte[] recv = new byte[WGPacketSize];     //接收到的数据

        public WGPacketShort()
        {
            Reset();
        }
        public void Reset()  //数据复位
        {
            for (int i = 0; i < 56; i++)
            {
                data[i] = 0;
            }
        }
        static long sequenceId;     //序列号	
        public byte[] toByte() //生成64字节指令包
        {
            byte[] buff = new byte[WGPacketSize];
            sequenceId++;

            buff[0] = (byte)Type;
            buff[1] = (byte)functionID;
            Array.Copy(System.BitConverter.GetBytes(iDevSn), 0, buff, 4, 4);
            Array.Copy(data, 0, buff, 8, data.Length);
            Array.Copy(System.BitConverter.GetBytes(sequenceId), 0, buff, 40, 4);
            return buff;
        }

        WG3000_COMM.Core.wgMjController controller = new WG3000_COMM.Core.wgMjController();
        public int run()  //发送指令 接收返回信息
        {
            byte[] buff = toByte();

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 进入重试 return -1;
                }
                else
                {
                    //流水号
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //类型一致
                        && (recv[1] == functionID)              //功能号一致
                        && (sequenceIdReceived == sequenceId))  //序列号对应
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //重试三次

            return -1;
        }
        public int run(string pcIPAddress)  //发送指令 接收返回信息  指定PC的IP地址
        {
            byte[] buff = toByte();

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv, pcIPAddress) < 0)
                {
                    //2015-11-03 20:26:52 进入重试 return -1;
                }
                else
                {
                    //流水号
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //类型一致
                        && (recv[1] == functionID)              //功能号一致
                        && (sequenceIdReceived == sequenceId))  //序列号对应
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //重试三次

            return -1;
        }



        //加密调用的动态库
        [DllImport("n3kWGCom.dll", EntryPoint = "ShortEncrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int ShortEncrypt(IntPtr command, IntPtr password);

        [DllImport("n3kWGCom.dll", EntryPoint = "ShortDecrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int ShortDecrypt(IntPtr command, IntPtr password);

        public static int Encrypt(ref byte[] command, byte[] password)  //2015-09-28 13:19:12 2013-4-2_07:31:32 加密数据
        {
            IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

            Marshal.Copy(command, 0, pkt, 64);

            IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
            Marshal.Copy(password, 0, commPassword, 16);

            int ret = ShortEncrypt(pkt, commPassword);
            if (ret > 0)
            {
                Marshal.Copy(pkt, command, 0, 64);  //复制回来
            }
            Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 释放内存
            Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 释放内存
            return ret;

        }
        public static int Decrypt(ref byte[] command, byte[] password)  //2015-09-28 15:12:12  解密数据
        {
            IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

            Marshal.Copy(command, 0, pkt, 64);

            IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
            Marshal.Copy(password, 0, commPassword, 16);


            int ret = ShortDecrypt(pkt, commPassword);
            if (ret > 0)
            {
                Marshal.Copy(pkt, command, 0, 64);  //复制回来
            }
            Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 释放内存
            Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 释放内存
            return ret;
        }

        public int run(byte[] commPassword)  //2015-10-28 10:16:38 通信密处理发送指令 接收返回信息
        {
            byte[] buff = toByte();
            Encrypt(ref buff, commPassword);

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 进入重试 return -1;
                }
                else
                {
                    if ((recv[0] & 0x7F) == Type)
                    {
                        Decrypt(ref recv, commPassword);

                        //流水号
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //类型一致
                            && (recv[1] == functionID)              //功能号一致
                            && (sequenceIdReceived == sequenceId))  //序列号对应
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //重试三次

            return -1;
        }

        public int run1024(byte[] buff)  //2015-11-05 14:50:45 1024字节指令 发送指令 接收返回信息 
        {
            return run1024(buff, null);
        }


        //commPassword 为空时不采用密码. 密码必须是16字节
        public int run1024(byte[] buff, byte[] commPassword)  //2015-11-05 14:50:45 1024字节指令 加密通信部分 发送指令 接收返回信息 
        {
            long sequenceIdSend = 0;
            for (int i = 0; i < 4; i++)
            {
                long lng = buff[40 + i];
                sequenceIdSend += (lng << (8 * i));
            }
            if (commPassword != null)
            {
                //如果加密
                byte[] buffBk = new byte[64];
                for (int i = 0; i < 1024; i = i + 64)
                {
                    Array.Copy(buff, i, buffBk, 0, 64);
                    Encrypt(ref buffBk, commPassword);
                    Array.Copy(buffBk, 0, buff, i, 64);
                }
            }
            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 进入重试 return -1;
                }
                else
                {
                    if ((recv[0] & 0x7F) == Type)
                    {
                        if ((commPassword != null) && recv.Length == 1024)
                        {
                            //如果加密了
                            byte[] buffBk = new byte[64];
                            for (int i = 0; i < 1024; i = i + 64)
                            {
                                Array.Copy(recv, i, buffBk, 0, 64);
                                Decrypt(ref buffBk, commPassword);
                                Array.Copy(buffBk, 0, recv, i, 64);
                            }
                        }
                    }
                    //流水号
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //类型一致
                        && (recv[1] == functionID)              //功能号一致
                        && (sequenceIdReceived == sequenceIdSend)  //序列号对应
                    )
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //重试三次

            return -1;
        }
        
        /// <summary>
        /// 最后发出的流水号
        /// </summary>
        /// <returns></returns>
        public static long sequenceIdSent()// 
        {
            return sequenceId; // 最后发出的流水号
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void close()
        {
            controller.Dispose();
        }
    }
}
