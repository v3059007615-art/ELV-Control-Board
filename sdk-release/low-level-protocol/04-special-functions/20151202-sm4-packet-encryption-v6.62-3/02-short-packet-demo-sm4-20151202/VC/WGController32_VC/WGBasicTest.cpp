/**
* $Id: WGBasicTest.cpp 2015-04-29 22:20:40 karl CSN 陈绍宁 $
*
* 门禁控制器 短报文协议 测试案例
* V1.1 版本  2013-11-05 15:02:02  
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
* V1.2 2013-11-07 12:40:49
*                 修改日期设置时的验证
*                 将数据包长度, Type, 控制器端口, 特殊标识固定化在WGPacketShort中
* V2.5 2015-04-29 20:41:30 采用 V6.56驱动版本 型号由0x19改为0x17
* V2.6 版本  2015-11-03 20:25:53 V6.60驱动版本 增加 通信密码测试, 1024字节用于权限上传和记录提取操作  
*                               修改通信的重试操作
* V2.7 版本  2015-12-05 17:18:00 V6.62驱动版本 增加 通信SM4通信密码测试
*                               
*/

#include "ace/INET_Addr.h"
#include "ace/SOCK_Dgram.h"
#include "ace/Time_Value.h"
#include "ace/SOCK_CODgram.h"
#include "ace/SOCK_Dgram_Bcast.h"
#include <time.h>   
#include <stdio.h>

#include "n3kWGCom.h"  

class WGPacketShort {				//短报文协议
public:
	const static unsigned int	 WGPacketSize = 64;			    //报文长度
	//2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//类型
	const static unsigned char	 Type = 0x17;		//2015-04-29 22:22:50			//类型
	const static unsigned int    ControllerPort = 60000;        //控制器端口
	const static unsigned int    SpecialFlag =0x55AAAA55;       //特殊标识 防止误操作

	unsigned char	 functionID;		    //功能号
	unsigned int	 iDevSn;                //设备序列号 4字节
	unsigned char    data[56];              //56字节的数据 [含流水号]

	unsigned char    recv[WGPacketSize];    //接收到的数据
	unsigned char    recv1024[1024];    //接收到的数据1024
	WGPacketShort(void)
	{
		Reset();
	}
	void Reset()  //数据复位
	{
		memset(data,0,sizeof(data));
	}
	void toByte(char* buff, size_t buflen) //生成64字节指令包
	{
		if (buflen == WGPacketSize)
		{
			memset(buff,0,sizeof(buff));
			buff[0] = Type;
			buff[1] = functionID;
			memcpy(&(buff[4]),&(iDevSn), 4);
			memcpy(&(buff[8]),data,sizeof(data));
			sequenceId++;
			memcpy(&(buff[40]),&(sequenceId), 4);
		}
	}
	int run(ACE_SOCK_CODgram udp)  //通过指定的UDP发送指令 接收返回信息
	{
		unsigned char buff[WGPacketSize];
		int errcnt =0;
		WGPacketShort::sequenceId++;
		memset(buff,0,sizeof(buff));
		buff[0] = Type;
		buff[1] = functionID;
		memcpy(&(buff[4]),&(iDevSn), 4);
		memcpy(&(buff[8]),data,sizeof(data));
		unsigned int currentSequenceId = WGPacketShort::sequenceId;
		memcpy(&(buff[40]),&(currentSequenceId), 4);
		int tries =3;
		do 
		{
			if (-1 == udp.send (buff, WGPacketSize))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv, WGPacketSize, &recvTimeout); 
				if (recv_cnt == WGPacketSize)
				{
					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //类型一致
						&& (recv[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

	int runWithPassword_SM4_ECB(ACE_SOCK_CODgram udp,unsigned char* password)  //2015-12-04 15:24:29 采用ECB方案
	{
		unsigned char buff[WGPacketSize];
		int errcnt =0;
		WGPacketShort::sequenceId++;
		memset(buff,0,sizeof(buff));
		buff[0] = Type;
		buff[1] = functionID;
		memcpy(&(buff[4]),&(iDevSn), 4);
		memcpy(&(buff[8]),data,sizeof(data));
		unsigned int currentSequenceId = WGPacketShort::sequenceId;
		memcpy(&(buff[40]),&(currentSequenceId), 4);
		ShortEncryptSM4_ECB((char*)buff,64, (char*)password);  //2015-11-02 15:40:59 加密
		int tries =3;
		do 
		{
			if (-1 == udp.send (buff, WGPacketSize))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv, WGPacketSize, &recvTimeout); 
				if (recv_cnt == WGPacketSize)
				{
					unsigned char buffbak[WGPacketSize];
					memcpy(&(buffbak[0]),recv, WGPacketSize);
					ShortDecryptSM4_ECB((char*)recv,64, (char*)password);  //2015-11-02 15:40:59 解密
					if (buffbak[0] == Type)  //2015-12-04 15:35:11 如果接收到是未加密的指令处理
					{
						if (recv[0] != Type) //2015-12-04 15:35:29解密是无效的指令
						{
							memcpy(&(recv[0]),buffbak, WGPacketSize); //恢复数据
						}
					}

					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //类型一致
						&& (recv[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

	int runWithPassword_SM4_CBC(ACE_SOCK_CODgram udp,unsigned char* password,unsigned char* IV)  //2015-12-04 15:24:29 采用CBC方案
	{
		unsigned char buff[WGPacketSize];
		int errcnt =0;
		WGPacketShort::sequenceId++;
		memset(buff,0,sizeof(buff));
		buff[0] = Type;
		buff[1] = functionID;
		memcpy(&(buff[4]),&(iDevSn), 4);
		memcpy(&(buff[8]),data,sizeof(data));
		unsigned int currentSequenceId = WGPacketShort::sequenceId;
		memcpy(&(buff[40]),&(currentSequenceId), 4);
		ShortEncryptSM4_CBC((char*)buff,64, (char*)password,(char*)IV);  //2015-11-02 15:40:59 加密
		int tries =3;
		do 
		{
			if (-1 == udp.send (buff, WGPacketSize))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv, WGPacketSize, &recvTimeout); 
				if (recv_cnt == WGPacketSize)
				{
					unsigned char buffbak[WGPacketSize];
					memcpy(&(buffbak[0]),recv, WGPacketSize);
					ShortDecryptSM4_CBC((char*)recv,64, (char*)password,(char*)IV);  //2015-11-02 15:40:59 解密
					if (buffbak[0] == Type)  //2015-12-04 15:35:11 如果接收到是未加密的指令处理
					{
						if (recv[0] != Type) //2015-12-04 15:35:29解密是无效的指令
						{
							memcpy(&(recv[0]),buffbak, WGPacketSize); //恢复数据
						}
					}

					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //类型一致
						&& (recv[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

	static unsigned int sequenceIdSent()// 最后发出流水号
	{
		return sequenceId; // 最后发出流水号
	}

	int run1024(ACE_SOCK_CODgram udp,unsigned char* buff1024)  //1024字节 通过指定的UDP发送指令 接收返回信息
	{
		unsigned int  currentSequenceId=0;
		memcpy(&currentSequenceId, &(buff1024[40]),4);

		int errcnt =0;

		int tries =3;
		do 
		{
			if (-1 == udp.send (buff1024, 1024))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv1024, 1024, &recvTimeout); 
				if (recv_cnt == 1024)
				{
					memcpy(&(recv[0]),&(recv1024[0]),WGPacketSize);
					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //类型一致
						&& (recv[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

	int runWithPassword1024_SM4_ECB(ACE_SOCK_CODgram udp,unsigned char* buff1024,unsigned char* password)  //1024字节 2015-11-02 15:39:39 通信密码 通过指定的UDP发送指令 接收返回信息
	{
		unsigned int  currentSequenceId=0;
		memcpy(&currentSequenceId, &(buff1024[40]),4);

		//	unsigned char buff[1024];
		int errcnt =0;

		ShortEncryptSM4_ECB((char*)buff1024,1024, (char*)password);  //2015-11-02 15:40:59 加密
		int tries =3;
		do 
		{
			if (-1 == udp.send (buff1024, 1024))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv1024, 1024, &recvTimeout); 
				if (recv_cnt == 1024)
				{
					ShortDecryptSM4_ECB((char*)recv1024,1024, (char*)password);
					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv1024[40]),4);

					if ((recv1024[0]== Type) //类型一致
						&& (recv1024[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

	int runWithPassword1024_SM4_CBC(ACE_SOCK_CODgram udp,unsigned char* buff1024,unsigned char* password,unsigned char* IV)  //1024字节 2015-11-02 15:39:39 通信密码 通过指定的UDP发送指令 接收返回信息
	{
		unsigned int  currentSequenceId=0;
		memcpy(&currentSequenceId, &(buff1024[40]),4);

		//	unsigned char buff[1024];
		volatile int errcnt =0;

		ShortEncryptSM4_CBC((char*)buff1024,1024, (char*)password, (char*)IV);  //2015-11-02 15:40:59 加密
		int tries =3;
		do 
		{
			if (-1 == udp.send (buff1024, 1024))
			{
				return -1;
			}
			else 
			{
				ACE_INET_Addr your_addr;
				ACE_Time_Value recvTimeout(0, 400*1000);
				size_t recv_cnt = udp.recv(recv1024, 1024, &recvTimeout); 
				if (recv_cnt == 1024)
				{
					ShortDecryptSM4_CBC((char*)recv1024,1024, (char*)password,(char*)IV);
					//流水号
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv1024[40]),4);

					if ((recv1024[0]== Type) //类型一致
						&& (recv1024[1]== functionID) //功能号一致
						&& (sequenceIdReceived == currentSequenceId) )  //序列号对应
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //重试三次

		return -1;
	}

private:
	static  unsigned int     sequenceId;     //序列号	
};
unsigned int WGPacketShort::sequenceId = 0;  //流水号值

unsigned char GetHex(int val) //获取Hex值, 主要用于日期时间格式
{
	return ((val % 10) + (((val -(val % 10)) / 10)%10) *16);
}
void log(char* info)  //日志信息
{
	//stdout<< info;
	//Console.WriteLine(info);
	//printf("%s\r\n", info);
	time_t nowtime;     
	struct tm* ptm;     
	time(&nowtime);     
	ptm = localtime(&nowtime);     
	printf("%02d:%02d:%02d %s\r\n", ptm->tm_hour, ptm->tm_min, ptm->tm_sec, info);
}
int testBasicFunction_SM4_ECB(char *ControllerIP, unsigned int controllerSN);  //ECB 基本功能测试
int testBasicFunction_SM4_CBC(char *ControllerIP, unsigned int controllerSN);  //CBC 基本功能测试
int testWatchingServer(char *ControllerIP, unsigned int controllerSN, char *watchServerIP,int watchServerPort);  //接收服务器测试
int WatchingServerRuning (char *watchServerIP,int watchServerPort);   //2013-11-05 13:08:39 进入服务器监控状态

#include<iostream>
using namespace std;


//本案例未作搜索控制器  及 设置IP的工作  (直接由IP设置工具来完成)
//本案例中测试说明
//控制器SN  = 422101164
//控制器IP  = 192.168.168.123
//电脑  IP  = 192.168.168.101
//用于作为接收服务器的IP (本电脑IP 192.168.168.101), 接收服务器端口 (61005)

int ACE_TMAIN (int, ACE_TCHAR *[]) //主程序口
{
	int ret =0;

	int sn =0;
	char ip[32];

	cout<<("请输入控制器SN(9位数):  ");
	cin>>sn;;

	//log("请输入控制器IP:");
	cout<< ("请输入控制器IP:  ");
	cin >>ip;


	//	
		log("...SM4_ECB通信加密测试 开始 .....................");
	ret = testBasicFunction_SM4_ECB(ip,sn); //基本功能测试
		log("...SM4_ECB通信加密测试结束.....................\r\n\r\n");

		log("...SM4_CBC 通信加密测试 开始 .....................\r\n");
	ret = testBasicFunction_SM4_CBC(ip,sn); //基本功能测试
		log("...SM4_CBC  通信加密测试结束.....................\r\n");

	//if (ret !=1)
	//{
	////	log("基本功能测试失败???, 按 X 回车键退出...");
	//	char stop;
	//	cin >>stop;
	//	return 0;
	//}

	log("测试结束, 按 X 回车键退出...");
	char stop2;
	cin >>stop2;
	return 1;
}

char* RecordDetails[] =
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
char* getReasonDetailChinese(int Reason) //中文
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

char* getReasonDetailEnglish(int Reason) //英文描述
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
/// 显示记录信息
/// </summary>
/// <param name="recv"></param>
void displayRecordInformation(unsigned char* recv)
{
	//	  	最后一条记录的信息		
	//8-11	最后一条记录的索引号
	//(=0表示没有记录)	4	0x00000000
	int recordIndex =0;
	memcpy(&recordIndex, &(recv[8]),4);

	//12	记录类型
	//0=无记录
	//1=刷卡记录
	//2=门磁,按钮, 设备启动, 远程开门记录
	//3=报警记录	1	
	int recordType = recv[12];

	//13	有效性(0 表示不通过, 1表示通过)	1	
	int recordValid = recv[13];

	//14	门号(1,2,3,4)	1	
	int recordDoorNO = recv[14];

	//15	进门/出门(1表示进门, 2表示出门)	1	0x01
	int recordInOrOut = recv[15];

	//16-19	卡号(类型是刷卡记录时)
	//或编号(其他类型记录)	4	
	long long recordCardNO = 0;
	memcpy(&recordCardNO, &(recv[16]),4);

	//20-26	刷卡时间:
	//年月日时分秒 (采用BCD码)见设置时间部分的说明
	char recordTime[]="2000-01-01 00:00:00";
	sprintf(recordTime,"%02X%02X-%02X-%02X %02X:%02X:%02X", 
		recv[20],recv[21],recv[22],recv[23],recv[24],recv[25],recv[26]);

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
		printf("索引位=%u  无记录\r\n", recordIndex);
	}
	else if (recordType == 0xff)
	{
		log(" 指定索引位的记录已被覆盖掉了,请使用索引0, 取回最早一条记录的索引值");
	}
	else if (recordType == 1) //2015-06-10 08:49:31 显示记录类型为卡号的数据
	{
		//卡号
		printf("索引位=%u\r\n", recordIndex);
		printf("  卡号 = %u\r\n", recordCardNO);
		printf("  门号 = %u\r\n", recordDoorNO);
		printf("  进出 = %s\r\n", recordInOrOut == 1 ? "进门" : "出门");
		printf("  有效 = %s\r\n", recordValid == 1 ? "通过" : "禁止");
		printf("  时间 = %s\r\n", recordTime);
		printf("  描述 = %s\r\n", getReasonDetailChinese(reason));
	}
	else if (recordType == 2)
	{
		//其他处理
		//门磁,按钮, 设备启动, 远程开门记录
		printf("索引位=%u  非刷卡记录\r\n ", recordIndex);
		printf("  编号 = %u\r\n", recordCardNO);
		printf("  门号 = %u\r\n", recordDoorNO);
		printf("  时间 = %s\r\n", recordTime);
		printf("  描述 = %s\r\n", getReasonDetailChinese(reason));
	}
	else if (recordType == 3)
	{
		//其他处理
		//报警记录

		printf("索引位=%u  报警记录\r\n ", recordIndex);
		printf("  编号 = %u\r\n", recordCardNO);
		printf("  门号 = %u\r\n", recordDoorNO);
		printf("  时间 = %s\r\n", recordTime);
		printf("  描述 = %s\r\n", getReasonDetailChinese(reason));
	}
}




//ControllerIP 控制器IP地址
//controllerSN 控制器序列号
int testBasicFunction_SM4_ECB(char *ControllerIP, unsigned int controllerSN)  //基本功能测试
{
	int ret =0;
	int success =0;  //0 失败, 1表示成功

	ACE_INET_Addr controller_addr (WGPacketShort::ControllerPort, ControllerIP); //端口  IP地址
	ACE_SOCK_CODgram udp;
	if (0 != udp.open (controller_addr))
	{
		//请输入有效IP
		log("请输入有效IP...");
		return -1;
	}

	//创建短报文 pkt
	WGPacketShort pkt;  

	//SM4 ECB 设置通信密码[功能号: 0xE0] **********************************************************************************
	log("SM4 ECB 通信测试 ...");
	pkt.Reset();
	pkt.functionID = 0xE0;
	pkt.iDevSn = controllerSN; 
	unsigned  char commPassword[] = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16字节密码
	//防止误操作标识
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);

	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00设置新密码
	{
		pkt.data[4 + i] = commPassword[i];
		pkt.data[36 + i] = commPassword[i];
	}

	//分两种情况: 密码为空  或者 已设置过密码
	ret = pkt.run(udp);  //2015-11-02 10:21:22 先尝试控制器没有密码的操作
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
		ret = pkt.runWithPassword_SM4_ECB(udp,commPassword);  //2015-11-02 10:21:22 再尝试控制器已有密码的操作
		if ((ret > 0) && (pkt.recv[8] == 1))
		{
			log("通信密码设置成功...[通过加密通信操作]");
			success = 1;
		}
		else
		{
			log("通信密码设置失败???...[通过加密通信操作]");
		}
	}


	//1.10	远程开门[功能号: 0x40] **********************************************************************************
	int doorNO =1;
	pkt.Reset();
	pkt.functionID = 0x40;
	pkt.iDevSn = controllerSN; 
	pkt.data[0] = (doorNO & 0xff); //2013-11-03 20:56:33
	ret = pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	success =0;
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
			log("1.10 远程开门	 失败???...[通过加密通信操作]");
		}
	}
	else
	{
		log("1.10 远程开门	 失败???...[通过加密通信操作]");
	}


	////SM4 清空通信密码[功能号: 0xE0] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xE0;
	////防止误操作标识
	//memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	//for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 清空密码
	//{
	//	pkt.data[4 + i] = 0;
	//	pkt.data[36 + i] = 0;
	//}

	//ret = pkt.runWithPassword_SM4_ECB(udp,commPassword);  //
	//if ((ret > 0) && (pkt.recv[8] == 1))
	//{
	//	log("通信密码清空成功...[通过加密通信操作]");
	//	success = 1;
	//}
	//else
	//{
	//	log("通信密码清空失败???...[通过加密通信操作]");
	//}
	//// **********************************************************************************
	//return success;

	log("开始采用 1024字节指令操作");

	unsigned char command1024[1024];
	unsigned char sendBuff[64];
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

	//. 发出报文 (取最早的一条记录 通过索引号 0x00000000) [此指令适合于 刷卡记录超过20万时环境下使用]
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	success =0;
	if (ret >0)
	{
		memcpy(&(firstRecordIndex), &(pkt.recv[8]), 4);
		printf("获取最早一条记录索引 = %u  \r\n", firstRecordIndex);
		success =1;
	}

	//发出报文 (取最新的一条记录 通过索引 0xffffffff)
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0xffffffff;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	success =0;
	if (ret >0)
	{
		memcpy(&(lastRecordIndex), &(pkt.recv[8]), 4);
		printf("获取最后一条记录索引 = %u  \r\n", lastRecordIndex);
		success =1;
	}

	//1.9	获取已读取过的记录索引号[功能号: 0xB4] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xB4;
	pkt.iDevSn = controllerSN; 
	recordIndexGotToRead =0x0;
	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	success =0;
	if (ret >0)
	{
		memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
		printf("获取已读取过的记录索引号 = %u  \r\n", recordIndexGotToRead);
		success =1;
	}

	long validRecordsCount = 0;
	//	recordIndexGotToRead = 0;  //2015-11-05 21:31:05 强制取所有记录
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
				memcpy(&(pkt.data[0]), &recordIndexToGetStart,4);
				pkt.toByte((char*)&(sendBuff[0]),64);
				memcpy(&(command1024[j]), &(sendBuff[0]),64);
				recordIndexToGetStart++;
				cnt++;
			}
			ret = pkt.runWithPassword1024_SM4_ECB(udp,command1024,commPassword); //SM4_ECB
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
					unsigned char recv[64];
					memcpy(recv, &(pkt.recv1024[j]), 64);
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

		printf("1.9 完全提取成功	 ... 有效记录数= %u\r\n" , validRecordsCount);
		if ((success > 0) && validRecordsCount>0)
		{
			//通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
			pkt.Reset();
			pkt.functionID = 0xB2;
			memcpy(&( pkt.data[0]),  &recordIndexValidGet,4);

			//12	标识(防止误设置)	1	0x55 [固定]
			memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

			ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
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
	printf("       %u万条权限...\r\n", cardCount / 10000);
	long cardArray[20*10000];
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
			memcpy(& (pkt.data[0]), &cardNOOfPrivilege,4);

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

			memcpy(& (pkt.data[32-8]), &cardCount,4); //总的权限数
			long itmp =i + 1;
			memcpy(& (pkt.data[35-8]), &itmp,4); //当前权限的索引位(从1开始)
			pkt.toByte((char*)&(sendBuff[0]),64);
			memcpy(&(command1024[j]), &(sendBuff[0]),64);
			i++;

		}
		ret =  pkt.runWithPassword1024_SM4_ECB(udp,command1024,commPassword); //SM4_ECB
		success = 0;
		if (ret > 0)
		{
			if (pkt.recv1024[8] == 1)
			{
				success = 1;
			}
			if (pkt.recv1024[8] == 0xE1)
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
		log("1.21	权限按从小到大顺序添加[功能号: 0x56]	 失败???...????");
	}


	//1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
	//读取所有权限
	pkt.Reset();
	pkt.functionID = 0x5C;
	pkt.iDevSn = controllerSN;
	long maxCount = 20 * 10000;
	//long cardArrayGet[20*10000];
	long *cardArrayGet = cardArray;
	long QueryIndex = 1; //索引号(从1开始);
	memcpy(&(pkt.data[0]),&QueryIndex, 4);
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
			pkt.recv1024[j] = 0; //2015-12-04 22:46:27 复位
		}
		for (int j = 0; j < 1024; j = j + 64)
		{
			memcpy(&(pkt.data[0]),&QueryIndex, 4);
			QueryIndex++; //索引号(从1开始);
			pkt.toByte((char*)&(sendBuff[0]),64);
			memcpy(&(command1024[j]), &(sendBuff[0]),64);

		}
		ret =  pkt.runWithPassword1024_SM4_ECB(udp,command1024,commPassword); //SM4_ECB
		success = 0;
		if (ret > 0)
		{
			long cardNOOfPrivilegeToGet = 0; //2015-12-04 22:55:07 
			for (int j = 0; j < 1024; j = j + 64)
			{
				success = 0;
				cardNOOfPrivilegeToGet = 0;
				memcpy(&cardNOOfPrivilegeToGet, &(pkt.recv1024[8+j]), 4);
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
					success = 1;
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
			if (cardNOOfPrivilegeToGet == 0)
			{
				break; //没有权限时: (卡号部分为0)
			}
		}
		else
		{
			printf("1.16     有问题 ret = %u\r\n" ,  ret);  
			break;
		}
	}
	printf("最后读取到的权限的卡号 = %u\r\n" , cardNOOfPrivilegeToGetlast);
	printf("提取到的权限数iCount = %u\r\n" ,  iCount);  //2015-11-04 19:59:50 提取权限数
	log("提取权限 结束");  //2015-11-04 19:59:50 提取权限数

	//SM4 ECB 清空通信密码[功能号: 0xE0] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xE0;
	//防止误操作标识
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 清空密码
	{
		pkt.data[4 + i] = 0;
		pkt.data[36 + i] = 0;
	}

	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	if ((ret > 0) && (pkt.recv[8] == 1))
	{
		log("通信密码清空成功...[通过加密通信操作]");
		success = 1;
	}
	else
	{
		log("通信密码清空失败???...[通过加密通信操作]");
	}
	// **********************************************************************************

	//结束  **********************************************************************************
	udp.close();
	return success;
}


int testBasicFunction_SM4_CBC(char *ControllerIP, unsigned int controllerSN)  //基本功能测试
{
	int ret =0;
	int success =0;  //0 失败, 1表示成功

	ACE_INET_Addr controller_addr (WGPacketShort::ControllerPort, ControllerIP); //端口  IP地址
	ACE_SOCK_CODgram udp;
	if (0 != udp.open (controller_addr))
	{
		//请输入有效IP
		log("请输入有效IP...");
		return -1;
	}

	//创建短报文 pkt
	WGPacketShort pkt;  

	//SM4 CBC 设置通信密码[功能号: 0xE2] **********************************************************************************
	log("SM4 CBC 通信测试 ...");
	pkt.Reset();
	pkt.functionID = 0xE2;
	pkt.iDevSn = controllerSN; 
	unsigned  char commPassword[] = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16字节密码
	unsigned  char  IV[] = { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }; //16字节 初始变量

	//防止误操作标识
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);

	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00设置新密码
	{
		pkt.data[4 + i] = commPassword[i];
		pkt.data[36 + i] = commPassword[i];
	}
	 for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00设置IV
    {
        if (i < 12)
        {
            pkt.data[20 + i] = IV[i];  //byte28-39
        }
        else
        {
            pkt.data[20 + 20 + i] = IV[i]; //byte60-63
        }
    }

	//分两种情况: 密码为空  或者 已设置过密码
	ret = pkt.run(udp);  //2015-11-02 10:21:22 先尝试控制器没有密码的操作
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
		ret = pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //2015-11-02 10:21:22 再尝试控制器已有密码的操作
		if ((ret > 0) && (pkt.recv[8] == 1))
		{
			log("通信密码设置成功...[通过加密通信操作]");
			success = 1;
		}
		else
		{
			log("通信密码设置失败???...[通过加密通信操作]");
		}
	}


	//1.10	远程开门[功能号: 0x40] **********************************************************************************
	int doorNO =1;
	pkt.Reset();
	pkt.functionID = 0x40;
	pkt.iDevSn = controllerSN; 
	pkt.data[0] = (doorNO & 0xff); //2013-11-03 20:56:33
	ret = pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	success =0;
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
			log("1.10 远程开门	 失败???...[通过加密通信操作]");
		}
	}
	else
	{
		log("1.10 远程开门	 失败???...[通过加密通信操作]");
	}


	////SM4 清空通信密码[功能号: 0xE0] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xE0;
	////防止误操作标识
	//memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	//for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 清空密码
	//{
	//	pkt.data[4 + i] = 0;
	//	pkt.data[36 + i] = 0;
	//}

	//ret = pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //
	//if ((ret > 0) && (pkt.recv[8] == 1))
	//{
	//	log("通信密码清空成功...[通过加密通信操作]");
	//	success = 1;
	//}
	//else
	//{
	//	log("通信密码清空失败???...[通过加密通信操作]");
	//}
	//// **********************************************************************************
	//return success;

	log("开始采用 1024字节指令操作");

	unsigned char command1024[1024];
	unsigned char sendBuff[64];
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

	//. 发出报文 (取最早的一条记录 通过索引号 0x00000000) [此指令适合于 刷卡记录超过20万时环境下使用]
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	success =0;
	if (ret >0)
	{
		memcpy(&(firstRecordIndex), &(pkt.recv[8]), 4);
		printf("获取最早一条记录索引 = %u  \r\n", firstRecordIndex);
		success =1;
	}

	//发出报文 (取最新的一条记录 通过索引 0xffffffff)
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0xffffffff;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	success =0;
	if (ret >0)
	{
		memcpy(&(lastRecordIndex), &(pkt.recv[8]), 4);
		printf("获取最后一条记录索引 = %u  \r\n", lastRecordIndex);
		success =1;
	}

	//1.9	获取已读取过的记录索引号[功能号: 0xB4] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xB4;
	pkt.iDevSn = controllerSN; 
	recordIndexGotToRead =0x0;
	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	success =0;
	if (ret >0)
	{
		memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
		printf("获取已读取过的记录索引号 = %u  \r\n", recordIndexGotToRead);
		success =1;
	}

	long validRecordsCount = 0;
	//	recordIndexGotToRead = 0;  //2015-11-05 21:31:05 强制取所有记录
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
				memcpy(&(pkt.data[0]), &recordIndexToGetStart,4);
				pkt.toByte((char*)&(sendBuff[0]),64);
				memcpy(&(command1024[j]), &(sendBuff[0]),64);
				recordIndexToGetStart++;
				cnt++;
			}
			ret = pkt.runWithPassword1024_SM4_CBC(udp,command1024,commPassword, IV); //SM4_CBC
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
					unsigned char recv[64];
					memcpy(recv, &(pkt.recv1024[j]), 64);
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
						}
					}
					//.......对收到的记录作存储处理
					//*****
					//###############
				}
			}
			else
			{
				//提取失败???
				break;
			}
			if (success != 1)
			{
				break;
			}
		} while (cnt < 200000);

		printf("1.9 完全提取成功	 ... 有效记录数= %u\r\n" , validRecordsCount);
		if ((success > 0) && validRecordsCount>0)
		{
			//通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
			pkt.Reset();
			pkt.functionID = 0xB2;
			memcpy(&( pkt.data[0]),  &recordIndexValidGet,4);

			//12	标识(防止误设置)	1	0x55 [固定]
			memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

			ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
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
	printf("       %u万条权限...\r\n", cardCount / 10000);
	long cardArray[20*10000];
	for (int i = 0; i < cardCount; i++)
	{
		cardArray[i] = 50001 + i;
	}
	long cardNOOfPrivilegeToGetlast = 0;
	long cardNOOfPrivilege;

	for (int i = 0; i < cardCount ; )
	{
		for (int j = 0; j < 1024; j++)
		{
			command1024[j] = 0; //复位
		}
		if (i >= cardCount)
		{
			break;
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
			memcpy(& (pkt.data[0]), &cardNOOfPrivilege,4);

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

			memcpy(& (pkt.data[32-8]), &cardCount,4); //总的权限数
			long itmp =i + 1;
			memcpy(& (pkt.data[35-8]), &itmp,4); //当前权限的索引位(从1开始)
			pkt.toByte((char*)&(sendBuff[0]),64);
			memcpy(&(command1024[j]), &(sendBuff[0]),64);
			i++;

		}

		ret =  pkt.runWithPassword1024_SM4_CBC(udp,command1024,commPassword, IV); //SM4_CBC
		success = 0;
		if (ret > 0)
		{
			if (pkt.recv1024[8] == 1)
			{
				success = 1;
			}
			if (pkt.recv1024[8] == 0xE1)
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
		log("1.21	权限按从小到大顺序添加[功能号: 0x56]	 失败???...????");
	}


	//1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
	//读取所有权限
	pkt.Reset();
	pkt.functionID = 0x5C;
	pkt.iDevSn = controllerSN;
	long maxCount = 20 * 10000;
	//long cardArrayGet[20*10000];
	long *cardArrayGet = cardArray;
	long QueryIndex = 1; //索引号(从1开始);
	memcpy(&(pkt.data[0]),&QueryIndex, 4);
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
			pkt.recv1024[j] = 0; //2015-12-04 22:46:27 复位
		}
		for (int j = 0; j < 1024; j = j + 64)
		{
			memcpy(&(pkt.data[0]),&QueryIndex, 4);
			QueryIndex++; //索引号(从1开始);
			pkt.toByte((char*)&(sendBuff[0]),64);
			memcpy(&(command1024[j]), &(sendBuff[0]),64);

		}
		ret =  pkt.runWithPassword1024_SM4_CBC(udp,command1024,commPassword, IV); //SM4_CBC
		success = 0;
		if (ret > 0)
		{
			long cardNOOfPrivilegeToGet = 0; //2015-12-04 22:55:07 
			for (int j = 0; j < 1024; j = j + 64)
			{
				success = 0;
				cardNOOfPrivilegeToGet = 0;
				memcpy(&cardNOOfPrivilegeToGet, &(pkt.recv1024[8+j]), 4);
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
					success = 1;
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
			if (cardNOOfPrivilegeToGet == 0)
			{
				break; //没有权限时: (卡号部分为0)
			}
		}
		else
		{
			printf("1.16     有问题 ret = %u\r\n" ,  ret);  
			break;
		}
	}
	printf("最后读取到的权限的卡号 = %u\r\n" , cardNOOfPrivilegeToGetlast);
	printf("提取到的权限数iCount = %u\r\n" ,  iCount);  //2015-11-04 19:59:50 提取权限数
	log("提取权限 结束");  //2015-11-04 19:59:50 提取权限数

	//SM4 CBC 清空通信密码[功能号: 0xE2] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xE2;
	//防止误操作标识
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 清空密码
	{
		pkt.data[4 + i] = 0;
		pkt.data[36 + i] = 0;
	}

	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	if ((ret > 0) && (pkt.recv[8] == 1))
	{
		log("通信密码清空成功...[通过加密通信操作]");
		success = 1;
	}
	else
	{
		log("通信密码清空失败???...[通过加密通信操作]");
	}
	// **********************************************************************************

	//结束  **********************************************************************************
	udp.close();
	return success;
}
