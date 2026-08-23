/**
* WGController32 2015-04-30 17:40:43 karl CSN Chan Shonin $
*
* Doorbar controller Shortcast agreement Test cases
* V2.7 Version  2015-12-05 17:18:00 V6.62Driver Version Increase CommunicationsSM4Communications password testing
* V2.6 Version  2015-11-03 20:25:53 V6.60Driver Version Increase Communications password testing, 1024Bytes for permission upload and log extraction operations  
*                               Retry to modify communication
*                               
* V2.5 Version  2015-04-29 20:41:30 Adopt V6.56Driver Version Model by0x19For0x17
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
            //'    'No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
            //'    'Test instructions in this case
            //'    'controllerSN  = 229999901
            //'    'controllerIP  = 192.168.168.123
            //'    'Computer  IP  = 192.168.168.101
            //'    'For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)

            //Basic function test
            //txtSN.Text controller9Series of bitsSN
            //txtIP.Text controllerIPAddress, Not adopted192.168.168.123  [Available Search Controller Modify controllerIP]
  
            //txtIP.Text = "10.0.1.123";// txtIP.Text;
            //txtSN.Text = "239999901"; // long.Parse(txtSN.Text);

            //2015-12-02 19:38:44 
            String ControllerIP=txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);


            log("SM4 ECB Communications Test...");

            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.


            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;
            byte[] commPassword = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16Byte Password


            //Set Communications Password SM4 ECB[Function Number: 0xE0] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xE0;
            //Ideas against error
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
            for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00Set New Password
            {
                pkt.data[4 + i] = commPassword[i];
                pkt.data[36 + i] = commPassword[i];
            }

            //In two cases.: Password is empty  Or... Password set
            ret = pkt.run();  //2015-11-02 10:21:22 Try the controller without password first.
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
                ret = pkt.run_ECB(commPassword);  //2015-11-02 10:21:22 Try the password operation for the controller.
                if ((ret > 0) && (pkt.recv[8] == 1))
                {
                    log("Communication password set successfully...[Can not open message]");
                    success = 1;
                }
                else
                {
                    log("Communication password setup failed...[Can not open message]");
                }
            }

            //Communication using coded communications
            //1.10	Open remote[Function Number: 0x40] **********************************************************************************
            int doorNO = 1;
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNO & 0xff); //2013-11-03 20:56:33
            ret = pkt.run_ECB(commPassword);
            success = 0;
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
                    log("1.10 Open remote	 Failed...[Can not open message]");
                }
            }
            else
            {
                log("1.10 Open remote	 Failed...[Can not open message]");
            }

            //Presentation1024Bytes Encryption Operations...
            byte[] command1024 = new byte[1024];


            //1.9	Extract Record Operation
            //1. Pass. 0xB0Command Fetch the earliest record index
            //2. Pass. 0xB0Command Get Last Record Index
            //3. Pass. 0xB4Command Get read record index numbers recordIndex
            //4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
            //5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
            //After the top steps,， The entire extraction record is complete.
            long firstRecordIndex = 0;  //First record index number
            long lastRecordIndex = 0;   //Last record index number.
            long recordIndexGotToRead = 0x0;
            long recordIndexToGet = 0;
            log("1.9 Extract Record Operation	 Start...[1024Byte Command]");
            pkt.Reset();
            pkt.functionID = 0xB0;//Take the earliest record index
            recordIndexToGet = 0x0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run_ECB(commPassword);
            success = 0;
            if (ret > 0)
            {
                firstRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" Fetch the earliest record index	 =" + firstRecordIndex.ToString());
            }
            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB0;//Take Last Record Index
                recordIndexToGet = 0xffffffff;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run_ECB(commPassword);
            }
            if (ret > 0)
            {
                lastRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" Get Last Record Index	  =" + lastRecordIndex.ToString());
            }

            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB4;//Get read record index numbers
                recordIndexToGet = 0x0;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run_ECB(commPassword);
            }
            if (ret > 0)
            {
                recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
                log("Get read record index numbers	  =" + recordIndexGotToRead.ToString());
            }
            long validRecordsCount = 0;
            //recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
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
                        LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
                        byte[] cmd = pkt.toByte();
                        Array.Copy(cmd, 0, command1024, j, 64);
                        recordIndexToGetStart++;
                        cnt++;
                    }
                    ret = pkt.run1024_ECB(command1024,commPassword);
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
                            byte[] recv = new byte[64];
                            Array.Copy(pkt.recv, j, recv, 0, 64);
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
                                    Application.DoEvents();
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

                log("1.9 Full extraction successful.	 ... Number of valid records= " + validRecordsCount.ToString());
                if ((success > 0) && validRecordsCount > 0)
                {
                    //Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
                    pkt.Reset();
                    pkt.functionID = 0xB2;
                    LongToBytes(ref pkt.data, 0, recordIndexValidGet);

                    //12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
                    LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

                    ret = pkt.run_ECB(commPassword);
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
            log(string.Format("       {0}Thousand powers...", cardCount / 10000));
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
                    LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

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

                    LongToBytes(ref pkt.data, 32 - 8, cardCount); //Total permissions
                    LongToBytes(ref pkt.data, 35 - 8, i + 1);//The index place for the current permission(From1Start)

                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);
                    i++;

                }
                ret = pkt.run1024_ECB(command1024,commPassword); //2015-11-04 19:15:48 
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

            //long cardNOOfPrivilegeToGetlast = 0;

            //1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
            //Read All Permissions
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long maxCount = 20 * 10000;
            long[] cardArrayGet = new long[maxCount];
            long QueryIndex = 1; //Index number(From1Start);
            LongToBytes(ref pkt.data, 0, QueryIndex);

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
                }
                for (int j = 0; j < 1024; j = j + 64)
                {
                    LongToBytes(ref pkt.data, 0, QueryIndex);
                    QueryIndex++; //Index number(From1Start);
                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);

                }
                ret = pkt.run1024_ECB(command1024, commPassword);
                success = 0;
                if (ret > 0)
                {
                    for (int j = 0; j < 1024; j = j + 64)
                    {
                        success = 0;
                        long cardNOOfPrivilegeToGet = 0;
                        cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8 + j, 4);
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
                }
                else
                {
                    log("1.16     Problem...." + ret.ToString());
                    break;
                }
            }
            log("Last read permission card number = " + cardNOOfPrivilegeToGetlast.ToString());
            log("Permissions extractediCount = " + iCount.ToString());  //2015-11-04 19:59:50 Extract permissions

            //Clear the code.[Function Number: 0xE0] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xE0;
            //Ideas against error
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
            for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
            {
                pkt.data[4 + i] = 0;
                pkt.data[36 + i] = 0;
            }

            ret = pkt.run_ECB(commPassword);
            if ((ret > 0) && (pkt.recv[8] == 1))
            {
                log("Communication code emptied....[Can not open message]");
                success = 1;
            }
            else
            {
                log("Communication password emptied...[Can not open message]");
            }



            // **********************************************************************************

            //End  **********************************************************************************
            pkt.close();  //Close communications
           // return success;


        }


         /// <summary>
        /// Shortcasts
        /// </summary>
        class WGPacketShort
        {
            public  const int WGPacketSize = 64;			    //Length of submission
            //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
            public  const int Type = 0x17;		//2015-04-29 22:22:50			//Type
            public  const int ControllerPort = 60000;        //controller port
            public  const long SpecialFlag = 0x55AAAA55;     //Special identification Prevent mishandling

            public int functionID;		                     //Function Number
            public long iDevSn;                              //Device serial number 4Bytes, 9Digits
            public string IP;                                //The controller.IPAddress

            public byte[] data = new byte[56];               //56Byte Data [Fluid]
            //public byte[] recv = new byte[WGPacketSize];     //Data received
            public byte[] recv = new byte[1024]; //WGPacketSize];     //Data received

            public WGPacketShort()
            {
                Reset();
            }
            public void Reset()  //Data Reunification
            {
                for (int i = 0; i < 56; i++)
                {
                    data[i] = 0;
                }
            }
            static long sequenceId;     //Serial number	
            public byte[] toByte() //Generate64Bytes package
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
            public int run()  //Send Command Can not open message
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
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                        //Water Stream
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //Align type
                            && (recv[1] == functionID)              //Function numbers are consistent
                            && (sequenceIdReceived == sequenceId))  //Serial number corresponding
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                } while (tries-- > 0); //Try again three times.

                return -1;
            }

            //Encrypt the call dynamic library
            [DllImport("n3kWGCom.dll", EntryPoint = "ShortEncrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int ShortEncrypt(IntPtr command, IntPtr password);

            [DllImport("n3kWGCom.dll", EntryPoint = "ShortDecrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            static extern int ShortDecrypt(IntPtr command, IntPtr password);
  
            [DllImport("n3kWGCom.dll", EntryPoint = "ShortEncryptSM4_ECB", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int ShortEncryptSM4_ECB(IntPtr command,int cmdLen, IntPtr password); //2015-09-28 12:05:54 EncryptionWGPacketShort Package Shortcasts

            [DllImport("n3kWGCom.dll", EntryPoint = "ShortDecryptSM4_ECB", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int ShortDecryptSM4_ECB(IntPtr command,int cmdLen, IntPtr password); //2015-09-28 12:05:54 EncryptionWGPacketShort Package Shortcasts

            [DllImport("n3kWGCom.dll", EntryPoint = "ShortEncryptSM4_CBC", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int ShortEncryptSM4_CBC(IntPtr command,int cmdLen, IntPtr password, IntPtr IV); //2015-09-28 12:05:54 EncryptionWGPacketShort Package Shortcasts

            [DllImport("n3kWGCom.dll", EntryPoint = "ShortDecryptSM4_CBC", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
            public static extern int ShortDecryptSM4_CBC(IntPtr command,int cmdLen, IntPtr password, IntPtr IV); //2015-09-28 12:05:54 EncryptionWGPacketShort Package Shortcasts


            public static int EncryptSM4_ECB(ref byte[] command, byte[] password)  //2015-09-28 13:19:12 2013-4-2_07:31:32 Encrypt data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

                Marshal.Copy(command, 0, pkt, 64);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);

                int ret = ShortEncryptSM4_ECB(pkt,64, commPassword);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 64);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;

            }
            public static int DecryptSM4_ECB(ref byte[] command, byte[] password)  //2015-09-28 15:12:12  Decrypt Data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

                Marshal.Copy(command, 0, pkt, 64);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);


                int ret = ShortDecryptSM4_ECB(pkt, 64, commPassword);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 64);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;
            }


            public static int EncryptSM4_ECB1024(ref byte[] command, byte[] password)  //2015-09-28 13:19:12 2013-4-2_07:31:32 Encrypt data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)1024);

                Marshal.Copy(command, 0, pkt, 1024);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);

                int ret = ShortEncryptSM4_ECB(pkt, 1024, commPassword);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 1024);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;

            }
            public static int DecryptSM4_ECB1024(ref byte[] command, byte[] password)  //2015-09-28 15:12:12  Decrypt Data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)1024);

                Marshal.Copy(command, 0, pkt, 1024);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);


                int ret = ShortDecryptSM4_ECB(pkt, 1024, commPassword);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 1024);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;
            }

            public static int EncryptSM4_CBC(ref byte[] command, byte[] password, byte[] IV)  //2015-09-28 13:19:12 2013-4-2_07:31:32 Encrypt data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

                Marshal.Copy(command, 0, pkt, 64);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);

                IntPtr iv = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(IV, 0, iv, 16);

                int ret = ShortEncryptSM4_CBC(pkt, 64, commPassword,iv);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 64);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;

            }
            public static int DecryptSM4_CBC(ref byte[] command, byte[] password, byte[] IV)  //2015-09-28 15:12:12  Decrypt Data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

                Marshal.Copy(command, 0, pkt, 64);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);

                IntPtr iv = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(IV, 0, iv, 16);

                int ret = ShortDecryptSM4_CBC(pkt, 64, commPassword,iv);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 64);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;
            }


            public static int EncryptSM4_CBC1024(ref byte[] command, byte[] password, byte[] IV)  //2015-09-28 13:19:12 2013-4-2_07:31:32 Encrypt data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)1024);

                Marshal.Copy(command, 0, pkt, 1024);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);

                IntPtr iv = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(IV, 0, iv, 16);

                int ret = ShortEncryptSM4_CBC(pkt, 1024, commPassword,iv);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 1024);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;

            }
            public static int DecryptSM4_CBC1024(ref byte[] command, byte[] password, byte[] IV)  //2015-09-28 15:12:12  Decrypt Data
            {
                IntPtr pkt = Marshal.AllocHGlobal((Int32)1024);

                Marshal.Copy(command, 0, pkt, 1024);

                IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(password, 0, commPassword, 16);
                IntPtr iv = Marshal.AllocHGlobal((Int32)16);
                Marshal.Copy(IV, 0, iv, 16);

                int ret = ShortDecryptSM4_CBC(pkt, 1024, commPassword,iv);
                if (ret > 0)
                {
                    Marshal.Copy(pkt, command, 0, 1024);  //Copy it.
                }
                Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
                Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
                return ret;
            }


            public int run_ECB(byte[] commPassword)  //2015-10-28 10:16:38 Message-processing dispatch instructions Can not open message
            {
                byte[] buff = toByte();
                EncryptSM4_ECB(ref buff, commPassword);

                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                            byte[] buffbak = new byte[64] ;
                            Array.Copy( recv, buffbak,64);
                            DecryptSM4_ECB(ref recv, commPassword);

                            if ((recv[0] & 0x7F) == Type)
                            {
                            }
                            else
                            {
                                if ((buffbak[0] & 0x7F) == Type) //2015-12-02 21:06:13 If the code before decryption is valid,
                                {
                                    Array.Copy(buffbak,recv,  64);
                                }
                            }
                        if ((recv[0] & 0x7F) == Type)
                        {

                            //Water Stream
                            long sequenceIdReceived = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                long lng = recv[40 + i];
                                sequenceIdReceived += (lng << (8 * i));
                            }

                            if ((recv[0] == Type)                       //Align type
                                && (recv[1] == functionID)              //Function numbers are consistent
                                && (sequenceIdReceived == sequenceId))  //Serial number corresponding
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
                } while (tries-- > 0); //Try again three times.

                return -1;
            }
            public int run_CBC(byte[] commPassword,byte[] IV)  //2015-10-28 10:16:38 Message-processing dispatch instructions Can not open message
            {
                byte[] buff = toByte();
                EncryptSM4_CBC(ref buff, commPassword,IV);

                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                        byte[] buffbak = new byte[64];
                        Array.Copy(recv, buffbak, 64);
                        DecryptSM4_CBC(ref recv, commPassword,IV);

                        if ((recv[0] & 0x7F) == Type)
                        {
                        }
                        else
                        {
                            if ((buffbak[0] & 0x7F) == Type) //2015-12-02 21:06:13 If the code before decryption is valid,
                            {
                                Array.Copy(buffbak, recv, 64);
                            }
                        }
                        if ((recv[0] & 0x7F) == Type)
                        {

                            //Water Stream
                            long sequenceIdReceived = 0;
                            for (int i = 0; i < 4; i++)
                            {
                                long lng = recv[40 + i];
                                sequenceIdReceived += (lng << (8 * i));
                            }

                            if ((recv[0] == Type)                       //Align type
                                && (recv[1] == functionID)              //Function numbers are consistent
                                && (sequenceIdReceived == sequenceId))  //Serial number corresponding
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
                } while (tries-- > 0); //Try again three times.

                return -1;
            }
            public int run1024(byte[] buff)  //2015-11-05 14:50:45 1024Byte Command Send Command Can not open message 
            {
                long sequenceIdSend = 0;
                for (int i = 0; i < 4; i++)
                {
                    long lng = buff[40 + i];
                    sequenceIdSend += (lng << (8 * i));
                }
                
                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                       
                        //Water Stream
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //Align type
                            && (recv[1] == functionID)              //Function numbers are consistent
                            && (sequenceIdReceived == sequenceIdSend)  //Serial number corresponding
                        )
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                } while (tries-- > 0); //Try again three times.

                return -1;
            }


            //commPassword Do not use passwords when empty. The password must be16Bytes
            public int run1024_ECB(byte[] buff,byte[] commPassword)  //2015-11-05 14:50:45 1024Byte Command Encrypted communications component Send Command Can not open message 
            {
                long sequenceIdSend = 0;
                for (int i = 0; i < 4; i++)
                {
                    long lng = buff[40 + i];
                    sequenceIdSend += (lng << (8 * i));
                }
                if (commPassword != null)  
                {
                    //If Encryption
                    EncryptSM4_ECB1024(ref buff, commPassword);
                }
                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                        //if ((recv[0] & 0x7F) == Type)
                        {
                            if ((commPassword != null) && recv.Length == 1024)
                            {
                                //If it's encrypted,
                                DecryptSM4_ECB1024(ref recv, commPassword);
                            }
                        }
                        //Water Stream
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //Align type
                            && (recv[1] == functionID)              //Function numbers are consistent
                            && (sequenceIdReceived == sequenceIdSend)  //Serial number corresponding
                        )
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                } while (tries-- > 0); //Try again three times.

                return -1;
            }

            public int run1024_CBC(byte[] buff, byte[] commPassword, byte[] IV)  //2015-11-05 14:50:45 1024Byte Command Encrypted communications component Send Command Can not open message 
            {
                long sequenceIdSend = 0;
                for (int i = 0; i < 4; i++)
                {
                    long lng = buff[40 + i];
                    sequenceIdSend += (lng << (8 * i));
                }
                if (commPassword != null)
                {
                    //If Encryption
                    EncryptSM4_CBC1024(ref buff, commPassword,IV);
                }
                int tries = 3;
                int errcnt = 0;
                controller.IP = IP;
                controller.PORT = ControllerPort;
                do
                {
                    if (controller.ShortPacketSend(buff, ref recv) < 0)
                    {
                        //2015-11-03 20:26:52 Enter Retry return -1;
                    }
                    else
                    {
                        //if ((recv[0] & 0x7F) == Type)
                        {
                            if ((commPassword != null) && recv.Length == 1024)
                            {
                                //If it's encrypted,
                                DecryptSM4_CBC1024(ref recv, commPassword,IV);
                            }
                        }
                        //Water Stream
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //Align type
                            && (recv[1] == functionID)              //Function numbers are consistent
                            && (sequenceIdReceived == sequenceIdSend)  //Serial number corresponding
                        )
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                } while (tries-- > 0); //Try again three times.

                return -1;
            }

            /// <summary>
            /// It's the last running water.
            /// </summary>
            /// <returns></returns>
            public static long sequenceIdSent()// 
            {
                return sequenceId; // It's the last running water.
            }
            /// <summary>
            /// Close
            /// </summary>
            public void close()
            {
                controller.Dispose();
            }
        }

        void log(string info)  //Log Information
        {
            //txtInfo.Text += string.Format("{0}\r\n", info);
            //txtInfo.AppendText(string.Format("{0}\r\n", info));
            txtInfo.AppendText(string.Format("{0} {1}\r\n", DateTime.Now.ToString("HH:mm:ss"), info)); //2015-11-03 20:55:49 Show Time
            txtInfo.ScrollToCaret();//Scroll to cursor
            Application.DoEvents();
        }

        /// <summary>
        /// 4Byte to Integer(Down front., Behind you.)
        /// </summary>
        /// <param name="buff">Bytes</param>
        /// <param name="start">Start Indexing Post(From0Start counting.)</param>
        /// <param name="len">Length</param>
        /// <returns>Integer</returns>
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
        /// Convert the integer to4Bytes
        /// </summary>
        /// <param name="outBytes">Array</param>
        /// <param name="startIndex">Start Indexing Post(From0Start counting.)</param>
        /// <param name="val">Value</param>
        void LongToBytes(ref byte[] outBytes, int startIndex, long val)
        {
            Array.Copy(System.BitConverter.GetBytes(val), 0, outBytes, startIndex, 4);
        }
        /// <summary>
        /// AccessHexValue, Mainly used in date time format
        /// </summary>
        /// <param name="val">Value</param>
        /// <returns>HexValue</returns>
        int GetHex(int val)
        {
            return ((val % 10) + (((val - (val % 10)) / 10) % 10) * 16);
        }

        /// <summary>
        /// Show Record Information
        /// </summary>
        /// <param name="recv"></param>
        void displayRecordInformation(byte[] recv)
        {
            //8-11	Record index number
            //(=0No record.)	4	0x00000000
            int recordIndex = 0;
            recordIndex = (int)byteToLong(recv, 8, 4);

            //12	Record type**********************************************
            //0=No record
            //1=Brush Card Record
            //2=Door Magnetic,button, Device startup, Remote Open Record
            //3=Call the police.	1	
            //0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
            int recordType = recv[12];

            //13	Validity(0 Not approved, 1Adopted)	1	
            int recordValid = recv[13];

            //14	Door number.(1,2,3,4)	1	
            int recordDoorNO = recv[14];

            //15	Come in./Out.(1It means coming in., 2Means out.)	1	0x01
            int recordInOrOut = recv[15];

            //16-19	Card(Type is when swiping a card.)
            //or numbering(Other types of records)	4	
            long recordCardNO = 0;
            recordCardNO = byteToLong(recv, 16, 4);

            //20-26	Brush Time:
            //Days and days of year (AdoptBCDCode)See description of the set-up segment
            string recordTime = "2000-01-01 00:00:00";
            recordTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                recv[20], recv[21], recv[22], recv[23], recv[24], recv[25], recv[26]);
            //2012.12.11 10:49:59	7	
            //27	Record cause code(You can check it out. “Checkcard log notes.xls”It's a file.ReasonNO)
            //It's only for complex information.	1	
            int reason = recv[27];


            //0=No record
            //1=Brush Card Record
            //2=Door Magnetic,button, Device startup, Remote Open Record
            //3=Call the police.	1	
            //0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
            if (recordType == 0)
            {
                log(string.Format("Index post={0}  No record", recordIndex));
            }
            else if (recordType == 0xff)
            {
                log(" The records of the specified index have been overwritten,Use the index.0, Retrieving index values from the earliest record");
            }
            else if (recordType == 1) //2015-06-10 08:49:31 Show data with card number type of record
            {
                //Card
                log(string.Format("Index post={0}  ", recordIndex));
                log(string.Format("  Card = {0}", recordCardNO));
                log(string.Format("  Door number. = {0}", recordDoorNO));
                log(string.Format("  Access = {0}", recordInOrOut == 1 ? "Come in." : "Out."));
                log(string.Format("  Valid. = {0}", recordValid == 1 ? "Pass." : "Ban"));
                log(string.Format("  Time = {0}", recordTime));
                log(string.Format("  Description = {0}", getReasonDetailChinese(reason)));
            }
            else if (recordType == 2)
            {
                //Other processing
                //Door Magnetic,button, Device startup, Remote Open Record
                log(string.Format("Index post={0}  Non-card records", recordIndex));
                log(string.Format("  Numbering = {0}", recordCardNO));
                log(string.Format("  Door number. = {0}", recordDoorNO));
                log(string.Format("  Time = {0}", recordTime));
                log(string.Format("  Description = {0}", getReasonDetailChinese(reason)));
            }
            else if (recordType == 3)
            {
                //Other processing
                //Call the police.
                log(string.Format("Index post={0}  Call the police.", recordIndex));
                log(string.Format("  Numbering = {0}", recordCardNO));
                log(string.Format("  Door number. = {0}", recordDoorNO));
                log(string.Format("  Time = {0}", recordTime));
                log(string.Format("  Description = {0}", getReasonDetailChinese(reason)));
            }
        }

        string[] RecordDetails =
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

        string getReasonDetailChinese(int Reason) //Chinese
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

        string getReasonDetailEnglish(int Reason) //English Description
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
        /// Basic function test
        /// </summary>
        /// <param name="ControllerIP">controllerIPAddress</param>
        /// <param name="controllerSN"> Control serial number</param>
        /// <returns>less than or equal to0 Failed, 1It means success.</returns>
  
  
        private void button3_Click(object sender, EventArgs e)
        {
            
            //TestCBCMethodology
            log("SM4 CBCCommunications Test...");
            String ControllerIP = txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);



            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.


            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;
            byte[] commPassword = { 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x00 }; //16Byte Password

            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 }; //16Bytes Initial Variables

            //Set Communications Password SM4 CBC [Function Number: 0xE2] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xE2;
            //Ideas against error
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
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
            ret = pkt.run();  //2015-11-02 10:21:22 Try the controller without password first.
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
                ret = pkt.run_CBC(commPassword, IV);  //2015-11-02 10:21:22 Try the password operation for the controller.
                if ((ret > 0) && (pkt.recv[8] == 1))
                {
                    log("Communication password set successfully...[Can not open message]");
                    success = 1;
                }
                else
                {
                    log("Communication password setup failed...[Can not open message]");
                }
            }

            //Communication using coded communications
            //1.10	Open remote[Function Number: 0x40] **********************************************************************************
            int doorNO = 1;
            pkt.Reset();
            pkt.functionID = 0x40;
            pkt.data[0] = (byte)(doorNO & 0xff); //2013-11-03 20:56:33
            ret = pkt.run_CBC(commPassword, IV);
            success = 0;
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
                    log("1.10 Open remote	 Failed...[Can not open message]");
                }
            }
            else
            {
                log("1.10 Open remote	 Failed...[Can not open message]");
            }

            //Presentation1024Bytes Encryption Operations...
            byte[] command1024 = new byte[1024];


            //1.9	Extract Record Operation
            //1. Pass. 0xB0Command Fetch the earliest record index
            //2. Pass. 0xB0Command Get Last Record Index
            //3. Pass. 0xB4Command Get read record index numbers recordIndex
            //4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
            //5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
            //After the top steps,， The entire extraction record is complete.
            long firstRecordIndex = 0;  //First record index number
            long lastRecordIndex = 0;   //Last record index number.
            long recordIndexGotToRead = 0x0;
            long recordIndexToGet = 0;
            log("1.9 Extract Record Operation	 Start...[1024Byte Command]");
            pkt.Reset();
            pkt.functionID = 0xB0;//Take the earliest record index
            recordIndexToGet = 0x0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run_CBC(commPassword, IV);
            success = 0;
            if (ret > 0)
            {
                firstRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" Fetch the earliest record index	 =" + firstRecordIndex.ToString());
            }
            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB0;//Take Last Record Index
                recordIndexToGet = 0xffffffff;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run_CBC(commPassword, IV); 
            }
            if (ret > 0)
            {
                lastRecordIndex = (int)byteToLong(pkt.recv, 8, 4);
                log(" Get Last Record Index	  =" + lastRecordIndex.ToString());
            }

            if (ret > 0)
            {
                pkt.Reset();
                pkt.functionID = 0xB4;//Get read record index numbers
                recordIndexToGet = 0x0;
                LongToBytes(ref pkt.data, 0, recordIndexToGet);
                ret = pkt.run_CBC(commPassword, IV);
            }
            if (ret > 0)
            {
                recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
                log("Get read record index numbers	  =" + recordIndexGotToRead.ToString());
            }
            long validRecordsCount = 0;
            //recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
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
                        LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
                        byte[] cmd = pkt.toByte();
                        Array.Copy(cmd, 0, command1024, j, 64);
                        recordIndexToGetStart++;
                        cnt++;
                    }
                    ret = pkt.run1024_CBC(command1024, commPassword, IV);
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
                            byte[] recv = new byte[64];
                            Array.Copy(pkt.recv, j, recv, 0, 64);
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
                                    Application.DoEvents();
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

                log("1.9 Full extraction successful.	 ... Number of valid records= " + validRecordsCount.ToString());
                if ((success > 0) && validRecordsCount > 0)
                {
                    //Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
                    pkt.Reset();
                    pkt.functionID = 0xB2;
                    LongToBytes(ref pkt.data, 0, recordIndexValidGet);

                    //12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
                    LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

                    ret = pkt.run_CBC(commPassword, IV);
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
            log(string.Format("       {0}Thousand powers...", cardCount / 10000));
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
                    LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

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

                    LongToBytes(ref pkt.data, 32 - 8, cardCount); //Total permissions
                    LongToBytes(ref pkt.data, 35 - 8, i + 1);//The index place for the current permission(From1Start)

                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);
                    i++;

                }
                ret = pkt.run1024_CBC(command1024, commPassword, IV);
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
            //1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
            //Presentation1024Bytes Encryption Operations...
            //byte[] command1024 = new byte[1024];
            //long cardNOOfPrivilegeToGetlast = 0;

            //Read All Permissions
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long maxCount = 20 * 10000;
            long[] cardArrayGet = new long[maxCount];
            long QueryIndex = 1; //Index number(From1Start);
            LongToBytes(ref pkt.data, 0, QueryIndex);

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
                }
                for (int j = 0; j < 1024; j = j + 64)
                {
                    LongToBytes(ref pkt.data, 0, QueryIndex);
                    QueryIndex++; //Index number(From1Start);
                    byte[] cmd = pkt.toByte();
                    Array.Copy(cmd, 0, command1024, j, 64);

                }
                ret = pkt.run1024_CBC(command1024, commPassword, IV);
                success = 0;
                if (ret > 0)
                {
                    for (int j = 0; j < 1024; j = j + 64)
                    {
                        success = 0;
                        long cardNOOfPrivilegeToGet = 0;
                        cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8 + j, 4);
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
                }
                else
                {
                    log("1.16     Problem...." + ret.ToString());
                    break;
                }
            }
            log("Last read permission card number = " + cardNOOfPrivilegeToGetlast.ToString());
            log("Permissions extractediCount = " + iCount.ToString());  //2015-11-04 19:59:50 Extract permissions

            //Clear the code.[Function Number: 0xE2] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xE2;
            //Ideas against error
            Array.Copy(System.BitConverter.GetBytes(WGPacketShort.SpecialFlag), 0, pkt.data, 0, 4);
            for (int i = 0; i < 16; i++)  //2015-11-02 10:21:00 Empty Password
            {
                pkt.data[4 + i] = 0;
                pkt.data[36 + i] = 0;
            }

            ret = pkt.run_CBC(commPassword, IV);
            if ((ret > 0) && (pkt.recv[8] == 1))
            {
                log("Communication code emptied....[Can not open message]");
                success = 1;
            }
            else
            {
                log("Communication password emptied...[Can not open message]");
            }



            // **********************************************************************************

            //End  **********************************************************************************
            pkt.close();  //Close communications
            // return success;
        }

    }
}
