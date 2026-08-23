VERSION 5.00
Object = "{248DD890-BB45-11CF-9ABC-0080C7E7B78D}#1.0#0"; "MSWINSCK.OCX"
Begin VB.Form Form1 
   Caption         =   "Form1 v2.6"
   ClientHeight    =   10935
   ClientLeft      =   120
   ClientTop       =   450
   ClientWidth     =   8745
   LinkTopic       =   "Form1"
   ScaleHeight     =   10935
   ScaleWidth      =   8745
   StartUpPosition =   3  'Window Default
   Begin VB.CommandButton Command2 
      Caption         =   "2 1024-Bytes Command (1024Byte Command Achieved Ripping records Upload Permissions Read Permissions)"
      Height          =   375
      Left            =   720
      TabIndex        =   6
      Top             =   1200
      Width           =   6735
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
      TabIndex        =   3
      Top             =   1920
      Width           =   8175
   End
   Begin VB.CommandButton Command1 
      Caption         =   "1. Test Encrypt Function"
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
   Begin VB.Label Label2 
      Caption         =   "IP"
      Height          =   255
      Left            =   5160
      TabIndex        =   5
      Top             =   480
      Width           =   375
   End
   Begin VB.Label Label1 
      Caption         =   "SN"
      Height          =   255
      Left            =   5160
      TabIndex        =   4
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
'*
'* V2.6 Version  2015-11-03 20:25:53 V6.60Driver Version Increase Communications password testing, 1024Bytes for permission upload and log extraction operations
'*                               Retry to modify communication
'*
'*/


Private sendSequenceId As Long       'The current number that sent the command.

Const WGPacketSize = 64              'Length of submission
Const WGPacketType = &H17            'Type
Const ControllerPort = 60000         'controller port
Const SpecialFlag = &H55AAAA55       'Special identification Prevent mishandling

Private buff(63) As Byte             'Data reception buffer(64Bytes)
Private buff1024(1024) As Byte             '2015-11-07 21:31:06 Data reception buffer(1024Bytes)

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

'2015-11-02 14:39:36 Introduction Fax code Operation
Private Function pktrunWithPassword(ByRef ASendBuff() As Byte, ByRef BReceiveBuff() As Byte, ByRef password() As Byte, Optional ByVal timeoutMs As Integer = 400) As Integer
    Dim tries As Integer
    Dim ret As Integer

    ret = arrayReset(BReceiveBuff, WGPacketSize)
    sendSequenceId = sendSequenceId + 1
    ret = IntToByte(sendSequenceId, ASendBuff, 40, 4) 'Serial number
 'Backup
    Dim cmdtype As Byte
    Dim cmdId As Byte
    cmdtype = ASendBuff(0)
    cmdId = ASendBuff(1)
    
    Dim ASendBuff40 As Byte
    Dim ASendBuff41 As Byte
    Dim ASendBuff42 As Byte
    Dim ASendBuff43 As Byte
    ASendBuff40 = ASendBuff(40)
    ASendBuff41 = ASendBuff(41)
    ASendBuff42 = ASendBuff(42)
    ASendBuff43 = ASendBuff(43)
   
    
    ret = ShortEncrypt(VarPtr(ASendBuff(0)), VarPtr(password(0)))
 
    
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
                
                Dim j As Integer
               ' j = ShortDecrypt(VarPtr(ASendBuff(0)), VarPtr(password(0)))
                j = ShortDecrypt(VarPtr(BReceiveBuff(0)), VarPtr(password(0)))

                'Type of inspection, Function Number, The current must be consistent.
              '  If ((ASendBuff(0) = BReceiveBuff(0)) And (ASendBuff(1) = BReceiveBuff(1)) And (ASendBuff(40) = BReceiveBuff(40)) And (ASendBuff(41) = BReceiveBuff(41)) And (ASendBuff(42) = BReceiveBuff(42)) And (ASendBuff(43) = BReceiveBuff(43))) Then
                If ((cmdtype = BReceiveBuff(0)) And (cmdId = BReceiveBuff(1)) And (ASendBuff40 = BReceiveBuff(40)) And (ASendBuff41 = BReceiveBuff(41)) And (ASendBuff42 = BReceiveBuff(42)) And (ASendBuff43 = BReceiveBuff(43))) Then
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

    pktrunWithPassword = ret
End Function


'Sending package/Packets received 1024Bytes
Private Function pktrun1024(ByRef ASendBuff() As Byte, Optional ByVal timeoutMs As Integer = 1000) As Integer
    Dim tries As Integer
    Dim ret As Integer
   ' Dim BReceiveBuff(1024 - 1) As Byte
Dim BReceiveBuff() As Byte

   ' ret = arrayReset(BReceiveBuff, 1024)
    tries = 3
    ret = -1
    Dim doeventCount As Integer
    doeventCount = 1000
    
    'Backup
    Dim cmdtype As Byte
    Dim cmdId As Byte
    cmdtype = ASendBuff(0)
    cmdId = ASendBuff(1)
    
    Dim ASendBuff40 As Byte
    Dim ASendBuff41 As Byte
    Dim ASendBuff42 As Byte
    Dim ASendBuff43 As Byte
    ASendBuff40 = ASendBuff(40)
    ASendBuff41 = ASendBuff(41)
    ASendBuff42 = ASendBuff(42)
    ASendBuff43 = ASendBuff(43)
   Dim waitcnt As Long
    Do While tries > 0
        Dim T As Long
        Me.Winsock1.SendData (ASendBuff)
        T = GetTickCount()
        Do
        Sleep (1)
            If (Me.Winsock1.BytesReceived = 1024) Then
               Me.Winsock1.GetData BReceiveBuff, vbArray + vbByte, 1024
                'Type of inspection, Function Number,
                If ((cmdtype = BReceiveBuff(0)) And (cmdId = BReceiveBuff(1)) And (ASendBuff40 = BReceiveBuff(40)) And (ASendBuff41 = BReceiveBuff(41)) And (ASendBuff42 = BReceiveBuff(42)) And (ASendBuff43 = BReceiveBuff(43))) Then
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
        For i = 0 To 1024 - 1
            buff1024(i) = BReceiveBuff(i)
        Next i
    End If

    pktrun1024 = ret
End Function


'Sending package/Packets received 1024Bytes
Private Function pktrun1024WithPassword(ByRef ASendBuff() As Byte, ByRef password() As Byte, Optional ByVal timeoutMs As Integer = 1000) As Integer
    Dim tries As Integer
    Dim ret As Integer
    Dim BReceiveBuff() As Byte

    'ret = arrayReset(BReceiveBuff, 1024)
    tries = 3
    ret = -1
    Dim doeventCount As Integer
    doeventCount = 1000
    
    'Backup
    Dim cmdtype As Byte
    Dim cmdId As Byte
    cmdtype = ASendBuff(0)
    cmdId = ASendBuff(1)
    
    Dim ASendBuff40 As Byte
    Dim ASendBuff41 As Byte
    Dim ASendBuff42 As Byte
    Dim ASendBuff43 As Byte
    ASendBuff40 = ASendBuff(40)
    ASendBuff41 = ASendBuff(41)
    ASendBuff42 = ASendBuff(42)
    ASendBuff43 = ASendBuff(43)
    Dim i As Integer
    For i = 0 To 16 - 1
       ret = ShortEncrypt(VarPtr(ASendBuff(i * 64)), VarPtr(password(0)))
    Next i

    ret = -1
    Do While tries > 0
        Dim T As Long
        Me.Winsock1.SendData (ASendBuff)
        T = GetTickCount()
        Do
        Sleep (1)
            If (Me.Winsock1.BytesReceived = 1024) Then
                Me.Winsock1.GetData BReceiveBuff, vbArray + vbByte, 1024
                
                For i = 0 To 16 - 1
                j = ShortDecrypt(VarPtr(BReceiveBuff(i * 64)), VarPtr(password(0)))
    
                Next i

                'Type of inspection, Function Number,
                If ((cmdtype = BReceiveBuff(0)) And (cmdId = BReceiveBuff(1)) And (ASendBuff40 = BReceiveBuff(40)) And (ASendBuff41 = BReceiveBuff(41)) And (ASendBuff42 = BReceiveBuff(42)) And (ASendBuff43 = BReceiveBuff(43))) Then
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
        'Dim i As Integer
        For i = 0 To 1024 - 1
            buff1024(i) = BReceiveBuff(i)
        Next i
    End If

    pktrun1024WithPassword = ret
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

Private Sub getReceiveBuffData1024(ByRef BReceiveBuff() As Byte)
    Dim i As Integer
    For i = 0 To 1024 - 1
        BReceiveBuff(i) = buff1024(i)
    Next i
End Sub

'Button Events
Private Sub Command1_Click()
    Dim controllerSN As Long
    Dim ControllerIP As String
    Dim watchServerIP As String
    Dim watchServerPort As Long


    '    'No search controller in this case  and SettingsIPWork  (Directly byIPSet tools to complete)
    '    'Test instructions in this case
    '    'controllerSN  = 229999901
    '    'controllerIP  = 192.168.168.123
    '    'Computer  IP  = 192.168.168.101
    '    'For receiving serverIP (This computer.IP 192.168.168.101), Receive Server Port (61005)

    controllerSN = Me.txtSN.Text ' 229999901
    ControllerIP = Me.txtIP.Text '"192.168.168.123"
   
    
    log ("controllerSN = " & controllerSN)
    log ("controllerIP = " & ControllerIP)
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
    If (Me.Winsock1.Protocol <> sckUDPProtocol) Then
      Me.Winsock1.Protocol = sckUDPProtocol
    End If

    testBasicFunction ControllerIP, controllerSN   'Basic function test
End Sub

  ''' Show Record Information
    ''' </summary>
    ''' <param name="pkt"></param>
    Private Sub displayRecordInformation(ByRef recvbuff() As Byte)
        '8-11   Record index number
        '(=0No record.)   4   0x00000000
        Dim recordIndex As Long
         recordIndex = (ByteToLong(recvbuff, 8, 4))
        '12 Record type**********************************************
        '0=No record
        '1=Brush Card Record
        '2=Door Magnetic,button, Device startup, Remote Open Record
        '3=Call the police. 1
        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
        Dim recordType As Integer
        recordType = recvbuff(12)
        '13 Validity(0 Not approved, 1Adopted) 1
        Dim recordValid As Integer
        recordValid = recvbuff(13)
        '14 Door number.(1,2,3,4)   1
        Dim recordDoorNO As Integer
        recordDoorNO = recvbuff(14)
        '15 Come in./Out.(1It means coming in., 2Means out.) 1   0x01
        Dim recordInOrOut As Integer
        recordInOrOut = recvbuff(15)
        '16-19  Card(Type is when swiping a card.)
        'or numbering(Other types of records)   4
        Dim recordCardNO As Double
        recordCardNO = (ByteToDouble(recvbuff, 16, 4))
        '20-26  Brush Time:
        'Days and days of year (AdoptBCDCode)See description of the set-up segment
        Dim recordTime As String
        recordTime = "2000-01-01 00:00:00"
        recordTime = getMsDate(recvbuff(20), recvbuff(21), recvbuff(22), recvbuff(23), recvbuff(24), recvbuff(25), recvbuff(26))

        '2012.12.11 10:49:59    7
        '27 Record cause code(You can check it out. ¡°Checkcard log notes.xls¡±It's a file.ReasonNO)
        'It's only for complex information.   1
        Dim Reason As Integer
        Reason = recvbuff(27)
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
Private Sub testBasicFunction(ByVal ControllerIP As String, ByVal controllerSN As Long)
    Dim sendBuff(63) As Byte    'Data sent buffer(64Bytes)
    Dim recvbuff(63) As Byte     'Data reception buffer(64Bytes)

    Me.Winsock1.RemoteHost = ControllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

    Dim ret As Integer
    Dim success As Integer

    'Control-related variables
    Dim controllerTime As Date
        Dim command1024(1024 - 1)  As Byte

                                        
     Dim commPassword(16 - 1) As Byte
     Dim arrcom
     arrcom = Array(&H11, &H22, &H33, &H44, &H55, &H66, &H77, &H88, &H99, &HAA, &HBB, &HCC, &HDD, &HEE, &HFF, &H0)  '16Byte Password
        Dim i  As Long
        For i = 0 To 16 - 1
        commPassword(i) = arrcom(i)
        Next i
                                   
    'Set Communications Password[Function Number: 0xF0] **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HF0
    'Ideas against error
    ret = IntToByte(SpecialFlag, sendBuff, 8, 4)
    For i = 0 To 16 - 1 '2015-11-02 10:21:00Set New Password
        sendBuff(12 + i) = commPassword(i)
        sendBuff(44 + i) = commPassword(i)
    Next i
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    
       'In two cases.: Password is empty  Or... Password set
      ret = pktrun(sendBuff(), recvbuff())
            success = 0
       If (ret = 1) Then
        getReceiveBuffData recvbuff

                If (recvbuff(8) = 1) Then
                
                    log ("Communication password set successfully...")
                     success = 1
                End If
       Else
                ret = pktrunWithPassword(sendBuff(), recvbuff(), commPassword()) '2015-11-02 10:21:22 Try the password operation for the controller.
                If (ret = 1) Then
                        getReceiveBuffData recvbuff
                End If
                If ((ret > 0) And (recvbuff(8) = 1)) Then
                
                    log ("Communication password set successfully...[Can not open message]")
                    success = 1
                
                Else
                
                    log ("Communication password setup failed...[Can not open message]")
                End If
       End If
       
   
  

    '1.10  Open remote(Function Number: &H40) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H40
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    doorNO = 1
    sendBuff(8) = doorNO
    ret = pktrunWithPassword(sendBuff, recvbuff, commPassword)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvbuff
        If (recvbuff(8) = 1) Then
            success = 1
            'Open the door effectively......
             log ("1.10 Open remote  Success...[Can not open message]")
             Else
                
                    log ("1.10 Open remote   Failed...[Can not open message]")
        End If
     Else
                
                    log ("1.10 Open remote   Failed...[Can not open message]")
    End If



    'Clear the code.[Function Number: 0xF0] **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HF0
    'Ideas against error
    ret = IntToByte(SpecialFlag, sendBuff, 8, 4)
    For i = 0 To 16 - 1 '2015-11-02 10:21:00  Empty Password
        sendBuff(12 + i) = 0
        sendBuff(44 + i) = 0
    Next i
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    
    ret = pktrunWithPassword(sendBuff(), recvbuff(), commPassword()) '2015-11-02 10:21:22 Try the password operation for the controller.
    If (ret = 1) Then
            getReceiveBuffData recvbuff
    End If
    If ((ret > 0) And (recvbuff(8) = 1)) Then
    
        log ("Communication code emptied....[Can not open message]")
        success = 1
    
    Else
    
        log ("Communication password emptied...[Can not open message]")
    End If

       
   

    ' **********************************************************************************

    'End  **********************************************************************************

End Sub


'ControllerIP Controls set upIPAddress
'controllerSN Setd controller serial number
'watchServerIP   Server to set upIP
'watchServerPort Port to set up
Private Sub testWatchingServer(ByVal ControllerIP As String, ByVal controllerSN As Long, ByVal watchServerIP As String, ByVal watchServerPort As Long)
    Dim sendBuff(63) As Byte    'Data sent buffer(64Bytes)
    Dim recvbuff(63) As Byte     'Data reception buffer(64Bytes)

    Me.Winsock1.RemoteHost = ControllerIP
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

    ret = pktrun(sendBuff, recvbuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvbuff
        If (recvbuff(8) = 1) Then

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
    ret = pktrun(sendBuff, recvbuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvbuff
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



Private Sub Command2_Click()

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

'2 1024-Bytes Command (1024Byte Command Achieved Ripping records Upload Permissions Read Permissions)
     Dim sendBuff(63) As Byte    'Data sent buffer(64Bytes)
    Dim recvbuff(63) As Byte     'Data reception buffer(64Bytes)
    Dim recvbuff1024(1024) As Byte     'Data reception buffer(1024Bytes)
       Dim ControllerIP As String
        Dim controllerSN As Long
               Me.txtSN.Text = 239999901
          Me.txtIP.Text = "10.0.1.123"

         controllerSN = Me.txtSN.Text ' 229999901
         ControllerIP = Me.txtIP.Text '"192.168.168.123"
    '  Adopt UDP Communications
    If (Me.Winsock1.Protocol <> sckUDPProtocol) Then
      Me.Winsock1.Protocol = sckUDPProtocol
    End If
    Me.Winsock1.RemoteHost = ControllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

        '1024Byte Command Test
        Dim ret As Integer
        ret = 0
        Dim success As Integer
        success = 0
        '0 Failed, 1It means success.
        Dim command1024(1024 - 1)  As Byte

    
        '1.9    Extract Record Operation
        '1. Pass. 0xB0Command Fetch the earliest record index
        '2. Pass. 0xB0Command Get Last Record Index
        '3. Pass. 0xB4Command Get read record index numbers recordIndex
        '4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records£¬ Until the records are empty.
        '5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
        'After three steps,£¬ The entire extraction record is complete.
        Dim firstRecordIndex As Long
        firstRecordIndex = 0
        'First record index number
        Dim lastRecordIndex As Long
        lastRecordIndex = 0
        'Last record index number.
        Dim recordIndexGotToRead As Long
        recordIndexGotToRead = 0
        Dim recordIndexToGet As Long
        recordIndexToGet = 0
        log ("1.9 Extract Record Operation" & Chr(9) & " Start...")

        ret = arrayReset(sendBuff, WGPacketSize)
        sendBuff(0) = WGPacketType
        sendBuff(1) = &HB0
        ret = IntToByte(controllerSN, sendBuff, 4, 4)
        'If=0, Retrieving the earliest recorded information
        recordIndexToGet = 0
        ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
        ret = pktrun(sendBuff, recvbuff)
        success = 0
        If (ret = 1) Then
            getReceiveBuffData recvbuff
            success = 1
            firstRecordIndex = ByteToLong(recvbuff, 8, 4)
             log (" Fetch the earliest record index =" & firstRecordIndex)
        End If

        ret = arrayReset(sendBuff, WGPacketSize)
        sendBuff(0) = WGPacketType
        sendBuff(1) = &HB0
        ret = IntToByte(controllerSN, sendBuff, 4, 4)
        ' Take Last Record Index
        recordIndexToGet = &HFFFFFFFF
        ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
        ret = pktrun(sendBuff, recvbuff)
        success = 0
        If (ret = 1) Then
            getReceiveBuffData recvbuff
            success = 1
            lastRecordIndex = ByteToLong(recvbuff, 8, 4)
             log (" Take Last Record Index =" & lastRecordIndex)
        End If

        'Get read record index numbers
        ret = arrayReset(sendBuff, WGPacketSize)
        sendBuff(0) = WGPacketType
        sendBuff(1) = &HB4
        ret = IntToByte(controllerSN, sendBuff, 4, 4)
        ret = pktrun(sendBuff, recvbuff)

         If ret > 0 Then
            getReceiveBuffData recvbuff
            success = 1
            recordIndexGotToRead = ByteToLong(recvbuff, 8, 4)
            log ("Get read record index numbers  =" & recordIndexGotToRead)
        End If


        Dim validRecordsCount As Long
         Dim cnt As Long
       validRecordsCount = 0
        'recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
        If ret > 0 Then
            Dim recordIndexValidGet As Long
            recordIndexValidGet = 0
            Dim recordIndexToGetStart As Long
            recordIndexToGetStart = recordIndexGotToRead + 1
            'Prepare record index to extract
            If (recordIndexGotToRead > lastRecordIndex) Or (recordIndexGotToRead < firstRecordIndex) Then
                'Beyond range Take index number for the first record
                recordIndexToGetStart = firstRecordIndex
            End If
            Dim recordIndexCurrent As Long
            cnt = 0
             ret = arrayReset(sendBuff, WGPacketSize)
             sendBuff(0) = WGPacketType
             sendBuff(1) = &HB0
            ret = IntToByte(controllerSN, sendBuff, 4, 4)

             Do While cnt < 200000
                Dim j As Long
                j = 0
                For j = 0 To 1024 - 1
                    'Restore
                    command1024(j) = 0
                Next
                recordIndexCurrent = recordIndexToGetStart
                j = 0
                Do While j < 1024
                     ret = IntToByte(recordIndexToGetStart, sendBuff, 8, 4)
                    sendSequenceId = sendSequenceId + 1
                    ret = IntToByte(sendSequenceId, sendBuff, 40, 4) 'Serial number
                    Dim k As Long
                    For k = 0 To 63
                      command1024(j + k) = sendBuff(k)
                    Next

                    recordIndexToGetStart = recordIndexToGetStart + 1
                    cnt = cnt + 1
                    j = j + 64
                Loop

                ret = pktrun1024(command1024)
                success = 0
                If ret > 0 Then

                    getReceiveBuffData1024 recvbuff1024
                    j = 0
                   Do While j < 1024

                        success = 0
                        '12 Record type
                        '0=No record
                        '1=Brush Card Record
                        '2=Door Magnetic,button, Device startup, Remote Open Record
                        '3=Call the police. 1
                        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record

                        For k = 0 To 64 - 1
                        recvbuff(k) = recvbuff1024(j + k)
                        Next
                        Dim recordType As Integer
                       recordType = recvbuff(12)
                        If recordType = 0 Then
                            success = 2
                            'No more records.
                            Exit Do
                        End If
                        If recordType = 255 Then
                            'This index number is invalid
                            success = 0
                            Exit Do
                        End If
                        success = 1
                        recordIndexValidGet = recordIndexCurrent
                        recordIndexCurrent = recordIndexCurrent + 1
                        validRecordsCount = validRecordsCount + 1
                        '
                        If validRecordsCount < 100 Then
                            '2015-11-05 14:59:20Show Before100individual, Too much shows slow processing. No analysis....
                            displayRecordInformation recvbuff
                            '2015-06-09 20:01:21
                            If validRecordsCount = 99 Then
                                log (" To speed up extraction, Over100Shit.  Do not display recording information again...")

                            End If
                            '.......Storage of records received
                            '*****
                            '###############
                        End If
                        j = j + 64
                    Loop
                If success <> 1 Then
                    Exit Do
                End If
            Else
                    'Ripping failed
                    Exit Do
            End If
          Loop

           log ("1.9 Full extraction successful. Success... Number of valid records= " & validRecordsCount)
            If (success > 0) And validRecordsCount > 0 Then
                'Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number

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
                ret = pktrun(sendBuff, recvbuff)
                success = 0
                If (ret = 1) Then
                    getReceiveBuffData recvbuff
                    If (recvbuff(8) = 1) Then
                        'Full extraction successful.....
                        success = 1
                        log ("1.9 Full extraction successful.   Success...")
                    End If
                End If
          End If
       End If
       
       '1.21   Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        'This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        'Suggested number of privileges updated over50individual, Use this command
        'Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
        
        log ("1.21 Permissions added from childhood to larger[Function Number: 0x56]Start...[1024Byte Command]")
        log ("       1Thousand powers...")

        Dim cardCount As Long
        Dim cardNOOfPrivilege As Long
        cardCount = 10000
        '2015-06-09 20:20:20 Total number of cards
        Dim cardArray(200000 - 1) As Long
        For i = 0 To cardCount - 1
            cardArray(i) = 50001 + i
        Next
        cnt = 0
        Do While cnt < cardCount
               '         Dim j As Long
                j = 0
                For j = 0 To 1024 - 1
                    'Restore
                    command1024(j) = 0
                Next
                j = 0
                Do While j < 1024
                     If cnt >= cardCount Then
                        Exit Do
                    End If
                    ret = arrayReset(sendBuff, WGPacketSize)
                     sendBuff(0) = WGPacketType
                     sendBuff(1) = &H56
                     ret = IntToByte(controllerSN, sendBuff, 4, 4)
                    
                     cardNOOfPrivilege = cardArray(cnt)
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
            ret = IntToByte(cnt + 1, sendBuff, 35, 4)      'The index place for the current permission(From1Start)
        
                    sendSequenceId = sendSequenceId + 1
                    ret = IntToByte(sendSequenceId, sendBuff, 40, 4) 'Serial number
                    'Dim k As Long
                    For k = 0 To 63
                      command1024(j + k) = sendBuff(k)
                    Next

                    cnt = cnt + 1
                    j = j + 64
                Loop

                ret = pktrun1024(command1024)
                
        
            success = 0
            If (ret = 1) Then
                getReceiveBuffData1024 recvbuff1024
                 For k = 0 To 64 - 1
                        recvbuff(k) = recvbuff1024(0 + k)
                 Next
                If (recvbuff(8) = 1) Then
                    success = 1
                Else
                     If recvbuff(8) = &HE1 Then
                        log ("1.21Permissions added from childhood to larger[Function Number: 0x56] =0xE1 Which means the card number has not been sorted from a small to a large size....[1024Byte Command]???")
                       success = 0
                       Exit Do
                   
                    End If
                  End If
            Else
               log ("1.21Permissions added from childhood to larger[Function Number: 0x56] No communication....[1024Byte Command]???")
                Exit Do
            End If
        Loop
        If success = 1 Then
            log ("1.21Permissions added from childhood to larger[Function Number: 0x56] Success...[1024Byte Command]")
        Else
            log ("1.21Permissions added from childhood to larger[Function Number: 0x56] Failed...[1024Byte Command]????")
        End If
        
        
   '1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
        Dim maxCount   As Long
        Dim iCount As Long
        Dim cardNOOfPrivilegeGet As Double
        Dim cardNOOfPrivilegeToGetlast As Double
        cardNOOfPrivilegeToGetlast = 0
        iCount = 0
        maxCount = 200000
        '2015-06-09 20:20:20 Total number of cards
        Dim cardArrayGet(200000 - 1) As Long
        For i = 0 To maxCount - 1
            cardArrayGet(i) = 0
        Next
        cnt = 0
        log ("Read All Permissions" & Chr(9) & " Start...[1024Byte Command]")
        Dim QueryIndex As Long
        QueryIndex = 1 'Index number(From1Start);
        Do While cnt < maxCount
               '         Dim j As Long
                j = 0
                For j = 0 To 1024 - 1
                    'Restore
                    command1024(j) = 0
                Next
                recordIndexCurrent = recordIndexToGetStart
                j = 0
                Do While j < 1024
                    ret = arrayReset(sendBuff, WGPacketSize)
                     sendBuff(0) = WGPacketType
                     sendBuff(1) = &H5C
                     ret = IntToByte(controllerSN, sendBuff, 4, 4)
                    
                      ret = IntToByte(QueryIndex, sendBuff, 8, 4) 'Index number(From1Start);
                      QueryIndex = QueryIndex + 1
                      
                    sendSequenceId = sendSequenceId + 1
                    ret = IntToByte(sendSequenceId, sendBuff, 40, 4) 'Serial number
                    'Dim k As Long
                    For k = 0 To 63
                      command1024(j + k) = sendBuff(k)
                    Next

                    cnt = cnt + 1
                    j = j + 64
                Loop

                ret = pktrun1024(command1024)
                
        
            success = 0
            If (ret = 1) Then
                getReceiveBuffData1024 recvbuff1024
                j = 0
                 Do While j < 1024
                  For k = 0 To 64 - 1
                        recvbuff(k) = recvbuff1024(j + k)
                   Next
                 
                    cardNOOfPrivilegeGet = ByteToDouble(recvbuff, 8, 4) ' ByteToLong(recvBuff, 8, 4)
                    If (&HFFFFFFFF = cardNOOfPrivilegeGet) Then
                              'FFFFFFFFResponse4294967295
                        'log ("1.16      Can not open message: (Permissions deleted)")
                        success = 1
                    ElseIf (0 = cardNOOfPrivilegeGet) Then
                       ' log ("1.16       Can not open message: (The card number is0)--This index number is no longer valid.")
                       Exit Do
                    Else
                        'log ("1.16      Can not open message...")
                        cardArrayGet(iCount) = cardNOOfPrivilegeToGet
                         iCount = iCount + 1
                         success = 1
                         cardNOOfPrivilegeToGetlast = cardNOOfPrivilegeGet
                    End If
                    j = j + 64
                 Loop
                 
                 If (success = 0) Then
                 Exit Do
                 End If
                 
            Else
            log ("1.16     Problem...." & ret)
            Exit Do
            End If
                 
         
        Loop

       log ("Last read permission card number = " & cardNOOfPrivilegeToGetlast)
       log ("Permissions extractediCount = " & iCount)
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


