'*
'* WGController32 2015-04-30 17:40:43 karl CSN  Chan Shonin $
'*
'* Doorbar controller Shortcast agreement Test cases
'* V2.6 Version  2015-11-03 20:25:53 V6.60Driver Version Communications password testing.  
'*                               Retry to modify communication
'* V2.5 Version  2015-04-29 20:41:30 Adopt V6.56Driver Version Model by0x19For0x17
'*            Basic functions:  Query controller status
'*                       Read Date Time
'*                       Set Date Time
'*                       Get a record of the given index number
'*                       Set a read record index number
'*                       Get read record index numbers
'*                       Open remote
'*                       Permissions to add or modify
'*                       Permission to delete(Individual Delete)
'*                       Clear Permissions(Clear it all.)
'*                       Total Permissions Read
'*                       Permission Query
'*                       Set door control parameters(Online/Delay)
'*                       Read door control parameters(Online/Delay)
'
'*                       Set up the receiver serverIPand Port
'*                       Read the receiver server.IPand Port
'*
'*
'*                       Receiving server realization (Yes.61005Port Reception Data) -- This function Be careful with the firewall. It has to be allowed to receive data..
'
Imports System
Imports System.Collections.Generic
Imports System.ComponentModel
Imports System.Data
Imports System.Drawing
Imports System.Text
Imports System.Windows.Forms
Imports System.Diagnostics


Partial Class Form1

    Private bStopWatchServer As Boolean = False
    '2015-05-05 17:35:07 Stop receiving server
    Private bStopBasicFunction As Boolean = False
    '2015-06-10 09:04:52 Basic tests
    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        bStopWatchServer = True
        bStopBasicFunction = True
        '2015-06-10 09:04:52 Basic tests
    End Sub
    Private Sub button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button1.Click
        Me.txtInfo.Text = ""
        'Stop receiving server identification 
        bStopWatchServer = True
        bStopBasicFunction = False
        '2015-06-10 09:04:52 Basic tests
        ''    'No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
        ''    'Test instructions in this case
        ''    'controllerSN  = 229999901
        ''    'controllerIP  = 192.168.168.123
        ''    'Computer  IP  = 192.168.168.101
        ''    'For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)
        'Basic function test
        'txtSN.Text controller9Series of bitsSN
        'txtIP.Text controllerIPAddress, Not adopted192.168.168.123  [Available Search Controller Modify controllerIP]
        testBasicFunction(txtIP.Text, Long.Parse(txtSN.Text))
        'txtWatchServerIP.Text  From the receiver.IP,Computer defaultIP 192.168.168.101 [It can also be used Search Controller Modify Settings]
        'txtWatchServerPort.Text  From the receiver.PORT, Defaults 61005
        testWatchingServer(txtIP.Text, Long.Parse(txtSN.Text), txtWatchServerIP.Text, Integer.Parse(Me.txtWatchServerPort.Text))
        'Receiving Server Settings
        bStopWatchServer = False
        WatchingServerRuning(txtWatchServerIP.Text, Integer.Parse(Me.txtWatchServerPort.Text))
        'Server Run....
        bStopWatchServer = True
    End Sub
    Private Sub button2_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button2.Click
        bStopWatchServer = True
        bStopBasicFunction = True
        '2015-06-10 09:04:52 Basic tests
    End Sub
    Private Sub button3_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button3.Click
        '2015-05-05 17:35:35 Search controller
        Try
            Dim pInfo As New ProcessStartInfo()
            pInfo.FileName = Environment.CurrentDirectory + "\IPCon2015_V2.17.exe"
            pInfo.UseShellExecute = True
            Dim p As Process = Process.Start(pInfo)
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            MessageBox.Show(ex.ToString())
        End Try
    End Sub
    Private Sub button4_Click(ByVal sender As Object, ByVal e As EventArgs) Handles Button4.Click
        bStopWatchServer = False
        WatchingServerRuning(txtWatchServerIP.Text, Integer.Parse(Me.txtWatchServerPort.Text))
        'Server Run....
        bStopWatchServer = True
    End Sub
    ''' <summary>
    ''' Shortcasts
    ''' </summary>
    Class WGPacketShort
        Public Shared WGPacketSize As Integer = 64
        'Length of submission
        '2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
        Public Shared Type As Integer = &H17
        '2015-04-29 22:22:50			//Type
        Public Shared ControllerPort As Integer = 60000
        'controller port
        Public Shared SpecialFlag As Long = &H55AAAA55
        'Special identification Prevent mishandling
        Public functionID As Integer
        'Function Number
        Public iDevSn As Long
        'Device serial number 4Bytes, 9Digits
        Public IP As String
        'The controller.IPAddress
        Public data As Byte() = New Byte(56 - 1) {}
        '56Byte Data [Fluid]
        Public recv As Byte() = New Byte(WGPacketSize - 1) {}
        'Data received
        Public Sub New()
            Reset()
        End Sub
        Public Sub Reset()
            'Data Reunification
            For i As Integer = 0 To 55
                data(i) = 0
            Next
        End Sub
        Shared sequenceId As Long
        'Serial number	
        Public Function toByte() As Byte()
            'Generate64Bytes package
            Dim buff As Byte() = New Byte(WGPacketSize - 1) {}
            sequenceId += 1
            buff(0) = Type
            buff(1) = functionID
            Array.Copy(System.BitConverter.GetBytes(iDevSn), 0, buff, 4, 4)
            Array.Copy(data, 0, buff, 8, data.Length)
            Array.Copy(System.BitConverter.GetBytes(sequenceId), 0, buff, 40, 4)
            Return buff
        End Function
        Private controller As New WG3000_COMM.Core.wgMjController()
        Public Function run() As Integer
            'Send Command Can not open message
            Dim buff As Byte() = toByte()
            Dim tries As Integer = 3
            Dim errcnt As Integer = 0
            controller.IP = IP
            controller.PORT = ControllerPort
            Do
                If controller.ShortPacketSend(buff, recv) < 0 Then
                    '2015-11-03 20:26:52 Enter Retry Return -1
                Else
                    'Water Stream
                    Dim sequenceIdReceived As Long = 0
                    For i As Integer = 0 To 3
                        Dim lng As Long = recv(40 + i)
                        sequenceIdReceived += (lng << (8 * i))
                    Next
                        If (recv(0) = Type) AndAlso (recv(1) = functionID) AndAlso (sequenceIdReceived = sequenceId) Then
                            'Align type
                            'Function numbers are consistent
                            'Serial number corresponding
                            Return 1
                        Else
                            errcnt += 1
                        End If
                End If
            Loop While System.Math.Max(System.Threading.Interlocked.Decrement(tries), tries + 1) > 0
            'Try again three times.
            Return -1
        End Function
        ''' <summary>
        ''' It's the last running water.
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function sequenceIdSent() As Long
            ' 
            Return sequenceId
            ' It's the last running water.
        End Function
        ''' <summary>
        ''' Close
        ''' </summary>
        Public Sub close()
            controller.Dispose()
        End Sub
    End Class
    Private Sub log(ByVal info As String)
        'Log Information
        'txtInfo.Text += String.Format("{0}" & Chr(13) & "" & Chr(10) & "", info)
        'txtInfo.AppendText(String.Format("{0}" & Chr(13) & "" & Chr(10) & "", info))
        txtInfo.AppendText(String.Format("{0} {1}" & Chr(13) & "" & Chr(10) & "", Date.Now.ToString("HH:mm:ss"), info)) '2015-11-03 21:04:24 Time
        txtInfo.ScrollToCaret()  'Scroll to cursor
        Application.DoEvents()
    End Sub
    ''' <summary>
    ''' 4Byte to Integer(Down front., Behind you.)
    ''' </summary>
    ''' <param name="buff">Bytes</param>
    ''' <param name="start">Start Indexing Post(From0Start counting.)</param>
    ''' <param name="len">Length</param>
    ''' <returns>Integer</returns>
    Private Function byteToLong(ByVal buff As Byte(), ByVal start As Integer, ByVal len As Integer) As Long
        Dim val As Long = 0
        Dim i As Integer = 0
        While i < len AndAlso i < 4
            '2015-06-10 10:29:42 Increase (long)
            Dim lng As Long = buff(i + start)
            val += (lng << (8 * i))
            i += 1
        End While
        Return val
    End Function
    ''' <summary>
    ''' Convert the integer to4Bytes
    ''' </summary>
    ''' <param name="outBytes">Array</param>
    ''' <param name="startIndex">Start Indexing Post(From0Start counting.)</param>
    ''' <param name="val">Value</param>
    Private Sub LongToBytes(ByRef outBytes As Byte(), ByVal startIndex As Integer, ByVal val As Long)
        Array.Copy(System.BitConverter.GetBytes(val), 0, outBytes, startIndex, 4)
    End Sub
    ''' <summary>
    ''' AccessHexValue, Mainly used in date time format
    ''' </summary>
    ''' <param name="val">Value</param>
    ''' <returns>HexValue</returns>
    Private Function GetHex(ByVal val As Integer) As Integer
        Return ((val Mod 10) + (((val - (val Mod 10)) / 10) Mod 10) * 16)
    End Function
   

    ''' <summary>
    ''' Show Record Information
    ''' </summary>
    ''' <param name="recv"></param>
    ''' <remarks></remarks>
    Private Sub displayRecordInformation(ByRef recv() As Byte)
        '8-11	Record index number
        '(=0No record.)	4	0x00000000
        Dim recordIndex As Integer = 0
        recordIndex = (byteToLong(recv, 8, 4))
        '12	Record type**********************************************
        '0=No record
        '1=Brush Card Record
        '2=Door Magnetic,button, Device startup, Remote Open Record
        '3=Call the police.	1	
        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        Dim recordType As Integer = recv(12)
        '13	Validity(0 Not approved, 1Adopted)	1	
        Dim recordValid As Integer = recv(13)
        '14	Door number.(1,2,3,4)	1	
        Dim recordDoorNO As Integer = recv(14)
        '15	Come in./Out.(1It means coming in., 2Means out.)	1	0x01
        Dim recordInOrOut As Integer = recv(15)
        '16-19	Card(Type is when swiping a card.)
        'or numbering(Other types of records)	4	
        Dim recordCardNO As Long = 0
        recordCardNO = (byteToLong(recv, 16, 4))
        '20-26	Brush Time:
        'Days and days of year (AdoptBCDCode)See description of the set-up segment
        Dim recordTime As String = "2000-01-01 00:00:00"
        recordTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", recv(20), recv(21), recv(22), recv(23), recv(24), _
            recv(25), recv(26))
        '2012.12.11 10:49:59	7	
        '27	Record cause code(You can check it out. “Checkcard log notes.xls”It's a file.ReasonNO)
        'It's only for complex information.	1	
        Dim reason As Integer = recv(27)
        '0=No record
        '1=Brush Card Record
        '2=Door Magnetic,button, Device startup, Remote Open Record
        '3=Call the police.	1	
        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        If recordType = 0 Then
            log(String.Format("Index post={0}  No record", recordIndex))
        ElseIf recordType = 255 Then
            log(String.Format(" The records of the specified index have been overwritten,Use the index.0, Retrieving index values from the earliest record"))
        ElseIf recordType = 1 Then
            '2015-06-10 08:49:31 Show data with card number type of record
            'Card
            log(String.Format("Index post={0}  ", recordIndex))
            log(String.Format("  Card = {0}", recordCardNO))
            log(String.Format("  Door number. = {0}", recordDoorNO))
            log(String.Format("  Access = {0}", IIf(recordInOrOut = 1, "Come in.", "Out.")))
            log(String.Format("  Valid. = {0}", IIf(recordValid = 1, "Pass.", "Ban")))
            log(String.Format("  Time = {0}", recordTime))
            log(String.Format("  Reason = {0}", getReasonDetailChinese(reason)))
        ElseIf recordType = 2 Then
            'Other processing
            'Door Magnetic,button, Device startup, Remote Open Record
            log(String.Format("Index post={0}  Non-card records", recordIndex))
            log(String.Format("  Numbering = {0}", recordCardNO))
            log(String.Format("  Door number. = {0}", recordDoorNO))
            log(String.Format("  Time = {0}", recordTime))
            log(String.Format("  Reason = {0}", getReasonDetailChinese(reason)))
        ElseIf recordType = 3 Then
            'Other processing
            'Call the police.
            log(String.Format("Index post={0}  Call the police.", recordIndex))
            log(String.Format("  Numbering = {0}", recordCardNO))
            log(String.Format("  Door number. = {0}", recordDoorNO))
            log(String.Format("  Time = {0}", recordTime))
            log(String.Format("  Reason = {0}", getReasonDetailChinese(reason)))
        End If
    End Sub

    'Record cause (Type SwipePass Adopted; SwipeNOPassMeans no pass.; ValidEvent Effective Event(Like buttons Door Magnetic Supercode open.); Warn Call the police.)
    'Code  Type   English Description  Chinese Description
    Private RecordDetails As String() = {
"1", "SwipePass", "Swipe", "Open the swipe.",
"2", "SwipePass", "Swipe Close", "Brush off",
"3", "SwipePass", "Swipe Open", "Open it.",
"4", "SwipePass", "Swipe Limited Times", "Open the swipe.(Time limit)",
"5", "SwipeNOPass", "Denied Access: PC Control", "It's forbidden to pass.: Computer control",
"6", "SwipeNOPass", "Denied Access: No PRIVILEGE", "It's forbidden to pass.: No Permissions",
"7", "SwipeNOPass", "Denied Access: Wrong PASSWORD", "It's forbidden to pass.: Wrong password.",
"8", "SwipeNOPass", "Denied Access: AntiBack", "It's forbidden to pass.: Backwards",
"9", "SwipeNOPass", "Denied Access: More Cards", "It's forbidden to pass.: Doc!",
"10", "SwipeNOPass", "Denied Access: First Card Open", "It's forbidden to pass.: First Card",
"11", "SwipeNOPass", "Denied Access: Door Set NC", "It's forbidden to pass.: It's always closed.",
"12", "SwipeNOPass", "Denied Access: InterLock", "It's forbidden to pass.: Interlock",
"13", "SwipeNOPass", "Denied Access: Limited Times", "It's forbidden to pass.: Limited number of brush cards",
"14", "SwipeNOPass", "Denied Access: Limited Person Indoor", "It's forbidden to pass.: Number of people in the door",
"15", "SwipeNOPass", "Denied Access: Invalid Timezone", "It's forbidden to pass.: Card expired or not valid",
"16", "SwipeNOPass", "Denied Access: In Order", "It's forbidden to pass.: Ordered access restrictions",
"17", "SwipeNOPass", "Denied Access: SWIPE GAP LIMIT", "It's forbidden to pass.: Brush Card Interval",
"18", "SwipeNOPass", "Denied Access", "It's forbidden to pass.: Reason unknown.",
"19", "SwipeNOPass", "Denied Access: Limited Times", "It's forbidden to pass.: Limit number of brushes",
"20", "ValidEvent", "Push Button", "Button open.",
"21", "ValidEvent", "Push Button Open", "Button On",
"22", "ValidEvent", "Push Button Close", "Button Off",
"23", "ValidEvent", "Door Open", "Open the door.[Door Magnetic Signal]",
"24", "ValidEvent", "Door Closed", "Door closed.[Door Magnetic Signal]",
"25", "ValidEvent", "Super Password Open Door", "Supercode open.",
"26", "ValidEvent", "Super Password Open", "Supercode open.",
"27", "ValidEvent", "Super Password Close", "Super Password Level",
"28", "Warn", "Controller Power On", "Power on the controller.",
"29", "Warn", "Controller Reset", "Control Reposition",
"30", "Warn", "Push Button Invalid: Disable", "Buttons don't open.: button disabled",
"31", "Warn", "Push Button Invalid: Forced Lock", "Buttons don't open.: Force the closing.",
"32", "Warn", "Push Button Invalid: Not On Line", "Buttons don't open.: The door's offline.",
"33", "Warn", "Push Button Invalid: InterLock", "Buttons don't open.: Interlock",
"34", "Warn", "Threat", "Coercion to the police.",
"35", "Warn", "Threat Open", "Coercion to call the police.",
"36", "Warn", "Threat Close", "Coercion to alarm.",
"37", "Warn", "Open too long", "The door was open for a long time.[After legally opening the door,]",
"38", "Warn", "Forced Open", "Forced breaking into the police.",
"39", "Warn", "Fire", "Fire!",
"40", "Warn", "Forced Close", "Force the closing.",
"41", "Warn", "Guard Against Theft", "It's an alarm.",
"42", "Warn", "7*24Hour Zone", "Smoke gas temperature alert.",
"43", "Warn", "Emergency Call", "Call 911.",
"44", "RemoteOpen", "Remote Open Door", "Operator opens the door remotely.",
"45", "RemoteOpen", "Remote Open Door By USB Reader", "The transmitter has confirmed the remote opening."
                                        }
    Private Function getReasonDetailChinese(ByVal Reason As Integer) As String
        'Chinese
        If Reason > 45 Then
            Return ""
        End If
        If Reason <= 0 Then
            Return ""
        End If
        Return RecordDetails((Reason - 1) * 4 + 3)
        'Chinese Information
    End Function
    Private Function getReasonDetailEnglish(ByVal Reason As Integer) As String
        'English Description
        If Reason > 45 Then
            Return ""
        End If
        If Reason <= 0 Then
            Return ""
        End If
        Return RecordDetails((Reason - 1) * 4 + 2)
        'Information in English
    End Function
    ''' <summary>
    ''' Basic function test
    ''' </summary>
    ''' <param name="ControllerIP">controllerIPAddress</param>
    ''' <param name="controllerSN"> Control serial number</param>
    ''' <returns>less than or equal to0 Failed, 1It means success.</returns>
    Private Function testBasicFunction(ByVal ControllerIP As String, ByVal controllerSN As Long) As Integer
        Dim ret As Integer = 0
        Dim success As Integer = 0
        '0 Failed, 1It means success.
        'Create short message pkt
        Dim pkt As New WGPacketShort()
        pkt.iDevSn = controllerSN
        pkt.IP = ControllerIP
        '1.4	Query controller status[Function Number: 0x20](Real time surveillance) **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H20
        ret = pkt.run()
        success = 0
        If ret = 1 Then
            'Read information successfully...
            success = 1
            log("1.4 Query controller status Success...")
            '	  	Last recorded information		
            displayRecordInformation(pkt.recv)
            '2015-06-09 20:01:21
            '	Other information		
            Dim doorStatus As Integer() = New Integer(4) {}
            '28	1Door no.(0Means close, 1Show Open)	1	0x00
            doorStatus(1 - 1) = pkt.recv(28)
            '29	2Door no.(0Means close, 1Show Open)	1	0x00
            doorStatus(2 - 1) = pkt.recv(29)
            '30	3Door no.(0Means close, 1Show Open)	1	0x00
            doorStatus(3 - 1) = pkt.recv(30)
            '31	4Door no.(0Means close, 1Show Open)	1	0x00
            doorStatus(4 - 1) = pkt.recv(31)
            Dim pbStatus As Integer() = New Integer(4) {}
            '32	1Door button.(0It means you let go., 1Means press)	1	0x00
            pbStatus(1 - 1) = pkt.recv(32)
            '33	2Door button.(0It means you let go., 1Means press)	1	0x00
            pbStatus(2 - 1) = pkt.recv(33)
            '34	3Door button.(0It means you let go., 1Means press)	1	0x00
            pbStatus(3 - 1) = pkt.recv(34)
            '35	4Door button.(0It means you let go., 1Means press)	1	0x00
            pbStatus(4 - 1) = pkt.recv(35)
            '36	Fault.
            'equals0 No malfunctions.
            'Not equal to0, It's not working.(Reset Time, If there's anything else,, We're going back to the factory.)	1	
            Dim errCode As Integer = pkt.recv(36)
            '37	Control Current Time
            'Time	1	0x21
            '38	min	1	0x30
            '39	sec	1	0x58
            '40-43	Water Stream	4	
            Dim sequenceId As Long = 0
            sequenceId = byteToLong(pkt.recv, 40, 4)
            '48
            'Special Information1(Return based on actual use)
            'Keyboard Key Information	1	
            '49	Relay status	1	 [0The door is locked., 1It means the door is locked.. When the normal door is locked, Value as0000]
            Dim relayStatus As Integer = pkt.recv(49)
            'Door one. Open the lock.
            If (relayStatus And 1) > 0 Then
                'Door one. Lock it.
            Else
            End If
            'Door two. Open the lock.
            If (relayStatus And 2) > 0 Then
                'Door two. Lock it.
            Else
            End If
            'Gate three. Open the lock.
            If (relayStatus And 4) > 0 Then
                'Gate three. Lock it.
            Else
            End If
            'Gate four. Open the lock.
            If (relayStatus And 8) > 0 Then
                'Gate four. Lock it.
            Else
            End If
            '50	Door magnetic.8-15bitbit[Fire!/Force locking]
            'Bit0  Force locking
            'Bit1  Fire!		
            Dim otherInputStatus As Integer = pkt.recv(50)
            'Force locking
            If (otherInputStatus And 1) > 0 Then
            End If
            'Fire!
            If (otherInputStatus And 2) > 0 Then
            End If
            '51	V5.46Version Support Control Current Year	1	0x13
            '52	V5.46Version Support Month	1	0x06
            '53	V5.46Version Support Day	1	0x22
            Dim controllerTime As String = "2000-01-01 00:00:00"
            'Control Current Time
            controllerTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", 32, pkt.recv(51), pkt.recv(52), pkt.recv(53), pkt.recv(37), _
             pkt.recv(38), pkt.recv(39))
        Else
            log("1.4 Query controller status Failed?????...")
            Return -1
        End If
        '1.5	Read Date Time(Function Number: 0x32) **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H32
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim controllerTime As String = "2000-01-01 00:00:00"
            'Control Current Time
            controllerTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", pkt.recv(8), pkt.recv(9), pkt.recv(10), pkt.recv(11), pkt.recv(12), _
             pkt.recv(13), pkt.recv(14))
            log("1.5 Read Date Time Success...")
            success = 1
        End If
        '1.6	Set Date Time[Function Number: 0x30] **********************************************************************************
        'calibrate controller at computer time.....
        pkt.Reset()
        pkt.functionID = &H30
        Dim ptm As DateTime = DateTime.Now
        pkt.data(0) = (GetHex((ptm.Year - ptm.Year Mod 100) / 100))
        pkt.data(1) = (GetHex((((ptm.Year) Mod 100))))
        'st.GetMonth()); 
        pkt.data(2) = (GetHex(ptm.Month))
        pkt.data(3) = (GetHex(ptm.Day))
        pkt.data(4) = (GetHex(ptm.Hour))
        pkt.data(5) = (GetHex(ptm.Minute))
        pkt.data(6) = (GetHex(ptm.Second))
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim bSame As Boolean = True
            For i As Integer = 0 To 6
                If pkt.data(i) <> pkt.recv(8 + i) Then
                    bSame = False
                    Exit For
                End If
            Next
            If bSame Then
                log("1.6 Set Date Time Success...")
                success = 1
            End If
        End If
        '1.7	Get a record of the given index number[Function Number: 0xB0] **********************************************************************************
        '(Take Index Number 0x00000001Records)
        Dim recordIndexToGet As Long = 0
        pkt.Reset()
        pkt.functionID = &HB0
        pkt.iDevSn = controllerSN
        '	(Special
        'If=0, Retrieving the earliest recorded information
        'If=0xffffffffRetrieving information from the last record)
        'Records index numbers are normally incremental., Max.0xffffff = 16,777,215 (Over1Millions.) . Due to limited storage space, Only the closest on the controller.20Thousands of records.. When index numbers exceed20After 10,000., The records of the old index numbers are overwritten., So at this point, check the records of these index numbers., The type of record returned will be0xff, It means it doesn't exist..
        recordIndexToGet = 1
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 Get Index As1Recorded information Success...")
            '	  	Index to1Recorded information		
            displayRecordInformation(pkt.recv)
            success = 1
        End If
        '. Communication (Take the earliest record By Index Number 0x00000000) [This command is appropriate Brushing card records over20Usage in time environment]
        pkt.Reset()
        pkt.functionID = &HB0
        recordIndexToGet = 0
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 Fetch information from the earliest record Success...")
            '	  	First recorded information		
            displayRecordInformation(pkt.recv)
            success = 1
        End If
        'Communication (Take the latest record. By Index 0xffffffff)
        pkt.Reset()
        pkt.functionID = &HB0
        recordIndexToGet = 4294967295
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 Getting information on the latest record Success...")
            '	  	Last recorded information		
            displayRecordInformation(pkt.recv)
            '2015-06-09 20:01:21
            success = 1
        End If
        '    '1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
        '    pkt.Reset()
        '    pkt.functionID = &HB2
        '    ' (Set read record index number as5)
        '    Dim recordIndexGot As Integer = 5
        '    LongToBytes(pkt.data, 0, recordIndexGot)

        '    '12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
        '    LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
        '    ret = pkt.run()
        '    success = 0
        '    If ret > 0 Then
        '        If pkt.recv(8) = 1 Then
        '            log("1.8 Set a read record index number Success...")
        '            success = 1
        '        End If
        '    End If

        '    '1.9	Get read record index numbers[Function Number: 0xB4] **********************************************************************************
        '    pkt.Reset()
        '    pkt.functionID = &HB4
        '    Dim recordIndexGotToRead As Integer = 0
        '    ret = pkt.run()
        '    success = 0
        '    If ret > 0 Then
        '        recordIndexGotToRead = (byteToLong(pkt.recv, 8, 4))
        '        log("1.9 Get read record index numbers Success...")
        '        success = 1
        '    End If


        ''1.8	Set a read record index number[Function Number: 0xB2] **********************************************************************************
        ''Restore extracted records, Yes1.9Prepare for full extraction-- In use, It's only recovered when problems arise., Normal....
        'pkt.Reset()
        'pkt.functionID = &HB2
        '' (Set read record index number as0)
        'Dim recordIndexGot As Integer = 0
        'LongToBytes(pkt.data, 0, recordIndexGot)
        ''12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
        'LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
        'ret = pkt.run()
        'success = 0
        'If ret > 0 Then
        '    If pkt.recv(8) = 1 Then
        '        log("1.8 Set a read record index number Success...")
        '        success = 1
        '    End If
        'End If


        '1.9	Extract Record Operation
        '1. Pass. 0xB4Command Get read record index numbers recordIndex
        '2. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
        '3. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
        'After three steps,， The entire extraction record is complete.
        log("1.9 Extract Record Operation	 Start...")
        pkt.Reset()
        pkt.functionID = &HB4
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim recordIndexGotToRead As Integer = 0
            recordIndexGotToRead = (byteToLong(pkt.recv, 8, 4))
            pkt.Reset()
            pkt.functionID = &HB0
            pkt.iDevSn = controllerSN
            Dim recordIndexToGetStart As Integer = recordIndexGotToRead + 1
            Dim recordIndexValidGet As Integer = 0
            Dim cnt As Integer = 0
            Do
                If bStopBasicFunction Then
                    '2015-06-10 09:08:14 Stop
                    Return 0
                End If
                LongToBytes(pkt.data, 0, recordIndexToGetStart)
                ret = pkt.run()
                success = 0
                If ret > 0 Then
                    success = 1
                    '12	Record type
                    '0=No record
                    '1=Brush Card Record
                    '2=Door Magnetic,button, Device startup, Remote Open Record
                    '3=Call the police.	1	
                    '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
                    Dim recordType As Integer = pkt.recv(12)
                    If recordType = 0 Then
                        'No more records.
                        Exit Do
                    End If
                    If recordType = 255 Then
                        'This index number is invalid  Reset Index Values
                        'Take the earliest record index bit
                        pkt.Reset()
                        pkt.functionID = &HB0
                        recordIndexToGet = 0
                        LongToBytes(pkt.data, 0, recordIndexToGet)
                        ret = pkt.run()
                        success = 0
                        If ret > 0 Then
                            log("1.7 Fetch information from the earliest record Success...")
                            recordIndexGotToRead = (byteToLong(pkt.recv, 8, 4))
                            recordIndexToGetStart = recordIndexGotToRead
                            Continue Do
                        End If
                        success = 0
                        Exit Do
                    End If
                    recordIndexValidGet = recordIndexToGetStart
                    '2015-06-09 20:01:21
                    '.......Storage of records received
                    '*****
                    '###############
                    displayRecordInformation(pkt.recv)
                Else
                    'Ripping failed
                    Exit Do
                End If
                recordIndexToGetStart += 1
            Loop While System.Math.Max(System.Threading.Interlocked.Increment(cnt), cnt - 1) < 200000
            If success > 0 Then
                'Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
                pkt.Reset()
                pkt.functionID = &HB2
                LongToBytes(pkt.data, 0, recordIndexValidGet)
                '12	Identification(Prevent Error Settings)	1	0x55 [Fixed]
                LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
                ret = pkt.run()
                success = 0
                If ret > 0 Then
                    If pkt.recv(8) = 1 Then
                        'Full extraction successful.....
                        log("1.9 Full extraction successful.  Success...")
                        success = 1
                    End If
                End If
            End If
        End If
        '1.10	Open remote[Function Number: 0x40] **********************************************************************************
        Dim doorNO As Integer = 1
        pkt.Reset()
        pkt.functionID = &H40
        pkt.data(0) = (doorNO And 255)
        '2013-11-03 20:56:33
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'Open the door effectively......
                log("1.10 Open remote Success...")
                success = 1
            End If
        End If
        '1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
        'Add card number0D D7 37 00, Through all doors of the current controller
        pkt.Reset()
        pkt.functionID = &H50
        '0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
        Dim cardNOOfPrivilege As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilege)
        '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
        pkt.data(4) = 32
        pkt.data(5) = 16
        pkt.data(6) = 1
        pkt.data(7) = 1
        '20 29 12 31 Deadline:  2029Year12Month31Day
        pkt.data(8) = 32
        pkt.data(9) = 41
        pkt.data(10) = 18
        pkt.data(11) = 49
        '01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
        pkt.data(12) = 1
        '01 Allow Pass Door two. [Two doors., Four controllers working.]
        pkt.data(13) = 1
        'If it's forbidden,2Door., As 0x00
        '01 Allow Pass Gate three. [It works on four controllers.]
        pkt.data(14) = 1
        '01 Allow Pass Gate four. [It works on four controllers.]
        pkt.data(15) = 1
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
                log("1.11 Permissions to add or modify  Success...")
                success = 1
            End If
        End If
        '1.12	Permission to delete(Individual Delete)[Function Number: 0x52] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H52
        pkt.iDevSn = controllerSN
        'Permission card number to delete0D D7 37 00  = 0x0037D70D = 3659533 (Decimal)
        Dim cardNOOfPrivilegeToDelete As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilegeToDelete)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay doesn't move..
                log("1.12 Permission to delete(Individual Delete)  Success...")
                success = 1
            End If
        End If
        '1.13	Clear Permissions(Clear it all.)[Function Number: 0x54] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H54
        pkt.iDevSn = controllerSN
        LongToBytes(pkt.data, 0, WGPacketShort.SpecialFlag)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'It's time to clear up.
                log("1.13 Clear Permissions(Clear it all.)  Success...")
                success = 1
            End If
        End If
        '1.14	Total Permissions Read[Function Number: 0x58] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H58
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim privilegeCount As Integer = 0
            privilegeCount = (byteToLong(pkt.recv, 8, 4))
            log("1.14 Total Permissions Read  Success...")
            success = 1
        End If
        'Add again as a query operation 1.11	Permissions to add or modify[Function Number: 0x50] **********************************************************************************
        'Add card number0D D7 37 00, Through all doors of the current controller
        pkt.Reset()
        pkt.functionID = &H50
        '0D D7 37 00 Card number in permission to add or modify = 0x0037D70D = 3659533 (Decimal)
        cardNOOfPrivilege = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilege)
        '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
        pkt.data(4) = 32
        pkt.data(5) = 16
        pkt.data(6) = 1
        pkt.data(7) = 1
        '20 29 12 31 Deadline:  2029Year12Month31Day
        pkt.data(8) = 32
        pkt.data(9) = 41
        pkt.data(10) = 18
        pkt.data(11) = 49
        '01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
        pkt.data(12) = 1
        '01 Allow Pass Door two. [Two doors., Four controllers working.]
        pkt.data(13) = 1
        'If it's forbidden,2Door., As 0x00
        '01 Allow Pass Gate three. [It works on four controllers.]
        pkt.data(14) = 1
        '01 Allow Pass Gate four. [It works on four controllers.]
        pkt.data(15) = 1
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'And then... The card number is= 0x0037D70D = 3659533 (Decimal)Card, 1Door relay action..
                log("1.11 Permissions to add or modify  Success...")
                success = 1
            End If
        End If
        '1.15	Permission Query[Function Number: 0x5A] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H5A
        pkt.iDevSn = controllerSN
        ' (The Chaka is 0D D7 37 00Competence)
        Dim cardNOOfPrivilegeToQuery As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilegeToQuery)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim cardNOOfPrivilegeToGet As Long = 0
            cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4)
            If cardNOOfPrivilegeToGet = 0 Then
                'When no permission: (The card number is0)
                log("1.15      Can not open message: (The card number is0)")
            Else
                'Specific Permission Information...
                log("1.15     Can not open message...")
            End If
            log("1.15 Permission Query  Success...")
            success = 1
        End If
        '1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H5C
        pkt.iDevSn = controllerSN
        Dim QueryIndex As Long = 1
        'Index number(From1Start);
        LongToBytes(pkt.data, 0, QueryIndex)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim cardNOOfPrivilegeToGet As Long = 0
            cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4)
            If 4294967295 = cardNOOfPrivilegeToGet Then
                'FFFFFFFFResponse4294967295
                log("1.16      Can not open message: (Permissions deleted)")
            ElseIf cardNOOfPrivilegeToGet = 0 Then
                'When no permission: (The card number is0)
                log("1.16       Can not open message: (The card number is0)--This index number is no longer valid.")
            Else
                'Specific Permission Information...
                log("1.16      Can not open message...")
            End If
            log("1.16 Access to specified index numbers  Success...")
            success = 1
        End If
        '1.17	Set door control parameters(Online/Delay) [Function Number: 0x80] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H80
        '(Settings2Door. Online  Open the door late. 3sec)
        pkt.data(0) = 2
        '2Door.
        pkt.data(1) = 3
        'Online
        pkt.data(2) = 3
        'Open the door late.
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.data(0) = pkt.recv(8) AndAlso pkt.data(1) = pkt.recv(9) AndAlso pkt.data(2) = pkt.recv(10) Then
                'When successful, Return values to match settings
                log("1.17 Set door control parameters Success...")
                success = 1
                'Failed
            Else
            End If
        End If


        '1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        'This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        'Suggested number of privileges updated over50individual, Use this command
        log("1.21 Permissions added from childhood to larger[Function Number: 0x56]Start...")
        log("       1Thousand powers...")
        'Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required

        Dim cardCount As Integer = 10000
        '2015-06-09 20:20:20 Total number of cards
        Dim cardArray As Long() = New Long(cardCount - 1) {}
        For i As Integer = 0 To cardCount - 1
            cardArray(i) = 50001 + i
        Next
        For i As Integer = 0 To cardCount - 1
            If bStopBasicFunction Then
                '2015-06-10 09:08:14 Stop
                Return 0
            End If
            pkt.Reset()
            pkt.functionID = &H56
            cardNOOfPrivilege = cardArray(i)
            LongToBytes(pkt.data, 0, cardNOOfPrivilege)
            'When other parameters are simplified Harmonization, You can make changes depending on each card.
            '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
            pkt.data(4) = 32
            pkt.data(5) = 16
            pkt.data(6) = 1
            pkt.data(7) = 1
            '20 29 12 31 Deadline:  2029Year12Month31Day
            pkt.data(8) = 32
            pkt.data(9) = 41
            pkt.data(10) = 18
            pkt.data(11) = 49
            '01 Allow Pass Door one. [Single door., Double door., Four controllers working.] 
            pkt.data(12) = 1
            '01 Allow Pass Door two. [Two doors., Four controllers working.]
            pkt.data(13) = 1
            'If it's forbidden,2Door., As 0x00
            '01 Allow Pass Gate three. [It works on four controllers.]
            pkt.data(14) = 1
            '01 Allow Pass Gate four. [It works on four controllers.]
            pkt.data(15) = 1
            LongToBytes(pkt.data, 32 - 8, cardCount)
            'Total permissions
            LongToBytes(pkt.data, 35 - 8, i + 1)
            'The index place for the current permission(From1Start)
            ret = pkt.run()
            success = 0
            If ret > 0 Then
                If pkt.recv(8) = 1 Then
                    success = 1
                End If
                If pkt.recv(8) = &HE1 Then
                    log("1.21Permissions added from childhood to larger[Function Number: 0x56] =0xE1 Which means the card number has not been sorted from a small to a large size....???")
                    success = 0
                    Exit For
                End If
            Else
                Exit For
            End If
        Next
        If success = 1 Then
            log("1.21Permissions added from childhood to larger[Function Number: 0x56] Success...")
        Else
            log("1.21Permissions added from childhood to larger[Function Number: 0x56] Failed...????")
        End If
        'Other instructions  **********************************************************************************
        ' **********************************************************************************
        'End  **********************************************************************************
        pkt.close()
        'Close communications
        Return success
    End Function
    ''' <summary>
    ''' Receiving Server Settings Test
    ''' </summary>
    ''' <param name="ControllerIP">Controls set upIPAddress</param>
    ''' <param name="controllerSN">Setd controller serial number</param>
    ''' <param name="watchServerIP">Server to set upIP</param>
    ''' <param name="watchServerPort">Port to set up</param>
    ''' <returns>0 Failed, 1It means success.</returns>
    Private Function testWatchingServer(ByVal ControllerIP As String, ByVal controllerSN As Long, ByVal watchServerIP As String, ByVal watchServerPort As Integer) As Integer
        'Receiving Server Test -- Settings
        Dim ret As Integer = 0
        Dim success As Integer = 0
        '0 Failed, 1It means success.
        Dim pkt As New WGPacketShort()
        pkt.iDevSn = controllerSN
        pkt.IP = ControllerIP
        '1.18	Set up the receiver serverIPand Port [Function Number: 0x90] **********************************************************************************
        '(If you don't want the controller to send the data,, As long as you're receiving the server.IPSet as0.0.0.0 There you go.)
        'Port of receiving server: 61005
        'Every5Seconds sent once.: 05
        pkt.Reset()
        pkt.functionID = &H90
        Dim strIP As String() = watchServerIP.Split("."c)
        If strIP.Length = 4 Then
            pkt.data(0) = Byte.Parse(strIP(0))
            pkt.data(1) = Byte.Parse(strIP(1))
            pkt.data(2) = Byte.Parse(strIP(2))
            pkt.data(3) = Byte.Parse(strIP(3))
        Else
            Return 0
        End If
        'Port of receiving server: 61005
        pkt.data(4) = (((watchServerPort And 255)))
        pkt.data(5) = (((watchServerPort >> 8) And 255))
        'Every5Seconds sent once.: 05 (Periodically upload information as5sec [Every time running properly5Seconds sent once.  Send it when you have a brush card])
        pkt.data(6) = 5
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                log("1.18 Set up the receiver serverIPand Port  Success...")
                success = 1
            End If
        End If
        '1.19	Read the receiver server.IPand Port [Function Number: 0x92] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H92
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.19 Read the receiver server.IPand Port  Success...")
            success = 1
        End If
        pkt.close()
        Return success
    End Function
    ''' <summary>
    ''' Open receiving server to receive data (Watch the firewall. You have to allow all packages at this port to enter.)
    ''' </summary>
    ''' <param name="watchServerIP">Receiving ServersIP(Usually the current computer.IP)</param>
    ''' <param name="watchServerPort">Receive Server Port</param>
    ''' <returns>1 It means success.,Otherwise, failure.</returns>
    Private Function WatchingServerRuning(ByVal watchServerIP As String, ByVal watchServerPort As Integer) As Integer
        'Watch the firewall. You have to allow all packages at this port to enter.
        Try
            Dim udpserver As New WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort)
            If Not udpserver.IsWatching() Then
                log("Enter receiving server surveillance status....Failed")
                Return -1
            End If
            log("Enter receiving server surveillance status....")
            Dim recordIndex As Long = 0
            Dim recv_cnt As Integer
            While Not bStopWatchServer
                recv_cnt = udpserver.receivedCount()
                If recv_cnt > 0 Then
                    Dim buff As Byte() = udpserver.getRecords()
                    If buff(1) = 32 Then
                        '
                        Dim sn As Long
                        Dim recordIndexGet As Long
                        sn = byteToLong(buff, 4, 4)
                        log(String.Format("Received from controllerSN = {0} Packages.." & Chr(13) & "" & Chr(10) & "", sn))
                        recordIndexGet = byteToLong(buff, 8, 4)
                        If recordIndex < recordIndexGet Then
                            recordIndex = recordIndexGet
                            displayRecordInformation(buff)
                        End If
                    End If
                Else
                    System.Threading.Thread.Sleep(10)
                    ''Delay10ms
                    Application.DoEvents()
                End If
            End While
            udpserver.Close()
            Return 1
        Catch ex As Exception
            Debug.WriteLine(ex.ToString())
            ' throw;
            MessageBox.Show(ex.ToString())
        End Try
        Return 0
    End Function

    
End Class

