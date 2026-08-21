/**
* WGController32 2015-04-30 17:40:43 karl CSN 陈绍宁 $
*
* 门禁控制器 短报文协议 测试案例
* V2.6 版本  2015-11-03 20:25:53 V6.60驱动版本 通信密码测试.  
*                               修改通信的重试操作
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

        Boolean bStopWatchServer = false; //2015-05-05 17:35:07 停止接收服务器
        Boolean bStopBasicFunction = false;  //2015-06-10 09:04:52 基本测试
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bStopWatchServer = true;
            bStopBasicFunction = true;  //2015-06-10 09:04:52 基本测试
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.txtInfo.Text = "";

            //停止接收服务器标识 
            bStopWatchServer = true;
            bStopBasicFunction = false;  //2015-06-10 09:04:52 基本测试

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


            //txtWatchServerIP.Text  接收服务器的IP,缺省采用电脑IP 192.168.168.101 [也可以采用 Search Controller 修改设置]
            //txtWatchServerPort.Text  接收服务器的PORT, 缺省 61005
            testWatchingServer(txtIP.Text, long.Parse(txtSN.Text), txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //接收服务器设置

            bStopWatchServer = false;
            WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //服务器运行....
            bStopWatchServer = true;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            bStopWatchServer = true;
            bStopBasicFunction = true;  //2015-06-10 09:04:52 基本测试
        }

        private void button3_Click(object sender, EventArgs e) //2015-05-05 17:35:35 搜索控制器
        {
            try
            {
                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = Environment.CurrentDirectory + "\\IPCon2015_V2.17.exe";
                pInfo.UseShellExecute = true;
                Process p = Process.Start(pInfo);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            bStopWatchServer = false;
            WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //服务器运行....
            bStopWatchServer = true;
        }

        /// <summary>
        /// 短报文
        /// </summary>
        class WGPacketShort
        {
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
            txtInfo.AppendText(string.Format("{0} {1}\r\n",DateTime.Now.ToString("HH:mm:ss"), info)); //2015-11-03 20:55:49 显示时间
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

            //1.4	查询控制器状态[功能号: 0x20](实时监控用) **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x20;
            ret = pkt.run();

            success = 0;
            if (ret == 1)
            {
                //读取信息成功...
                success = 1;
                log("1.4 查询控制器状态 成功...");

                //	  	最后一条记录的信息		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21
                
                //	其他信息		
                int[] doorStatus = new int[4];
                //28	1号门门磁(0表示关上, 1表示打开)	1	0x00
                doorStatus[1 - 1] = pkt.recv[28];
                //29	2号门门磁(0表示关上, 1表示打开)	1	0x00
                doorStatus[2 - 1] = pkt.recv[29];
                //30	3号门门磁(0表示关上, 1表示打开)	1	0x00
                doorStatus[3 - 1] = pkt.recv[30];
                //31	4号门门磁(0表示关上, 1表示打开)	1	0x00
                doorStatus[4 - 1] = pkt.recv[31];

                int[] pbStatus = new int[4];
                //32	1号门按钮(0表示松开, 1表示按下)	1	0x00
                pbStatus[1 - 1] = pkt.recv[32];
                //33	2号门按钮(0表示松开, 1表示按下)	1	0x00
                pbStatus[2 - 1] = pkt.recv[33];
                //34	3号门按钮(0表示松开, 1表示按下)	1	0x00
                pbStatus[3 - 1] = pkt.recv[34];
                //35	4号门按钮(0表示松开, 1表示按下)	1	0x00
                pbStatus[4 - 1] = pkt.recv[35];
                
                //36	故障号
                //等于0 无故障
                //不等于0, 有故障(先重设时间, 如果还有问题, 则要返厂家维护)	1	
                int errCode = pkt.recv[36];
               
                //37	控制器当前时间
                //时	1	0x21
                //38	分	1	0x30
                //39	秒	1	0x58

                //40-43	流水号	4	
                long sequenceId = 0;
                sequenceId = byteToLong(pkt.recv, 40, 4);

                //48
                //特殊信息1(依据实际使用中返回)
                //键盘按键信息	1	


                //49	继电器状态	1	 [0表示门上锁, 1表示门开锁. 正常门上锁时, 值为0000]
                int relayStatus = pkt.recv[49];
                if ((relayStatus & 0x1) > 0)
                {
                    //一号门 开锁
                }
                else
                {
                    //一号门 上锁
                }
                if ((relayStatus & 0x2) > 0)
                {
                    //二号门 开锁
                }
                else
                {
                    //二号门 上锁
                }
                if ((relayStatus & 0x4) > 0)
                {
                    //三号门 开锁
                }
                else
                {
                    //三号门 上锁
                }
                if ((relayStatus & 0x8) > 0)
                {
                    //四号门 开锁
                }
                else
                {
                    //四号门 上锁
                }

                //50	门磁状态的8-15bit位[火警/强制锁门]
                //Bit0  强制锁门
                //Bit1  火警		
                int otherInputStatus = pkt.recv[50];
                if ((otherInputStatus & 0x1) > 0)
                {
                    //强制锁门
                }
                if ((otherInputStatus & 0x2) > 0)
                {
                    //火警
                }

                //51	V5.46版本支持 控制器当前年	1	0x13
                //52	V5.46版本支持 月	1	0x06
                //53	V5.46版本支持 日	1	0x22

                string controllerTime = "2000-01-01 00:00:00"; //控制器当前时间
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    0x20, pkt.recv[51], pkt.recv[52], pkt.recv[53], pkt.recv[37], pkt.recv[38], pkt.recv[39]);
            }
            else
            {
                log("1.4 查询控制器状态 失败?????...");
                return -1;
            }

            //1.5	读取日期时间(功能号: 0x32) **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x32;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                string controllerTime = "2000-01-01 00:00:00"; //控制器当前时间
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    pkt.recv[8], pkt.recv[9], pkt.recv[10], pkt.recv[11], pkt.recv[12], pkt.recv[13], pkt.recv[14]);

                log("1.5 读取日期时间 成功...");
                success = 1;
            }

            //1.6	设置日期时间[功能号: 0x30] **********************************************************************************
            //按电脑当前时间校准控制器.....
            pkt.Reset();
            pkt.functionID = 0x30;

            DateTime ptm = DateTime.Now;
            pkt.data[0] = (byte)GetHex((ptm.Year - ptm.Year % 100) / 100);
            pkt.data[1] = (byte)GetHex((int)((ptm.Year) % 100)); //st.GetMonth()); 
            pkt.data[2] = (byte)GetHex(ptm.Month);
            pkt.data[3] = (byte)GetHex(ptm.Day);
            pkt.data[4] = (byte)GetHex(ptm.Hour);
            pkt.data[5] = (byte)GetHex(ptm.Minute);
            pkt.data[6] = (byte)GetHex(ptm.Second);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                Boolean bSame = true;
                for (int i = 0; i < 7; i++)
                {
                    if (pkt.data[i] != pkt.recv[8 + i])
                    {
                        bSame = false;
                        break;
                    }
                }
                if (bSame)
                {
                    log("1.6 设置日期时间 成功...");
                    success = 1;
                }
            }

            //1.7	获取指定索引号的记录[功能号: 0xB0] **********************************************************************************
            //(取索引号 0x00000001的记录)
            long recordIndexToGet = 0;
            pkt.Reset();
            pkt.functionID = 0xB0;
            pkt.iDevSn = controllerSN;

            //	(特殊
            //如果=0, 则取回最早一条记录信息
            //如果=0xffffffff则取回最后一条记录的信息)
            //记录索引号正常情况下是顺序递增的, 最大可达0xffffff = 16,777,215 (超过1千万) . 由于存储空间有限, 控制器上只会保留最近的20万个记录. 当索引号超过20万后, 旧的索引号位的记录就会被覆盖, 所以这时查询这些索引号的记录, 返回的记录类型将是0xff, 表示不存在了.
            recordIndexToGet = 1;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 获取索引为1号记录的信息	 成功...");
                //	  	索引为1号记录的信息		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            //. 发出报文 (取最早的一条记录 通过索引号 0x00000000) [此指令适合于 刷卡记录超过20万时环境下使用]
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 获取最早一条记录的信息	 成功...");
                //	  	最早一条记录的信息		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            //发出报文 (取最新的一条记录 通过索引 0xffffffff)
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0xffffffff;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 获取最新一条记录的信息	 成功...");
                //	  	最新一条记录的信息		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21
                success = 1;
            }

            ////1.8	设置已读取过的记录索引号[功能号: 0xB2] **********************************************************************************
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (设为已读取过的记录索引号为5)
            //int recordIndexGot = 0x5;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);

            ////12	标识(防止误设置)	1	0x55 [固定]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    if (pkt.recv[8] == 1)
            //    {
            //        log("1.8 设置已读取过的记录索引号	 成功...");
            //        success = 1;
            //    }
            //}

            ////1.9	获取已读取过的记录索引号[功能号: 0xB4] **********************************************************************************
            //pkt.Reset();
            //pkt.functionID = 0xB4;
            //int recordIndexGotToRead = 0x0;
            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
            //    log("1.9 获取已读取过的记录索引号	 成功...");
            //    success = 1;
            //}

            ////1.8	设置已读取过的记录索引号[功能号: 0xB2] **********************************************************************************
            ////恢复已提取过的记录, 为1.9的完整提取操作作准备-- 实际使用中, 在出现问题时才恢复, 正常不用恢复...
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (设为已读取过的记录索引号为5)
            //int recordIndexGot = 0x0;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);
            ////12	标识(防止误设置)	1	0x55 [固定]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    if (pkt.recv[8] == 1)
            //    {
            //        log("1.8 设置已读取过的记录索引号	 成功...");
            //        success = 1;
            //    }
            //}


            //1.9	提取记录操作
            //1. 通过 0xB4指令 获取已读取过的记录索引号 recordIndex
            //2. 通过 0xB0指令 获取指定索引号的记录  从recordIndex + 1开始提取记录， 直到记录为空为止
            //3. 通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
            //经过上面三个步骤， 整个提取记录的操作完成
            log("1.9 提取记录操作	 开始...");
            pkt.Reset();
            pkt.functionID = 0xB4;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                long recordIndexGotToRead = 0x0;
                recordIndexGotToRead = (long)byteToLong(pkt.recv, 8, 4);
                pkt.Reset();
                pkt.functionID = 0xB0;
                pkt.iDevSn = controllerSN;
                long recordIndexToGetStart = recordIndexGotToRead + 1;
                long recordIndexValidGet = 0;
                int cnt = 0;
                do
                {
                    if (bStopBasicFunction)
                    {
                        return 0;  //2015-06-10 09:08:14 停止
                    }
                    LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
                    ret = pkt.run();
                    success = 0;
                    if (ret > 0)
                    {
                        success = 1;

                        //12	记录类型
                        //0=无记录
                        //1=刷卡记录
                        //2=门磁,按钮, 设备启动, 远程开门记录
                        //3=报警记录	1	
                        //0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
                        int recordType = pkt.recv[12];
                        if (recordType == 0)
                        {
                            break; //没有更多记录
                        }
                        if (recordType == 0xff)//此索引号无效  重新设置索引值
                        {
                            //取最早一条记录的索引位
                            pkt.Reset();
                            pkt.functionID = 0xB0;
                            recordIndexToGet = 0;
                            LongToBytes(ref pkt.data, 0, recordIndexToGet);

                            ret = pkt.run();
                            success = 0;
                            if (ret > 0)
                            {
                                log("1.7 获取最早一条记录的信息	 成功...");
                                recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
                                recordIndexToGetStart = recordIndexGotToRead;
                                continue;
                            }
                            success = 0;  
                            break;
                        }
                        recordIndexValidGet = recordIndexToGetStart;

                        displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                        //.......对收到的记录作存储处理
                        //*****
                        //###############
                    }
                    else
                    {
                        //提取失败
                        break;
                    }
                    recordIndexToGetStart++;
                } while (cnt++ < 200000);
                if (success > 0)
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

            //1.10	远程开门[功能号: 0x40] **********************************************************************************
            int doorNO = 1;
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNO & 0xff); //2013-11-03 20:56:33
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //有效开门.....
                    log("1.10 远程开门	 成功...");
                    success = 1;
                }
            }

            //1.11	权限添加或修改[功能号: 0x50] **********************************************************************************
            //增加卡号0D D7 37 00, 通过当前控制器的所有门
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 要添加或修改的权限中的卡号 = 0x0037D70D = 3659533 (十进制)
            long cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

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

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
                    log("1.11 权限添加或修改	 成功...");
                    success = 1;
                }
            }

            //1.12	权限删除(单个删除)[功能号: 0x52] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x52;
            pkt.iDevSn = controllerSN;
            //要删除的权限卡号0D D7 37 00  = 0x0037D70D = 3659533 (十进制)
            long cardNOOfPrivilegeToDelete = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeToDelete);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器不会动作.
                    log("1.12 权限删除(单个删除)	 成功...");
                    success = 1;
                }
            }

            //1.13	权限清空(全部清掉)[功能号: 0x54] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x54;
            pkt.iDevSn = controllerSN;
            LongToBytes(ref pkt.data, 0, WGPacketShort.SpecialFlag);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //这时清空成功
                    log("1.13 权限清空(全部清掉)	 成功...");
                    success = 1;
                }
            }

            //1.14	权限总数读取[功能号: 0x58] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x58;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                int privilegeCount = 0;
                privilegeCount = (int)byteToLong(pkt.recv, 8, 4);
                log("1.14 权限总数读取	 成功...");

                success = 1;
            }


            //再次添加为查询操作 1.11	权限添加或修改[功能号: 0x50] **********************************************************************************
            //增加卡号0D D7 37 00, 通过当前控制器的所有门
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 要添加或修改的权限中的卡号 = 0x0037D70D = 3659533 (十进制)
            cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);
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

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
                    log("1.11 权限添加或修改	 成功...");
                    success = 1;
                }
            }

            //1.15	权限查询[功能号: 0x5A] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x5A;
            pkt.iDevSn = controllerSN;
            // (查卡号为 0D D7 37 00的权限)
            long cardNOOfPrivilegeToQuery = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeToQuery);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                long cardNOOfPrivilegeToGet = 0;
                cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4);
                if (cardNOOfPrivilegeToGet == 0)
                {
                    //没有权限时: (卡号部分为0)
                    log("1.15      没有权限信息: (卡号部分为0)");
                }
                else
                {
                    //具体权限信息...
                    log("1.15     有权限信息...");
                }
                log("1.15 权限查询	 成功...");
                success = 1;
            }

            //1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long QueryIndex = 1; //索引号(从1开始);
            LongToBytes(ref pkt.data, 0, QueryIndex);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                long cardNOOfPrivilegeToGet = 0;
                cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4);
                if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFF对应于4294967295
                {
                    log("1.16      没有权限信息: (权限已删除)");
                }
                else if (cardNOOfPrivilegeToGet == 0)
                {
                    //没有权限时: (卡号部分为0)
                    log("1.16       没有权限信息: (卡号部分为0)--此索引号之后没有权限了");
                }
                else
                {
                    //具体权限信息...
                    log("1.16      有权限信息...");
                }
                log("1.16 获取指定索引号的权限	 成功...");
                success = 1;
            }


            //1.17	设置门控制参数(在线/延时) [功能号: 0x80] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x80;
            //(设置2号门 在线  开门延时 3秒)
            pkt.data[0] = 0x02; //2号门
            pkt.data[1] = 0x03; //在线
            pkt.data[2] = 0x03; //开门延时

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.data[0] == pkt.recv[8] && pkt.data[1] == pkt.recv[9] && pkt.data[2] == pkt.recv[10])
                {
                    //成功时, 返回值与设置一致
                    log("1.17 设置门控制参数	 成功...");
                    success = 1;
                }
                else
                {
                    //失败
                }
            }

            //1.21	权限按从小到大顺序添加[功能号: 0x56] 适用于权限数过1000, 少于8万 **********************************************************************************
            //此功能实现 完全更新全部权限, 用户不用清空之前的权限. 只是将上传的权限顺序从第1个依次到最后一个上传完成. 如果中途中断的话, 仍以原权限为主
            //建议权限数更新超过50个, 即可使用此指令

            log("1.21	权限按从小到大顺序添加[功能号: 0x56]	开始...");
            log("       1万条权限...");

            //以10000个卡号为例, 此处简化的排序, 直接是以50001开始的10000个卡. 用户按照需要将要上传的卡号排序存放
            int cardCount = 10000;  //2015-06-09 20:20:20 卡总数量
            long[] cardArray = new long[cardCount];
            for (int i = 0; i < cardCount; i++)
            {
                cardArray[i] = 50001+i;
            }

            for (int i = 0; i < cardCount; i++)
            {
                if (bStopBasicFunction)
                {
                    return 0;  //2015-06-10 09:08:14 停止
                }
                pkt.Reset();
                pkt.functionID = 0x56;

                cardNOOfPrivilege = cardArray[i];
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

                LongToBytes(ref pkt.data, 32-8, cardCount); //总的权限数
                LongToBytes(ref pkt.data, 35-8, i+1);//当前权限的索引位(从1开始)

                ret = pkt.run();
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
           

            //其他指令  **********************************************************************************


            // **********************************************************************************

            //结束  **********************************************************************************
            pkt.close();  //关闭通信
            return success;
        }

        /// <summary>
        /// 接收服务器设置测试
        /// </summary>
        /// <param name="ControllerIP">被设置的控制器IP地址</param>
        /// <param name="controllerSN">被设置的控制器序列号</param>
        /// <param name="watchServerIP">要设置的服务器IP</param>
        /// <param name="watchServerPort">要设置的端口</param>
        /// <returns>0 失败, 1表示成功</returns>
        int testWatchingServer(string ControllerIP, long controllerSN, string watchServerIP, int watchServerPort)  //接收服务器测试 -- 设置
        {
            int ret = 0;
            int success = 0;  //0 失败, 1表示成功

            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.18	设置接收服务器的IP和端口 [功能号: 0x90] **********************************************************************************
            //(如果不想让控制器发出数据, 只要将接收服务器的IP设为0.0.0.0 就行了)
            //接收服务器的端口: 61005
            //每隔5秒发送一次: 05
            pkt.Reset();
            pkt.functionID = 0x90;
            string[] strIP = watchServerIP.Split('.');
            if (strIP.Length == 4)
            {
                pkt.data[0] = byte.Parse(strIP[0]); 
                pkt.data[1] = byte.Parse(strIP[1]);
                pkt.data[2] = byte.Parse(strIP[2]);  
                pkt.data[3] = byte.Parse(strIP[3]);
            }
            else
            {
                return 0;
            }

            //接收服务器的端口: 61005
            pkt.data[4] = (byte)((watchServerPort & 0xff));
            pkt.data[5] = (byte)((watchServerPort >> 8) & 0xff);

            //每隔5秒发送一次: 05 (定时上传信息的周期为5秒 [正常运行时每隔5秒发送一次  有刷卡时立即发送])
            pkt.data[6] = 5;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    log("1.18 设置接收服务器的IP和端口 	 成功...");
                    success = 1;
                }
            }


            //1.19	读取接收服务器的IP和端口 [功能号: 0x92] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x92;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.19 读取接收服务器的IP和端口 	 成功...");
                success = 1;
            }
            pkt.close();
            return success;
        }


        /// <summary>
        /// 打开接收服务器接收数据 (注意防火墙 要允许此端口的所有包进入才行)
        /// </summary>
        /// <param name="watchServerIP">接收服务器IP(一般是当前电脑IP)</param>
        /// <param name="watchServerPort">接收服务器端口</param>
        /// <returns>1 表示成功,否则失败</returns>
        int WatchingServerRuning(string watchServerIP, int watchServerPort)
        {
            //注意防火墙 要允许此端口的所有包进入才行
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort);

                if (!udpserver.IsWatching())
                {
                    log("进入接收服务器监控状态....失败");
                    return -1;
                }
                log("进入接收服务器监控状态....");
                long recordIndex = 0;
                int recv_cnt;
                while (!bStopWatchServer)
                {
                    recv_cnt = udpserver.receivedCount();
                    if (recv_cnt > 0)
                    {
                        byte[] buff = udpserver.getRecords();
                        if (buff[1] == 0x20) //
                        {
                            long sn;
                            long recordIndexGet;
                            sn = byteToLong(buff, 4, 4);
                            log(string.Format("接收到来自控制器SN = {0} 的数据包..\r\n", sn));

                            recordIndexGet = byteToLong(buff, 8, 4);

                            if (recordIndex < recordIndexGet)
                            {
                                recordIndex = recordIndexGet;
                                
                                displayRecordInformation(buff); //2015-06-09 20:01:21

                             }
                        }

                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);  //'延时10ms
                        Application.DoEvents();

                    }
                }
                udpserver.Close();
                return 1;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());
                // throw;
            }
            return 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }



    }
}
