/**
* WGController32 2015-04-30 17:40:43 karl CSN 陈绍宁 $
*
* 门禁控制器 短报文协议 测试案例
* V2.6 版本  2015-11-03 20:25:53 V6.60驱动版本 增加 通信密码测试, 1024字节用于权限上传和记录提取操作  
*                               修改通信的重试操作
*                               
* V2.5 版本  2015-04-29 20:41:30 采用 V6.56驱动版本 型号由0x19改为0x17
*            基本功能:  查询控制器状态
*                       读取日期时间
*                       设置日期时间
*                       获取指定索引号的记录
*                       设置已读取过的记录索引号
*                       获取已读取过的记录索引号
*                       远程开门
*                       权限添加或修改
*                       权限删除(单个删除)
*                       权限清空(全部清掉)
*                       权限总数读取
*                       权限查询
*                       设置门控制参数(在线/延时)
*                       读取门控制参数(在线/延时)

*                       设置接收服务器的IP和端口
*                       读取接收服务器的IP和端口
*
*
*                       接收服务器的实现 (在61005端口接收数据) -- 此项功能 一定要注意防火墙设置 必须是允许接收数据的.
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WGController32_CSharp
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 
        /// </summary>
        public Form1()
        {
            InitializeComponent();
        }



        private void button1_Click(object sender, EventArgs e)
        {
            //'    '本案例未作搜索控制器  及 设置IP的工作  (直接由IP设置工具来完成)
            //'    '本案例中测试说明
            //'    '控制器SN  = 229999901
            //'    '控制器IP  = 192.168.168.123
            //'    '电脑  IP  = 192.168.168.101
            //'    '用于作为接收服务器的IP (本电脑IP 192.168.168.101), 接收服务器端口 (61005)

            //基本功能测试
            //txtSN.Text 控制器9位数的序列SN
            //txtIP.Text 控制器IP地址, 缺省采用192.168.168.123  [可以采用 Search Controller 修改控制器IP]
            testBasicFunction(txtIP.Text, long.Parse(txtSN.Text)); 
        }


         /// <summary>
        /// 短报文
        /// </summary>
        class WGPacketShort
        {
            public  const int WGPacketSize = 64;			    //报文长度
            //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//类型
            public  const int Type = 0x17;		//2015-04-29 22:22:50			//类型
            public  const int ControllerPort = 60000;        //控制器端口
            public  const long SpecialFlag = 0x55AAAA55;     //特殊标识 防止误操作

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
            public int run1024(byte[] buff,byte[] commPassword)  //2015-11-05 14:50:45 1024字节指令 加密通信部分 发送指令 接收返回信息 
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
                    for (int i = 0; i < 1024; i=i+64)
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

        void log(string info)  //日志信息
        {
            //txtInfo.Text += string.Format("{0}\r\n", info);
            //txtInfo.AppendText(string.Format("{0}\r\n", info));
            txtInfo.AppendText(string.Format("{0} {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), info)); //2015-11-03 20:55:49 显示时间
            txtInfo.ScrollToCaret();//滚动到光标处
            Application.DoEvents();
        }

        /// <summary>
        /// 4字节转成整型数(低位前, 高位后)
        /// </summary>
        /// <param name="buff">字节数组</param>
        /// <param name="start">起始索引位(从0开始计)</param>
        /// <param name="len">长度</param>
        /// <returns>整型数</returns>
        long byteToLong(byte[] buff, int start, int len)
        {
            long val = 0;
            for (int i = 0; i < len && i < 4; i++)
            {
                long lng = buff[i + start];
                val += (lng << (8 * i));  
            }
            return val;
        }

        /// <summary>
        /// 整型数转换为4字节数组
        /// </summary>
        /// <param name="outBytes">数组</param>
        /// <param name="startIndex">起始索引位(从0开始计)</param>
        /// <param name="val">数值</param>
        void LongToBytes(ref byte[] outBytes, int startIndex, long val)
        {
            Array.Copy(System.BitConverter.GetBytes(val), 0, outBytes, startIndex, 4);
        }
        /// <summary>
        /// 获取Hex值, 主要用于日期时间格式
        /// </summary>
        /// <param name="val">数值</param>
        /// <returns>Hex值</returns>
        int GetHex(int val)
        {
            return ((val % 10) + (((val - (val % 10)) / 10) % 10) * 16);
        }

        /// <summary>
        /// 显示记录信息
        /// </summary>
        /// <param name="recv"></param>
        void displayRecordInformation(byte[] recv)
        {
            //8-11	记录的索引号
            //(=0表示没有记录)	4	0x00000000
            int recordIndex = 0;
            recordIndex = (int)byteToLong(recv, 8, 4);

            //12	记录类型**********************************************
            //0=无记录
            //1=刷卡记录
            //2=门磁,按钮, 设备启动, 远程开门记录
            //3=报警记录	1	
            //0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
            int recordType = recv[12];

            //13	有效性(0 表示不通过, 1表示通过)	1	
            int recordValid = recv[13];

            //14	门号(1,2,3,4)	1	
            int recordDoorNO = recv[14];

            //15	进门/出门(1表示进门, 2表示出门)	1	0x01
            int recordInOrOut = recv[15];

            //16-19	卡号(类型是刷卡记录时)
            //或编号(其他类型记录)	4	
            long recordCardNO = 0;
            recordCardNO = byteToLong(recv, 16, 4);

            //20-26	刷卡时间:
            //年月日时分秒 (采用BCD码)见设置时间部分的说明
            string recordTime = "2000-01-01 00:00:00";
            recordTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                recv[20], recv[21], recv[22], recv[23], recv[24], recv[25], recv[26]);
            //2012.12.11 10:49:59	7	
            //27	记录原因代码(可以查 “刷卡记录说明.xls”文件的ReasonNO)
            //处理复杂信息才用	1	
            int reason = recv[27];


            //0=无记录
            //1=刷卡记录
            //2=门磁,按钮, 设备启动, 远程开门记录
            //3=报警记录	1	
            //0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
            if (recordType == 0)
            {
                log(string.Format("索引位={0}  无记录", recordIndex));
            }
            else if (recordType == 0xff)
            {
                log(" 指定索引位的记录已被覆盖掉了,请使用索引0, 取回最早一条记录的索引值");
            }
            else if (recordType == 1) //2015-06-10 08:49:31 显示记录类型为卡号的数据
            {
                //卡号
                log(string.Format("索引位={0}  ", recordIndex));
                log(string.Format("  卡号 = {0}", recordCardNO));
                log(string.Format("  门号 = {0}", recordDoorNO));
                log(string.Format("  进出 = {0}", recordInOrOut == 1 ? "进门" : "出门"));
                log(string.Format("  有效 = {0}", recordValid == 1 ? "通过" : "禁止"));
                log(string.Format("  时间 = {0}", recordTime));
                log(string.Format("  描述 = {0}", getReasonDetailChinese(reason)));
            }
            else if (recordType == 2)
            {
                //其他处理
                //门磁,按钮, 设备启动, 远程开门记录
                log(string.Format("索引位={0}  非刷卡记录", recordIndex));
                log(string.Format("  编号 = {0}", recordCardNO));
                log(string.Format("  门号 = {0}", recordDoorNO));
                log(string.Format("  时间 = {0}", recordTime));
                log(string.Format("  描述 = {0}", getReasonDetailChinese(reason)));
            }
            else if (recordType == 3)
            {
                //其他处理
                //报警记录
                log(string.Format("索引位={0}  报警记录", recordIndex));
                log(string.Format("  编号 = {0}", recordCardNO));
                log(string.Format("  门号 = {0}", recordDoorNO));
                log(string.Format("  时间 = {0}", recordTime));
                log(string.Format("  描述 = {0}", getReasonDetailChinese(reason)));
            }
        }

        string[] RecordDetails =
        {
//记录原因 (类型中 SwipePass 表示通过; SwipeNOPass表示禁止通过; ValidEvent 有效事件(如按钮 门磁 超级密码开门); Warn 报警事件)
//代码  类型   英文描述  中文描述
"1","SwipePass","Swipe","刷卡开门",
"2","SwipePass","Swipe Close","刷卡关",
"3","SwipePass","Swipe Open","刷卡开",
"4","SwipePass","Swipe Limited Times","刷卡开门(带限次)",
"5","SwipeNOPass","Denied Access: PC Control","刷卡禁止通过: 电脑控制",
"6","SwipeNOPass","Denied Access: No PRIVILEGE","刷卡禁止通过: 没有权限",
"7","SwipeNOPass","Denied Access: Wrong PASSWORD","刷卡禁止通过: 密码不对",
"8","SwipeNOPass","Denied Access: AntiBack","刷卡禁止通过: 反潜回",
"9","SwipeNOPass","Denied Access: More Cards","刷卡禁止通过: 多卡",
"10","SwipeNOPass","Denied Access: First Card Open","刷卡禁止通过: 首卡",
"11","SwipeNOPass","Denied Access: Door Set NC","刷卡禁止通过: 门为常闭",
"12","SwipeNOPass","Denied Access: InterLock","刷卡禁止通过: 互锁",
"13","SwipeNOPass","Denied Access: Limited Times","刷卡禁止通过: 受刷卡次数限制",
"14","SwipeNOPass","Denied Access: Limited Person Indoor","刷卡禁止通过: 门内人数限制",
"15","SwipeNOPass","Denied Access: Invalid Timezone","刷卡禁止通过: 卡过期或不在有效时段",
"16","SwipeNOPass","Denied Access: In Order","刷卡禁止通过: 按顺序进出限制",
"17","SwipeNOPass","Denied Access: SWIPE GAP LIMIT","刷卡禁止通过: 刷卡间隔约束",
"18","SwipeNOPass","Denied Access","刷卡禁止通过: 原因不明",
"19","SwipeNOPass","Denied Access: Limited Times","刷卡禁止通过: 刷卡次数限制",
"20","ValidEvent","Push Button","按钮开门",
"21","ValidEvent","Push Button Open","按钮开",
"22","ValidEvent","Push Button Close","按钮关",
"23","ValidEvent","Door Open","门打开[门磁信号]",
"24","ValidEvent","Door Closed","门关闭[门磁信号]",
"25","ValidEvent","Super Password Open Door","超级密码开门",
"26","ValidEvent","Super Password Open","超级密码开",
"27","ValidEvent","Super Password Close","超级密码关",
"28","Warn","Controller Power On","控制器上电",
"29","Warn","Controller Reset","控制器复位",
"30","Warn","Push Button Invalid: Disable","按钮不开门: 按钮禁用",
"31","Warn","Push Button Invalid: Forced Lock","按钮不开门: 强制关门",
"32","Warn","Push Button Invalid: Not On Line","按钮不开门: 门不在线",
"33","Warn","Push Button Invalid: InterLock","按钮不开门: 互锁",
"34","Warn","Threat","胁迫报警",
"35","Warn","Threat Open","胁迫报警开",
"36","Warn","Threat Close","胁迫报警关",
"37","Warn","Open too long","门长时间未关报警[合法开门后]",
"38","Warn","Forced Open","强行闯入报警",
"39","Warn","Fire","火警",
"40","Warn","Forced Close","强制关门",
"41","Warn","Guard Against Theft","防盗报警",
"42","Warn","7*24Hour Zone","烟雾煤气温度报警",
"43","Warn","Emergency Call","紧急呼救报警",
"44","RemoteOpen","Remote Open Door","操作员远程开门",
"45","RemoteOpen","Remote Open Door By USB Reader","发卡器确定发出的远程开门"
        };

        string getReasonDetailChinese(int Reason) //中文
        {
            if (Reason > 45)
            {
                return "";
            }
            if (Reason <= 0)
            {
                return "";
            }
            return RecordDetails[(Reason - 1) * 4 + 3]; //中文信息
        }

        string getReasonDetailEnglish(int Reason) //英文描述
        {
            if (Reason > 45)
            {
                return "";
            }
            if (Reason <= 0)
            {
                return "";
            }
            return RecordDetails[(Reason - 1) * 4 + 2]; //英文信息
        }
        /// <summary>
        /// 基本功能测试
        /// </summary>
        /// <param name="ControllerIP">控制器IP地址</param>
        /// <param name="controllerSN"> 控制器序列号</param>
        /// <returns>小于或等于0 失败, 1表示成功</returns>
        int testBasicFunction(String ControllerIP, long controllerSN)
        {
            int ret = 0;
            int success = 0;  //0 失败, 1表示成功


            //创建短报文 pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;
            byte[] commPassword = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16字节密码


            //设置通信密码[功能号: 0xF0] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xF0;
            //防止误操作标识
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
            for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00设置新密码
            {
                pkt.data[4 + i] = commPassword[i];
                pkt.data[36 + i] = commPassword[i];
            }

            //分两种情况: 密码为空  或者 已设置过密码
            ret = pkt.run();  //2015-11-02 10:21:22 先尝试控制器没有密码的操作
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    log("通信密码设置成功...");
                    success = 1;
                }
            }
            if (success == 0)
            {
                ret = pkt.run(commPassword);  //2015-11-02 10:21:22 再尝试控制器已有密码的操作
                if ((ret > 0) && (pkt.recv[8] == 1))
                {
                    log("通信密码设置成功...[通过加密通信操作]");
                    success = 1;
                }
                else
                {
                    log("通信密码设置失败...[通过加密通信操作]");
                }
            }

            //采用通信密码通信
            //1.10	远程开门[功能号: 0x40] **********************************************************************************
            int doorNO = 1;
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNO & 0xff); //2013-11-03 20:56:33
            ret = pkt.run(commPassword);
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //有效开门.....
                    log("1.10 远程开门	 成功...[通过加密通信操作]");
                    success = 1;
                }
                else
                {
                    log("1.10 远程开门	 失败...[通过加密通信操作]");
                }
            }
            else
            {
                log("1.10 远程开门	 失败...[通过加密通信操作]");
            }

            //清空通信密码[功能号: 0xF0] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xF0;
            //防止误操作标识
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
            for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 清空密码
            {
                pkt.data[4 + i] = 0;
                pkt.data[36 + i] = 0;
            }

            ret = pkt.run(commPassword);
            if ((ret > 0) && (pkt.recv[8] == 1))
            {
                log("通信密码清空成功...[通过加密通信操作]");
                success = 1;
            }
            else
            {
                log("通信密码清空失败...[通过加密通信操作]");
            }



            // **********************************************************************************

            //结束  **********************************************************************************
            pkt.close();  //关闭通信
            return success;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String ControllerIP=txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);

            //1024字节指令 测试
            int ret = 0;
            int success = 0;  //0 失败, 1表示成功
            byte[] command1024 = new byte[1024];


            //创建短报文 pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;


            //1.9	提取记录操作
            //1. 通过 0xB0指令 获取最早一条记录索引
            //2. 通过 0xB0指令 获取最后一条记录索引
            //3. 通过 0xB4指令 获取已读取过的记录索引号 recordIndex
            //4. 通过 0xB0指令 获取指定索引号的记录  从recordIndex + 1开始提取记录， 直到记录为空为止
            //5. 通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
            //经过上面步骤， 整个提取记录的操作完成
            long firstRecordIndex = 0;  //第一条记录索引号
            long lastRecordIndex = 0;   //最后一条记录索引号
            long recordIndexGotToRead = 0x0;
            long recordIndexToGet = 0;
            log("1.9 提取记录操作	 开始...[1024字节指令]");
            pkt.Reset();
            pkt.functionID = 0xB0;//取最早的一条记录索引
            recordIndexToGet = 0x0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                firstRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" 获取最早一条记录索引	 =" + firstRecordIndex.ToString());
            }
            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB0;//取最后的一条记录索引
                recordIndexToGet = 0xffffffff;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run();
            }
            if (ret > 0)
            {
                lastRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" 获取最后一条记录索引	  =" + lastRecordIndex.ToString());
            }

            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB4;//获取已读取过的记录索引号
                recordIndexToGet = 0x0;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run();
            }
            if (ret > 0)
            {
                recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
                log("获取已读取过的记录索引号	  =" + recordIndexGotToRead.ToString());
            }
            long validRecordsCount = 0;
            //recordIndexGotToRead = 0;  //2015-11-05 21:31:05 强制取所有记录
            if (ret > 0)
            {
                long recordIndexValidGet = 0;

                long recordIndexToGetStart = recordIndexGotToRead + 1;  //准备要提取的记录索引位
                if (recordIndexGotToRead > lastRecordIndex || recordIndexGotToRead < firstRecordIndex) //超过范围 取第一个记录的索引号
                {
                    recordIndexToGetStart = firstRecordIndex;
                }

                long recordIndexCurrent;
                int cnt = 0;
                pkt.Reset();
                pkt.functionID = 0xB0;
                pkt.iDevSn = controllerSN;
                do
                {
                     for (int j = 0; j < 1024; j++)
                    {
                        command1024[j] = 0; //复位
                    }
                    recordIndexCurrent = recordIndexToGetStart;
                    for (int j = 0; j < 1024; j = j + 64)
                    {
                        LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
                        byte[] cmd = pkt.toByte();
                        Array.Copy(cmd, 0, command1024, j, 64);
                        recordIndexToGetStart++;
                        cnt++;
                    }
                    ret = pkt.run1024(command1024);
                    success = 0;
                    if (ret > 0)
                    {
                        for (int j = 0; j < 1024; j = j + 64)
                        {
                            success = 0;

                            //12	记录类型
                            //0=无记录
                            //1=刷卡记录
                            //2=门磁,按钮, 设备启动, 远程开门记录
                            //3=报警记录	1	
                            //0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
                            byte[] recv = new byte[64];
                            Array.Copy(pkt.recv, j, recv, 0, 64);
                            int recordType = recv[12];
                            if (recordType == 0)
                            {
                                success = 2;
                                break; //没有更多记录
                            }
                            if (recordType == 0xff)//此索引号无效
                            {
                                success = 0;
                                break;
                            }
                            success = 1;
                            recordIndexValidGet = recordIndexCurrent;
                            recordIndexCurrent++;
                            validRecordsCount++;
                            //
                            if (validRecordsCount < 100) //2015-11-05 14:59:20显示前100个, 太多显示处理速度慢 不作分析了...
                            {
                                displayRecordInformation(recv); //2015-06-09 20:01:21
                                if (validRecordsCount == 99)
                                {
                                    log(" 为加快提取速度, 超过100个的  不再显示记录信息.......");
                                    Application.DoEvents();
                                }
                            }
                            //.......对收到的记录作存储处理
                            //*****
                            //###############
                        }
                    }
                    else
                    {
                        //提取失败
                        break;
                    }
                    if (success != 1)
                    {
                        break;
                    }
                } while (cnt < 200000);

                log("1.9 完全提取成功	 ... 有效记录数= " + validRecordsCount.ToString());
                if ((success > 0) && validRecordsCount>0)
                {
                    //通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
                    pkt.Reset();
                    pkt.functionID = 0xB2;
                    LongToBytes(ref pkt.data, 0, recordIndexValidGet);

                    //12	标识(防止误设置)	1	0x55 [固定]
                    LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

                    ret = pkt.run();
                    success = 0;
                    if (ret > 0)
                    {
                        if (pkt.recv[8] == 1)
                        {
                            //完全提取成功....
                            log("1.9 完全提取成功	 成功...");
                            success = 1;
                        }
                    }

                }
            }


            //1.21	权限按从小到大顺序添加[功能号: 0x56] 适用于权限数过1000, 少于8万 **********************************************************************************
            //此功能实现 完全更新全部权限, 用户不用清空之前的权限. 只是将上传的权限顺序从第1个依次到最后一个上传完成. 如果中途中断的话, 仍以原权限为主
            //建议权限数更新超过50个, 即可使用此指令
            //如果权限数超过8万时, 中途中断的话, 权限会为空. 所以要上传完整

            log("1.21	权限按从小到大顺序添加[功能号: 0x56]	开始...[采用1024字节指令, 每次上传16个权限]");

            //以10000个卡号为例, 此处简化的排序, 直接是以50001开始的10000个卡. 用户按照需要将要上传的卡号排序存放
            int cardCount = 1 * 10000; // 10000;  //2015-06-09 20:20:20 卡总数量
            log(string.Format("       {0}万条权限...", cardCount / 10000));
            long[] cardArray = new long[cardCount];
            for (int i = 0; i < cardCount; i++)
            {
                cardArray[i] = 50001 + i;
            }
            long cardNOOfPrivilegeToGetlast = 0;
            long cardNOOfPrivilege;
            for (int i = 0; i < cardCount; )
            {
                for (int j = 0; j < 1024; j++)
                {
                    command1024[j] = 0; //复位
                }
                
                for (int j = 0; j < 1024; j = j + 64)
                {
                    if (i >= cardCount)
                    {
                        break;
                    }
                    pkt.Reset();
                    pkt.functionID = 0x56;

                    cardNOOfPrivilege = cardArray[i];
                    cardNOOfPrivilegeToGetlast = cardNOOfPrivilege;
                    LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

                    //其他参数简化时 统一, 可以依据每个卡的不同进行修改
                    //20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
                    pkt.data[4] = 0x20;
                    pkt.data[5] = 0x10;
                    pkt.data[6] = 0x01;
                    pkt.data[7] = 0x01;
                    //20 29 12 31 截止日期:  2029年12月31日
                    pkt.data[8] = 0x20;
                    pkt.data[9] = 0x29;
                    pkt.data[10] = 0x12;
                    pkt.data[11] = 0x31;
                    //01 允许通过 一号门 [对单门, 双门, 四门控制器有效] 
                    pkt.data[12] = 0x01;
                    //01 允许通过 二号门 [对双门, 四门控制器有效]
                    pkt.data[13] = 0x01;  //如果禁止2号门, 则只要设为 0x00
                    //01 允许通过 三号门 [对四门控制器有效]
                    pkt.data[14] = 0x01;
                    //01 允许通过 四号门 [对四门控制器有效]
                    pkt.data[15] = 0x01;

                    LongToBytes(ref pkt.data, 32 - 8, cardCount); //总的权限数
                    LongToBytes(ref pkt.data, 35 - 8, i + 1);//当前权限的索引位(从1开始)

                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);
                    i++;

                }
                ret = pkt.run1024(command1024); //2015-11-04 19:15:48 
                success = 0;
                if (ret > 0)
                {
                    if (pkt.recv[8] == 1)
                    {
                        success = 1;
                    }
                    if (pkt.recv[8] == 0xE1)
                    {
                        log("1.21	权限按从小到大顺序添加[功能号: 0x56]	 =0xE1 表示卡号没有从小到大排序...???");
                        success = 0;
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            if (success == 1)
            {
                log("1.21	权限按从小到大顺序添加[功能号: 0x56]	 成功...");
            }
            else
            {
                log("1.21	权限按从小到大顺序添加[功能号: 0x56]	 失败...????");
            }

            //1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
            //读取所有权限
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long maxCount = 20 * 10000;
            long[] cardArrayGet = new long[maxCount];
            long QueryIndex = 1; //索引号(从1开始);
            LongToBytes(ref pkt.data, 0, QueryIndex);

            for (int i = 0; i < maxCount; i++)
            {
                cardArrayGet[i] = 0;
            }
            log("读取所有权限	 开始...[1024字节指令]");
            long iCount = 0;
            for (int i = 0; i < maxCount; i++)
            {
                for (int j = 0; j < 1024; j++)
                {
                    command1024[j] = 0; //复位
                }
                for (int j = 0; j < 1024; j = j + 64)
                {
                    LongToBytes(ref pkt.data, 0, QueryIndex);
                    QueryIndex++; //索引号(从1开始);
                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);

                }
                ret = pkt.run1024(command1024);
                success = 0;
                if (ret > 0)
                {
                    for (int j = 0; j < 1024; j = j + 64)
                    {
                        success = 0;
                        long cardNOOfPrivilegeToGet = 0;
                        cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8 + j, 4);
                        if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFF对应于4294967295
                        {
                            success = 1;
                            //log("1.16      没有权限信息: (权限已删除)");
                            //break;
                        }
                        else if (cardNOOfPrivilegeToGet == 0)
                        {
                            //没有权限时: (卡号部分为0)
                            //log("1.16       没有权限信息: (卡号部分为0)--此索引号之后没有权限了");
                            break;
                        }
                        else
                        {
                            //具体权限信息...
                            //  log("1.16      有权限信息...");
                            // log("1.16 获取指定索引号的权限	 成功...");
                            cardArrayGet[iCount] = cardNOOfPrivilegeToGet;
                            iCount++;
                            success = 1;
                            cardNOOfPrivilegeToGetlast = cardNOOfPrivilegeToGet;
                        }

                    }
                    if (success == 0)
                    {
                        break;
                    }
                }
                else
                {
                    log("1.16     有问题..." + ret.ToString());
                    break;
                }
            }
            log("最后读取到的权限的卡号 = " + cardNOOfPrivilegeToGetlast.ToString());
            log("提取到的权限数iCount = " + iCount.ToString());  //2015-11-04 19:59:50 提取权限数

            // **********************************************************************************

            //结束  **********************************************************************************
            pkt.close();  //关闭通信
        }

    }
}
