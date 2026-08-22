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
*/

#include "ace/INET_Addr.h"
#include "ace/SOCK_Dgram.h"
#include "ace/Time_Value.h"
#include "ace/SOCK_CODgram.h"
#include "ace/SOCK_Dgram_Bcast.h"
#include <time.h>   
#include <stdio.h>

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
	static unsigned int sequenceIdSent()// And finally, the current.
	{
		return sequenceId; // And finally, the current.
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
int testBasicFunction(char *ControllerIP, unsigned int controllerSN);  //Basic function test
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
	cout<<("Please enter the controller.SN(9Digits):  ");
	cin>>sn;;
	
	char ip[32];
	//log("Please enter the controller.IP:");
	cout<< ("Please enter the controller.IP:  ");
	cin >>ip;
	
	
	ret = testBasicFunction(ip,sn); //Basic function test
	if (ret !=1)
	{
	  log("Basic function test failed, Press X Return key exit...");
	  char stop;
	  cin >>stop;
	  return 0;
    }
	
	//Receiving Server Test
	char watchServerIP[32]; // = "192.168.168.101";
	cout<< ("Please enter the recipient server.IP:  ");
	cin >>watchServerIP;
    int  watchServerPort = 61005;
	//cout<<("Please enter the receiving server port:  ");
	//cin>>watchServerPort;;

	ret = testWatchingServer(ip,sn,watchServerIP, watchServerPort); //Receiving Server Settings

	ret = WatchingServerRuning(watchServerIP, watchServerPort); //Server Run....
	
	log("Test is over., Press Back to Exit...");
	getchar();
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
int testBasicFunction(char *ControllerIP, unsigned int controllerSN)  //Basic function test
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


	//1.4	Query controller status[Function Number: 0x20](Real time surveillance) **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x20;
	pkt.iDevSn = controllerSN; 
	ret = pkt.run(udp);

	success =0;
	if (ret == 1)
	{
		//Read information successfully...
		success =1;
		log("1.4 Query controller status Success...");

		//	  	Last recorded information		
		displayRecordInformation(pkt.recv);


		//	Other information		
		int doorStatus[4];
		//28	1Door no.(0Means close, 1Show Open)	1	0x00
		doorStatus[1-1] = pkt.recv[28];
		//29	2Door no.(0Means close, 1Show Open)	1	0x00
		doorStatus[2-1] = pkt.recv[29];
		//30	3Door no.(0Means close, 1Show Open)	1	0x00
		doorStatus[3-1] = pkt.recv[30];
		//31	4Door no.(0Means close, 1Show Open)	1	0x00
		doorStatus[4-1] = pkt.recv[31];

		int pbStatus[4];
		//32	1Door button.(0It means you let go., 1Means press)	1	0x00
		pbStatus[1-1] = pkt.recv[32];
		//33	2Door button.(0It means you let go., 1Means press)	1	0x00
		pbStatus[2-1] = pkt.recv[33];
		//34	3Door button.(0It means you let go., 1Means press)	1	0x00
		pbStatus[3-1] = pkt.recv[34];
		//35	4Door button.(0It means you let go., 1Means press)	1	0x00
		pbStatus[4-1] = pkt.recv[35];
		//36	Fault.
		//equals0 No malfunctions.
		//Not equal to0, It's not working.(Reset Time, If there's anything else,, We're going back to the factory.)	1	
		int errCode = pkt.recv[36];
		//37	Control Current Time
		//Time	1	0x21
		//38	min	1	0x30
		//39	sec	1	0x58

		//40-43	Water Stream	4	
		long long  sequenceId=0;
		memcpy(&sequenceId, &(pkt.recv[40]),4);

		//48
		//Special Information1(Return based on actual use)
		//Keyboard Key Information	1	
		//49	Relay status	1	
		int relayStatus = pkt.recv[49];
		//50	Door magnetic.8-15bitbit[Fire!/Force locking]
		//Bit0  Force locking
		//Bit1  Fire!		
		int otherInputStatus = pkt.recv[50];
		if ((otherInputStatus & 0x1) > 0)
		{
			//Force locking
		}
		if ((otherInputStatus & 0x2) > 0)
		{
			//Fire!
		}

		//51	V5.46Version Support Control Current Year	1	0x13
		//52	V5.46Version Support Month	1	0x06
		//53	V5.46Version Support Day	1	0x22

		char controllerTime[]="2000-01-01 00:00:00"; //Control Current Time
		sprintf(controllerTime,"20%02X-%02X-%02X %02X:%02X:%02X", 
			pkt.recv[51],pkt.recv[52],pkt.recv[53],pkt.recv[37],pkt.recv[38],pkt.recv[39]);
	}
	else
	{
		log("1.4 Query controller status Failed?????...");
		return -1;
	}

	//1.5	Read Date Time(Function Number: 0x32) **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x32;
	pkt.iDevSn = controllerSN; 
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		char controllerTime[]="2000-01-01 00:00:00"; //Control Current Time
		sprintf(controllerTime,"%02X%02X-%02X-%02X %02X:%02X:%02X", 
			pkt.recv[8],pkt.recv[9],pkt.recv[10],pkt.recv[11],pkt.recv[12],pkt.recv[13],pkt.recv[14]);

		log("1.5 Read Date Time Success...");
		//log(controllerTime);
		success =1;
	}

	//1.6	Set Date Time[Function Number: 0x30] **********************************************************************************
	//calibrate controller at computer time.....
	pkt.Reset();
	pkt.functionID = 0x30;
	pkt.iDevSn = controllerSN; 

	time_t nowtime;     
	struct tm* ptm;     
	time(&nowtime);     
	ptm = localtime(&nowtime);     
	pkt.data[0] =GetHex((int)(( ptm->tm_year + 1900-( ptm->tm_year + 1900)%100)/100)); 
	pkt.data[1] =GetHex((int)(( ptm->tm_year + 1900)%100)); //st.GetMonth()); 
	pkt.data[2] =GetHex(ptm->tm_mon + 1); 
	pkt.data[3] =GetHex( ptm->tm_mday); 
	pkt.data[4] =GetHex(ptm->tm_hour); 
	pkt.data[5] =GetHex(ptm->tm_min); 
	pkt.data[6] = GetHex(ptm->tm_sec); 
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if(memcmp(&(pkt.data[0]), &(pkt.recv[8]),7)==0)
		{
			log("1.6 Set Date Time Success...");
			success =1;
		}	
	}

	//1.7	Get a record of the given index number[Function Number: 0xB0] **********************************************************************************
	//(Take Index Number 0x00000001Records)
	int  recordIndexToGet =0;
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 

	//	(Special
	//If=0, Retrieving the earliest recorded information
	//If=0xffffffffRetrieving information from the last record)
	//Records index numbers are normally incremental., Max.0xffffff = 16,777,215 (Over1Millions.) . Due to limited storage space, Only the closest on the controller.20Thousands of records.. When index numbers exceed20After 10,000., The records of the old index numbers are overwritten., So at this point, check the records of these index numbers., The type of record returned will be0xff, It means it doesn't exist..
	recordIndexToGet =1;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		log("1.7 Get Index As1Recorded information	 Success...");
		//	  	Index to1Recorded information		
		displayRecordInformation(pkt.recv);

		success =1;
	}

	//. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		log("1.7 Fetch information from the earliest record	 Success...");
		//	  	First recorded information		
		displayRecordInformation(pkt.recv);
        success =1;
	}

	//Communication (Take the latest record. By Index 0xffffffff)
	pkt.Reset();
	pkt.functionID = 0xB0;
	pkt.iDevSn = controllerSN; 
	recordIndexToGet =0xffffffff;
	memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		log("1.7 Access to the latest recorded information	 Success...");
		//	  	Latest recorded information		
				displayRecordInformation(pkt.recv);
		success =1;
	}

	int recordIndexGotToRead;

	////1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xB2;
	//pkt.iDevSn = controllerSN; 
	//// (Set read record index number as5)
	//int recordIndexGot =0x5;
	//memcpy(&(pkt.data[0]), &recordIndexGot, 4);

	////12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
	//memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

	//ret = pkt.run(udp);
	//success =0;
	//if (ret >0)
	//{
	//	if (pkt.recv[8] == 1)
	//	{
	//		log("1.8 Set a read record index number	 Success...");
	//		success =1;
	//	}
	//}

	////1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
	//pkt.Reset();
	//pkt.functionID = 0xB4;
	//pkt.iDevSn = controllerSN; 
	// recordIndexGotToRead =0x0;
	//ret = pkt.run(udp);
	//success =0;
	//if (ret >0)
	//{
	//	memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
	//	log("1.9 Get read record index numbers	 Success...");
	//	success =1;
	//}

	//1.9	Extract Record Operation
	//1. Pass. 0xB4Command Get read record index numbers recordIndex
	//2. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records£¬ Until the records are empty.
	//3. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
	//After three steps,£¬ The entire extraction record is complete.
    log("1.9 Extract Record Operation	 Start...");
	pkt.Reset();
	pkt.functionID = 0xB4;
	pkt.iDevSn = controllerSN; 
	int recordIndexGotToRead2 =0x0;
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		memcpy(&(recordIndexGotToRead), &(pkt.recv[8]),4);
		pkt.Reset();
		pkt.functionID = 0xB0;
		pkt.iDevSn = controllerSN; 
		int recordIndexToGetStart = recordIndexGotToRead + 1;
		int recordIndexValidGet = 0;
		int cnt=0;
		do
		{
			memcpy(&(pkt.data[0]), &recordIndexToGetStart, 4);
			ret = pkt.run(udp);
			success =0;
			if (ret >0)
			{
				success =1;

				//12	Record type
				//0=No record
				//1=Brush Card Record
				//2=Door Magnetic,button, Device startup, Remote Open Record
				//3=Call the police.	1	
				//0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
				int recordType = pkt.recv[12];
				if (recordType == 0)
				{
					break; //No more records.
				}
				if (recordType == 0xff)
				{
					success = 0;  //This index number is invalid  Reset Index Values
					//Take the earliest record index bit
                    pkt.iDevSn = controllerSN; 
					recordIndexToGet =0;
					memcpy(&(pkt.data[0]), &recordIndexToGet, 4);
					ret = pkt.run(udp);
					success =0;
					if (ret >0)
                    {
                        log("1.7 Fetch information from the earliest record	 Success...");
						int recordIndex =0;
		                memcpy(&recordIndex, &(pkt.recv[8]),4);
                        recordIndexGotToRead = recordIndex;
                        recordIndexToGetStart = recordIndexGotToRead;
                        continue;
                    }
                    success = 0;  
					break; 
				}

				recordIndexValidGet = recordIndexToGetStart;
                //.......Storage of records received
			    displayRecordInformation(pkt.recv);
				//*****
				//###############
			}
			else
			{
				//Ripping failed
				break;
			}
			recordIndexToGetStart++;
		}while(cnt++ < 200000);
		if (success >0)
		{
			//Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
			pkt.Reset();
			pkt.functionID = 0xB2;
			pkt.iDevSn = controllerSN; 
			memcpy(&(pkt.data[0]), &recordIndexValidGet, 4);

			//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
	        memcpy(&(pkt.data[4]), &(WGPacketShort::SpecialFlag), 4);

			ret = pkt.run(udp);
			success =0;
			if (ret >0)
			{
				if (pkt.recv[8] == 1)
				{
					//Full extraction successful.....
					log("1.9 Full extraction successful.	 Success...");
					success =1;
				}
			}

		}
	}

	//1.10	Open remote[Function Number: 0x40] **********************************************************************************
	int doorNO =1;
	pkt.Reset();
	pkt.functionID = 0x40;
	pkt.iDevSn = controllerSN; 
	pkt.data[0] = (doorNO & 0xff); //2013-11-03 20:56:33
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			//Open the door effectively......
			log("1.10 Open remote	 Success...");
			success =1;
		}
	}

	//1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
	//Add card number0D D7 37 00, Through all doors of the current controller
	pkt.Reset();
	pkt.functionID = 0x50;
	pkt.iDevSn = controllerSN; 
	//0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
	long long cardNOOfPrivilege =0x0037D70D;
	memcpy(&(pkt.data[0]), &cardNOOfPrivilege, 4);
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

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
			log("1.11 Permissions to add or modify	 Success...");
			success =1;
		}
	}

	//1.12	Permission to delete(Individual Delete)[Function Number: 0x52] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x52;
	pkt.iDevSn = controllerSN; 
	//Permission card number to delete0D D7 37 00  = 0x0037D70D = 3659533 (Decimal)
	long long cardNOOfPrivilegeToDelete =0x0037D70D;
	memcpy(&(pkt.data[0]), &cardNOOfPrivilegeToDelete, 4);

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay doesn't move..
			log("1.12 Permission to delete(Individual Delete)	 Success...");
			success =1;
		}
	}

	//1.13	Clear Permissions(Clear it all.)[Function Number: 0x54] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x54;
	pkt.iDevSn = controllerSN; 
	memcpy(&(pkt.data[0]), &(WGPacketShort::SpecialFlag), 4);

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			//It's time to clear up.
			log("1.13 Clear Permissions(Clear it all.)	 Success...");
			success =1;
		}
	}

	//1.14	Total Permissions Read[Function Number: 0x58] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x58;
	pkt.iDevSn = controllerSN; 
	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		int privilegeCount =0;
		memcpy(&privilegeCount, &(pkt.recv[8]),4);
		log("1.14 Total Permissions Read	 Success...");

		success =1;
	}


	//Add again as a query operation 1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
	//Add card number0D D7 37 00, Through all doors of the current controller
	pkt.Reset();
	pkt.functionID = 0x50;
	pkt.iDevSn = controllerSN; 
	//0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
	//long long 
		cardNOOfPrivilege =0x0037D70D;
	memcpy(&(pkt.data[0]), &cardNOOfPrivilege, 4);
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

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
			log("1.11 Permissions to add or modify	 Success...");
			success =1;
		}
	}

	//1.15	Permission Query[Function Number: 0x5A] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x5A;
	pkt.iDevSn = controllerSN; 
	// (The Chaka is 0D D7 37 00Competence)
	long long cardNOOfPrivilegeToQuery =0x0037D70D;
	memcpy(&(pkt.data[0]), &cardNOOfPrivilegeToQuery, 4);

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{

		long long cardNOOfPrivilegeToGet=0;
		memcpy(&cardNOOfPrivilegeToGet, &(pkt.recv[8]),4);
		if (cardNOOfPrivilegeToGet == 0)
		{
			//When no permission: (The card number is0)
			log ("1.15      Can not open message: (The card number is0)");
		}
		else
		{
			//Specific Permission Information...
			log ("1.15     Can not open message...");
		}
		log("1.15 Permission Query	 Success...");
		success =1;
	}

	//1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x5C;
	pkt.iDevSn = controllerSN; 
	long long QueryIndex = 1; //Index number(From1Start);
	memcpy(&(pkt.data[0]), &QueryIndex, 4);

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{

		long long cardNOOfPrivilegeToGet=0;
		memcpy(&cardNOOfPrivilegeToGet, &(pkt.recv[8]),4);
		if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFFResponse4294967295
		{
			log ("1.16      Can not open message: (Permissions deleted)");
		}
		else if (cardNOOfPrivilegeToGet == 0)
		{
			//When no permission: (The card number is0)
			log ("1.16       Can not open message: (The card number is0)--This index number is no longer valid.");
		}
		else
		{
			//Specific Permission Information...
			 log ("1.16      Can not open message...");
		}
		log("1.16 Access to specified index numbers	 Success...");
		success =1;
	}


	//1.17	Set door control parameters(Online/Delay) [Function Number: 0x80] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x80;
	pkt.iDevSn = controllerSN; 
	//(Settings2Door. Online  Open the door late. 3sec)
	pkt.data[0] = 0x02; //2Door.
	pkt.data[1] = 0x03; //Online
	pkt.data[2] = 0x03; //Open the door late.

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
//2013-11-06 15:33:16		if (memcmp(&(pkt.data[0]), &(pkt.recv[8]),4) == 0)
		if (memcmp(&(pkt.data[0]), &(pkt.recv[8]),3) == 0) //2013-11-06 15:33:23  Compare three.
		{
			//When successful, Return values to match settings
			log("1.17 Set door control parameters	 Success...");
			success =1;			
		}
		else
		{
			//Failed
		}
	}

	//1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        //This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        //Suggested number of privileges updated over50individual, Use this command

        log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	Start...");
        log("       1Thousand powers...");

        //Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
        int cardCount = 10000;  //2015-06-09 20:20:20 Total number of cards
        long cardArray[10000];
        for (int i = 0; i < cardCount; i++)
        {
            cardArray[i] = 50001+i;
        }

        for (int i = 0; i < cardCount; i++)
        {
            pkt.Reset();
            pkt.functionID = 0x56;
			pkt.iDevSn = controllerSN; 

            cardNOOfPrivilege = cardArray[i];
	    	memcpy(&(pkt.data[0]), &cardNOOfPrivilege, 4);
                
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

			memcpy(&(pkt.data[32-8]), &cardCount, 4);//Total permissions
			int i2=i+1;
			memcpy(&(pkt.data[35-8]), &i2, 4);//The index place for the current permission(From1Start)

            ret = pkt.run(udp);
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    success = 1;
                }
                if (pkt.recv[8] == 0xE1)
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
            log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Failed...????");
        }
           



	//Other instructions  **********************************************************************************


	// **********************************************************************************

	//End  **********************************************************************************
	udp.close();
	return success;
}

//ControllerIP Controls set upIPAddress
//controllerSN Setd controller serial number
//watchServerIP   Server to set upIP
//watchServerPort Port to set up
int testWatchingServer(char *ControllerIP, unsigned int controllerSN, char *watchServerIP,int watchServerPort)  //Receiving Server Test -- Settings
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


	WGPacketShort pkt;
	//1.18	Set up the receiver serverIPand Port [Function Number: 0x90] **********************************************************************************
	//	From the receiver.IP: 192.168.168.101  [Current computerIP]
	//(If you don't want the controller to send the data,, As long as you're receiving the server.IPSet as0.0.0.0 There you go.)
	//Port of receiving server: 61005
	//Every5Seconds sent once.: 05
	pkt.Reset();
	pkt.functionID = 0x90;
	pkt.iDevSn = controllerSN; 

	//ServersIP: 192.168.168.101
	//pkt.data[0] = 192; 
	//pkt.data[1] = 168; 
	//pkt.data[2] = 168; 
	//pkt.data[3] = 101; 
	ACE_INET_Addr watchServer_addr (watchServerPort, watchServerIP); //Port  IPAddress
	unsigned int iwatchServerIPInfo = watchServer_addr.get_ip_address();
	pkt.data[0] = (iwatchServerIPInfo >> 24) & 0xff;   
	pkt.data[1] = (iwatchServerIPInfo >> 16) & 0xff;   
	pkt.data[2] = (iwatchServerIPInfo >> 8) & 0xff;  
	pkt.data[3] = iwatchServerIPInfo & 0xff; 


	//Port of receiving server: 61005
	pkt.data[4] =(watchServerPort & 0xff);
	pkt.data[5] =(watchServerPort >>8) & 0xff;

	//Every5Seconds sent once.: 05 (Periodically upload information as5sec [Every time running properly5Seconds sent once.  Send it when you have a brush card])
	pkt.data[6] = 5;

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		if (pkt.recv[8] == 1)
		{
			log("1.18 Set up the receiver serverIPand Port 	 Success...");
			success =1;
		}
	}


	//1.19	Read the receiver server.IPand Port [Function Number: 0x92] **********************************************************************************
	pkt.Reset();
	pkt.functionID = 0x92;
	pkt.iDevSn = controllerSN; 

	ret = pkt.run(udp);
	success =0;
	if (ret >0)
	{
		log("1.19 Read the receiver server.IPand Port 	 Success...");
		success =1;
	}
	udp.close();
	return 1;
}

int WatchingServerRuning (char *watchServerIP,int watchServerPort)
{
	//Watch the firewall. You have to allow all packages at this port to enter.
  ACE_INET_Addr server_addr (static_cast<u_short> (watchServerPort),watchServerIP); 
  ACE_SOCK_Dgram_Bcast udp (server_addr);
  unsigned char buff[WGPacketShort::WGPacketSize];
  size_t buflen = sizeof (buff);
  ssize_t recv_cnt;
  log("Enter receiving server surveillance status....");
  unsigned int recordIndex = 0;
  while(true)
  {
      ACE_INET_Addr any_addr;
	  recv_cnt = udp.recv(buff, buflen, any_addr); 
	  if (recv_cnt > 0)
	  {
		if (recv_cnt == WGPacketShort::WGPacketSize)
		{
			if (buff[1]== 0x20) //
			{
				unsigned int sn;
				unsigned int recordIndexGet;
				memcpy(&sn, &( buff[4]),4);
				printf("Received from controllerSN = %d Packages..\r\n", sn);

				memcpy(&recordIndexGet, &(buff[8]),4);
				if (recordIndex < recordIndexGet)
				{
					recordIndex = recordIndexGet;
					displayRecordInformation(buff);				
				}
			}
		}
	  }
  }
  udp.close ();
  return 0;
}
