VERSION 5.00
Object = "{248DD890-BB45-11CF-9ABC-0080C7E7B78D}#1.0#0"; "MSWINSCK.OCX"
Begin VB.Form Form1 
   Caption         =   "Form1 v2.5"
   ClientHeight    =   10935
   ClientLeft      =   120
   ClientTop       =   450
   ClientWidth     =   8745
   LinkTopic       =   "Form1"
   ScaleHeight     =   10935
   ScaleWidth      =   8745
   StartUpPosition =   3  'Window Default
   Begin VB.TextBox txtWatchServerIP 
      Height          =   270
      Left            =   5640
      TabIndex        =   3
      Text            =   "192.168.168.101"
      Top             =   960
      Width           =   1695
   End
   Begin VB.TextBox txtWatchServerPort 
      Height          =   270
      Left            =   5640
      TabIndex        =   4
      Text            =   "61005"
      Top             =   1440
      Width           =   855
   End
   Begin VB.TextBox txtIP 
      Height          =   270
      Left            =   5640
      TabIndex        =   2
      Text            =   "192.168.168.123"
      Top             =   480
      Width           =   1695
   End
   Begin VB.TextBox txtSN 
      Height          =   270
      Left            =   5640
      TabIndex        =   1
      Text            =   "229999901"
      Top             =   120
      Width           =   1215
   End
   Begin MSWinsockLib.Winsock WinsockServer 
      Left            =   3960
      Top             =   0
      _ExtentX        =   741
      _ExtentY        =   741
      _Version        =   393216
      Protocol        =   1
   End
   Begin VB.TextBox Text1 
      Height          =   9015
      Left            =   360
      MultiLine       =   -1  'True
      ScrollBars      =   3  'Both
      TabIndex        =   5
      Top             =   1920
      Width           =   8175
   End
   Begin VB.CommandButton Command1 
      Caption         =   "1. Test Basic Function"
      Height          =   495
      Left            =   960
      TabIndex        =   0
      Top             =   240
      Width           =   2895
   End
   Begin MSWinsockLib.Winsock Winsock1 
      Left            =   240
      Top             =   240
      _ExtentX        =   741
      _ExtentY        =   741
      _Version        =   393216
      Protocol        =   1
   End
   Begin VB.Label Label4 
      Caption         =   "watchServer IP"
      Height          =   255
      Left            =   4080
      TabIndex        =   8
      Top             =   960
      Width           =   1575
   End
   Begin VB.Label Label3 
      Caption         =   "watchServerPort"
      Height          =   255
      Left            =   3960
      TabIndex        =   9
      Top             =   1440
      Width           =   1335
   End
   Begin VB.Label Label2 
      Caption         =   "IP"
      Height          =   255
      Left            =   5160
      TabIndex        =   7
      Top             =   480
      Width           =   375
   End
   Begin VB.Label Label1 
      Caption         =   "SN"
      Height          =   255
      Left            =   5160
      TabIndex        =   6
      Top             =   120
      Width           =   375
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
'/**
'* WGBasicTestVB 2015-04-29 20:41:30 karl CSN Chan Shonin $
'*
'* Doorbar controller Shortcast agreement Test cases
'* V1.4 Version  2014-09-20 18:04:38
'*            Main use Winsock Control to complete [Mswinsck.ocx   Microsoft Winsock Control 6.0 (SP6)]
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
'* V2.5 Version  2015-04-29 20:41:30 Adopt V6.56Driver Version Model by0x19For0x17
'*/


Private sendSequenceId As Long       'The current number that sent the command.

Const WGPacketSize = 64              'Length of submission
Const WGPacketType = &H17            'Type
Const ControllerPort = 60000         'controller port
Const SpecialFlag = &H55AAAA55       'Special identification Prevent mishandling

Private buff(63) As Byte             'Data reception buffer(64Bytes)

Private watchingrecordIndex As Long  'Index numbers of records processed during server surveillance
 
 'Record cause (Type SwipePass Adopted; SwipeNOPassMeans no pass.; ValidEvent Effective Event(Like buttons Door Magnetic Supercode open.); Warn Call the police.)
Private RecordDetails()

'Sending package/Packets received
Private Function pktrun(ByRef ASendBuff() As Byte, ByRef BReceiveBuff() As Byte, Optional ByVal timeoutMs As Integer = 400) As Integer
    Dim tries As Integer
    Dim ret As Integer

    ret = arrayReset(BReceiveBuff, WGPacketSize)
    sendSequenceId = sendSequenceId + 1
    ret = IntToByte(sendSequenceId, ASendBuff, 40, 4) 'Serial number
    tries = 3
    ret = -1
    Dim doeventCount As Integer
    doeventCount = 1000
    Do While tries > 0
        Dim T As Long
        Me.Winsock1.SendData (ASendBuff)
        T = GetTickCount()
        Do
        Sleep (1)
            If (Me.Winsock1.BytesReceived = WGPacketSize) Then
                Me.Winsock1.GetData BReceiveBuff, vbArray + vbByte, WGPacketSize
                'Type of inspection, Function Number, The current must be consistent.
                If ((ASendBuff(0) = BReceiveBuff(0)) And (ASendBuff(1) = BReceiveBuff(1)) And (ASendBuff(40) = BReceiveBuff(40)) And (ASendBuff(41) = BReceiveBuff(41)) And (ASendBuff(42) = BReceiveBuff(42)) And (ASendBuff(43) = BReceiveBuff(43))) Then
                    ret = 1
                    Exit Do
                End If
            End If
            If (GetTickCount - T >= 5) Then
               doeventCount = doeventCount + 1
               If (doeventCount >= 1000) Then
               doeventCount = 0
                 DoEvents
               End If
            End If
        Loop Until GetTickCount - T >= timeoutMs  'Defaults400msTimeout

        If (ret > 0) Then
            Exit Do
        End If
        tries = tries - 1
    Loop

    If (ret > 0) Then
        Dim i As Integer
        For i = 0 To WGPacketSize - 1
            buff(i) = BReceiveBuff(i)
        Next i
    End If

    pktrun = ret
End Function


'Record information
Private Function log(ByVal info As String)
    'Me.Text1.Text = Me.Text1.Text & info & vbCrLf
    Me.Text1.Text = Me.Text1.Text & Time() & " " & info & vbCrLf
    Text1.SelLength = 1
    Text1.SelStart = Len(Text1.Text) 'Keep in the last line
    log = Me.Text1.Text
End Function


'Data actually obtained
Private Sub getReceiveBuffData(ByRef BReceiveBuff() As Byte)
    Dim i As Integer
    For i = 0 To WGPacketSize - 1
        BReceiveBuff(i) = buff(i)
    Next i
End Sub


'Button Events
Private Sub Command1_Click()
    Dim controllerSN As Long
    Dim controllerIP As String
    Dim watchServerIP As String
    Dim watchServerPort As Long


    '    'No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
    '    'Test instructions in this case
    '    'controllerSN  = 229999901
    '    'controllerIP  = 192.168.168.123
    '    'Computer  IP  = 192.168.168.101
    '    'For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)

    controllerSN = Me.txtSN.Text ' 229999901
    controllerIP = Me.txtIP.Text '"192.168.168.123"
    watchServerIP = txtWatchServerIP.Text '"192.168.168.101"
    watchServerPort = Me.txtWatchServerPort.Text ' 61005
    
    log ("controllerSN = " & controllerSN)
    log ("controllerIP = " & controllerIP)
    log ("watchServerIP = " & watchServerIP)
    log ("watchServerPort = " & watchServerPort)
    log (vbCrLf)

 'Record cause (Type SwipePass Adopted; SwipeNOPassMeans no pass.; ValidEvent Effective Event(Like buttons Door Magnetic Supercode open.); Warn Call the police.)
    'Code  Type   English Description  Chinese Description
     RecordDetails = Array("1", "SwipePass", "Swipe", "Open the swipe.", "2", "SwipePass", "Swipe Close", "Brush off", "3", "SwipePass", "Swipe Open", "Open it.", "4", "SwipePass", "Swipe Limited Times", "Open the swipe.(Time limit)", _
"5", "SwipeNOPass", "Denied Access: PC Control", "It's forbidden to pass.: Computer control", "6", "SwipeNOPass", "Denied Access: No PRIVILEGE", "It's forbidden to pass.: No Permissions", "7", "SwipeNOPass", "Denied Access: Wrong PASSWORD", "It's forbidden to pass.: Wrong password.", "8", "SwipeNOPass", "Denied Access: AntiBack", "It's forbidden to pass.: Backwards", _
"9", "SwipeNOPass", "Denied Access: More Cards", "It's forbidden to pass.: Doc!", "10", "SwipeNOPass", "Denied Access: First Card Open", "It's forbidden to pass.: First Card", "11", "SwipeNOPass", "Denied Access: Door Set NC", "It's forbidden to pass.: It's always closed.", "12", "SwipeNOPass", "Denied Access: InterLock", "It's forbidden to pass.: Interlock", _
"13", "SwipeNOPass", "Denied Access: Limited Times", "It's forbidden to pass.: Limited number of brush cards", "14", "SwipeNOPass", "Denied Access: Limited Person Indoor", "It's forbidden to pass.: Number of people in the door", "15", "SwipeNOPass", "Denied Access: Invalid Timezone", "It's forbidden to pass.: Card expired or not valid", "16", "SwipeNOPass", "Denied Access: In Order", "It's forbidden to pass.: Ordered access restrictions", _
"17", "SwipeNOPass", "Denied Access: SWIPE GAP LIMIT", "It's forbidden to pass.: Brush Card Interval", "18", "SwipeNOPass", "Denied Access", "It's forbidden to pass.: Reason unknown.", "19", "SwipeNOPass", "Denied Access: Limited Times", "It's forbidden to pass.: Limit number of brushes", "20", "ValidEvent", "Push Button", "Button open.", _
"21", "ValidEvent", "Push Button Open", "Button On", "22", "ValidEvent", "Push Button Close", "Button Off", "23", "ValidEvent", "Door Open", "Open the door.[Door Magnetic Signal]", "24", "ValidEvent", "Door Closed", "Door closed.[Door Magnetic Signal]", _
"25", "ValidEvent", "Super Password Open Door", "Supercode open.", "26", "ValidEvent", "Super Password Open", "Supercode open.", "27", "ValidEvent", "Super Password Close", "Super Password Level", "28", "Warn", "Controller Power On", "Power on the controller.", _
"29", "Warn", "Controller Reset", "Control Reposition", "30", "Warn", "Push Button Invalid: Disable", "Buttons don't open.: button disabled", "31", "Warn", "Push Button Invalid: Forced Lock", "Buttons don't open.: Force the closing.", "32", "Warn", "Push Button Invalid: Not On Line", "Buttons don't open.: The door's offline.", _
"33", "Warn", "Push Button Invalid: InterLock", "Buttons don't open.: Interlock", "34", "Warn", "Threat", "Coercion to the police.", "35", "Warn", "Threat Open", "Coercion to call the police.", "36", "Warn", "Threat Close", "Coercion to alarm.", _
"37", "Warn", "Open too long", "The door was open for a long time.[After legally opening the door,]", "38", "Warn", "Forced Open", "Forced breaking into the police.", "39", "Warn", "Fire", "Fire!", "40", "Warn", "Forced Close", "Force the closing.", _
"41", "Warn", "Guard Against Theft", "It's an alarm.", "42", "Warn", "7*24Hour Zone", "Smoke gas temperature alert.", "43", "Warn", "Emergency Call", "Call 911.", "44", "RemoteOpen", "Remote Open Door", "Operator opens the door remotely.", _
"45", "RemoteOpen", "Remote Open Door By USB Reader", "The transmitter has confirmed the remote opening.")



    '  Adopt UDP Communications
    Me.Winsock1.Protocol = sckUDPProtocol
    Me.WinsockServer.Protocol = sckUDPProtocol

    testBasicFunction controllerIP, controllerSN   'Basic function test
    log (vbCrLf)
    log (vbCrLf)

    '(Yes.61005Port Reception Data) -- This function Be careful with the firewall. It has to be allowed to receive data..
    testWatchingServer controllerIP, controllerSN, watchServerIP, watchServerPort 'Receiving Server Settings
    WatchingServerRuning watchServerIP, watchServerPort  'Start server receiving data
End Sub

  ''' Show Record Information
    ''' </summary>
    ''' <param name="pkt"></param>
    Private Sub displayRecordInformation(ByRef recvBuff() As Byte)
        '8-11   Record index number
        '(=0No record.)   4   0x00000000
        Dim recordIndex As Long
         recordIndex = (ByteToLong(recvBuff, 8, 4))
        '12 Record type**********************************************
        '0=No record
        '1=Brush Card Record
        '2=Door Magnetic,button, Device startup, Remote Open Record
        '3=Call the police. 1
        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        Dim recordType As Integer
        recordType = recvBuff(12)
        '13 Validity(0 Not approved, 1Adopted) 1
        Dim recordValid As Integer
        recordValid = recvBuff(13)
        '14 Door number.(1,2,3,4)   1
        Dim recordDoorNO As Integer
        recordDoorNO = recvBuff(14)
        '15 Come in./Out.(1It means coming in., 2Means out.) 1   0x01
        Dim recordInOrOut As Integer
        recordInOrOut = recvBuff(15)
        '16-19  Card(Type is when swiping a card.)
        'or numbering(Other types of records)   4
        Dim recordCardNO As Double
        recordCardNO = (ByteToDouble(recvBuff, 16, 4))
        '20-26  Brush Time:
        'Days and days of year (AdoptBCDCode)See description of the set-up segment
        Dim recordTime As String
        recordTime = "2000-01-01 00:00:00"
        recordTime = getMsDate(recvBuff(20), recvBuff(21), recvBuff(22), recvBuff(23), recvBuff(24), recvBuff(25), recvBuff(26))

        '2012.12.11 10:49:59    7
        '27 Record cause code(You can check it out. ¡°Checkcard log notes.xls¡±It's a file.ReasonNO)
        'It's only for complex information.   1
        Dim Reason As Integer
        Reason = recvBuff(27)
        '0=No record
        '1=Brush Card Record
        '2=Door Magnetic,button, Device startup, Remote Open Record
        '3=Call the police. 1
        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        If recordType = 0 Then
            log ("Index post= " & recordIndex & "No record")
        ElseIf recordType = 255 Then
            log (" The records of the specified index have been overwritten,Use the index.0, Retrieving index values from the earliest record")
        ElseIf recordType = 1 Then
            '2015-06-10 08:49:31 Show data with card number type of record
            'Card
            log ("Index post = " & recordIndex)
            log ("  Card = " & recordCardNO)
            log ("  Door number. = " & recordDoorNO)
            log ("  Access = " & IIf(recordInOrOut = 1, "Come in.", "Out."))
            log ("  Valid. = " & IIf(recordValid = 1, "Pass.", "Ban"))
            log ("  Time = " & recordTime)
            log ("  Reason = " & getReasonDetailChinese(Reason))
        ElseIf recordType = 2 Then
            'Other processing
            'Door Magnetic,button, Device startup, Remote Open Record
            log ("Index post = " & recordIndex & " Non-card records")
            log ("  Numbering = " & recordCardNO)
            log ("  Door number. = " & recordDoorNO)
            log ("  Time = " & recordTime)
            log ("  Reason = " & getReasonDetailChinese(Reason))
        ElseIf recordType = 3 Then
            'Other processing
            'Call the police.
            log ("Index post = " & recordIndex & "  Call the police.")
            log ("  Numbering = " & recordCardNO)
            log ("  Door number. = " & recordDoorNO)
            log ("  Time = " & recordTime)
            log ("  Reason = " & getReasonDetailChinese(Reason))
        End If
               
        Text1.SelLength = 1              'Show Last Line
        Text1.SelStart = Len(Text1.Text) 'Show Last Line

    End Sub
    

         'Chinese Information
 Private Function getReasonDetailChinese(ByVal Reason As Integer) As String
        'Chinese
        Dim ret As String
        If Reason > 45 Then
            ret = ""
        ElseIf Reason <= 0 Then
            ret = ""
         Else
           ret = RecordDetails((Reason - 1) * 4 + 3)
          
        End If
          getReasonDetailChinese = ret
    End Function
        'Information in English
    Private Function getReasonDetailEnglish(ByVal Reason As Integer) As String
        'English Description
         If Reason > 45 Then
            ret = ""
         ElseIf Reason <= 0 Then
            ret = ""
         Else
           ret = RecordDetails((Reason - 1) * 4 + 2)
        End If
         getReasonDetailEnglish = ret
    End Function
    
'ControllerIP Controls set upIPAddress
'controllerSN Setd controller serial number
Private Sub testBasicFunction(ByVal controllerIP As String, ByVal controllerSN As Long)
    Dim sendBuff(63) As Byte    'Data sent buffer(64Bytes)
    Dim recvBuff(63) As Byte     'Data reception buffer(64Bytes)

    Me.Winsock1.RemoteHost = controllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

    Dim ret As Integer
    Dim success As Integer

    'Control-related variables
    Dim controllerTime As Date

                                        
    '1.4   Query controller status(Function Number: &H20)(Real time surveillance) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H20
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff(), recvBuff())
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        '        'Read information successfully...
        success = 1
        
        
        log ("1.4 Query controller status Success...")
        
        'Recording information section
        '        '      Last recorded information
        displayRecordInformation recvBuff


        '       '  Other information
        Dim doorStatus(3) As Integer
        '       '28    1Door no.(0Means close, 1Show Open) 1   &H00
        doorStatus(1 - 1) = recvBuff(28)
        '29    2Door no.(0Means close, 1Show Open) 1   &H00
        doorStatus(2 - 1) = recvBuff(29)
        '30    3Door no.(0Means close, 1Show Open) 1   &H00
        doorStatus(3 - 1) = recvBuff(30)
        '31    4Door no.(0Means close, 1Show Open) 1   &H00
        doorStatus(4 - 1) = recvBuff(31)

        Dim pbStatus(3) As Integer

        '32    1Door button.(0It means you let go., 1Means press) 1   &H00
        pbStatus(1 - 1) = recvBuff(32)
        '33    2Door button.(0It means you let go., 1Means press) 1   &H00
        pbStatus(2 - 1) = recvBuff(33)
        '34    3Door button.(0It means you let go., 1Means press) 1   &H00
        pbStatus(3 - 1) = recvBuff(34)
        '35    4Door button.(0It means you let go., 1Means press) 1   &H00
        pbStatus(4 - 1) = recvBuff(35)
        '36    Fault.
        'equals0 No malfunctions.
        'Not equal to0, It's not working.(Reset Time, If there's anything else,, We're going back to the factory.) 1
        Dim errCode As Integer
        errCode = recvBuff(36)

        '37    Control Current Time
        'Time    1   &H21
        '38    min  1   &H30
        '39    sec  1   &H58

        '40-43 Water Stream  4
        Dim sequenceId As Long
        sequenceId = ByteToLong(recvBuff, 40, 4)

        '48
        'Special Information1(Return based on actual use)
        'Keyboard Key Information  1
        '49    Relay status  1
        Dim relayStatus As Integer
        relayStatus = recvBuff(49)

        '50    Door magnetic.8-15bitbit(Fire!/Force locking)
        'Bit0  Force locking
        'Bit1  Fire!
        Dim otherInputStatus As Integer
        otherInputStatus = recvBuff(50)
        If ((otherInputStatus And 1) > 0) Then
            log ("Force locking")
        End If
        If ((otherInputStatus And 2) > 0) Then
            log ("Fire!")
        End If


        '51    V5.46Version Support Control Current Year  1   &H13
        '52    V5.46Version Support Month    1   &H06
        '53    V5.46Version Support Day    1   &H22
        'Control Current Time
        'Dim controllerTime As Date
        controllerTime = getMsDate(&H20, recvBuff(51), recvBuff(52), recvBuff(53), recvBuff(37), recvBuff(38), recvBuff(39))

        log ("controller time:" & controllerTime)
    Else
        log ("1.4 Query controller status Failed?????...")
        Exit Sub
    End If

    '1.5   Read Date Time(Function Number: &H32) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H32
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff

        success = 1
        log ("1.5 Read Date Time Success...")
        'Control Current Time
        controllerTime = getMsDate(recvBuff(8), recvBuff(9), recvBuff(10), recvBuff(11), recvBuff(12), recvBuff(13), recvBuff(14))
        log ("controller time:" & controllerTime)
    End If

    '1.6   Set Date Time(Function Number: &H30) **********************************************************************************
    'calibrate controller at computer time.....
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H30
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    Dim pcTime As Date
    pcTime = Now()
    sendBuff(8 + 0) = GetHex(((Year(pcTime) - (Year(pcTime) Mod 100)) / 100))
    sendBuff(8 + 1) = GetHex(Year(pcTime) Mod 100)
    sendBuff(8 + 2) = GetHex(month(pcTime))
    sendBuff(8 + 3) = GetHex(day(pcTime))
    sendBuff(8 + 4) = GetHex(hour(pcTime))
    sendBuff(8 + 5) = GetHex(minute(pcTime))
    sendBuff(8 + 6) = GetHex(second(pcTime))

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.6  Set Date Time Success...")
    End If

    '1.7   Get a record of the given index number(Function Number: &HB0) **********************************************************************************
    '(Take Index Number &H00000001Records)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '  (Special
    'If=0, Retrieving the earliest recorded information
    'If=&HffffffffRetrieving information from the last record)
    'Records index numbers are normally incremental., Max.&Hffffff = 16,777,215 (Over1Millions.) . Due to limited storage space, Only the closest on the controller.20Thousands of records.. When index numbers exceed20After 10,000., The records of the old index numbers are overwritten., So at this point, check the records of these index numbers., The type of record returned will be&Hff, It means it doesn't exist..
    recordIndexToGet = 1
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 Get Index As1Recorded information Success...")
        '      Index to1Recorded information
                displayRecordInformation recvBuff
    End If


    '. Communication (Take the earliest record By Index Number &H00000000) (This command is appropriate Brushing card records over20Usage in time environment)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    'If=0, Retrieving the earliest recorded information
    recordIndexToGet = 0
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 Fetch information from the earliest record Success...")
        '      First recorded information
        '8-11  Record index number
        '(=0No record.)  4   &H00000000
        recordIndex = ByteToLong(recvBuff, 8, 4)
        If recordIndex = 0 Then
            log ("The index location specified is not recorded ")
        Else
           displayRecordInformation recvBuff
        End If
    End If



    'Communication (Take the latest record. By Index &Hffffffff)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    'If=&Hffffffff, Retrieving the latest recorded information
    recordIndexToGet = &HFFFFFFFF
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 Access to the latest recorded information Success...")
        '      Latest recorded information
        '8-11  Record index number
        '(=0No record.)  4   &H00000000
        recordIndex = ByteToLong(recvBuff, 8, 4)
        If recordIndex = 0 Then
            log ("The index location specified is not recorded ")
        Else
            displayRecordInformation recvBuff
        End If
    End If



'    '1.8   Set a read record index number(Function Number: &HB2) **********************************************************************************
'    ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB2
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ' (Set read record index number as5)
'    Dim recordIndexGot As Long
'    recordIndexGot = 6
'    ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
'    '12    Identification(Prevent Error Settings)    1   &H55 (Fixed)
'    i = SpecialFlag
'    ret = IntToByte(i, sendBuff, 8 + 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        success = 1
'        log ("1.8 Set a read record index number Success...")
'    End If
'
'    '1.9   Get read record index numbers(Function Number: &HB4) **********************************************************************************
'    ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB4
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        log ("1.9 Get read record index numbers Success...")
'        recordIndexGot = ByteToLong(recvBuff, 8, 4)
'        success = 1
'    End If

'        '1.8   Set a read record index number[Function Number: 0xB2] **********************************************************************************
'        'Restore extracted records, Yes1.9Prepare for full extraction-- In use, It's only recovered when problems arise., Normal....
'     ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB2
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ' (Set read record index number as0)
'    Dim recordIndexGot As Long
'    recordIndexGot = 0
'    ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
'    '12    Identification(Prevent Error Settings)    1   &H55 (Fixed)
'    i = SpecialFlag
'    ret = IntToByte(i, sendBuff, 8 + 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        success = 1
'        log ("1.8 Set a read record index number Success...")
'    End If


    '1.9   Extract Record Operation
    '1. Pass. &HB4Command Get read record index numbers recordIndex
    '2. Pass. &HB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records£¬ Until the records are empty.
    '3. Pass. &HB2Command Set a read record index number  Sets the value as the last read brush record index number
    'After three steps,£¬ The entire extraction record is complete.
    
    log ("1.9 Extract Record Operation    Start...")
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB4
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        log ("Start extracting records ...")
        recordIndexGot = ByteToLong(recvBuff, 8, 4)
        recordIndexToGetStart = recordIndexGot + 1
        recordIndexValidGet = 0

        ret = arrayReset(sendBuff, WGPacketSize)
        sendBuff(0) = WGPacketType
        sendBuff(1) = &HB0
        ret = IntToByte(controllerSN, sendBuff, 4, 4)
        i = 0
        Do While i <= 200000
            ret = IntToByte(recordIndexToGetStart, sendBuff, 8, 4)
            ret = pktrun(sendBuff, recvBuff)
            success = 0
            If (ret = 1) Then
                getReceiveBuffData recvBuff
                success = 1
                '12    Record type
                '0=No record
                '1=Brush Card Record
                '2=Door Magnetic,button, Device startup, Remote Open Record
                '3=Call the police.    1
                '&HFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
                recordType = recvBuff(12)
                If (recordType = 0) Then
                    Exit Do
                End If 'No more records.

                If (recordType = &HFF) Then
                    'success = 0  'This index number is invalid  Reset Index Values
                    'Exit Do
                    'Take the earliest record index bit
                     ret = arrayReset(sendBuff, WGPacketSize)
                    sendBuff(0) = WGPacketType
                    sendBuff(1) = &HB0
                    ret = IntToByte(controllerSN, sendBuff, 4, 4)
                    'If=0, Retrieving the earliest recorded information
                    recordIndexToGet = 0
                    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
                    ret = pktrun(sendBuff, recvBuff)
                    success = 0
                    If (ret = 1) Then
                        getReceiveBuffData recvBuff
                        success = 1
                        log ("1.7 Fetch information from the earliest record Success...")
                        '      First recorded information
                        recordIndex = ByteToLong(recvBuff, 8, 4)
                        recordIndexToGetStart = recordIndex
                    End If
                End If
                If (success > 0) Then
                    recordIndexValidGet = recordIndexToGetStart
                    '.......Storage of records received
                    displayRecordInformation recvBuff
                    '*****
                    '###############
                    recordIndexToGetStart = recordIndexToGetStart + 1
                    i = i + 1
                End If
            Else
                Exit Do
            End If
        Loop
        If (success > 0) Then
            ret = arrayReset(sendBuff, WGPacketSize)
            sendBuff(0) = WGPacketType
            sendBuff(1) = &HB2
            ret = IntToByte(controllerSN, sendBuff, 4, 4)
            'Pass. &HB2Command Set a read record index number  Sets the value as the last read brush record index number
            recordIndexGot = recordIndexValidGet
            ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
            '12    Identification(Prevent Error Settings)    1   &H55 (Fixed)
            i = SpecialFlag
            ret = IntToByte(i, sendBuff, 8 + 4, 4)
            ret = pktrun(sendBuff, recvBuff)
            success = 0
            If (ret = 1) Then
                getReceiveBuffData recvBuff
                If (recvBuff(8) = 1) Then
                    'Full extraction successful.....
                    success = 1
                    log ("1.9 Full extraction successful.   Success...")
                End If

            End If
        End If
    End If

    '1.10  Open remote(Function Number: &H40) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H40
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    doorNO = 1
    sendBuff(8) = doorNO
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then
            success = 1
            'Open the door effectively......
            log ("1.10 Open remote   Success...")
        End If
    End If


    '1.11  Permissions to add or modify(Function Number: &H50) **********************************************************************************
    'Add card number0D D7 37 00, Through all doors of the current controller
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H50
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '0D D7 37 00 Card number in permission to add or modify = &H0037D70D = 3659533 (Decimal)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
    sendBuff(8 + 4) = &H20
    sendBuff(8 + 5) = &H10
    sendBuff(8 + 6) = &H1
    sendBuff(8 + 7) = &H1
    '20 29 12 31 Deadline:  2029Year12Month31Day
    sendBuff(8 + 8) = &H20
    sendBuff(8 + 9) = &H29
    sendBuff(8 + 10) = &H12
    sendBuff(8 + 11) = &H31
    '01 Allow Pass Door one. (Single door., Double door., Four controllers working.)
    sendBuff(8 + 12) = &H1
    '01 Allow Pass Door two. (Two doors., Four controllers working.)
    sendBuff(8 + 13) = &H1  'If it's forbidden,2Door., As &H00
    '01 Allow Pass Gate three. (It works on four controllers.)
    sendBuff(8 + 14) = &H1
    '01 Allow Pass Gate four. (It works on four controllers.)
    sendBuff(8 + 15) = &H1

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            'And then... The card number is= &H0037D70D = 3659533 (Decimal)Card, 1Door relay action..
            log ("1.11 Permissions to add or modify     Success...")
        End If
    End If


    '1.12  Permission to delete(Individual Delete)(Function Number: &H52) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H52
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    'Permission card number to delete0D D7 37 00  = &H0037D70D = 3659533 (Decimal)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then
            success = 1
            'And then... The card number is= &H0037D70D = 3659533 (Decimal)Card, 1Door relay doesn't move..
            log ("1.12 Permission to delete(Individual Delete)     Success...")
        End If
    End If


    '1.13  Clear Permissions(Clear it all.)(Function Number: &H54) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H54
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '  Identification(Prevent Error Settings)    1   &H55 (Fixed)
    i = SpecialFlag
    ret = IntToByte(i, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff, 2000)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then
            success = 1
            'It's time to clear up.
            log ("1.13 Clear Permissions(Clear it all.)     Success...")
        End If
    End If


    '1.14  Total Permissions Read(Function Number: &H58) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H58
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        privilegeCount = ByteToLong(recvBuff, 8, 4)
        log ("1.14 Total Permissions Read   Success...")
    End If
    
    
     'Add again as a query operation  1.11  Permissions to add or modify(Function Number: &H50) **********************************************************************************
    'Add card number0D D7 37 00, Through all doors of the current controller
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H50
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '0D D7 37 00 Card number in permission to add or modify = &H0037D70D = 3659533 (Decimal)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
    sendBuff(8 + 4) = &H20
    sendBuff(8 + 5) = &H10
    sendBuff(8 + 6) = &H1
    sendBuff(8 + 7) = &H1
    '20 29 12 31 Deadline:  2029Year12Month31Day
    sendBuff(8 + 8) = &H20
    sendBuff(8 + 9) = &H29
    sendBuff(8 + 10) = &H12
    sendBuff(8 + 11) = &H31
    '01 Allow Pass Door one. (Single door., Double door., Four controllers working.)
    sendBuff(8 + 12) = &H1
    '01 Allow Pass Door two. (Two doors., Four controllers working.)
    sendBuff(8 + 13) = &H1  'If it's forbidden,2Door., As &H00
    '01 Allow Pass Gate three. (It works on four controllers.)
    sendBuff(8 + 14) = &H1
    '01 Allow Pass Gate four. (It works on four controllers.)
    sendBuff(8 + 15) = &H1

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            'And then... The card number is= &H0037D70D = 3659533 (Decimal)Card, 1Door relay action..
            log ("1.11 Permissions to add or modify     Success...")
        End If
    End If
    

    '1.15  Permission Query(Function Number: &H5A) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H5A
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ' (The Chaka is 0D D7 37 00Competence)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        cardNOOfPrivilegeGet = ByteToDouble(recvBuff, 8, 4)
        If (cardNOOfPrivilege = cardNOOfPrivilegeGet) Then
            log ("1.15     Can not open message...")
        Else
            log ("1.15      Can not open message: (The card number is0)")
        End If
        log ("1.15 Permission Query   Success...")
    End If
    '
    
     '1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H5C
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    i = 1 'Index number(From1Start)
    ret = IntToByte(i, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        cardNOOfPrivilegeGet = ByteToDouble(recvBuff, 8, 4) ' ByteToLong(recvBuff, 8, 4)
        If (4294967295# = cardNOOfPrivilegeGet) Then 'FFFFFFFFResponse4294967295
            log ("1.16      Can not open message: (Permissions deleted)")
        ElseIf (0 = cardNOOfPrivilegeGet) Then
            log ("1.16       Can not open message: (The card number is0)--This index number is no longer valid.")
        Else
            log ("1.16      Can not open message...")
        End If
        log ("1.16  Access to specified index numbers   Success...")
    End If
    '
    
    '1.17  Set door control parameters(Online/Delay) (Function Number: &H80) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H80
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '(Settings1Door. Online  Open the door late. 3sec)
    sendBuff(8 + 0) = &H1 '1Door.
    sendBuff(8 + 1) = &H3 'Online
    sendBuff(8 + 2) = &H3 'Open the door late.
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If ((sendBuff(8) = recvBuff(8)) And (sendBuff(9) = recvBuff(9)) And (sendBuff(10) = recvBuff(10))) Then
            'When successful, Return values to match settings
            success = 1
            log ("1.17 Set door control parameters         Success...")
        End If
    End If



'1.21   Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        'This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        'Suggested number of privileges updated over50individual, Use this command
        'Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
        
        log ("1.21 Permissions added from childhood to larger[Function Number: 0x56]Start...")
        log ("       1Thousand powers...")

        Dim cardCount As Integer
        cardCount = 10000
        '2015-06-09 20:20:20 Total number of cards
        Dim cardArray(10000 - 1) As Long
        For i = 0 To cardCount - 1
            cardArray(i) = 50001 + i
        Next
        For i = 0 To cardCount - 1
            ret = arrayReset(sendBuff, WGPacketSize)
            sendBuff(0) = WGPacketType
            sendBuff(1) = &H56
            ret = IntToByte(controllerSN, sendBuff, 4, 4)
           
            cardNOOfPrivilege = cardArray(i)
            ret = DoubleToByte(cardNOOfPrivilege, sendBuff, 8, 4)
            '20 10 01 01 Start date:  2010Year01Month01Day   (Must be greater than2001Year)
            sendBuff(8 + 4) = &H20
            sendBuff(8 + 5) = &H10
            sendBuff(8 + 6) = &H1
            sendBuff(8 + 7) = &H1
            '20 29 12 31 Deadline:  2029Year12Month31Day
            sendBuff(8 + 8) = &H20
            sendBuff(8 + 9) = &H29
            sendBuff(8 + 10) = &H12
            sendBuff(8 + 11) = &H31
            '01 Allow Pass Door one. (Single door., Double door., Four controllers working.)
            sendBuff(8 + 12) = &H1
            '01 Allow Pass Door two. (Two doors., Four controllers working.)
            sendBuff(8 + 13) = &H1  'If it's forbidden,2Door., As &H00
            '01 Allow Pass Gate three. (It works on four controllers.)
            sendBuff(8 + 14) = &H1
            '01 Allow Pass Gate four. (It works on four controllers.)
            sendBuff(8 + 15) = &H1
        
            ret = IntToByte(cardCount, sendBuff, 32, 4)            'Total permissions
            ret = IntToByte(i + 1, sendBuff, 35, 4)      'The index place for the current permission(From1Start)
        
            ret = pktrun(sendBuff, recvBuff)
            success = 0
            If (ret = 1) Then
                getReceiveBuffData recvBuff
                If (recvBuff(8) = 1) Then
                    success = 1
                Else
                     If recvBuff(8) = &HE1 Then
                        log ("1.21Permissions added from childhood to larger[Function Number: 0x56] =0xE1 Which means the card number has not been sorted from a small to a large size....???")
                       
                    End If
                    success = 0
                    Exit For
                 End If
            Else
               log ("1.21Permissions added from childhood to larger[Function Number: 0x56] No communication....???")
                Exit For
            End If
        Next
        If success = 1 Then
            log ("1.21Permissions added from childhood to larger[Function Number: 0x56] Success...")
        Else
            log ("1.21Permissions added from childhood to larger[Function Number: 0x56] Failed...????")
        End If
    'Other instructions  **********************************************************************************


    ' **********************************************************************************

    'End  **********************************************************************************

    If (ret = 1) Then
        log ("Basic function test Success...")
    Else
        log ("Basic function test Failed????...")
    End If
End Sub


'ControllerIP Controls set upIPAddress
'controllerSN Setd controller serial number
'watchServerIP   Server to set upIP
'watchServerPort Port to set up
Private Sub testWatchingServer(ByVal controllerIP As String, ByVal controllerSN As Long, ByVal watchServerIP As String, ByVal watchServerPort As Long)
    Dim sendBuff(63) As Byte    'Data sent buffer(64Bytes)
    Dim recvBuff(63) As Byte     'Data reception buffer(64Bytes)

    Me.Winsock1.RemoteHost = controllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

    Dim ret As Integer
    Dim success As Integer

    '1.18  Set up the receiver serverIPand Port (Function Number: 0x90) **********************************************************************************
    '  From the receiver.IP: 192.168.168.101  (Current computerIP)
    '(If you don't want the controller to send the data,, As long as you're receiving the server.IPSet as0.0.0.0 There you go.)
    'Port of receiving server: 61005
    'Every5Seconds sent once.: 05
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H90
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    'ServersIP: 192.168.168.101
    'sendBuff(8 + 0) = 192
    'sendBuff(8 + 1) = 168
    'sendBuff(8 + 2) = 168
    'sendBuff(8 + 3) = 101
    Dim Ar() As String
    Ar = Split(watchServerIP, ".", , vbTextCompare)
    If UBound(Ar) <> 4 - 1 Then

        log ("watchServerIP The address doesn't make sense.")
        Exit Sub
    End If
    sendBuff(8 + 0) = CInt(Ar(0))
    sendBuff(8 + 1) = CInt(Ar(1))
    sendBuff(8 + 2) = CInt(Ar(2))
    sendBuff(8 + 3) = CInt(Ar(3))
    'Port of receiving server: 61005
    sendBuff(8 + 4) = (watchServerPort And &HFF)
    sendBuff(8 + 5) = ((watchServerPort - (watchServerPort And &HFF)) / 256) And &HFF

    'Every5Seconds sent once.: 05 (Periodically upload information as5sec (Every time running properly5Seconds sent once.  Send it when you have a brush card))
    sendBuff(8 + 6) = 5

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            log ("1.18 Set up the receiver serverIPand Port   Success...")
        Else
            log ("1.18 Set up the receiver serverIPand Port   Failed????...")
            
        End If
    Else
        log ("1.18 Set up the receiver serverIPand Port   Failed????...")
    End If
    Sleep (1000) 'One second delay. Read again
    '1.19  Read the receiver server.IPand Port (Function Number: 0x92) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H92
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.19 Read the receiver server.IPand Port   Success...")
    Else
        log ("1.19 Read the receiver server.IPand Port   Failed????...")
    End If

End Sub


'Enter receiving server surveillance status
Private Sub WatchingServerRuning(ByVal watchServerIP As String, ByVal watchServerPort As Long)

    watchingrecordIndex = -1
     
    Me.WinsockServer.Bind watchServerPort  'Use current computerwatchServerPort
        log ("Enter receiving server surveillance status....")
End Sub



Private Sub Form_Unload(Cancel As Integer)
    Me.Winsock1.Close
    Me.WinsockServer.Close
End Sub

'Servers receiving data processing
Private Sub WinsockServer_DataArrival(ByVal bytesTotal As Long)
    Dim sn As Long
    If (bytesTotal > 0 And ((bytesTota Mod WGPacketSize) = 0)) Then
        'It's a valid data.
    Else
        Dim varlose As Object
        Me.WinsockServer.GetData (varlose) 'Empty it.
        Exit Sub
    End If

    Dim receivedByteCnt As Integer
    Dim watchingRecvBuffVar As Variant
    Dim watchingRecvBuff(63) As Byte    'Server Monitor Receiving data

    receivedByteCnt = 0
    Do While (receivedByteCnt < bytesTotal)
        Me.WinsockServer.GetData watchingRecvBuffVar, vbArray + vbByte, WGPacketSize

        'Type of inspection, Function Number, It's consistent.
        Dim i As Integer
        For i = 0 To WGPacketSize - 1
            watchingRecvBuff(i) = watchingRecvBuffVar(i)
        Next i
        If (watchingRecvBuff(1) = &H20) Then
            sn = ByteToLong(watchingRecvBuff, 4, 4)

            log ("Received from controllerSN = " & sn & " Packages..")

            Dim recordIndex As Long
            recordIndex = ByteToLong(watchingRecvBuff, 8, 4)
            If (recordIndex > watchingrecordIndex) Then
                watchingrecordIndex = recordIndex
               displayRecordInformation watchingRecvBuff
            End If
            
            Text1.SelLength = 1              'Show Last Line
            Text1.SelStart = Len(Text1.Text) 'Show Last Line
        End If

        receivedByteCnt = receivedByteCnt + WGPacketSize
    Loop
End Sub


