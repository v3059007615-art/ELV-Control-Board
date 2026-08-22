/**
* $Id: WGBasicTest.cpp 2015-04-29 22:20:40 karl CSN Chan Shonin $
*
* Doorbar controller Shortcast agreement Test cases
* V1.1 Version  2013-11-05 15:02:02  
*            Basic functions:  Query controller status 
*                       Read Date Time
*                       Set Date Time
*                       Get a record of the given index number
*                       Set a read record index number
*                       Get read record index numbers
*                       Open remote
*                       Permissions to add or modify
*                       Permission to delete(Individual Delete)
*                       Clear Permissions(Clear it all.)
*                       Total Permissions Read
*                       Permission Query
*                       Set door control parameters(Online/Delay)
*                       Read door control parameters(Online/Delay)

*                       Set up the receiver serverIPand Port
*                       Read the receiver server.IPand Port
*                       
*
*                       Receiving server realization (Yes.61005Port Reception Data) -- This function Be careful with the firewall. It has to be allowed to receive data..
* V1.2 2013-11-07 12:40:49
*                 Validation when changing date settings
*                 Length of the package, Type, controller port, Special marking fixed inWGPacketShortMedium
* V2.5 2015-04-29 20:41:30 Adopt V6.56Driver Version Model by0x19For0x17
* V2.6 Version  2015-11-03 20:25:53 V6.60Driver Version Increase Communications password testing, 1024Bytes for permission upload and log extraction operations  
*                               Retry to modify communication
* V2.7 Version  2015-12-05 17:18:00 V6.62Driver Version Increase CommunicationsSM4Communications password testing
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

class WGPacketShort {				//Shortcast agreement
public:
	const static unsigned int	 WGPacketSize = 64;			    //Length of submission
	//2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
	const static unsigned char	 Type = 0x17;		//2015-04-29 22:22:50			//Type
	const static unsigned int    ControllerPort = 60000;        //controller port
	const static unsigned int    SpecialFlag =0x55AAAA55;       //Special identification Prevent mishandling

	unsigned char	 functionID;		    //Function Number
	unsigned int	 iDevSn;                //Device serial number 4Bytes
	unsigned char    data[56];              //56Byte Data [Fluid]

	unsigned char    recv[WGPacketSize];    //Data received
	unsigned char    recv1024[1024];    //Data received1024
	WGPacketShort(void)
	{
		Reset();
	}
	void Reset()  //Data Reunification
	{
		memset(data,0,sizeof(data));
	}
	void toByte(char* buff, size_t buflen) //Generate64Bytes package
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
	int run(ACE_SOCK_CODgram udp)  //ByUDPSend Command Can not open message
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
					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //Align type
						&& (recv[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

	int runWithPassword_SM4_ECB(ACE_SOCK_CODgram udp,unsigned char* password)  //2015-12-04 15:24:29 AdoptECBProgramme
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
		ShortEncryptSM4_ECB((char*)buff,64, (char*)password);  //2015-11-02 15:40:59 Encryption
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
					ShortDecryptSM4_ECB((char*)recv,64, (char*)password);  //2015-11-02 15:40:59 Decrypt
					if (buffbak[0] == Type)  //2015-12-04 15:35:11 If received is unencrypted command processing
					{
						if (recv[0] != Type) //2015-12-04 15:35:29Decrypt is an invalid command.
						{
							memcpy(&(recv[0]),buffbak, WGPacketSize); //Restore Data
						}
					}

					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //Align type
						&& (recv[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

	int runWithPassword_SM4_CBC(ACE_SOCK_CODgram udp,unsigned char* password,unsigned char* IV)  //2015-12-04 15:24:29 AdoptCBCProgramme
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
		ShortEncryptSM4_CBC((char*)buff,64, (char*)password,(char*)IV);  //2015-11-02 15:40:59 Encryption
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
					ShortDecryptSM4_CBC((char*)recv,64, (char*)password,(char*)IV);  //2015-11-02 15:40:59 Decrypt
					if (buffbak[0] == Type)  //2015-12-04 15:35:11 If received is unencrypted command processing
					{
						if (recv[0] != Type) //2015-12-04 15:35:29Decrypt is an invalid command.
						{
							memcpy(&(recv[0]),buffbak, WGPacketSize); //Restore Data
						}
					}

					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //Align type
						&& (recv[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

	static unsigned int sequenceIdSent()// And finally, the current.
	{
		return sequenceId; // And finally, the current.
	}

	int run1024(ACE_SOCK_CODgram udp,unsigned char* buff1024)  //1024Bytes ByUDPSend Command Can not open message
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
					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv[40]),4);

					if ((recv[0]== Type) //Align type
						&& (recv[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

	int runWithPassword1024_SM4_ECB(ACE_SOCK_CODgram udp,unsigned char* buff1024,unsigned char* password)  //1024Bytes 2015-11-02 15:39:39 Communication password ByUDPSend Command Can not open message
	{
		unsigned int  currentSequenceId=0;
		memcpy(&currentSequenceId, &(buff1024[40]),4);

		//	unsigned char buff[1024];
		int errcnt =0;

		ShortEncryptSM4_ECB((char*)buff1024,1024, (char*)password);  //2015-11-02 15:40:59 Encryption
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
					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv1024[40]),4);

					if ((recv1024[0]== Type) //Align type
						&& (recv1024[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

	int runWithPassword1024_SM4_CBC(ACE_SOCK_CODgram udp,unsigned char* buff1024,unsigned char* password,unsigned char* IV)  //1024Bytes 2015-11-02 15:39:39 Communication password ByUDPSend Command Can not open message
	{
		unsigned int  currentSequenceId=0;
		memcpy(&currentSequenceId, &(buff1024[40]),4);

		//	unsigned char buff[1024];
		volatile int errcnt =0;

		ShortEncryptSM4_CBC((char*)buff1024,1024, (char*)password, (char*)IV);  //2015-11-02 15:40:59 Encryption
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
					//Water Stream
					unsigned int  sequenceIdReceived=0;
					memcpy(&sequenceIdReceived, &(recv1024[40]),4);

					if ((recv1024[0]== Type) //Align type
						&& (recv1024[1]== functionID) //Function numbers are consistent
						&& (sequenceIdReceived == currentSequenceId) )  //Serial number corresponding
					{
						return 1;
					}
					else
					{
						errcnt++;
					}
				}
			}
		} while(tries-- >0); //Try again three times.

		return -1;
	}

private:
	static  unsigned int     sequenceId;     //Serial number	
};
unsigned int WGPacketShort::sequenceId = 0;  //Water Stream Value

unsigned char GetHex(int val) //AccessHexValue, Mainly used in date time format
{
	return ((val % 10) + (((val -(val % 10)) / 10)%10) *16);
}
void log(char* info)  //Log Information
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
int testBasicFunction_SM4_ECB(char *ControllerIP, unsigned int controllerSN);  //ECB Basic function test
int testBasicFunction_SM4_CBC(char *ControllerIP, unsigned int controllerSN);  //CBC Basic function test
int testWatchingServer(char *ControllerIP, unsigned int controllerSN, char *watchServerIP,int watchServerPort);  //Receiving Server Test
int WatchingServerRuning (char *watchServerIP,int watchServerPort);   //2013-11-05 13:08:39 Access to server monitoring status

#include<iostream>
using namespace std;


//No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
//Test instructions in this case
//controllerSN  = 422101164
//controllerIP  = 192.168.168.123
//Computer  IP  = 192.168.168.101
//For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)

int ACE_TMAIN (int, ACE_TCHAR *[]) //Main process portal
{
	int ret =0;

	int sn =0;
	char ip[32];

	cout<<("Please enter the controller.SN(9Digits):  ");
	cin>>sn;;

	//log("Please enter the controller.IP:");
	cout<< ("Please enter the controller.IP:  ");
	cin >>ip;


	//	
		log("...SM4_ECBCommunication encryption test Start .....................");
	ret = testBasicFunction_SM4_ECB(ip,sn); //Basic function test
		log("...SM4_ECBCommunication encryption test complete......................\r\n\r\n");

		log("...SM4_CBC Communication encryption test Start .....................\r\n");
	ret = testBasicFunction_SM4_CBC(ip,sn); //Basic function test
		log("...SM4_CBC  Communication encryption test complete......................\r\n");

	//if (ret !=1)
	//{
	////	log("Basic function test failed???, Press X Return key exit...");
	//	char stop;
	//	cin >>stop;
	//	return 0;
	//}

	log("Test is over., Press X Return key exit...");
	char stop2;
	cin >>stop2;
	return 1;
}

char* RecordDetails[] =
{
	//Record cause (Type SwipePass Adopted; SwipeNOPassMeans no pass.; ValidEvent Effective Event(Like buttons Door Magnetic Supercode open.); Warn Call the police.)
	//Code  Type   English Description  Chinese Description
	"1","SwipePass","Swipe","Open the swipe.",
	"2","SwipePass","Swipe Close","Brush off",
	"3","SwipePass","Swipe Open","Open it.",
	"4","SwipePass","Swipe Limited Times","Open the swipe.(Time limit)",
	"5","SwipeNOPass","Denied Access: PC Control","It's forbidden to pass.: Computer control",
	"6","SwipeNOPass","Denied Access: No PRIVILEGE","It's forbidden to pass.: No Permissions",
	"7","SwipeNOPass","Denied Access: Wrong PASSWORD","It's forbidden to pass.: Wrong password.",
	"8","SwipeNOPass","Denied Access: AntiBack","It's forbidden to pass.: Backwards",
	"9","SwipeNOPass","Denied Access: More Cards","It's forbidden to pass.: Doc!",
	"10","SwipeNOPass","Denied Access: First Card Open","It's forbidden to pass.: First Card",
	"11","SwipeNOPass","Denied Access: Door Set NC","It's forbidden to pass.: It's always closed.",
	"12","SwipeNOPass","Denied Access: InterLock","It's forbidden to pass.: Interlock",
	"13","SwipeNOPass","Denied Access: Limited Times","It's forbidden to pass.: Limited number of brush cards",
	"14","SwipeNOPass","Denied Access: Limited Person Indoor","It's forbidden to pass.: Number of people in the door",
	"15","SwipeNOPass","Denied Access: Invalid Timezone","It's forbidden to pass.: Card expired or not valid",
	"16","SwipeNOPass","Denied Access: In Order","It's forbidden to pass.: Ordered access restrictions",
	"17","SwipeNOPass","Denied Access: SWIPE GAP LIMIT","It's forbidden to pass.: Brush Card Interval",
	"18","SwipeNOPass","Denied Access","It's forbidden to pass.: Reason unknown.",
	"19","SwipeNOPass","Denied Access: Limited Times","It's forbidden to pass.: Limit number of brushes",
	"20","ValidEvent","Push Button","Button open.",
	"21","ValidEvent","Push Button Open","Button On",
	"22","ValidEvent","Push Button Close","Button Off",
	"23","ValidEvent","Door Open","Open the door.[Door Magnetic Signal]",
	"24","ValidEvent","Door Closed","Door closed.[Door Magnetic Signal]",
	"25","ValidEvent","Super Password Open Door","Supercode open.",
	"26","ValidEvent","Super Password Open","Supercode open.",
	"27","ValidEvent","Super Password Close","Super Password Level",
	"28","Warn","Controller Power On","Power on the controller.",
	"29","Warn","Controller Reset","Control Reposition",
	"30","Warn","Push Button Invalid: Disable","Buttons don't open.: button disabled",
	"31","Warn","Push Button Invalid: Forced Lock","Buttons don't open.: Force the closing.",
	"32","Warn","Push Button Invalid: Not On Line","Buttons don't open.: The door's offline.",
	"33","Warn","Push Button Invalid: InterLock","Buttons don't open.: Interlock",
	"34","Warn","Threat","Coercion to the police.",
	"35","Warn","Threat Open","Coercion to call the police.",
	"36","Warn","Threat Close","Coercion to alarm.",
	"37","Warn","Open too long","The door was open for a long time.[After legally opening the door,]",
	"38","Warn","Forced Open","Forced breaking into the police.",
	"39","Warn","Fire","Fire!",
	"40","Warn","Forced Close","Force the closing.",
	"41","Warn","Guard Against Theft","It's an alarm.",
	"42","Warn","7*24Hour Zone","Smoke gas temperature alert.",
	"43","Warn","Emergency Call","Call 911.",
	"44","RemoteOpen","Remote Open Door","Operator opens the door remotely.",
	"45","RemoteOpen","Remote Open Door By USB Reader","The transmitter has confirmed the remote opening."
};
char* getReasonDetailChinese(int Reason) //Chinese
{
	if (Reason > 45)
	{
		return "";
	}
	if (Reason <= 0)
	{
		return "";
	}
	return RecordDetails[(Reason - 1) * 4 + 3]; //Chinese Information
}

char* getReasonDetailEnglish(int Reason) //English Description
{
	if (Reason > 45)
	{
		return "";
	}
	if (Reason <= 0)
	{
		return "";
	}
	return RecordDetails[(Reason - 1) * 4 + 2]; //Information in English
}
/// <summary>
/// Show Record Information
/// </summary>
/// <param name="recv"></param>
void displayRecordInformation(unsigned char* recv)
{
	//	  	Last recorded information		
	//8-11	Index number for the last record.
	//(=0No record.)	4	0x00000000
	int recordIndex =0;
	memcpy(&recordIndex, &(recv[8]),4);

	//12	Record type
	//0=No record
	//1=Brush Card Record
	//2=Door Magnetic,button, Device startup, Remote Open Record
	//3=Call the police.	1	
	int recordType = recv[12];

	//13	Validity(0 Not approved, 1Adopted)	1	
	int recordValid = recv[13];

	//14	Door number.(1,2,3,4)	1	
	int recordDoorNO = recv[14];

	//15	Come in./Out.(1It means coming in., 2Means out.)	1	0x01
	int recordInOrOut = recv[15];

	//16-19	Card(Type is when swiping a card.)
	//or numbering(Other types of records)	4	
	long long recordCardNO = 0;
	memcpy(&recordCardNO, &(recv[16]),4);

	//20-26	Brush Time:
	//Days and days of year (AdoptBCDCode)See description of the set-up segment
	char recordTime[]="2000-01-01 00:00:00";
	sprintf(recordTime,"%02X%02X-%02X-%02X %02X:%02X:%02X", 
		recv[20],recv[21],recv[22],recv[23],recv[24],recv[25],recv[26]);

	//2012.12.11 10:49:59	7	
	//27	Record cause code(You can check it out. ¡°Checkcard log notes.xls¡±It's a file.ReasonNO)
	//It's only for complex information.	1	
	int reason = recv[27];

	//0=No record
	//1=Brush Card Record
	//2=Door Magnetic,button, Device startup, Remote Open Record
	//3=Call the police.	1	
	//0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
	if (recordType == 0)
	{
		printf("Index post=%u  No record\r\n", recordIndex);
	}
	else if (recordType == 0xff)
	{
		log(" The records of the specified index have been overwritten,Use the index.0, Retrieving index values from the earliest record");
	}
	else if (recordType == 1) //2015-06-10 08:49:31 Show data with card number type of record
	{
		//Card
		printf("Index post=%u\r\n", recordIndex);
		printf("  Card = %u\r\n", recordCardNO);
		printf("  Door number. = %u\r\n", recordDoorNO);
		printf("  Access = %s\r\n", recordInOrOut == 1 ? "Come in." : "Out.");
		printf("  Valid. = %s\r\n", recordValid == 1 ? "Pass." : "Ban");
		printf("  Time = %s\r\n", recordTime);
		printf("  Description = %s\r\n", getReasonDetailChinese(reason));
	}
	else if (recordType == 2)
	{
		//Other processing
		//Door Magnetic,button, Device startup, Remote Open Record
		printf("Index post=%u  Non-card records\r\n ", recordIndex);
		printf("  Numbering = %u\r\n", recordCardNO);
		printf("  Door number. = %u\r\n", recordDoorNO);
		printf("  Time = %s\r\n", recordTime);
		printf("  Description = %s\r\n", getReasonDetailChinese(reason));
	}
	else if (recordType == 3)
	{
		//Other processing
		//Call the police.

		printf("Index post=%u  Call the police.\r\n ", recordIndex);
		printf("  Numbering = %u\r\n", recordCardNO);
		printf("  Door number. = %u\r\n", recordDoorNO);
		printf("  Time = %s\r\n", recordTime);
		printf("  Description = %s\r\n", getReasonDetailChinese(reason));
	}
}




//ControllerIP controllerIPAddress
//controllerSN Control serial number
int testBasicFunction_SM4_ECB(char *ControllerIP, unsigned int controllerSN)  //Basic function test
{
	int ret =0;
	int success =0;  //0 Failed, 1It means success.

	ACE_INET_Addr controller_addr (WGPacketShort::ControllerPort, ControllerIP); //Port  IPAddress
	ACE_SOCK_CODgram udp;
	if (0 != udp.open (controller_addr))
	{
		//Please enter validIP
		log("Please enter validIP...");
		return -1;
	}

	//Create short message pkt
	WGPacketShort pkt;  

	//SM4 ECB Set Communications Password[Function Number: 0xE0] **********************************************************************************
	log("SM4 ECB Communications testing ...");
	pkt.Reset();
	pkt.functionID = 0xE0;
	pkt.iDevSn = controllerSN; 
	unsigned  char commPassword[] = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16Byte Password
	//Ideas against error
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);

	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00Set New Password
	{
		pkt.data[4 + i] = commPassword[i];
		pkt.data[36 + i] = commPassword[i];
	}

	//In two cases.: Password is empty  Or... Password set
	ret = pkt.run(udp);  //2015-11-02 10:21:22 Try the controller without password first.
	success = 0;
	if (ret > 0)
	{
		if (pkt.recv[8] == 1)
		{
			log("Communication password set successfully...");
			success = 1;
		}
	}
	if (success == 0)
	{
		ret = pkt.runWithPassword_SM4_ECB(udp,commPassword);  //2015-11-02 10:21:22 Try the password operation for the controller.
		if ((ret > 0) && (pkt.recv[8] == 1))
		{
			log("Communication password set successfully...[Can not open message]");
			success = 1;
		}
		else
		{
			log("Communication password setup failed???...[Can not open message]");
		}
	}


	//1.10	Open remote[Function Number: 0x40] **********************************************************************************
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
			//Open the door effectively......
			log("1.10 Open remote	 Success...[Can not open message]");
			success = 1;
		}
		else
		{
			log("1.10 Open remote	 Failed???...[Can not open message]");
		}
	}
	else
	{
		log("1.10 Open remote	 Failed???...[Can not open message]");
	}


	////SM4 Clear the code.[Function Number: 0xE0] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xE0;
	////Ideas against error
	//memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	//for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
	//{
	//	pkt.data[4 + i] = 0;
	//	pkt.data[36 + i] = 0;
	//}

	//ret = pkt.runWithPassword_SM4_ECB(udp,commPassword);  //
	//if ((ret > 0) && (pkt.recv[8] == 1))
	//{
	//	log("Communication code emptied....[Can not open message]");
	//	success = 1;
	//}
	//else
	//{
	//	log("Communication password emptied???...[Can not open message]");
	//}
	//// **********************************************************************************
	//return success;

	log("Start 1024Byte Command Operations");

	unsigned char command1024[1024];
	unsigned char sendBuff[64];
	//1.9	Extract Record Operation
	//1. Pass. 0xB0Command Fetch the earliest record index
	//2. Pass. 0xB0Command Get Last Record Index
	//3. Pass. 0xB4Command Get read record index numbers recordIndex
	//4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records£¬ Until the records are empty.
	//5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
	//After the top steps,£¬ The entire extraction record is complete.
	long firstRecordIndex = 0;  //First record index number
	long lastRecordIndex = 0;   //Last record index number.
	long recordIndexGotToRead = 0x0;
	long recordIndexToGet = 0;
	log("1.9 Extract Record Operation	 Start...[1024Byte Command]");

	//. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
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
		printf("Fetch the earliest record index = %u  \r\n", firstRecordIndex);
		success =1;
	}

	//Communication (Take the latest record. By Index 0xffffffff)
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
		printf("Get Last Record Index = %u  \r\n", lastRecordIndex);
		success =1;
	}

	//1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xB4;
	pkt.iDevSn = controllerSN; 
	recordIndexGotToRead =0x0;
	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	success =0;
	if (ret >0)
	{
		memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
		printf("Get read record index numbers = %u  \r\n", recordIndexGotToRead);
		success =1;
	}

	long validRecordsCount = 0;
	//	recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
	if (ret > 0)
	{
		long recordIndexValidGet = 0;

		long recordIndexToGetStart = recordIndexGotToRead + 1;  //Prepare record index to extract
		if (recordIndexGotToRead > lastRecordIndex || recordIndexGotToRead < firstRecordIndex) //Beyond range Take index number for the first record
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
				command1024[j] = 0; //Restore
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

					//12	Record type
					//0=No record
					//1=Brush Card Record
					//2=Door Magnetic,button, Device startup, Remote Open Record
					//3=Call the police.	1	
					//0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
					unsigned char recv[64];
					memcpy(recv, &(pkt.recv1024[j]), 64);
					int recordType = recv[12];
					if (recordType == 0)
					{
						success = 2;
						break; //No more records.
					}
					if (recordType == 0xff)//This index number is invalid
					{
						success = 0;
						break;
					}
					success = 1;
					recordIndexValidGet = recordIndexCurrent;
					recordIndexCurrent++;
					validRecordsCount++;
					//
					if (validRecordsCount < 100) //2015-11-05 14:59:20Show Before100individual, Too much shows slow processing. No analysis....
					{
						displayRecordInformation(recv); //2015-06-09 20:01:21
						if (validRecordsCount == 99)
						{
							log(" To speed up extraction, Over100Shit.  Do not display recording information again.......");
						}
					}
					//.......Storage of records received
					//*****
					//###############
				}
			}
			else
			{
				//Ripping failed
				break;
			}
			if (success != 1)
			{
				break;
			}
		} while (cnt < 200000);

		printf("1.9 Full extraction successful.	 ... Number of valid records= %u\r\n" , validRecordsCount);
		if ((success > 0) && validRecordsCount>0)
		{
			//Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
			pkt.Reset();
			pkt.functionID = 0xB2;
			memcpy(&( pkt.data[0]),  &recordIndexValidGet,4);

			//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
			memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

			ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
			success = 0;
			if (ret > 0)
			{
				if (pkt.recv[8] == 1)
				{
					//Full extraction successful.....
					log("1.9 Full extraction successful.	 Success...");
					success = 1;
				}
			}

		}
	}


	//1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
	//This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
	//Suggested number of privileges updated over50individual, Use this command
	//If the number of privileges exceeds8As soon as possible., If you interrupt., Permissions will be empty. That's why we have to upload the whole thing.

	log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	Start...[Adopt1024Byte Command, Every upload16Permissions]");

	//Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
	int cardCount = 1 * 10000; // 10000;  //2015-06-09 20:20:20 Total number of cards
	printf("       %uThousand powers...\r\n", cardCount / 10000);
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
			command1024[j] = 0; //Restore
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

			//When other parameters are simplified Harmonization, You can make changes depending on each card.
			//20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
			pkt.data[4] = 0x20;
			pkt.data[5] = 0x10;
			pkt.data[6] = 0x01;
			pkt.data[7] = 0x01;
			//20 29 12 31 Deadline:  2029Year12Month31Day
			pkt.data[8] = 0x20;
			pkt.data[9] = 0x29;
			pkt.data[10] = 0x12;
			pkt.data[11] = 0x31;
			//01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
			pkt.data[12] = 0x01;
			//01 Allow Pass Door two. [Two doors., Four controllers working.]
			pkt.data[13] = 0x01;  //If it's forbidden,2Door., As 0x00
			//01 Allow Pass Gate three. [It works on four controllers.]
			pkt.data[14] = 0x01;
			//01 Allow Pass Gate four. [It works on four controllers.]
			pkt.data[15] = 0x01;

			memcpy(& (pkt.data[32-8]), &cardCount,4); //Total permissions
			long itmp =i + 1;
			memcpy(& (pkt.data[35-8]), &itmp,4); //The index place for the current permission(From1Start)
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
				log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 =0xE1 Which means the card number has not been sorted from a small to a large size....???");
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
		log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Success...");
	}
	else
	{
		log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Failed???...????");
	}


	//1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
	//Read All Permissions
	pkt.Reset();
	pkt.functionID = 0x5C;
	pkt.iDevSn = controllerSN;
	long maxCount = 20 * 10000;
	//long cardArrayGet[20*10000];
	long *cardArrayGet = cardArray;
	long QueryIndex = 1; //Index number(From1Start);
	memcpy(&(pkt.data[0]),&QueryIndex, 4);
	for (int i = 0; i < maxCount; i++)
	{
		cardArrayGet[i] = 0;
	}
	log("Read All Permissions	 Start...[1024Byte Command]");
	long iCount = 0;
	for (int i = 0; i < maxCount; i++)
	{
		for (int j = 0; j < 1024; j++)
		{
			command1024[j] = 0; //Restore
			pkt.recv1024[j] = 0; //2015-12-04 22:46:27 Restore
		}
		for (int j = 0; j < 1024; j = j + 64)
		{
			memcpy(&(pkt.data[0]),&QueryIndex, 4);
			QueryIndex++; //Index number(From1Start);
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
				if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFFResponse4294967295
				{
					success = 1;
					//log("1.16      Can not open message: (Permissions deleted)");
					//break;
				}
				else if (cardNOOfPrivilegeToGet == 0)
				{
					//When no permission: (The card number is0)
					//log("1.16       Can not open message: (The card number is0)--This index number is no longer valid.");
					success = 1;
					break;
				}
				else
				{
					//Specific Permission Information...
					//  log("1.16      Can not open message...");
					// log("1.16 Access to specified index numbers	 Success...");
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
				break; //When no permission: (The card number is0)
			}
		}
		else
		{
			printf("1.16     Problem. ret = %u\r\n" ,  ret);  
			break;
		}
	}
	printf("Last read permission card number = %u\r\n" , cardNOOfPrivilegeToGetlast);
	printf("Permissions extractediCount = %u\r\n" ,  iCount);  //2015-11-04 19:59:50 Extract permissions
	log("Extract Permissions End");  //2015-11-04 19:59:50 Extract permissions

	//SM4 ECB Clear the code.[Function Number: 0xE0] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xE0;
	//Ideas against error
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
	{
		pkt.data[4 + i] = 0;
		pkt.data[36 + i] = 0;
	}

	ret =  pkt.runWithPassword_SM4_ECB(udp,commPassword);  //SM4 ECB 
	if ((ret > 0) && (pkt.recv[8] == 1))
	{
		log("Communication code emptied....[Can not open message]");
		success = 1;
	}
	else
	{
		log("Communication password emptied???...[Can not open message]");
	}
	// **********************************************************************************

	//End  **********************************************************************************
	udp.close();
	return success;
}


int testBasicFunction_SM4_CBC(char *ControllerIP, unsigned int controllerSN)  //Basic function test
{
	int ret =0;
	int success =0;  //0 Failed, 1It means success.

	ACE_INET_Addr controller_addr (WGPacketShort::ControllerPort, ControllerIP); //Port  IPAddress
	ACE_SOCK_CODgram udp;
	if (0 != udp.open (controller_addr))
	{
		//Please enter validIP
		log("Please enter validIP...");
		return -1;
	}

	//Create short message pkt
	WGPacketShort pkt;  

	//SM4 CBC Set Communications Password[Function Number: 0xE2] **********************************************************************************
	log("SM4 CBC Communications testing ...");
	pkt.Reset();
	pkt.functionID = 0xE2;
	pkt.iDevSn = controllerSN; 
	unsigned  char commPassword[] = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16Byte Password
	unsigned  char  IV[] = { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }; //16Bytes Initial Variables

	//Ideas against error
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);

	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00Set New Password
	{
		pkt.data[4 + i] = commPassword[i];
		pkt.data[36 + i] = commPassword[i];
	}
	 for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00SettingsIV
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

	//In two cases.: Password is empty  Or... Password set
	ret = pkt.run(udp);  //2015-11-02 10:21:22 Try the controller without password first.
	success = 0;
	if (ret > 0)
	{
		if (pkt.recv[8] == 1)
		{
			log("Communication password set successfully...");
			success = 1;
		}
	}
	if (success == 0)
	{
		ret = pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //2015-11-02 10:21:22 Try the password operation for the controller.
		if ((ret > 0) && (pkt.recv[8] == 1))
		{
			log("Communication password set successfully...[Can not open message]");
			success = 1;
		}
		else
		{
			log("Communication password setup failed???...[Can not open message]");
		}
	}


	//1.10	Open remote[Function Number: 0x40] **********************************************************************************
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
			//Open the door effectively......
			log("1.10 Open remote	 Success...[Can not open message]");
			success = 1;
		}
		else
		{
			log("1.10 Open remote	 Failed???...[Can not open message]");
		}
	}
	else
	{
		log("1.10 Open remote	 Failed???...[Can not open message]");
	}


	////SM4 Clear the code.[Function Number: 0xE0] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xE0;
	////Ideas against error
	//memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	//for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
	//{
	//	pkt.data[4 + i] = 0;
	//	pkt.data[36 + i] = 0;
	//}

	//ret = pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //
	//if ((ret > 0) && (pkt.recv[8] == 1))
	//{
	//	log("Communication code emptied....[Can not open message]");
	//	success = 1;
	//}
	//else
	//{
	//	log("Communication password emptied???...[Can not open message]");
	//}
	//// **********************************************************************************
	//return success;

	log("Start 1024Byte Command Operations");

	unsigned char command1024[1024];
	unsigned char sendBuff[64];
	//1.9	Extract Record Operation
	//1. Pass. 0xB0Command Fetch the earliest record index
	//2. Pass. 0xB0Command Get Last Record Index
	//3. Pass. 0xB4Command Get read record index numbers recordIndex
	//4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records£¬ Until the records are empty.
	//5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
	//After the top steps,£¬ The entire extraction record is complete.
	long firstRecordIndex = 0;  //First record index number
	long lastRecordIndex = 0;   //Last record index number.
	long recordIndexGotToRead = 0x0;
	long recordIndexToGet = 0;
	log("1.9 Extract Record Operation	 Start...[1024Byte Command]");

	//. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
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
		printf("Fetch the earliest record index = %u  \r\n", firstRecordIndex);
		success =1;
	}

	//Communication (Take the latest record. By Index 0xffffffff)
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
		printf("Get Last Record Index = %u  \r\n", lastRecordIndex);
		success =1;
	}

	//1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xB4;
	pkt.iDevSn = controllerSN; 
	recordIndexGotToRead =0x0;
	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	success =0;
	if (ret >0)
	{
		memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
		printf("Get read record index numbers = %u  \r\n", recordIndexGotToRead);
		success =1;
	}

	long validRecordsCount = 0;
	//	recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
	if (ret > 0)
	{
		long recordIndexValidGet = 0;

		long recordIndexToGetStart = recordIndexGotToRead + 1;  //Prepare record index to extract
		if (recordIndexGotToRead > lastRecordIndex || recordIndexGotToRead < firstRecordIndex) //Beyond range Take index number for the first record
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
				command1024[j] = 0; //Restore
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

					//12	Record type
					//0=No record
					//1=Brush Card Record
					//2=Door Magnetic,button, Device startup, Remote Open Record
					//3=Call the police.	1	
					//0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
					unsigned char recv[64];
					memcpy(recv, &(pkt.recv1024[j]), 64);
					int recordType = recv[12];
					if (recordType == 0)
					{
						success = 2;
						break; //No more records.
					}
					if (recordType == 0xff)//This index number is invalid
					{
						success = 0;
						break;
					}
					success = 1;
					recordIndexValidGet = recordIndexCurrent;
					recordIndexCurrent++;
					validRecordsCount++;
					//
					if (validRecordsCount < 100) //2015-11-05 14:59:20Show Before100individual, Too much shows slow processing. No analysis....
					{
						displayRecordInformation(recv); //2015-06-09 20:01:21
						if (validRecordsCount == 99)
						{
							log(" To speed up extraction, Over100Shit.  Do not display recording information again.......");
						}
					}
					//.......Storage of records received
					//*****
					//###############
				}
			}
			else
			{
				//Ripping failed???
				break;
			}
			if (success != 1)
			{
				break;
			}
		} while (cnt < 200000);

		printf("1.9 Full extraction successful.	 ... Number of valid records= %u\r\n" , validRecordsCount);
		if ((success > 0) && validRecordsCount>0)
		{
			//Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
			pkt.Reset();
			pkt.functionID = 0xB2;
			memcpy(&( pkt.data[0]),  &recordIndexValidGet,4);

			//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
			memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

			ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
			success = 0;
			if (ret > 0)
			{
				if (pkt.recv[8] == 1)
				{
					//Full extraction successful.....
					log("1.9 Full extraction successful.	 Success...");
					success = 1;
				}
			}

		}
	}


	//1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
	//This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
	//Suggested number of privileges updated over50individual, Use this command
	//If the number of privileges exceeds8As soon as possible., If you interrupt., Permissions will be empty. That's why we have to upload the whole thing.

	log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	Start...[Adopt1024Byte Command, Every upload16Permissions]");

	//Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
	int cardCount = 1 * 10000; // 10000;  //2015-06-09 20:20:20 Total number of cards
	printf("       %uThousand powers...\r\n", cardCount / 10000);
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
			command1024[j] = 0; //Restore
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

			//When other parameters are simplified Harmonization, You can make changes depending on each card.
			//20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
			pkt.data[4] = 0x20;
			pkt.data[5] = 0x10;
			pkt.data[6] = 0x01;
			pkt.data[7] = 0x01;
			//20 29 12 31 Deadline:  2029Year12Month31Day
			pkt.data[8] = 0x20;
			pkt.data[9] = 0x29;
			pkt.data[10] = 0x12;
			pkt.data[11] = 0x31;
			//01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
			pkt.data[12] = 0x01;
			//01 Allow Pass Door two. [Two doors., Four controllers working.]
			pkt.data[13] = 0x01;  //If it's forbidden,2Door., As 0x00
			//01 Allow Pass Gate three. [It works on four controllers.]
			pkt.data[14] = 0x01;
			//01 Allow Pass Gate four. [It works on four controllers.]
			pkt.data[15] = 0x01;

			memcpy(& (pkt.data[32-8]), &cardCount,4); //Total permissions
			long itmp =i + 1;
			memcpy(& (pkt.data[35-8]), &itmp,4); //The index place for the current permission(From1Start)
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
				log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 =0xE1 Which means the card number has not been sorted from a small to a large size....???");
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
		log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Success...");
	}
	else
	{
		log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Failed???...????");
	}


	//1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
	//Read All Permissions
	pkt.Reset();
	pkt.functionID = 0x5C;
	pkt.iDevSn = controllerSN;
	long maxCount = 20 * 10000;
	//long cardArrayGet[20*10000];
	long *cardArrayGet = cardArray;
	long QueryIndex = 1; //Index number(From1Start);
	memcpy(&(pkt.data[0]),&QueryIndex, 4);
	for (int i = 0; i < maxCount; i++)
	{
		cardArrayGet[i] = 0;
	}
	log("Read All Permissions	 Start...[1024Byte Command]");
	long iCount = 0;
	for (int i = 0; i < maxCount; i++)
	{
		for (int j = 0; j < 1024; j++)
		{
			command1024[j] = 0; //Restore
			pkt.recv1024[j] = 0; //2015-12-04 22:46:27 Restore
		}
		for (int j = 0; j < 1024; j = j + 64)
		{
			memcpy(&(pkt.data[0]),&QueryIndex, 4);
			QueryIndex++; //Index number(From1Start);
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
				if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFFResponse4294967295
				{
					success = 1;
					//log("1.16      Can not open message: (Permissions deleted)");
					//break;
				}
				else if (cardNOOfPrivilegeToGet == 0)
				{
					//When no permission: (The card number is0)
					//log("1.16       Can not open message: (The card number is0)--This index number is no longer valid.");
					success = 1;
					break;
				}
				else
				{
					//Specific Permission Information...
					//  log("1.16      Can not open message...");
					// log("1.16 Access to specified index numbers	 Success...");
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
				break; //When no permission: (The card number is0)
			}
		}
		else
		{
			printf("1.16     Problem. ret = %u\r\n" ,  ret);  
			break;
		}
	}
	printf("Last read permission card number = %u\r\n" , cardNOOfPrivilegeToGetlast);
	printf("Permissions extractediCount = %u\r\n" ,  iCount);  //2015-11-04 19:59:50 Extract permissions
	log("Extract Permissions End");  //2015-11-04 19:59:50 Extract permissions

	//SM4 CBC Clear the code.[Function Number: 0xE2] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0xE2;
	//Ideas against error
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);
	for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
	{
		pkt.data[4 + i] = 0;
		pkt.data[36 + i] = 0;
	}

	ret =  pkt.runWithPassword_SM4_CBC(udp,commPassword,IV);  //SM4 CBC 
	if ((ret > 0) && (pkt.recv[8] == 1))
	{
		log("Communication code emptied....[Can not open message]");
		success = 1;
	}
	else
	{
		log("Communication password emptied???...[Can not open message]");
	}
	// **********************************************************************************

	//End  **********************************************************************************
	udp.close();
	return success;
}
