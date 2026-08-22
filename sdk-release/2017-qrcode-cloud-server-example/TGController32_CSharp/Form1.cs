/**
* WGController32 2015-04-30 17:40:43 karl CSN Chan Shonin $
*
* Doorbar controller Shortcast agreement Test cases
* V2.6 Version  2015-11-03 20:25:53 V6.60Driver Version Communications password testing.  
*                               Retry to modify communication
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
using System.Collections;

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

        Boolean bStopWatchServer = true; //2015-05-05 17:35:07 Stop receiving server
        Boolean bStopBasicFunction = false;  //2015-06-10 09:04:52 Basic tests
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            bStopWatchServer = true;
            bStopBasicFunction = true;  //2015-06-10 09:04:52 Basic tests
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.txtInfo.Text = "";

            //Stop receiving server identification 
            bStopWatchServer = true;
            this.button4.BackColor = Color.Transparent; //2017-09-09 14:57:15

            bStopBasicFunction = false;  //2015-06-10 09:04:52 Basic tests

            //'    'No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
            //'    'Test instructions in this case
            //'    'controllerSN  = 229999901
            //'    'controllerIP  = 192.168.168.123
            //'    'Computer  IP  = 192.168.168.101
            //'    'For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)

            this.Cursor = Cursors.WaitCursor;
            //Basic function test
            //txtSN.Text controller9Series of bitsSN
            //txtIP.Text controllerIPAddress, Not adopted192.168.168.123  [Available Search Controller Modify controllerIP]
            testBasicFunction(txtIP.Text, long.Parse(txtSN.Text));
            this.Cursor = Cursors.Default;


            //txtWatchServerIP.Text  From the receiver.IP,Computer defaultIP 192.168.168.101 [It can also be used Search Controller Modify Settings]
            //txtWatchServerPort.Text  From the receiver.PORT, Defaults 61005
            if (bStopWatchServer)
            {
                txtIP.Text = txtIP.Text.Trim();
                if (string.IsNullOrEmpty(txtIP.Text) || txtIP.Text.Equals("192.168.0.0") || txtIP.Text.Equals("192.168.168.0"))
                {
                    MessageBox.Show("If using receiving server functionality. \r\n\r\nPlease set the controller first.IP (AssignIPorDHCPModalities). \r\n\r\nPass. Search controller=>Configure.");
                    return;
                }
                testWatchingServer(txtIP.Text, long.Parse(txtSN.Text), txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Receiving Server Settings
                bStopWatchServer = false;
                this.button4.BackColor = Color.Yellow; //2017-09-09 14:57:15
                WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Server Run....
                this.button4.BackColor = Color.Transparent; //2017-09-09 14:57:15
                bStopWatchServer = true;
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            bStopWatchServer = true;
            this.button4.BackColor = Color.Transparent; //2017-09-09 14:57:15
            bStopBasicFunction = true;  //2015-06-10 09:04:52 Basic tests
        }

        private void button3_Click(object sender, EventArgs e) //2015-05-05 17:35:35 Search controller
        {
            try
            {
                using (dfrmNetControllerConfig dfrm = new dfrmNetControllerConfig())
                {
                    dfrm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                MessageBox.Show(ex.ToString());

            }
        }
        private void button4_Click(object sender, EventArgs e)
        {
            if (bStopWatchServer)
            {
                bStopWatchServer = false;
                this.button4.BackColor = Color.Yellow;
                WatchingServerRuning(txtWatchServerIP.Text, int.Parse(this.txtWatchServerPort.Text)); //Server Run....
                this.button4.BackColor = Color.Transparent; //2017-09-09 14:57:15
                bStopWatchServer = true;
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
            long recordCardNOHigh = 0;
            recordCardNOHigh = byteToLong(recv, 44, 4);              //2017-10-30 16:52:38 Add
            recordCardNO = recordCardNO + (recordCardNOHigh << 32);  //2017-10-30 16:52:29 Add

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
                if ((reason == 44) && (recordCardNO > 1)) //2017-09-07 12:08:34 Open remote
                {
                    log(string.Format("  Simulation card number = {0}", recordCardNO)); //2017-09-07 12:09:41
                }
                else
                {
                    log(string.Format("  Numbering = {0}", recordCardNO));
                }
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
        int testBasicFunction(String ControllerIP, long controllerSN)
        {
            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.


            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.4	Query controller status[Function Number: 0x20](Real time surveillance) **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x20;
            ret = pkt.run();

            success = 0;
            if (ret == 1)
            {
                //Read information successfully...
                success = 1;
                log("1.4 Query controller status Success...");

                //	  	Last recorded information		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                //	Other information		
                int[] doorStatus = new int[4];
                //28	1Door no.(0Means close, 1Show Open)	1	0x00
                doorStatus[1 - 1] = pkt.recv[28];
                //29	2Door no.(0Means close, 1Show Open)	1	0x00
                doorStatus[2 - 1] = pkt.recv[29];
                //30	3Door no.(0Means close, 1Show Open)	1	0x00
                doorStatus[3 - 1] = pkt.recv[30];
                //31	4Door no.(0Means close, 1Show Open)	1	0x00
                doorStatus[4 - 1] = pkt.recv[31];

                int[] pbStatus = new int[4];
                //32	1Door button.(0It means you let go., 1Means press)	1	0x00
                pbStatus[1 - 1] = pkt.recv[32];
                //33	2Door button.(0It means you let go., 1Means press)	1	0x00
                pbStatus[2 - 1] = pkt.recv[33];
                //34	3Door button.(0It means you let go., 1Means press)	1	0x00
                pbStatus[3 - 1] = pkt.recv[34];
                //35	4Door button.(0It means you let go., 1Means press)	1	0x00
                pbStatus[4 - 1] = pkt.recv[35];

                //36	Fault.
                //equals0 No malfunctions.
                //Not equal to0, It's not working.(Reset Time, If there's anything else,, We're going back to the factory.)	1	
                int errCode = pkt.recv[36];

                //37	Control Current Time
                //Time	1	0x21
                //38	min	1	0x30
                //39	sec	1	0x58

                //40-43	Water Stream	4	
                long sequenceId = 0;
                sequenceId = byteToLong(pkt.recv, 40, 4);

                //48
                //Special Information1(Return based on actual use)
                //Keyboard Key Information	1	


                //49	Relay status	1	 [0The door is locked., 1It means the door is locked.. When the normal door is locked, Value as0000]
                int relayStatus = pkt.recv[49];
                if ((relayStatus & 0x1) > 0)
                {
                    //Door one. Open the lock.
                }
                else
                {
                    //Door one. Lock it.
                }
                if ((relayStatus & 0x2) > 0)
                {
                    //Door two. Open the lock.
                }
                else
                {
                    //Door two. Lock it.
                }
                if ((relayStatus & 0x4) > 0)
                {
                    //Gate three. Open the lock.
                }
                else
                {
                    //Gate three. Lock it.
                }
                if ((relayStatus & 0x8) > 0)
                {
                    //Gate four. Open the lock.
                }
                else
                {
                    //Gate four. Lock it.
                }

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

                string controllerTime = "2000-01-01 00:00:00"; //Control Current Time
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    0x20, pkt.recv[51], pkt.recv[52], pkt.recv[53], pkt.recv[37], pkt.recv[38], pkt.recv[39]);
            }
            else
            {
                log("1.4 Query controller status Failed?????...");
                return -1;
            }

            //1.5	Read Date Time(Function Number: 0x32) **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x32;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                string controllerTime = "2000-01-01 00:00:00"; //Control Current Time
                controllerTime = string.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}",
                    pkt.recv[8], pkt.recv[9], pkt.recv[10], pkt.recv[11], pkt.recv[12], pkt.recv[13], pkt.recv[14]);

                log("1.5 Read Date Time Success...");
                success = 1;
            }

            //1.6	Set Date Time[Function Number: 0x30] **********************************************************************************
            //calibrate controller at computer time.....
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
                    log("1.6 Set Date Time Success...");
                    success = 1;
                }
            }

            //1.7	Get a record of the given index number[Function Number: 0xB0] **********************************************************************************
            //(Take Index Number 0x00000001Records)
            long recordIndexToGet = 0;
            pkt.Reset();
            pkt.functionID = 0xB0;
            pkt.iDevSn = controllerSN;

            //	(Special
            //If=0, Retrieving the earliest recorded information
            //If=0xffffffffRetrieving information from the last record)
            //Records index numbers are normally incremental., Max.0xffffff = 16,777,215 (Over1Millions.) . Due to limited storage space, Only the closest on the controller.20Thousands of records.. When index numbers exceed20After 10,000., The records of the old index numbers are overwritten., So at this point, check the records of these index numbers., The type of record returned will be0xff, It means it doesn't exist..
            recordIndexToGet = 1;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Get Index As1Recorded information	 Success...");
                //	  	Index to1Recorded information		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            //. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Fetch information from the earliest record	 Success...");
                //	  	First recorded information		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

                success = 1;
            }

            //Communication (Take the latest record. By Index 0xffffffff)
            pkt.Reset();
            pkt.functionID = 0xB0;
            recordIndexToGet = 0xffffffff;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.7 Getting information on the latest record	 Success...");
                //	  	Last recorded information		
                displayRecordInformation(pkt.recv); //2015-06-09 20:01:21
                success = 1;
            }

            ////1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (Set read record index number as5)
            //int recordIndexGot = 0x5;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);

            ////12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    if (pkt.recv[8] == 1)
            //    {
            //        log("1.8 Set a read record index number	 Success...");
            //        success = 1;
            //    }
            //}

            ////1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
            //pkt.Reset();
            //pkt.functionID = 0xB4;
            //int recordIndexGotToRead = 0x0;
            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
            //    log("1.9 Get read record index numbers	 Success...");
            //    success = 1;
            //}

            ////1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
            ////Restore extracted records, Yes1.9Prepare for full extraction-- In use, It's only recovered when problems arise., Normal....
            //pkt.Reset();
            //pkt.functionID = 0xB2;
            //// (Set read record index number as5)
            //int recordIndexGot = 0x0;
            //LongToBytes(ref pkt.data, 0, recordIndexGot);
            ////12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
            //LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    if (pkt.recv[8] == 1)
            //    {
            //        log("1.8 Set a read record index number	 Success...");
            //        success = 1;
            //    }
            //}

            //2017-09-09 15:16:36 Record extraction modified...
            ////1.9	Extract Record Operation
            ////1. Pass. 0xB4Command Get read record index numbers recordIndex
            ////2. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
            ////3. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
            ////After three steps,， The entire extraction record is complete.
            //log("1.9 Extract Record Operation	 Start...");
            //pkt.Reset();
            //pkt.functionID = 0xB4;
            //ret = pkt.run();
            //success = 0;
            //if (ret > 0)
            //{
            //    long recordIndexGotToRead = 0x0;
            //    recordIndexGotToRead = (long)byteToLong(pkt.recv, 8, 4);
            //    pkt.Reset();
            //    pkt.functionID = 0xB0;
            //    pkt.iDevSn = controllerSN;
            //    long recordIndexToGetStart = recordIndexGotToRead + 1;
            //    long recordIndexValidGet = 0;
            //    int cnt = 0;
            //    do
            //    {
            //        if (bStopBasicFunction)
            //        {
            //            return 0;  //2015-06-10 09:08:14 Stop
            //        }
            //        LongToBytes(ref pkt.data, 0, recordIndexToGetStart);
            //        ret = pkt.run();
            //        success = 0;
            //        if (ret > 0)
            //        {
            //            success = 1;

            //            //12	Record type
            //            //0=No record
            //            //1=Brush Card Record
            //            //2=Door Magnetic,button, Device startup, Remote Open Record
            //            //3=Call the police.	1	
            //            //0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
            //            int recordType = pkt.recv[12];
            //            if (recordType == 0)
            //            {
            //                break; //No more records.
            //            }
            //            if (recordType == 0xff)//This index number is invalid  Reset Index Values
            //            {
            //                //Take the earliest record index bit
            //                pkt.Reset();
            //                pkt.functionID = 0xB0;
            //                recordIndexToGet = 0;
            //                LongToBytes(ref pkt.data, 0, recordIndexToGet);

            //                ret = pkt.run();
            //                success = 0;
            //                if (ret > 0)
            //                {
            //                    log("1.7 Fetch information from the earliest record	 Success...");
            //                    recordIndexGotToRead = (int)byteToLong(pkt.recv, 8, 4);
            //                    recordIndexToGetStart = recordIndexGotToRead;
            //                    continue;
            //                }
            //                success = 0;  
            //                break;
            //            }
            //            recordIndexValidGet = recordIndexToGetStart;

            //            displayRecordInformation(pkt.recv); //2015-06-09 20:01:21

            //            //.......Storage of records received
            //            //*****
            //            //###############
            //        }
            //        else
            //        {
            //            //Ripping failed
            //            break;
            //        }
            //        recordIndexToGetStart++;
            //    } while (cnt++ < 200000);
            //    if (success > 0)
            //    {
            //        //Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
            //        pkt.Reset();
            //        pkt.functionID = 0xB2;
            //        LongToBytes(ref pkt.data, 0, recordIndexValidGet);

            //        //12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
            //        LongToBytes(ref pkt.data, 4, WGPacketShort.SpecialFlag);

            //        ret = pkt.run();
            //        success = 0;
            //        if (ret > 0)
            //        {
            //            if (pkt.recv[8] == 1)
            //            {
            //                //Full extraction successful.....
            //                log("1.9 Full extraction successful.	 Success...");
            //                success = 1;
            //            }
            //        }

            //    }
            //}

            byte[] command1024 = new byte[1024]; //2017-09-09 15:18:04 Adopt1024

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
            //2017-09-09 15:17:30 long recordIndexToGet = 0;
            log("1.9 Extract Record Operation	 Start...[1024Byte Command]");
            pkt.Reset();
            pkt.functionID = 0xB0;//Take the earliest record index
            recordIndexToGet = 0x0;
            LongToBytes(ref pkt.data, 0, recordIndexToGet);
            ret = pkt.run();
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
                ret = pkt.run();
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
                ret = pkt.run();
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
                    ret = pkt.run1024(command1024);
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

                    ret = pkt.run();
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





            //1.10	Open remote[Function Number: 0x40] **********************************************************************************
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
                    //Open the door effectively......
                    log("1.10 Open remote	 Success...");
                    success = 1;
                }
            }

            //1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
            //Add card number0D D7 37 00, Through all doors of the current controller
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
            long cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

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

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
                    log("1.11 Permissions to add or modify	 Success...");
                    success = 1;
                }
            }

            //1.12	Permission to delete(Individual Delete)[Function Number: 0x52] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x52;
            pkt.iDevSn = controllerSN;
            //Permission card number to delete0D D7 37 00  = 0x0037D70D = 3659533 (Decimal)
            long cardNOOfPrivilegeToDelete = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeToDelete);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay doesn't move..
                    log("1.12 Permission to delete(Individual Delete)	 Success...");
                    success = 1;
                }
            }

            //1.13	Clear Permissions(Clear it all.)[Function Number: 0x54] **********************************************************************************
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
                    //It's time to clear up.
                    log("1.13 Clear Permissions(Clear it all.)	 Success...");
                    success = 1;
                }
            }

            //1.14	Total Permissions Read[Function Number: 0x58] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x58;
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                int privilegeCount = 0;
                privilegeCount = (int)byteToLong(pkt.recv, 8, 4);
                log("1.14 Total Permissions Read	 Success...");

                success = 1;
            }


            //Add again as a query operation 1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
            //Add card number0D D7 37 00, Through all doors of the current controller
            pkt.Reset();
            pkt.functionID = 0x50;
            //0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
            cardNOOfPrivilege = 0x0037D70D;
            LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);
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

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    //And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
                    log("1.11 Permissions to add or modify	 Success...");
                    success = 1;
                }
            }

            //1.15	Permission Query[Function Number: 0x5A] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x5A;
            pkt.iDevSn = controllerSN;
            // (The Chaka is 0D D7 37 00Competence)
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
                    //When no permission: (The card number is0)
                    log("1.15      Can not open message: (The card number is0)");
                }
                else
                {
                    //Specific Permission Information...
                    log("1.15     Can not open message...");
                }
                log("1.15 Permission Query	 Success...");
                success = 1;
            }

            //1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long QueryIndex = 1; //Index number(From1Start);
            LongToBytes(ref pkt.data, 0, QueryIndex);

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {

                long cardNOOfPrivilegeToGet = 0;
                cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4);
                if (4294967295 == cardNOOfPrivilegeToGet) //FFFFFFFFResponse4294967295
                {
                    log("1.16      Can not open message: (Permissions deleted)");
                }
                else if (cardNOOfPrivilegeToGet == 0)
                {
                    //When no permission: (The card number is0)
                    log("1.16       Can not open message: (The card number is0)--This index number is no longer valid.");
                }
                else
                {
                    //Specific Permission Information...
                    log("1.16      Can not open message...");
                }
                log("1.16 Access to specified index numbers	 Success...");
                success = 1;
            }


            //1.17	Set door control parameters(Online/Delay) [Function Number: 0x80] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x80;
            //(Settings2Door. Online  Open the door late. 3sec)
            pkt.data[0] = 0x02; //2Door.
            pkt.data[1] = 0x03; //Online
            pkt.data[2] = 0x03; //Open the door late.

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.data[0] == pkt.recv[8] && pkt.data[1] == pkt.recv[9] && pkt.data[2] == pkt.recv[10])
                {
                    //When successful, Return values to match settings
                    log("1.17 Set door control parameters	 Success...");
                    success = 1;
                }
                else
                {
                    //Failed
                }
            }

            //For1024Byte Command
            ////1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
            ////This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
            ////Suggested number of privileges updated over50individual, Use this command

            //log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	Start...");
            //log("       1Thousand powers...");

            ////Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
            //int cardCount = 10000;  //2015-06-09 20:20:20 Total number of cards
            //long[] cardArray = new long[cardCount];
            //for (int i = 0; i < cardCount; i++)
            //{
            //    cardArray[i] = 50001+i;
            //}

            //for (int i = 0; i < cardCount; i++)
            //{
            //    if (bStopBasicFunction)
            //    {
            //        return 0;  //2015-06-10 09:08:14 Stop
            //    }
            //    pkt.Reset();
            //    pkt.functionID = 0x56;

            //    cardNOOfPrivilege = cardArray[i];
            //    LongToBytes(ref pkt.data, 0, cardNOOfPrivilege);

            //    //When other parameters are simplified Harmonization, You can make changes depending on each card.
            //    //20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
            //    pkt.data[4] = 0x20;
            //    pkt.data[5] = 0x10;
            //    pkt.data[6] = 0x01;
            //    pkt.data[7] = 0x01;
            //    //20 29 12 31 Deadline:  2029Year12Month31Day
            //    pkt.data[8] = 0x20;
            //    pkt.data[9] = 0x29;
            //    pkt.data[10] = 0x12;
            //    pkt.data[11] = 0x31;
            //    //01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
            //    pkt.data[12] = 0x01;
            //    //01 Allow Pass Door two. [Two doors., Four controllers working.]
            //    pkt.data[13] = 0x01;  //If it's forbidden,2Door., As 0x00
            //    //01 Allow Pass Gate three. [It works on four controllers.]
            //    pkt.data[14] = 0x01;
            //    //01 Allow Pass Gate four. [It works on four controllers.]
            //    pkt.data[15] = 0x01;

            //    LongToBytes(ref pkt.data, 32-8, cardCount); //Total permissions
            //    LongToBytes(ref pkt.data, 35-8, i+1);//The index place for the current permission(From1Start)

            //    ret = pkt.run();
            //    success = 0;
            //    if (ret > 0)
            //    {
            //        if (pkt.recv[8] == 1)
            //        {
            //            success = 1;
            //        }
            //        if (pkt.recv[8] == 0xE1)
            //        {
            //            log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 =0xE1 Which means the card number has not been sorted from a small to a large size....???");
            //            success = 0;
            //            break;
            //        }
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //if (success == 1)
            //{
            //    log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Success...");
            //}
            //else
            //{
            //    log("1.21	Permissions added from childhood to larger[Function Number: 0x56]	 Failed...????");
            //}


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
            long cardNOOfPrivilegeB;
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

                    cardNOOfPrivilegeB = cardArray[i];
                    cardNOOfPrivilegeToGetlast = cardNOOfPrivilegeB;
                    LongToBytes(ref pkt.data, 0, cardNOOfPrivilegeB);

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
            //Read All Permissions
            pkt.Reset();
            pkt.functionID = 0x5C;
            pkt.iDevSn = controllerSN;
            long maxCount = 20 * 10000;
            long[] cardArrayGet = new long[maxCount];
            long QueryIndexB = 1; //Index number(From1Start);
            LongToBytes(ref pkt.data, 0, QueryIndexB);

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
                    LongToBytes(ref pkt.data, 0, QueryIndexB);
                    QueryIndexB++; //Index number(From1Start);
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

            //Other instructions  **********************************************************************************


            // **********************************************************************************

            //End  **********************************************************************************
            pkt.close();  //Close communications
            return success;
        }

        /// <summary>
        /// Receiving Server Settings Test
        /// </summary>
        /// <param name="ControllerIP">Controls set upIPAddress</param>
        /// <param name="controllerSN">Setd controller serial number</param>
        /// <param name="watchServerIP">Server to set upIP</param>
        /// <param name="watchServerPort">Port to set up</param>
        /// <returns>0 Failed, 1It means success.</returns>
        int testWatchingServer(string ControllerIP, long controllerSN, string watchServerIP, int watchServerPort)  //Receiving Server Test -- Settings
        {
            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.

            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.18	Set up the receiver serverIPand Port [Function Number: 0x90] **********************************************************************************
            //(If you don't want the controller to send the data,, As long as you're receiving the server.IPSet as0.0.0.0 There you go.)
            //Port of receiving server: 61005
            //Every5Seconds sent once.: 05
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

            //Port of receiving server: 61005
            pkt.data[4] = (byte)((watchServerPort & 0xff));
            pkt.data[5] = (byte)((watchServerPort >> 8) & 0xff);

            //Every5Seconds sent once.: 05 (Periodically upload information as5sec [Every time running properly5Seconds sent once.  Send it when you have a brush card])
            pkt.data[6] = 5;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    log("1.18 Set up the receiver serverIPand Port 	 Success...");
                    success = 1;
                }
            }


            //1.19	Read the receiver server.IPand Port [Function Number: 0x92] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x92;

            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                log("1.19 Read the receiver server.IPand Port 	 Success...");
                success = 1;
            }
            pkt.close();
            return success;
        }


        /// <summary>
        /// Open receiving server to receive data (Watch the firewall. You have to allow all packages at this port to enter.)
        /// </summary>
        /// <param name="watchServerIP">Receiving ServersIP(Usually the current computer.IP)</param>
        /// <param name="watchServerPort">Receive Server Port</param>
        /// <returns>1 It means success.,Otherwise, failure.</returns>
        int WatchingServerRuning(string watchServerIP, int watchServerPort)
        {
            //Watch the firewall. You have to allow all packages at this port to enter.
            try
            {
                WG3000_COMM.Core.wgUdpServerCom udpserver = new WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort);
                //2017-09-07 16:42:33 Do Not ShowIP                 udpserver.IncludeIPInfo = true; //2016-01-05 12:51:55 AccessIP

                if (!udpserver.IsWatching())
                {
                    log("Enter receiving server surveillance status....Failed");
                    return -1;
                }
                log("Enter receiving server surveillance status....");
                long recordIndex = 0;
                ArrayList arrRecordIndex = new ArrayList(); //2017-09-07 11:18:40 Use array records
                ArrayList arrControllerSN = new ArrayList(); //2017-09-07 11:18:40 Use array records


                int recv_cnt;
                while (!bStopWatchServer)
                {
                    recv_cnt = udpserver.receivedCount();
                    if (recv_cnt > 0)
                    {
                        byte[] buff = udpserver.getRecords();
                        if (buff[1] == 0x20)
                        {
                            long sn;
                            long recordIndexGet;
                            sn = byteToLong(buff, 4, 4);
                            log(string.Format("Received from controllerSN = {0} Packages..\r\n", sn));
                            //                          if (udpserver.IncludeIPInfo && buff.Length == 68) //2016-01-05 14:10:23 
                            if (udpserver.IncludeIPInfo && (buff.Length % 64) == 4) //2017-09-07 15:33:25 2016-01-05 14:10:23 
                            {
                                //long ip = byteToLong(buff, 64, 4);
                                //2017-09-07 15:33:48 log(string.Format("Received from controllerIP = {0:d}.{1:d}.{2:d}.{3:d} Packages..\r\n", buff[64], buff[65], buff[66], buff[67]));  //2016-01-05 14:10:29 AccessIP
                                log(string.Format("Received from controllerIP = {0:d}.{1:d}.{2:d}.{3:d} Packages..\r\n",
                                    buff[buff.Length - 4], buff[buff.Length - 3], buff[buff.Length - 2], buff[buff.Length - 1]));  //2017-09-07 15:34:22 2016-01-05 14:10:29 AccessIP
                            }
                            recordIndexGet = byteToLong(buff, 8, 4);

                            int iLoc = arrControllerSN.IndexOf(sn);
                            if (iLoc >= 0)
                            {
                                recordIndex = (long)arrRecordIndex[iLoc];
                                arrRecordIndex[iLoc] = recordIndexGet; //2017-09-07 11:23:50 Save new value
                            }
                            else
                            {
                                recordIndex = 0;
                                arrControllerSN.Add(sn);
                                arrRecordIndex.Add(recordIndexGet);
                            }

                            if (recordIndex < recordIndexGet)
                            {
                                recordIndex = recordIndexGet;

                                displayRecordInformation(buff); //2015-06-09 20:01:21
                                dealSwipeRecord(buff, ref udpserver); //2017-09-07 11:18:09 Handle swipe card records
                            }

                        }

                        //************************Two-dimensional code.
                        if (buff[1] == 0x22) //2017-09-07 15:38:29 Add 2D data
                        {
                            long sn;
                            long qrDataLen;
                            sn = byteToLong(buff, 4, 4);
                            log(string.Format("Received from controllerSN = {0} 2D code package..\r\n", sn));
                            //                          if (udpserver.IncludeIPInfo && buff.Length == 68) //2016-01-05 14:10:23 
                            if (udpserver.IncludeIPInfo && (buff.Length % 64) == 4) //2017-09-07 15:33:25 2016-01-05 14:10:23 
                            {
                                //long ip = byteToLong(buff, 64, 4);
                                //2017-09-07 15:33:48 log(string.Format("Received from controllerIP = {0:d}.{1:d}.{2:d}.{3:d} Packages..\r\n", buff[64], buff[65], buff[66], buff[67]));  //2016-01-05 14:10:29 AccessIP
                                log(string.Format("Received from controllerIP = {0:d}.{1:d}.{2:d}.{3:d} 2D code package..\r\n",
                                    buff[buff.Length - 4], buff[buff.Length - 3], buff[buff.Length - 2], buff[buff.Length - 1]));  //2017-09-07 15:34:22 2016-01-05 14:10:29 AccessIP
                            }
                            qrDataLen = byteToLong(buff, 8, 4);
                            dealQRData(buff, ref udpserver); //2017-09-07 11:18:09 Process 2D code records

                        }

                    }
                    else
                    {
                        System.Threading.Thread.Sleep(10);  //'Delay10ms
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

        static long sequenceId4RemoteOpen = 0x40000000; //2017-09-07 11:04:33 It's for the remote opener.
        void dealSwipeRecord(byte[] recv, ref WG3000_COMM.Core.wgUdpServerCom server)
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
            long recordCardNOHigh = 0;
            recordCardNOHigh = byteToLong(recv, 44, 4);              //2017-10-30 16:52:38 Add
            recordCardNO = recordCardNO + (recordCardNOHigh << 32);  //2017-10-30 16:52:29 Add

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

            if (recordType == 1) //2015-06-10 08:49:31 Show data with card number type of record
            {
                //Card
                //log(string.Format("Index post={0}  ", recordIndex));
                //log(string.Format("  Card = {0}", recordCardNO));
                //log(string.Format("  Door number. = {0}", recordDoorNO));
                //log(string.Format("  Access = {0}", recordInOrOut == 1 ? "Come in." : "Out."));
                //log(string.Format("  Valid. = {0}", recordValid == 1 ? "Pass." : "Ban"));
                //log(string.Format("  Time = {0}", recordTime));
                //log(string.Format("  Description = {0}", getReasonDetailChinese(reason)));
                if (recordValid == 0)  //2017-09-07 10:56:00 When adopted
                {
                    //2017-09-07 10:56:10 Check if the card is up to speed.
                    long sn;
                    sn = byteToLong(recv, 4, 4);

                    byte[] buff = new byte[WGPacketShort.WGPacketSize];
                    sequenceId4RemoteOpen++;
                    //Flow range for remote opening [Users can define themselves]
                    if ((sequenceId4RemoteOpen >= 0x7fffffff)  //2017-09-07 11:06:04 
                        || (sequenceId4RemoteOpen < 0x40000001))
                    {
                        sequenceId4RemoteOpen = 0x40000001;
                    }
                    for (int i = 0; i < WGPacketShort.WGPacketSize; i++)
                    {
                        buff[i] = 0;
                    }
                    buff[0] = (byte)0x17; //Type;
                    buff[1] = (byte)0x40; //functionID;
                    Array.Copy(System.BitConverter.GetBytes(sn), 0, buff, 4, 4);

                    //Array.Copy(data, 0, buff, 8, data.Length);
                    buff[8 + 0] = (byte)(recordDoorNO & 0xff); //Door number.
                    buff[28] = (byte)(recordInOrOut == 1 ? 0 : 1); // recordInOrOut == 1 ? "Come in." : "Out."));
                    Array.Copy(System.BitConverter.GetBytes(recordCardNO), 0, buff, 20, 4); //Simulation card number
                    Array.Copy(System.BitConverter.GetBytes(recordCardNO), 4, buff, 24, 4); //2017-10-31 14:51:50 Simulation card number High4Bytes
                    buff[32] = (byte)(0x5A); //Not bound by permissions within the device
                    Array.Copy(System.BitConverter.GetBytes(sequenceId4RemoteOpen), 0, buff, 40, 4);


                    int ret = server.UDP_OnlySend(buff);
                    if (ret > 0)
                    {
                        log(string.Format("\r\n    ====>Success Send a remote opening order. controllerSN={0}, Door number.= {1}, {2} Simulation card number= {3} \r\n",
                            sn.ToString(), recordDoorNO.ToString(), (recordInOrOut == 1 ? "Come in." : "Out."), recordCardNO));
                    }
                    else
                    {
                        log(string.Format("\r\n    ====>??? Failed: Send a remote opening order. controllerSN={0}, Door number.= {1}, {2} Simulation card number= {3} \r\n",
                          sn.ToString(), recordDoorNO.ToString(), (recordInOrOut == 1 ? "Come in." : "Out."), recordCardNO));
                    }
                }
            }

        }

        void dealQRData(byte[] recv, ref WG3000_COMM.Core.wgUdpServerCom server) //2017-09-07 15:40:56 ProcessingQRData
        {
            //8-11	2D data length
            //(=0No record.)	4	0x00000000
            int qrDataLen = 0;
            qrDataLen = (int)byteToLong(recv, 8, 4);

            //12	I don't think so.[2017-09-07 15:47:32]
            //2017-09-07 15:47:36  int recordType = recv[12];

            //13	Serial slogan(1or2) 
            int serialPort = recv[13];

            //14	Door number.(1,2,3,4)	1	
            int recordDoorNO = recv[14];

            //15	Come in./Out.(1It means coming in., 2Means out.)	1	0x01
            int recordInOrOut = recv[15];

            //16-36	I don't think so.


            long cmdSequenceId = byteToLong(recv, 40, 4); //2017-09-07 15:56:06 Water Stream

            if (qrDataLen >= 1) //2017-09-07 15:49:29 There's two-dimensional data.
            {
                byte[] qrData = new Byte[qrDataLen];
                Array.Copy(recv, 64, qrData, 0, qrDataLen); //Data


                log(string.Format("Water Stream={0} 2D raw data:\r\n        {1}\r\n", cmdSequenceId, System.BitConverter.ToString(qrData)).Replace('-', ' '));

                //Convert to String Data 
                log(string.Format("Water Stream={0} 2D raw data(Convert to String):\r\n        {1}",
                    cmdSequenceId, System.Text.Encoding.GetEncoding("GB2312").GetString(qrData).Trim()));


                //2017-09-07 10:56:10 Analyse 2D data
                //...............
                //...............
                long recordCardNO = 0;
                //                   recordCardNO = byteToLong(recv, 64, 4);  //2017-09-07 15:59:19 Test PickQROther Organiser8Bytes You can modify it.
                recordCardNO = cmdSequenceId; //2017-09-07 16:21:18 Replace with streaming. It may also be replaced by Work or card number of the user(It must be numbers.)

                //And we'll do the remote opening.
                long sn;
                sn = byteToLong(recv, 4, 4);

                byte[] buff = new byte[WGPacketShort.WGPacketSize];
                sequenceId4RemoteOpen++;
                //Flow range for remote opening [Users can define themselves]
                if ((sequenceId4RemoteOpen >= 0x7fffffff)  //2017-09-07 11:06:04 
                    || (sequenceId4RemoteOpen < 0x40000001))
                {
                    sequenceId4RemoteOpen = 0x40000001;
                }
                for (int i = 0; i < WGPacketShort.WGPacketSize; i++)
                {
                    buff[i] = 0;
                }
                buff[0] = (byte)0x17; //Type;
                buff[1] = (byte)0x40; //functionID;
                Array.Copy(System.BitConverter.GetBytes(sn), 0, buff, 4, 4);

                //Array.Copy(data, 0, buff, 8, data.Length);
                buff[8 + 0] = (byte)(recordDoorNO & 0xff); //Door number.
                buff[28] = (byte)(recordInOrOut == 1 ? 0 : 1); // recordInOrOut == 1 ? "Come in." : "Out."));
                Array.Copy(System.BitConverter.GetBytes(recordCardNO), 0, buff, 20, 4); //Simulation card number
                Array.Copy(System.BitConverter.GetBytes(recordCardNO), 4, buff, 24, 4); //2017-10-31 14:51:50 Simulation card number High4Bytes
                buff[32] = (byte)(0x5A); //Not bound by permissions within the device
                Array.Copy(System.BitConverter.GetBytes(sequenceId4RemoteOpen), 0, buff, 40, 4);


                int ret = server.UDP_OnlySend(buff);
                if (ret > 0)
                {
                    log(string.Format("\r\n    ====>Success Send a remote opening order. controllerSN={0}, Door number.= {1}, {2} Simulation card number= {3} \r\n",
                        sn.ToString(), recordDoorNO.ToString(), (recordInOrOut == 1 ? "Come in." : "Out."), recordCardNO));
                }
                else
                {
                    log(string.Format("\r\n    ====>??? Failed: Send a remote opening order. controllerSN={0}, Door number.= {1}, {2} Simulation card number= {3} \r\n",
                      sn.ToString(), recordDoorNO.ToString(), (recordInOrOut == 1 ? "Come in." : "Out."), recordCardNO));
                }

            }

        }
        private void Form1_Load(object sender, EventArgs e)
        {
            string hostName = System.Net.Dns.GetHostName();

            Boolean bFound = false;
            foreach (System.Net.IPAddress ipaddr in System.Net.Dns.GetHostEntry(hostName).AddressList) //Get HostIPChile Get HostIPAddress
            {
                if (ipaddr.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork) //2011-12-29_18:53:13 Only allowed IPV4Pass.
                {
                    continue;
                }
                if (ipaddr.IsIPv6LinkLocal)
                {
                    continue; //
                }
                if (ipaddr.ToString() == "127.0.0.1")
                {
                    continue; //
                }
                if (bFound)
                {
                    MessageBox.Show("There are multiple computers.IP, Suggest only one for prior developmentIPOperation.  [If the Wireless and the Internet are in use at the same time,, Key wireless, please.]");
                    break;
                }

                bFound = true;
                txtWatchServerIP.Text = ipaddr.ToString();
            }
            if (!bFound)
            {
                MessageBox.Show("Network's out.! Connect the line, please...");
            }

        }

        //2017-09-08 17:48:33 Get LAN inner controller
        private void btnGetController_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            System.Collections.ArrayList arrControllers = new System.Collections.ArrayList();

            using (WG3000_COMM.Core.wgMjController controllers = new WG3000_COMM.Core.wgMjController())
            {
                controllers.SearchControlers(ref arrControllers);
            }
            this.Cursor = Cursors.Default;

            if (arrControllers != null)
            {
                if (arrControllers.Count <= 0)
                {
                    MessageBox.Show("Not Found No controller found");
                    return;
                }

                //2017-09-08 17:50:36 First
                string[] conf = arrControllers[0].ToString().Split(',');
                this.txtSN.Text = conf[0];
                this.txtIP.Text = conf[1];

            }

        }

        //Open remote
        private void btnRemoteOpenDoor1_Click(object sender, EventArgs e)
        {
            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.
            if (string.IsNullOrEmpty(txtSN.Text))
            {
                MessageBox.Show("Please enter a valid controllerSN");
                return;
            }
            String ControllerIP = txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);

            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //1.10	Open remote[Function Number: 0x40] **********************************************************************************
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
                    //Open the door effectively......
                    log("1.10 Open remote	 Success...");
                    success = 1;
                }
            }
            if (success == 0)
            {
                log("1.10 Open remote	 Failed...");
            }
        }

        private void btnQRFunction_Click(object sender, EventArgs e)
        {
            int ret = 0;
            int success = 0;  //0 Failed, 1It means success.
            if (string.IsNullOrEmpty(txtSN.Text))
            {
                MessageBox.Show("Please enter a valid controllerSN");
                return;
            }
            String ControllerIP = txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);

            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //Query controller driver version[Function Number: 0x94] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x94;
            ret = pkt.run();

            success = 0;
            if (ret == 1)
            {
                string controllerVersion = "0"; //controller version
                controllerVersion = string.Format("{0:X}.{1:X}", pkt.recv[26], pkt.recv[27]);
                log(" Current controller driver version = V" + controllerVersion);
                if (float.Parse(controllerVersion) < 8.0)
                {
                    MessageBox.Show("Control driver version is lower thanV8.76. \r\nPlease upgrade the controller back to the latest driver.. \r\nOr replace the new high-end controller..");
                }
            }
            else
            {
                log("Query controller driver version Failed?????...");
                return;
            }

            //QRSerial Passage Configuration See protocol documents  20170708Add-Set a double entry(Two-dimensional code.)-TestingV8.2Above.doc**********************************************************************************
            pkt.Reset();
            pkt.functionID = 0xF2;
            pkt.data[0] = 0x55; pkt.data[1] = 0xAA; pkt.data[2] = 0xAA; pkt.data[3] = 0x55;
            if (sender == this.btnQR1)
            {
                //2017-09-10 10:37:33Serial1 As [1Door in.]
                pkt.data[4] = 0xE6; pkt.data[5] = 0x01; pkt.data[6] = 0x81;  
            }
            else if (sender == this.btnQR2)
            {
                //2017-09-10 10:37:33Serial2 As [2Door No.]
                pkt.data[4] = 0xEC; pkt.data[5] = 0x01; pkt.data[6] = 0xE1;  
            }
            else if (sender == this.btnQRRestore)
            {
                //Close Thread, Get your card readers back to normal.
                pkt.data[4] = 0xE6; pkt.data[5] = 0x01; pkt.data[6] = 0x0;
                pkt.data[7] = 0xEC; pkt.data[8] = 0x01; pkt.data[9] = 0x0;
            }
            else
            {
                return; //
            }
            ret = pkt.run();
            success = 0;
            if (ret > 0)
            {
                if (pkt.recv[8] == 1)
                {
                    log(string.Format("2D Code Operations	 Success...{0}", (sender as Button).Text));
                    success = 1;
                    
                }
            }
            if (success == 0)
            {
                log(string.Format("2D Code Operations	 Failed????...{0}", (sender as Button).Text));
            }
        }

        //2017-09-12 12:13:49 Get controller driver version
        private void btnGetDriverVersion_Click(object sender, EventArgs e)
        {
            int ret = 0;
            if (string.IsNullOrEmpty(txtSN.Text))
            {
                MessageBox.Show("Please enter a valid controllerSN");
                return;
            }
            String ControllerIP = txtIP.Text;
            long controllerSN = long.Parse(txtSN.Text);

            //Create short message pkt
            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = controllerSN;
            pkt.IP = ControllerIP;

            //Query controller driver version[Function Number: 0x94] **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x94;
            ret = pkt.run();

            if (ret == 1)
            {
                string controllerVersion = "0"; //controller version
                controllerVersion = string.Format("{0:X}.{1:X}", pkt.recv[26], pkt.recv[27]);
                log(" Current controller driver version = V" + controllerVersion);
            }
            else
            {
                log("Query controller driver version Failed?????...");
                return;
            }
            
        }



    }
}
