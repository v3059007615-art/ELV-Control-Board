package org.wiegand.TestZone;

import java.io.IOException;
import java.net.InetSocketAddress;

import java.util.Calendar;
import java.util.LinkedList;

import java.util.Queue;


import org.apache.mina.transport.socket.DatagramSessionConfig;
import org.apache.mina.transport.socket.nio.NioDatagramAcceptor;

import org.wiegand.at8000.WgUdpCommShort;

public class TestShort {
	/**
	* AT8000_Java 2015-04-30 12:47:48 karl CSN Chan Shonin $
	*
	* Doorbar controller Shortcast agreement Test cases
	* V2.1 Version  2013-11-09
	*            Main use MINACompleted
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
	* V2.5 Version  2015-04-30   Adopt V6.56Driver Version Model by0x19For0x17
	* V2.6 Version  2017-11-01   Add delay on data reception
	        long times = 100; 
		    	try {
					 Thread.sleep(times);
				  } catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				  }  //2017-11-01 14:45:57 Increase delay
	*/
	/**
	 * @param args
	 */
	public static void main(String[] args) {
		
		//This case Not available Search controller  and SettingsIPWork  (Directly byIPSet tools to complete)
		//Test instructions in this case
		//controllerSN  = 229999901
		//controllerIP  = 192.168.168.123
		//Computer  IP  = 192.168.168.101
		//For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)
		long controllerSN = 229999901;
		String controllerIP = "192.168.168.123";
		String watchServerIP = "192.168.168.101";
		int watchServerPort = 61005;

	
		int ret =0;
		
		log("Test started....");
		
		ret = testBasicFunction(controllerIP,controllerSN ); //Basic function test
		if (ret ==0 )
		{
			log("Basic function test Failed...");
			log("Test is over....");
			return;
		}

		ret = testWatchingServer(controllerIP,controllerSN,watchServerIP, watchServerPort); //Receiving Server Settings

		ret = WatchingServerRuning(watchServerIP, watchServerPort); //Server Run....

		log("Test is over....");
	}

	public static void log(String info) //Log Information
	{
		System.out.println(info);
	}

	public static byte GetHex(int val) //AccessHexValue, Mainly used in date time format
    {
	    return (byte)((val % 10) + (((val -(val % 10)) / 10)%10) *16);
    }
	
	
    /// <summary>
    /// Show Record Information
    /// </summary>
    /// <param name="recvBuff"></param>
	public static void displayRecordInformation(byte[] recvBuff)
    {

      //8-11	Index number for the last record.
		//(=0No record.)	4	0x00000000
		long recordIndex = WgUdpCommShort.getLongByByte(recvBuff, 8, 4);


		//12	Record type
		//0=No record
		//1=Brush Card Record
		//2=Door Magnetic,button, Device startup, Remote Open Record
		//3=Call the police.	1	
		int recordType =WgUdpCommShort.getIntByByte(recvBuff[12]);

		//13	Validity(0 Not approved, 1Adopted)	1	
		int recordValid = WgUdpCommShort.getIntByByte(recvBuff[13]);

		//14	Door number.(1,2,3,4)	1	
		int recordDoorNO = WgUdpCommShort.getIntByByte(recvBuff[14]);

		//15	Come in./Out.(1It means coming in., 2Means out.)	1	0x01
		int recordInOrOut = WgUdpCommShort.getIntByByte(recvBuff[15]);

		//16-19	Card(Type is when swiping a card.)
		//or numbering(Other types of records)	4	
		long  recordCardNO =WgUdpCommShort.getLongByByte(recvBuff, 16, 4);

		
		//20-26	Brush Time:
		//Days and days of year (AdoptBCDCode)See description of the set-up segment
		String recordTime=  String.format("%02X%02X-%02X-%02X %02X:%02X:%02X", 
			WgUdpCommShort.getIntByByte(recvBuff[20]),
			WgUdpCommShort.getIntByByte(recvBuff[21]),
			WgUdpCommShort.getIntByByte(recvBuff[22]),
			WgUdpCommShort.getIntByByte(recvBuff[23]),
			WgUdpCommShort.getIntByByte(recvBuff[24]),
			WgUdpCommShort.getIntByByte(recvBuff[25]),
			WgUdpCommShort.getIntByByte(recvBuff[26]));

		//2012.12.11 10:49:59	7	
		//27	Record cause code(You can check it out. "Checkcard log notes.xls"It's a file.ReasonNO)
		//It's only for complex information.	1	
		int reason = WgUdpCommShort.getIntByByte(recvBuff[27]);
		
        //0=No record
        //1=Brush Card Record
        //2=Door Magnetic,button, Device startup, Remote Open Record
        //3=Call the police.	1	
        //0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        if (recordType == 0)
        {
            log(String.format("Index post=%u  No record", recordIndex));
        }
        else if (recordType == 0xff)
        {
            log(" The records of the specified index have been overwritten,Use the index.0, Retrieving index values from the earliest record");
        }
        else if (recordType == 1) //2015-06-10 08:49:31 Show data with card number type of record
        {
            //Card
            log(String.format("Index post=%d  ", recordIndex));
            log(String.format("  Card = %d", recordCardNO));
            log(String.format("  Door number. = %d", recordDoorNO));
            log(String.format("  Access = %s", recordInOrOut == 1 ? "Come in." : "Out."));
            log(String.format("  Valid. = %s", recordValid == 1 ? "Pass." : "Ban"));
            log(String.format("  Time = %s", recordTime));
            log(String.format("  Description = %s", getReasonDetailChinese(reason)));
        }
        else if (recordType == 2)
        {
            //Other processing
            //Door Magnetic,button, Device startup, Remote Open Record
            log(String.format("Index post=%d  Non-card records", recordIndex));
            log(String.format("  Numbering = %d", recordCardNO));
            log(String.format("  Door number. = %d", recordDoorNO));
            log(String.format("  Time = %s", recordTime));
            log(String.format("  Description = %s", getReasonDetailChinese(reason)));
        }
        else if (recordType == 3)
        {
            //Other processing
            //Call the police.
            log(String.format("Index post=%d  Call the police.", recordIndex));
            log(String.format("  Numbering = %d", recordCardNO));
            log(String.format("  Door number. = %d", recordDoorNO));
            log(String.format("  Time = %s", recordTime));
            log(String.format("  Description = %s", getReasonDetailChinese(reason)));
        }
    }

	public static  String RecordDetails[] =
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

    public static   String getReasonDetailChinese(int Reason) //Chinese
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

    public static String getReasonDetailEnglish(int Reason) //English Description
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
    
	@SuppressWarnings("unused")
	public static int 	 testBasicFunction(String controllerIP, long controllerSN)  //Basic function test
	{
		byte[] recvBuff;
		int success =0;
		WgUdpCommShort pkt = new WgUdpCommShort();
		pkt.iDevSn = controllerSN;
		
		
		log(String.format("controllerSN = %d \r\n", controllerSN));
		
		//OpenudpConnection
		pkt.CommOpen(controllerIP);
		
		//1.4	Query controller status[Function Number: 0x20](Real time surveillance) **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x20;
		pkt.iDevSn = controllerSN; 
		recvBuff = pkt.run();

		success =0;
		if (recvBuff != null)
		{
			//Read information successfully...
			success =1;
			log("1.4 Query controller status Success...");

			//	  	Last recorded information		
			displayRecordInformation(recvBuff); 

			//	Other information		
			int[] doorStatus=new int[4];
			//28	1Door no.(0Means close, 1Show Open)	1	0x00
			doorStatus[1-1] = WgUdpCommShort.getIntByByte(recvBuff[28]);
			//29	2Door no.(0Means close, 1Show Open)	1	0x00
			doorStatus[2-1] = WgUdpCommShort.getIntByByte(recvBuff[29]);
			//30	3Door no.(0Means close, 1Show Open)	1	0x00
			doorStatus[3-1] = WgUdpCommShort.getIntByByte(recvBuff[30]);
			//31	4Door no.(0Means close, 1Show Open)	1	0x00
			doorStatus[4-1] = WgUdpCommShort.getIntByByte(recvBuff[31]);

			int[] pbStatus= new int[4];
			//32	1Door button.(0It means you let go., 1Means press)	1	0x00
			pbStatus[1-1] = WgUdpCommShort.getIntByByte(recvBuff[32]);
			//33	2Door button.(0It means you let go., 1Means press)	1	0x00
			pbStatus[2-1] = WgUdpCommShort.getIntByByte(recvBuff[33]);
			//34	3Door button.(0It means you let go., 1Means press)	1	0x00
			pbStatus[3-1] = WgUdpCommShort.getIntByByte(recvBuff[34]);
			//35	4Door button.(0It means you let go., 1Means press)	1	0x00
			pbStatus[4-1] = WgUdpCommShort.getIntByByte(recvBuff[35]);
			//36	Fault.
			//equals0 No malfunctions.
			//Not equal to0, It's not working.(Reset Time, If there's anything else,, We're going back to the factory.)	1	
			int errCode = WgUdpCommShort.getIntByByte(recvBuff[36]);
			//37	Control Current Time
			//Time	1	0x21
			//38	min	1	0x30
			//39	sec	1	0x58

			//40-43	Water Stream	4	
			long   sequenceId= WgUdpCommShort.getLongByByte(recvBuff, 40, 4);

			//48
			//Special Information1(Return based on actual use)
			//Keyboard Key Information	1	
			//49	Relay status	1	
			int relayStatus = WgUdpCommShort.getIntByByte(recvBuff[49]);
			//50	Door magnetic.8-15bitbit[Fire!/Force locking]
			//Bit0  Force locking
			//Bit1  Fire!		
			int otherInputStatus = WgUdpCommShort.getIntByByte(recvBuff[50]);
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

			String controllerTime; //Control Current Time
			controllerTime= String.format("20%02X-%02X-%02X %02X:%02X:%02X", 
				WgUdpCommShort.getIntByByte(recvBuff[51]),
				WgUdpCommShort.getIntByByte(recvBuff[52]),
				WgUdpCommShort.getIntByByte(recvBuff[53]),
				WgUdpCommShort.getIntByByte(recvBuff[37]),
				WgUdpCommShort.getIntByByte(recvBuff[38]),
				WgUdpCommShort.getIntByByte(recvBuff[39]));
		}
		else
		{
			log("1.4 Query controller status Failed...");
			return 0;
		}



		//1.5	Read Date Time(Function Number: 0x32) **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x32;
		pkt.iDevSn = controllerSN; 
		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			String controllerTime; //Control Current Time
			controllerTime= String.format("%02X%02X-%02X-%02X %02X:%02X:%02X", 
				WgUdpCommShort.getIntByByte(recvBuff[8]),WgUdpCommShort.getIntByByte(recvBuff[9]),WgUdpCommShort.getIntByByte(recvBuff[10]),WgUdpCommShort.getIntByByte(recvBuff[11]),WgUdpCommShort.getIntByByte(recvBuff[12]),WgUdpCommShort.getIntByByte(recvBuff[13]),WgUdpCommShort.getIntByByte(recvBuff[14]));

			log("1.5 Read Date Time Success...");
			//log(controllerTime);
			success =1;
		}

		//1.6	Set Date Time[Function Number: 0x30] **********************************************************************************
		//calibrate controller at computer time.....
		pkt.Reset();
		pkt.functionID = (byte) 0x30;
		pkt.iDevSn = controllerSN; 

		Calendar cal = (Calendar.getInstance());
   
		pkt.data[0] =GetHex((int)(( cal.get(Calendar.YEAR) -(cal.get(Calendar.YEAR)%100))/100)); 
		pkt.data[1] =GetHex((int)(( cal.get(Calendar.YEAR))%100)); //st.GetMonth()); 
		pkt.data[2] =GetHex( cal.get(Calendar.MONTH) + 1); 
		pkt.data[3] =GetHex(cal.get(Calendar.DAY_OF_MONTH)); 
		pkt.data[4] =GetHex(cal.get(Calendar.HOUR_OF_DAY)); 
		pkt.data[5] =GetHex(cal.get(Calendar.MINUTE)); 
		pkt.data[6] = GetHex(cal.get(Calendar.SECOND)); 
		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			success =1;
			for(int i=0; i<7; i++)
			{
				if(pkt.data[i] != recvBuff[8+i])
				{
					success = 0;
					break;
				}	
			}
			if (success >0)
			{
				log("1.6 Set Date Time Success...");
			}
		}

		//1.7	Get a record of the given index number[Function Number: 0xB0] **********************************************************************************
		//(Take Index Number 0x00000001Records)
		int  recordIndexToGet =0;
		pkt.Reset();
		pkt.functionID =(byte) 0xB0;
		pkt.iDevSn = controllerSN; 

		//	(Special
		//If=0, Retrieving the earliest recorded information
		//If=0xffffffffRetrieving information from the last record)
		//Records index numbers are normally incremental., Max.0xffffff = 16,777,215 (Over1Millions.) . Due to limited storage space, Only the closest on the controller.20Thousands of records.. When index numbers exceed20After 10,000., The records of the old index numbers are overwritten., So at this point, check the records of these index numbers., The type of record returned will be0xff, It means it doesn't exist..
		recordIndexToGet =1;
 	    System.arraycopy(WgUdpCommShort.longToByte(recordIndexToGet) , 0, pkt.data, 0, 4);
		

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			log("1.7 Get Index As1Recorded information	 Success...");
			//	  	Index to1Recorded information		
			displayRecordInformation(recvBuff); 

			success =1;
		}

		//. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
		pkt.Reset();
		pkt.functionID = (byte) 0xB0;
		pkt.iDevSn = controllerSN; 
		recordIndexToGet =0;
 	    System.arraycopy(WgUdpCommShort.longToByte(recordIndexToGet) , 0, pkt.data, 0, 4);

 	    recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			log("1.7 Fetch information from the earliest record	 Success...");
			//	  	First recorded information		
			displayRecordInformation(recvBuff); 
			success =1;
		}

		//Communication (Take the latest record. By Index 0xffffffff)
		pkt.Reset();
		pkt.functionID = (byte) 0xB0;
		pkt.iDevSn = controllerSN; 
		recordIndexToGet =0xffffffff;
 	    System.arraycopy(WgUdpCommShort.longToByte(recordIndexToGet) , 0, pkt.data, 0, 4);

 	    recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			log("1.7 Access to the latest recorded information	 Success...");
			//	  	Latest recorded information		
			displayRecordInformation(recvBuff); 

			success =1;
		}

//		//1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
//		pkt.Reset();
//		pkt.functionID = (byte) 0xB2;
//		pkt.iDevSn = controllerSN; 
//		// (Set read record index number as5)
//		long recordIndexGotToSet = 0x5;
// 	    System.arraycopy(WgUdpCommShort.longToByte(recordIndexGotToSet) , 0, pkt.data, 0, 4);
//
//		//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
// 	    System.arraycopy(WgUdpCommShort.longToByte(WgUdpCommShort.SpecialFlag) , 0, pkt.data, 4, 4);
//
//		recvBuff = pkt.run();
//		success =0;
//		if (recvBuff != null)
//		{
//			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
//			{
//				log("1.8 Set a read record index number	 Success...");
//				success =1;
//			}
//		}
//
//		//1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
//		pkt.Reset();
//		pkt.functionID = (byte) 0xB4;
//		pkt.iDevSn = controllerSN; 
//		long recordIndexGotToRead =0x0;
//		recvBuff = pkt.run();
//		success =0;
//		if (recvBuff != null)
//		{
//			recordIndexGotToRead = WgUdpCommShort.getLongByByte(recvBuff, 8, 4);
//			log("1.9 Get read record index numbers	 Success...");
//			success =1;
//		}

		//1.9	Extract Record Operation
		//1. Pass. 0xB4Command Get read record index numbers recordIndex
		//2. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
		//3. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
		//After three steps,， The entire extraction record is complete.
	    log("1.9 Extract Record Operation	 Start...");
		pkt.Reset();
		pkt.functionID = (byte) 0xB4;
		pkt.iDevSn = controllerSN; 
		long recordIndexGot4GetSwipe =0x0;
		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			recordIndexGot4GetSwipe= WgUdpCommShort.getLongByByte(recvBuff, 8, 4);
			pkt.Reset();
			pkt.functionID = (byte) 0xB0;
			pkt.iDevSn = controllerSN; 
			long recordIndexToGetStart = recordIndexGot4GetSwipe + 1;
			long recordIndexValidGet = 0;
			int cnt=0;
			do
			{
				System.arraycopy(WgUdpCommShort.longToByte(recordIndexToGetStart) , 0, pkt.data, 0, 4);
				recvBuff = pkt.run();
				success =0;
				if (recvBuff != null)
				{
					success =1;

					//12	Record type
					//0=No record
					//1=Brush Card Record
					//2=Door Magnetic,button, Device startup, Remote Open Record
					//3=Call the police.	1	
					//0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
					int recordType = WgUdpCommShort.getIntByByte(recvBuff[12]);
					if (recordType == 0)
					{
						break; //No more records.
					}
					
					if (recordType == 0xff)
					{
						success = 0;   //This index number is invalid  Reset Index Values
						//Take the earliest record index bit
						recordIndexToGet =0;
				 	    System.arraycopy(WgUdpCommShort.longToByte(recordIndexToGet) , 0, pkt.data, 0, 4);

				 	    recvBuff = pkt.run();
						success =0;
						if (recvBuff != null)
						{
							log("1.7 Fetch information from the earliest record	 Success...");
							//	  	First recorded information		
							
							success =1;
							long recordIndex =0;
			                recordIndex  = WgUdpCommShort.getLongByByte(recvBuff,8, 4);
	                        recordIndexToGetStart = recordIndex;
	                        continue;
						}
						
						
	                    success = 0;  
						break; 
					}
					recordIndexValidGet = recordIndexToGetStart;
					//.......Storage of records received
					 displayRecordInformation(recvBuff);
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
				pkt.functionID = (byte) 0xB2;
				pkt.iDevSn = controllerSN; 
				System.arraycopy(WgUdpCommShort.longToByte(recordIndexValidGet) , 0, pkt.data, 0, 4);
				
				//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
		 	    System.arraycopy(WgUdpCommShort.longToByte(WgUdpCommShort.SpecialFlag) , 0, pkt.data, 4, 4);

				recvBuff = pkt.run();
				success =0;
				if (recvBuff != null)
				{
					if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
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
		pkt.functionID = (byte) 0x40;
		pkt.iDevSn = controllerSN; 
		pkt.data[0] =(byte) (doorNO & 0xff); //2013-11-03 20:56:33
		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
			{
				//Open the door effectively......
				log("1.10 Open remote	 Success...");
				success =1;
			}
		}

		//1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
		//Add card number0D D7 37 00, Through all doors of the current controller
		pkt.Reset();
		pkt.functionID = (byte) 0x50;
		pkt.iDevSn = controllerSN; 
		//0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
		long cardNOOfPrivilege =0x0037D70D;
		//memcpy(&(pkt.data[0]), &cardNOOfPrivilege, 4);
		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilege) , 0, pkt.data, 0, 4);
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

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
			{
				//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
				log("1.11 Permissions to add or modify	 Success...");
				success =1;
			}
		}

		//1.12	Permission to delete(Individual Delete)[Function Number: 0x52] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x52;
		pkt.iDevSn = controllerSN; 
		//Permission card number to delete0D D7 37 00  = 0x0037D70D = 3659533 (Decimal)
		long cardNOOfPrivilegeToDelete =0x0037D70D;
		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilegeToDelete) , 0, pkt.data, 0, 4);

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
			{
				//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay doesn't move..
				log("1.12 Permission to delete(Individual Delete)	 Success...");
				success =1;
			}
		}

		//1.13	Clear Permissions(Clear it all.)[Function Number: 0x54] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x54;
		pkt.iDevSn = controllerSN; 
		//12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
 	    System.arraycopy(WgUdpCommShort.longToByte(WgUdpCommShort.SpecialFlag) , 0, pkt.data, 0, 4);

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
			{
				//It's time to clear up.
				log("1.13 Clear Permissions(Clear it all.)	 Success...");
				success =1;
			}
		}

		//1.14	Total Permissions Read[Function Number: 0x58] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x58;
		pkt.iDevSn = controllerSN; 
		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			long privilegeCount  = WgUdpCommShort.getLongByByte(recvBuff,8, 4);
			log("1.14 Total Permissions Read	 Success...");

			success =1;
		}

		
		//Add again as a query operation 1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
		//Add card number0D D7 37 00, Through all doors of the current controller
		pkt.Reset();
		pkt.functionID = (byte) 0x50;
		pkt.iDevSn = controllerSN; 
		//0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
		//long 
		cardNOOfPrivilege =0x0037D70D;
		
		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilege) , 0, pkt.data, 0, 4);
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

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
			{
				//And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
				log("1.11 Permissions to add or modify	 Success...");
				success =1;
			}
		}
		
		//1.15	Permission Query[Function Number: 0x5A] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x5A;
		pkt.iDevSn = controllerSN; 
		// (The Chaka is 0D D7 37 00Competence)
		long cardNOOfPrivilegeToQuery =0x0037D70D;
		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilegeToQuery) , 0, pkt.data, 0, 4);

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{

			long cardNOOfPrivilegeToGet = WgUdpCommShort.getLongByByte(recvBuff,8, 4);
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
		pkt.functionID = (byte) 0x5C;
		pkt.iDevSn = controllerSN; 
		
		cardNOOfPrivilegeToQuery =1;  //Index number(From1Start)
		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilegeToQuery) , 0, pkt.data, 0, 4);

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{

			long cardNOOfPrivilegeToGet = WgUdpCommShort.getLongByByte(recvBuff,8, 4);
			if (cardNOOfPrivilegeToGet == 4294967295l) //'FFFFFFFFResponse4294967295
			{
				//When no permission: (The card number is0)
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
				log ("1.16     Can not open message...");

			}
			log("1.15 Permission Query	 Success...");
			success =1;
		}
		


		//1.17	Set door control parameters(Online/Delay) [Function Number: 0x80] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x80;
		pkt.iDevSn = controllerSN; 
		//(Settings2Door. Online  Open the door late. 3sec)
		pkt.data[0] = 0x02; //2Door.
		pkt.data[1] = 0x03; //Online
		pkt.data[2] = 0x03; //Open the door late.

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			success =1;		
			for(int i=0; i<3; i++)
			{
				if (pkt.data[i] != recvBuff[8+i])
				{
				  success = 0;
				   break;
				}
			}
			if (success > 0)
			{
				//When successful, Return values to match settings
				log("1.17 Set door control parameters	 Success...");
				success =1;			
			}
		}
		
		
		//1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        //This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        //Suggested number of privileges updated over50individual, Use this command

        log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	Start...");
        log("       1Thousand powers...");

        //Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
        int cardCount = 10000;  //2015-06-09 20:20:20 Total number of cards
        long cardArray[]= new long[10000];
        for (int i = 0; i < cardCount; i++)
        {
            cardArray[i] = 50001+i;
        }

        for (int i = 0; i < cardCount; i++)
        {
        	pkt.Reset();
    		pkt.functionID = (byte) 0x56;
    		pkt.iDevSn = controllerSN; 
    		
    		cardNOOfPrivilege =cardArray[i];
    		
    		System.arraycopy(WgUdpCommShort.longToByte(cardNOOfPrivilege) , 0, pkt.data, 0, 4);
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

    		
    		System.arraycopy(WgUdpCommShort.longToByte(cardCount) , 0, pkt.data, 32-8, 4);//Total permissions
			int i2=i+1;
			System.arraycopy(WgUdpCommShort.longToByte(i2) , 0, pkt.data, 35-8, 4);//The index place for the current permission(From1Start)

    		recvBuff = pkt.run();
    		success =0;
    		if (recvBuff != null)
    		{
    			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 1)
    			{
    				success =1;
    			}
    			if (WgUdpCommShort.getIntByByte(recvBuff[8]) == 0xE1)
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

		
		//End  **********************************************************************************

		//CloseudpConnection
		pkt.CommClose();
		
		return success;
	}

	
	//controllerIP Controls set upIPAddress
	//controllerSN Setd controller serial number
	//watchServerIP   Server to set upIP
	//watchServerPort Port to set up
	@SuppressWarnings("unused")
	public static int testWatchingServer(String controllerIP, long controllerSN, String watchServerIP,int watchServerPort)  //Receiving Server Test -- Settings
	{
		byte[] recvBuff;
		int success =0;  //0 Failed, 1It means success.
		WgUdpCommShort pkt = new WgUdpCommShort();
		pkt.iDevSn = controllerSN;
		
		//OpenudpConnection
		pkt.CommOpen(controllerIP);
		
		//1.18	Set up the receiver serverIPand Port [Function Number: 0x90] **********************************************************************************
		//	From the receiver.IP: 192.168.168.101  [Current computerIP]
		//(If you don't want the controller to send the data,, As long as you're receiving the server.IPSet as0.0.0.0 There you go.)
		//Port of receiving server: 61005
		//Every5Seconds sent once.: 05
		pkt.Reset();
		pkt.functionID = (byte)0x90;
		pkt.iDevSn = controllerSN;


			
		//ServersIP: 192.168.168.101
		//pkt.data[0] = 192; 
		//pkt.data[1] = 168; 
		//pkt.data[2] = 168; 
		//pkt.data[3] = 101; 
		String[] ip;
        ip= watchServerIP.split("\\.");
        if (ip.length == 4)
        {
			pkt.data[0] =  (byte)Integer.parseInt(ip[0]);   
			pkt.data[1] =  (byte)Integer.parseInt(ip[1]);   
			pkt.data[2] =  (byte)Integer.parseInt(ip[2]);  
			pkt.data[3] =  (byte)Integer.parseInt(ip[3]); 
        }

		//Port of receiving server: 61005
		pkt.data[4] =(byte)(watchServerPort & 0xff);
		pkt.data[5] =(byte)((watchServerPort >>8) & 0xff);

		//Every5Seconds sent once.: 05 (Periodically upload information as5sec [Every time running properly5Seconds sent once.  Send it when you have a brush card])
		pkt.data[6] = 5;

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			if (recvBuff[8] == 1)
			{
				log("1.18 Set up the receiver serverIPand Port 	 Success...");
				success =1;
			}
		}


		//1.19	Read the receiver server.IPand Port [Function Number: 0x92] **********************************************************************************
		pkt.Reset();
		pkt.functionID = (byte) 0x92;
		pkt.iDevSn = controllerSN; 

		recvBuff = pkt.run();
		success =0;
		if (recvBuff != null)
		{
			log("1.19 Read the receiver server.IPand Port 	 Success...");
			success =1;
		}
		
		//CloseudpConnection
		pkt.CommClose();
		return 1;
	}

	public static int WatchingServerRuning(String watchServerIP,int watchServerPort) // Access to server monitoring status
	{
		Queue<byte[]> queue = new LinkedList<byte[]>();
		
		 // CreateUDPPackageNIO
        NioDatagramAcceptor acceptor = new NioDatagramAcceptor();
        // NIOSet BottomIOHandler
        acceptor.setHandler(new WatchingShortHandler(queue));

        // Set whether to reuse the address？ That's everything.udpIt's all an address.？
        DatagramSessionConfig dcfg = acceptor.getSessionConfig();
        dcfg.setReuseAddress(true);

        // Tie Port Address
        try {
			acceptor.bind(new InetSocketAddress(watchServerIP, watchServerPort));
		} catch (IOException e) {
            log("Failed to bind receiver....");
			e.printStackTrace();
			return 0;
		}
        log("Enter receiving server surveillance status....[If inwin7Use below Be careful with the firewall.]");
        
      long recordIndex = 0;
	  while(true)
	  {
		  if (!queue.isEmpty())
		    {
		         byte[] recvBuff;
		         synchronized (queue)
		         {
		        	 recvBuff= queue.poll();
		         }
		         if (recvBuff[1]== 0x20)
						{
							long sn = WgUdpCommShort.getLongByByte(recvBuff, 4, 4);
							long recordIndexGet = WgUdpCommShort.getLongByByte(recvBuff, 8, 4);
							log(String.format("Received from controllerSN = %d Packages..", sn));

							if (recordIndex < recordIndexGet)
							{
								recordIndex = recordIndexGet;
								displayRecordInformation(recvBuff); 
							}
						}
		    }
		    else
		    {
          long times = 100; 
		    	try {
					 Thread.sleep(times);
				  } catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				  }  //2017-11-01 14:45:57 Increase delay
		    }	     
	  }
// If Nope.while(true), Opens the comments below...	  
//	  acceptor.unbind();
//	  acceptor.dispose();
//	  return 0;
	}

}
    


    

