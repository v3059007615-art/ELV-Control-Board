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
   StartUpPosition =   3  '窗口缺省
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
'* WGBasicTestVB 2015-04-29 20:41:30 karl CSN 陈绍宁 $
'*
'* 门禁控制器 短报文协议 测试案例
'* V1.4 版本  2014-09-20 18:04:38
'*            主要使用 Winsock 控件来完成 [Mswinsck.ocx   Microsoft Winsock Control 6.0 (SP6)]
'*            基本功能:  查询控制器状态
'*                       读取日期时间
'*                       设置日期时间
'*                       获取指定索引号的记录
'*                       设置已读取过的记录索引号
'*                       获取已读取过的记录索引号
'*                       远程开门
'*                       权限添加或修改
'*                       权限删除(单个删除)
'*                       权限清空(全部清掉)
'*                       权限总数读取
'*                       权限查询
'*                       设置门控制参数(在线/延时)
'*                       读取门控制参数(在线/延时)
'
'*                       设置接收服务器的IP和端口
'*                       读取接收服务器的IP和端口
'*
'*
'*                       接收服务器的实现 (在61005端口接收数据) -- 此项功能 一定要注意防火墙设置 必须是允许接收数据的.
'* V2.5 版本  2015-04-29 20:41:30 采用 V6.56驱动版本 型号由0x19改为0x17
'*/


Private sendSequenceId As Long       '发送指令的流水号

Const WGPacketSize = 64              '报文长度
Const WGPacketType = &H17            '类型
Const ControllerPort = 60000         '控制器端口
Const SpecialFlag = &H55AAAA55       '特殊标识 防止误操作

Private buff(63) As Byte             '数据接收缓冲区(64字节)

Private watchingrecordIndex As Long  '服务器监控时处理的记录索引号
 
 '记录原因 (类型中 SwipePass 表示通过; SwipeNOPass表示禁止通过; ValidEvent 有效事件(如按钮 门磁 超级密码开门); Warn 报警事件)
Private RecordDetails()

'发送数据包/接收数据包
Private Function pktrun(ByRef ASendBuff() As Byte, ByRef BReceiveBuff() As Byte, Optional ByVal timeoutMs As Integer = 400) As Integer
    Dim tries As Integer
    Dim ret As Integer

    ret = arrayReset(BReceiveBuff, WGPacketSize)
    sendSequenceId = sendSequenceId + 1
    ret = IntToByte(sendSequenceId, ASendBuff, 40, 4) '序号
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
                '检查类型, 功能号, 流水号要一致
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
        Loop Until GetTickCount - T >= timeoutMs  '缺省400ms超时

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


'记录信息
Private Function log(ByVal info As String)
    'Me.Text1.Text = Me.Text1.Text & info & vbCrLf
    Me.Text1.Text = Me.Text1.Text & Time() & " " & info & vbCrLf
    Text1.SelLength = 1
    Text1.SelStart = Len(Text1.Text) '保持在最后一行
    log = Me.Text1.Text
End Function


'实际获取到的数据
Private Sub getReceiveBuffData(ByRef BReceiveBuff() As Byte)
    Dim i As Integer
    For i = 0 To WGPacketSize - 1
        BReceiveBuff(i) = buff(i)
    Next i
End Sub


'按钮事件
Private Sub Command1_Click()
    Dim controllerSN As Long
    Dim controllerIP As String
    Dim watchServerIP As String
    Dim watchServerPort As Long


    '    '本案例未作搜索控制器  及 设置IP的工作  (直接由IP设置工具来完成)
    '    '本案例中测试说明
    '    '控制器SN  = 229999901
    '    '控制器IP  = 192.168.168.123
    '    '电脑  IP  = 192.168.168.101
    '    '用于作为接收服务器的IP (本电脑IP 192.168.168.101), 接收服务器端口 (61005)

    controllerSN = Me.txtSN.Text ' 229999901
    controllerIP = Me.txtIP.Text '"192.168.168.123"
    watchServerIP = txtWatchServerIP.Text '"192.168.168.101"
    watchServerPort = Me.txtWatchServerPort.Text ' 61005
    
    log ("controllerSN = " & controllerSN)
    log ("controllerIP = " & controllerIP)
    log ("watchServerIP = " & watchServerIP)
    log ("watchServerPort = " & watchServerPort)
    log (vbCrLf)

 '记录原因 (类型中 SwipePass 表示通过; SwipeNOPass表示禁止通过; ValidEvent 有效事件(如按钮 门磁 超级密码开门); Warn 报警事件)
    '代码  类型   英文描述  中文描述
     RecordDetails = Array("1", "SwipePass", "Swipe", "刷卡开门", "2", "SwipePass", "Swipe Close", "刷卡关", "3", "SwipePass", "Swipe Open", "刷卡开", "4", "SwipePass", "Swipe Limited Times", "刷卡开门(带限次)", _
"5", "SwipeNOPass", "Denied Access: PC Control", "刷卡禁止通过: 电脑控制", "6", "SwipeNOPass", "Denied Access: No PRIVILEGE", "刷卡禁止通过: 没有权限", "7", "SwipeNOPass", "Denied Access: Wrong PASSWORD", "刷卡禁止通过: 密码不对", "8", "SwipeNOPass", "Denied Access: AntiBack", "刷卡禁止通过: 反潜回", _
"9", "SwipeNOPass", "Denied Access: More Cards", "刷卡禁止通过: 多卡", "10", "SwipeNOPass", "Denied Access: First Card Open", "刷卡禁止通过: 首卡", "11", "SwipeNOPass", "Denied Access: Door Set NC", "刷卡禁止通过: 门为常闭", "12", "SwipeNOPass", "Denied Access: InterLock", "刷卡禁止通过: 互锁", _
"13", "SwipeNOPass", "Denied Access: Limited Times", "刷卡禁止通过: 受刷卡次数限制", "14", "SwipeNOPass", "Denied Access: Limited Person Indoor", "刷卡禁止通过: 门内人数限制", "15", "SwipeNOPass", "Denied Access: Invalid Timezone", "刷卡禁止通过: 卡过期或不在有效时段", "16", "SwipeNOPass", "Denied Access: In Order", "刷卡禁止通过: 按顺序进出限制", _
"17", "SwipeNOPass", "Denied Access: SWIPE GAP LIMIT", "刷卡禁止通过: 刷卡间隔约束", "18", "SwipeNOPass", "Denied Access", "刷卡禁止通过: 原因不明", "19", "SwipeNOPass", "Denied Access: Limited Times", "刷卡禁止通过: 刷卡次数限制", "20", "ValidEvent", "Push Button", "按钮开门", _
"21", "ValidEvent", "Push Button Open", "按钮开", "22", "ValidEvent", "Push Button Close", "按钮关", "23", "ValidEvent", "Door Open", "门打开[门磁信号]", "24", "ValidEvent", "Door Closed", "门关闭[门磁信号]", _
"25", "ValidEvent", "Super Password Open Door", "超级密码开门", "26", "ValidEvent", "Super Password Open", "超级密码开", "27", "ValidEvent", "Super Password Close", "超级密码关", "28", "Warn", "Controller Power On", "控制器上电", _
"29", "Warn", "Controller Reset", "控制器复位", "30", "Warn", "Push Button Invalid: Disable", "按钮不开门: 按钮禁用", "31", "Warn", "Push Button Invalid: Forced Lock", "按钮不开门: 强制关门", "32", "Warn", "Push Button Invalid: Not On Line", "按钮不开门: 门不在线", _
"33", "Warn", "Push Button Invalid: InterLock", "按钮不开门: 互锁", "34", "Warn", "Threat", "胁迫报警", "35", "Warn", "Threat Open", "胁迫报警开", "36", "Warn", "Threat Close", "胁迫报警关", _
"37", "Warn", "Open too long", "门长时间未关报警[合法开门后]", "38", "Warn", "Forced Open", "强行闯入报警", "39", "Warn", "Fire", "火警", "40", "Warn", "Forced Close", "强制关门", _
"41", "Warn", "Guard Against Theft", "防盗报警", "42", "Warn", "7*24Hour Zone", "烟雾煤气温度报警", "43", "Warn", "Emergency Call", "紧急呼救报警", "44", "RemoteOpen", "Remote Open Door", "操作员远程开门", _
"45", "RemoteOpen", "Remote Open Door By USB Reader", "发卡器确定发出的远程开门")



    '  采用 UDP 通信
    Me.Winsock1.Protocol = sckUDPProtocol
    Me.WinsockServer.Protocol = sckUDPProtocol

    testBasicFunction controllerIP, controllerSN   '基本功能测试
    log (vbCrLf)
    log (vbCrLf)

    '(在61005端口接收数据) -- 此项功能 一定要注意防火墙设置 必须是允许接收数据的.
    testWatchingServer controllerIP, controllerSN, watchServerIP, watchServerPort '接收服务器设置
    WatchingServerRuning watchServerIP, watchServerPort  '启动服务器接收数据
End Sub

  ''' 显示记录信息
    ''' </summary>
    ''' <param name="pkt"></param>
    Private Sub displayRecordInformation(ByRef recvBuff() As Byte)
        '8-11   记录的索引号
        '(=0表示没有记录)   4   0x00000000
        Dim recordIndex As Long
         recordIndex = (ByteToLong(recvBuff, 8, 4))
        '12 记录类型**********************************************
        '0=无记录
        '1=刷卡记录
        '2=门磁,按钮, 设备启动, 远程开门记录
        '3=报警记录 1
        '0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
        Dim recordType As Integer
        recordType = recvBuff(12)
        '13 有效性(0 表示不通过, 1表示通过) 1
        Dim recordValid As Integer
        recordValid = recvBuff(13)
        '14 门号(1,2,3,4)   1
        Dim recordDoorNO As Integer
        recordDoorNO = recvBuff(14)
        '15 进门/出门(1表示进门, 2表示出门) 1   0x01
        Dim recordInOrOut As Integer
        recordInOrOut = recvBuff(15)
        '16-19  卡号(类型是刷卡记录时)
        '或编号(其他类型记录)   4
        Dim recordCardNO As Double
        recordCardNO = (ByteToDouble(recvBuff, 16, 4))
        '20-26  刷卡时间:
        '年月日时分秒 (采用BCD码)见设置时间部分的说明
        Dim recordTime As String
        recordTime = "2000-01-01 00:00:00"
        recordTime = getMsDate(recvBuff(20), recvBuff(21), recvBuff(22), recvBuff(23), recvBuff(24), recvBuff(25), recvBuff(26))

        '2012.12.11 10:49:59    7
        '27 记录原因代码(可以查 “刷卡记录说明.xls”文件的ReasonNO)
        '处理复杂信息才用   1
        Dim Reason As Integer
        Reason = recvBuff(27)
        '0=无记录
        '1=刷卡记录
        '2=门磁,按钮, 设备启动, 远程开门记录
        '3=报警记录 1
        '0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
        If recordType = 0 Then
            log ("索引位= " & recordIndex & "无记录")
        ElseIf recordType = 255 Then
            log (" 指定索引位的记录已被覆盖掉了,请使用索引0, 取回最早一条记录的索引值")
        ElseIf recordType = 1 Then
            '2015-06-10 08:49:31 显示记录类型为卡号的数据
            '卡号
            log ("索引位 = " & recordIndex)
            log ("  卡号 = " & recordCardNO)
            log ("  门号 = " & recordDoorNO)
            log ("  进出 = " & IIf(recordInOrOut = 1, "进门", "出门"))
            log ("  有效 = " & IIf(recordValid = 1, "通过", "禁止"))
            log ("  时间 = " & recordTime)
            log ("  原因 = " & getReasonDetailChinese(Reason))
        ElseIf recordType = 2 Then
            '其他处理
            '门磁,按钮, 设备启动, 远程开门记录
            log ("索引位 = " & recordIndex & " 非刷卡记录")
            log ("  编号 = " & recordCardNO)
            log ("  门号 = " & recordDoorNO)
            log ("  时间 = " & recordTime)
            log ("  原因 = " & getReasonDetailChinese(Reason))
        ElseIf recordType = 3 Then
            '其他处理
            '报警记录
            log ("索引位 = " & recordIndex & "  报警记录")
            log ("  编号 = " & recordCardNO)
            log ("  门号 = " & recordDoorNO)
            log ("  时间 = " & recordTime)
            log ("  原因 = " & getReasonDetailChinese(Reason))
        End If
               
        Text1.SelLength = 1              '显示最后一行
        Text1.SelStart = Len(Text1.Text) '显示最后一行

    End Sub
    

         '中文信息
 Private Function getReasonDetailChinese(ByVal Reason As Integer) As String
        '中文
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
        '英文信息
    Private Function getReasonDetailEnglish(ByVal Reason As Integer) As String
        '英文描述
         If Reason > 45 Then
            ret = ""
         ElseIf Reason <= 0 Then
            ret = ""
         Else
           ret = RecordDetails((Reason - 1) * 4 + 2)
        End If
         getReasonDetailEnglish = ret
    End Function
    
'ControllerIP 被设置的控制器IP地址
'controllerSN 被设置的控制器序列号
Private Sub testBasicFunction(ByVal controllerIP As String, ByVal controllerSN As Long)
    Dim sendBuff(63) As Byte    '数据发送缓冲区(64字节)
    Dim recvBuff(63) As Byte     '数据接收缓冲区(64字节)

    Me.Winsock1.RemoteHost = controllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

    Dim ret As Integer
    Dim success As Integer

    '控制器相关变量
    Dim controllerTime As Date

                                        
    '1.4   查询控制器状态(功能号: &H20)(实时监控用) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H20
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff(), recvBuff())
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        '        '读取信息成功...
        success = 1
        
        
        log ("1.4 查询控制器状态 成功...")
        
        '记录信息部分
        '        '      最后一条记录的信息
        displayRecordInformation recvBuff


        '       '  其他信息
        Dim doorStatus(3) As Integer
        '       '28    1号门门磁(0表示关上, 1表示打开) 1   &H00
        doorStatus(1 - 1) = recvBuff(28)
        '29    2号门门磁(0表示关上, 1表示打开) 1   &H00
        doorStatus(2 - 1) = recvBuff(29)
        '30    3号门门磁(0表示关上, 1表示打开) 1   &H00
        doorStatus(3 - 1) = recvBuff(30)
        '31    4号门门磁(0表示关上, 1表示打开) 1   &H00
        doorStatus(4 - 1) = recvBuff(31)

        Dim pbStatus(3) As Integer

        '32    1号门按钮(0表示松开, 1表示按下) 1   &H00
        pbStatus(1 - 1) = recvBuff(32)
        '33    2号门按钮(0表示松开, 1表示按下) 1   &H00
        pbStatus(2 - 1) = recvBuff(33)
        '34    3号门按钮(0表示松开, 1表示按下) 1   &H00
        pbStatus(3 - 1) = recvBuff(34)
        '35    4号门按钮(0表示松开, 1表示按下) 1   &H00
        pbStatus(4 - 1) = recvBuff(35)
        '36    故障号
        '等于0 无故障
        '不等于0, 有故障(先重设时间, 如果还有问题, 则要返厂家维护) 1
        Dim errCode As Integer
        errCode = recvBuff(36)

        '37    控制器当前时间
        '时    1   &H21
        '38    分  1   &H30
        '39    秒  1   &H58

        '40-43 流水号  4
        Dim sequenceId As Long
        sequenceId = ByteToLong(recvBuff, 40, 4)

        '48
        '特殊信息1(依据实际使用中返回)
        '键盘按键信息  1
        '49    继电器状态  1
        Dim relayStatus As Integer
        relayStatus = recvBuff(49)

        '50    门磁状态的8-15bit位(火警/强制锁门)
        'Bit0  强制锁门
        'Bit1  火警
        Dim otherInputStatus As Integer
        otherInputStatus = recvBuff(50)
        If ((otherInputStatus And 1) > 0) Then
            log ("强制锁门")
        End If
        If ((otherInputStatus And 2) > 0) Then
            log ("火警")
        End If


        '51    V5.46版本支持 控制器当前年  1   &H13
        '52    V5.46版本支持 月    1   &H06
        '53    V5.46版本支持 日    1   &H22
        '控制器当前时间
        'Dim controllerTime As Date
        controllerTime = getMsDate(&H20, recvBuff(51), recvBuff(52), recvBuff(53), recvBuff(37), recvBuff(38), recvBuff(39))

        log ("控制器时间:" & controllerTime)
    Else
        log ("1.4 查询控制器状态 失败?????...")
        Exit Sub
    End If

    '1.5   读取日期时间(功能号: &H32) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H32
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff

        success = 1
        log ("1.5 读取日期时间 成功...")
        '控制器当前时间
        controllerTime = getMsDate(recvBuff(8), recvBuff(9), recvBuff(10), recvBuff(11), recvBuff(12), recvBuff(13), recvBuff(14))
        log ("控制器时间:" & controllerTime)
    End If

    '1.6   设置日期时间(功能号: &H30) **********************************************************************************
    '按电脑当前时间校准控制器.....
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
        log ("1.6  设置日期时间 成功...")
    End If

    '1.7   获取指定索引号的记录(功能号: &HB0) **********************************************************************************
    '(取索引号 &H00000001的记录)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '  (特殊
    '如果=0, 则取回最早一条记录信息
    '如果=&Hffffffff则取回最后一条记录的信息)
    '记录索引号正常情况下是顺序递增的, 最大可达&Hffffff = 16,777,215 (超过1千万) . 由于存储空间有限, 控制器上只会保留最近的20万个记录. 当索引号超过20万后, 旧的索引号位的记录就会被覆盖, 所以这时查询这些索引号的记录, 返回的记录类型将是&Hff, 表示不存在了.
    recordIndexToGet = 1
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 获取索引为1号记录的信息 成功...")
        '      索引为1号记录的信息
                displayRecordInformation recvBuff
    End If


    '. 发出报文 (取最早的一条记录 通过索引号 &H00000000) (此指令适合于 刷卡记录超过20万时环境下使用)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '如果=0, 则取回最早一条记录信息
    recordIndexToGet = 0
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 获取最早一条记录的信息 成功...")
        '      最早一条记录的信息
        '8-11  记录的索引号
        '(=0表示没有记录)  4   &H00000000
        recordIndex = ByteToLong(recvBuff, 8, 4)
        If recordIndex = 0 Then
            log ("指定的索引位置没有记录 ")
        Else
           displayRecordInformation recvBuff
        End If
    End If



    '发出报文 (取最新的一条记录 通过索引 &Hffffffff)
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB0
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '如果=&Hffffffff, 则取回最新一条记录信息
    recordIndexToGet = &HFFFFFFFF
    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.7 获取最新记录的信息 成功...")
        '      最新记录的信息
        '8-11  记录的索引号
        '(=0表示没有记录)  4   &H00000000
        recordIndex = ByteToLong(recvBuff, 8, 4)
        If recordIndex = 0 Then
            log ("指定的索引位置没有记录 ")
        Else
            displayRecordInformation recvBuff
        End If
    End If



'    '1.8   设置已读取过的记录索引号(功能号: &HB2) **********************************************************************************
'    ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB2
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ' (设为已读取过的记录索引号为5)
'    Dim recordIndexGot As Long
'    recordIndexGot = 6
'    ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
'    '12    标识(防止误设置)    1   &H55 (固定)
'    i = SpecialFlag
'    ret = IntToByte(i, sendBuff, 8 + 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        success = 1
'        log ("1.8 设置已读取过的记录索引号 成功...")
'    End If
'
'    '1.9   获取已读取过的记录索引号(功能号: &HB4) **********************************************************************************
'    ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB4
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        log ("1.9 获取已读取过的记录索引号 成功...")
'        recordIndexGot = ByteToLong(recvBuff, 8, 4)
'        success = 1
'    End If

'        '1.8   设置已读取过的记录索引号[功能号: 0xB2] **********************************************************************************
'        '恢复已提取过的记录, 为1.9的完整提取操作作准备-- 实际使用中, 在出现问题时才恢复, 正常不用恢复...
'     ret = arrayReset(sendBuff, WGPacketSize)
'    sendBuff(0) = WGPacketType
'    sendBuff(1) = &HB2
'    ret = IntToByte(controllerSN, sendBuff, 4, 4)
'    ' (设为已读取过的记录索引号为0)
'    Dim recordIndexGot As Long
'    recordIndexGot = 0
'    ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
'    '12    标识(防止误设置)    1   &H55 (固定)
'    i = SpecialFlag
'    ret = IntToByte(i, sendBuff, 8 + 4, 4)
'    ret = pktrun(sendBuff, recvBuff)
'    success = 0
'    If (ret = 1) Then
'        getReceiveBuffData recvBuff
'        success = 1
'        log ("1.8 设置已读取过的记录索引号 成功...")
'    End If


    '1.9   提取记录操作
    '1. 通过 &HB4指令 获取已读取过的记录索引号 recordIndex
    '2. 通过 &HB0指令 获取指定索引号的记录  从recordIndex + 1开始提取记录， 直到记录为空为止
    '3. 通过 &HB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
    '经过上面三个步骤， 整个提取记录的操作完成
    
    log ("1.9 提取记录操作    开始...")
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &HB4
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        log ("开始提取记录 ...")
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
                '12    记录类型
                '0=无记录
                '1=刷卡记录
                '2=门磁,按钮, 设备启动, 远程开门记录
                '3=报警记录    1
                '&HFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
                recordType = recvBuff(12)
                If (recordType = 0) Then
                    Exit Do
                End If '没有更多记录

                If (recordType = &HFF) Then
                    'success = 0  '此索引号无效  重新设置索引值
                    'Exit Do
                    '取最早一条记录的索引位
                     ret = arrayReset(sendBuff, WGPacketSize)
                    sendBuff(0) = WGPacketType
                    sendBuff(1) = &HB0
                    ret = IntToByte(controllerSN, sendBuff, 4, 4)
                    '如果=0, 则取回最早一条记录信息
                    recordIndexToGet = 0
                    ret = IntToByte(recordIndexToGet, sendBuff, 8, 4)
                    ret = pktrun(sendBuff, recvBuff)
                    success = 0
                    If (ret = 1) Then
                        getReceiveBuffData recvBuff
                        success = 1
                        log ("1.7 获取最早一条记录的信息 成功...")
                        '      最早一条记录的信息
                        recordIndex = ByteToLong(recvBuff, 8, 4)
                        recordIndexToGetStart = recordIndex
                    End If
                End If
                If (success > 0) Then
                    recordIndexValidGet = recordIndexToGetStart
                    '.......对收到的记录作存储处理
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
            '通过 &HB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
            recordIndexGot = recordIndexValidGet
            ret = IntToByte(recordIndexGot, sendBuff, 8, 4)
            '12    标识(防止误设置)    1   &H55 (固定)
            i = SpecialFlag
            ret = IntToByte(i, sendBuff, 8 + 4, 4)
            ret = pktrun(sendBuff, recvBuff)
            success = 0
            If (ret = 1) Then
                getReceiveBuffData recvBuff
                If (recvBuff(8) = 1) Then
                    '完全提取成功....
                    success = 1
                    log ("1.9 完全提取成功   成功...")
                End If

            End If
        End If
    End If

    '1.10  远程开门(功能号: &H40) **********************************************************************************
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
            '有效开门.....
            log ("1.10 远程开门   成功...")
        End If
    End If


    '1.11  权限添加或修改(功能号: &H50) **********************************************************************************
    '增加卡号0D D7 37 00, 通过当前控制器的所有门
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H50
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '0D D7 37 00 要添加或修改的权限中的卡号 = &H0037D70D = 3659533 (十进制)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
    sendBuff(8 + 4) = &H20
    sendBuff(8 + 5) = &H10
    sendBuff(8 + 6) = &H1
    sendBuff(8 + 7) = &H1
    '20 29 12 31 截止日期:  2029年12月31日
    sendBuff(8 + 8) = &H20
    sendBuff(8 + 9) = &H29
    sendBuff(8 + 10) = &H12
    sendBuff(8 + 11) = &H31
    '01 允许通过 一号门 (对单门, 双门, 四门控制器有效)
    sendBuff(8 + 12) = &H1
    '01 允许通过 二号门 (对双门, 四门控制器有效)
    sendBuff(8 + 13) = &H1  '如果禁止2号门, 则只要设为 &H00
    '01 允许通过 三号门 (对四门控制器有效)
    sendBuff(8 + 14) = &H1
    '01 允许通过 四号门 (对四门控制器有效)
    sendBuff(8 + 15) = &H1

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            '这时 刷卡号为= &H0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
            log ("1.11 权限添加或修改     成功...")
        End If
    End If


    '1.12  权限删除(单个删除)(功能号: &H52) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H52
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '要删除的权限卡号0D D7 37 00  = &H0037D70D = 3659533 (十进制)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then
            success = 1
            '这时 刷卡号为= &H0037D70D = 3659533 (十进制)的卡, 1号门继电器不会动作.
            log ("1.12 权限删除(单个删除)     成功...")
        End If
    End If


    '1.13  权限清空(全部清掉)(功能号: &H54) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H54
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '  标识(防止误设置)    1   &H55 (固定)
    i = SpecialFlag
    ret = IntToByte(i, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff, 2000)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then
            success = 1
            '这时清空成功
            log ("1.13 权限清空(全部清掉)     成功...")
        End If
    End If


    '1.14  权限总数读取(功能号: &H58) **********************************************************************************
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
        log ("1.14 权限总数读取   成功...")
    End If
    
    
     '再次添加为查询操作  1.11  权限添加或修改(功能号: &H50) **********************************************************************************
    '增加卡号0D D7 37 00, 通过当前控制器的所有门
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H50
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '0D D7 37 00 要添加或修改的权限中的卡号 = &H0037D70D = 3659533 (十进制)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
    sendBuff(8 + 4) = &H20
    sendBuff(8 + 5) = &H10
    sendBuff(8 + 6) = &H1
    sendBuff(8 + 7) = &H1
    '20 29 12 31 截止日期:  2029年12月31日
    sendBuff(8 + 8) = &H20
    sendBuff(8 + 9) = &H29
    sendBuff(8 + 10) = &H12
    sendBuff(8 + 11) = &H31
    '01 允许通过 一号门 (对单门, 双门, 四门控制器有效)
    sendBuff(8 + 12) = &H1
    '01 允许通过 二号门 (对双门, 四门控制器有效)
    sendBuff(8 + 13) = &H1  '如果禁止2号门, 则只要设为 &H00
    '01 允许通过 三号门 (对四门控制器有效)
    sendBuff(8 + 14) = &H1
    '01 允许通过 四号门 (对四门控制器有效)
    sendBuff(8 + 15) = &H1

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            '这时 刷卡号为= &H0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
            log ("1.11 权限添加或修改     成功...")
        End If
    End If
    

    '1.15  权限查询(功能号: &H5A) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H5A
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ' (查卡号为 0D D7 37 00的权限)
    cardNOOfPrivilege = &H37D70D
    ret = IntToByte(cardNOOfPrivilege, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        cardNOOfPrivilegeGet = ByteToDouble(recvBuff, 8, 4)
        If (cardNOOfPrivilege = cardNOOfPrivilegeGet) Then
            log ("1.15     有权限信息...")
        Else
            log ("1.15      没有权限信息: (卡号部分为0)")
        End If
        log ("1.15 权限查询   成功...")
    End If
    '
    
     '1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H5C
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    i = 1 '索引号(从1开始)
    ret = IntToByte(i, sendBuff, 8, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        cardNOOfPrivilegeGet = ByteToDouble(recvBuff, 8, 4) ' ByteToLong(recvBuff, 8, 4)
        If (4294967295# = cardNOOfPrivilegeGet) Then 'FFFFFFFF对应于4294967295
            log ("1.16      没有权限信息: (权限已删除)")
        ElseIf (0 = cardNOOfPrivilegeGet) Then
            log ("1.16       没有权限信息: (卡号部分为0)--此索引号之后没有权限了")
        Else
            log ("1.16      有权限信息...")
        End If
        log ("1.16  获取指定索引号的权限   成功...")
    End If
    '
    
    '1.17  设置门控制参数(在线/延时) (功能号: &H80) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H80
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '(设置1号门 在线  开门延时 3秒)
    sendBuff(8 + 0) = &H1 '1号门
    sendBuff(8 + 1) = &H3 '在线
    sendBuff(8 + 2) = &H3 '开门延时
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If ((sendBuff(8) = recvBuff(8)) And (sendBuff(9) = recvBuff(9)) And (sendBuff(10) = recvBuff(10))) Then
            '成功时, 返回值与设置一致
            success = 1
            log ("1.17 设置门控制参数         成功...")
        End If
    End If



'1.21   权限按从小到大顺序添加[功能号: 0x56] 适用于权限数过1000, 少于8万 **********************************************************************************
        '此功能实现 完全更新全部权限, 用户不用清空之前的权限. 只是将上传的权限顺序从第1个依次到最后一个上传完成. 如果中途中断的话, 仍以原权限为主
        '建议权限数更新超过50个, 即可使用此指令
        '以10000个卡号为例, 此处简化的排序, 直接是以50001开始的10000个卡. 用户按照需要将要上传的卡号排序存放
        
        log ("1.21 权限按从小到大顺序添加[功能号: 0x56]开始...")
        log ("       1万条权限...")

        Dim cardCount As Integer
        cardCount = 10000
        '2015-06-09 20:20:20 卡总数量
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
            '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
            sendBuff(8 + 4) = &H20
            sendBuff(8 + 5) = &H10
            sendBuff(8 + 6) = &H1
            sendBuff(8 + 7) = &H1
            '20 29 12 31 截止日期:  2029年12月31日
            sendBuff(8 + 8) = &H20
            sendBuff(8 + 9) = &H29
            sendBuff(8 + 10) = &H12
            sendBuff(8 + 11) = &H31
            '01 允许通过 一号门 (对单门, 双门, 四门控制器有效)
            sendBuff(8 + 12) = &H1
            '01 允许通过 二号门 (对双门, 四门控制器有效)
            sendBuff(8 + 13) = &H1  '如果禁止2号门, 则只要设为 &H00
            '01 允许通过 三号门 (对四门控制器有效)
            sendBuff(8 + 14) = &H1
            '01 允许通过 四号门 (对四门控制器有效)
            sendBuff(8 + 15) = &H1
        
            ret = IntToByte(cardCount, sendBuff, 32, 4)            '总的权限数
            ret = IntToByte(i + 1, sendBuff, 35, 4)      '当前权限的索引位(从1开始)
        
            ret = pktrun(sendBuff, recvBuff)
            success = 0
            If (ret = 1) Then
                getReceiveBuffData recvBuff
                If (recvBuff(8) = 1) Then
                    success = 1
                Else
                     If recvBuff(8) = &HE1 Then
                        log ("1.21权限按从小到大顺序添加[功能号: 0x56] =0xE1 表示卡号没有从小到大排序...???")
                       
                    End If
                    success = 0
                    Exit For
                 End If
            Else
               log ("1.21权限按从小到大顺序添加[功能号: 0x56] 通信不上...???")
                Exit For
            End If
        Next
        If success = 1 Then
            log ("1.21权限按从小到大顺序添加[功能号: 0x56] 成功...")
        Else
            log ("1.21权限按从小到大顺序添加[功能号: 0x56] 失败...????")
        End If
    '其他指令  **********************************************************************************


    ' **********************************************************************************

    '结束  **********************************************************************************

    If (ret = 1) Then
        log ("基本功能测试 成功...")
    Else
        log ("基本功能测试 失败????...")
    End If
End Sub


'ControllerIP 被设置的控制器IP地址
'controllerSN 被设置的控制器序列号
'watchServerIP   要设置的服务器IP
'watchServerPort 要设置的端口
Private Sub testWatchingServer(ByVal controllerIP As String, ByVal controllerSN As Long, ByVal watchServerIP As String, ByVal watchServerPort As Long)
    Dim sendBuff(63) As Byte    '数据发送缓冲区(64字节)
    Dim recvBuff(63) As Byte     '数据接收缓冲区(64字节)

    Me.Winsock1.RemoteHost = controllerIP
    Me.Winsock1.RemotePort = ControllerPort '60000

    Dim ret As Integer
    Dim success As Integer

    '1.18  设置接收服务器的IP和端口 (功能号: 0x90) **********************************************************************************
    '  接收服务器的IP: 192.168.168.101  (当前电脑IP)
    '(如果不想让控制器发出数据, 只要将接收服务器的IP设为0.0.0.0 就行了)
    '接收服务器的端口: 61005
    '每隔5秒发送一次: 05
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H90
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    '服务器IP: 192.168.168.101
    'sendBuff(8 + 0) = 192
    'sendBuff(8 + 1) = 168
    'sendBuff(8 + 2) = 168
    'sendBuff(8 + 3) = 101
    Dim Ar() As String
    Ar = Split(watchServerIP, ".", , vbTextCompare)
    If UBound(Ar) <> 4 - 1 Then

        log ("watchServerIP 地址不合理")
        Exit Sub
    End If
    sendBuff(8 + 0) = CInt(Ar(0))
    sendBuff(8 + 1) = CInt(Ar(1))
    sendBuff(8 + 2) = CInt(Ar(2))
    sendBuff(8 + 3) = CInt(Ar(3))
    '接收服务器的端口: 61005
    sendBuff(8 + 4) = (watchServerPort And &HFF)
    sendBuff(8 + 5) = ((watchServerPort - (watchServerPort And &HFF)) / 256) And &HFF

    '每隔5秒发送一次: 05 (定时上传信息的周期为5秒 (正常运行时每隔5秒发送一次  有刷卡时立即发送))
    sendBuff(8 + 6) = 5

    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        If (recvBuff(8) = 1) Then

            success = 1
            log ("1.18 设置接收服务器的IP和端口   成功...")
        Else
            log ("1.18 设置接收服务器的IP和端口   失败????...")
            
        End If
    Else
        log ("1.18 设置接收服务器的IP和端口   失败????...")
    End If
    Sleep (1000) '延时一秒 再读取
    '1.19  读取接收服务器的IP和端口 (功能号: 0x92) **********************************************************************************
    ret = arrayReset(sendBuff, WGPacketSize)
    sendBuff(0) = WGPacketType
    sendBuff(1) = &H92
    ret = IntToByte(controllerSN, sendBuff, 4, 4)
    ret = pktrun(sendBuff, recvBuff)
    success = 0
    If (ret = 1) Then
        getReceiveBuffData recvBuff
        success = 1
        log ("1.19 读取接收服务器的IP和端口   成功...")
    Else
        log ("1.19 读取接收服务器的IP和端口   失败????...")
    End If

End Sub


'进入接收服务器监控状态
Private Sub WatchingServerRuning(ByVal watchServerIP As String, ByVal watchServerPort As Long)

    watchingrecordIndex = -1
     
    Me.WinsockServer.Bind watchServerPort  '使用当前电脑的watchServerPort
        log ("进入接收服务器监控状态....")
End Sub



Private Sub Form_Unload(Cancel As Integer)
    Me.Winsock1.Close
    Me.WinsockServer.Close
End Sub

'服务器接收数据处理
Private Sub WinsockServer_DataArrival(ByVal bytesTotal As Long)
    Dim sn As Long
    If (bytesTotal > 0 And ((bytesTota Mod WGPacketSize) = 0)) Then
        '是有效数据
    Else
        Dim varlose As Object
        Me.WinsockServer.GetData (varlose) '清空掉
        Exit Sub
    End If

    Dim receivedByteCnt As Integer
    Dim watchingRecvBuffVar As Variant
    Dim watchingRecvBuff(63) As Byte    '服务器监控 接收数据

    receivedByteCnt = 0
    Do While (receivedByteCnt < bytesTotal)
        Me.WinsockServer.GetData watchingRecvBuffVar, vbArray + vbByte, WGPacketSize

        '检查类型, 功能号, 要一致
        Dim i As Integer
        For i = 0 To WGPacketSize - 1
            watchingRecvBuff(i) = watchingRecvBuffVar(i)
        Next i
        If (watchingRecvBuff(1) = &H20) Then
            sn = ByteToLong(watchingRecvBuff, 4, 4)

            log ("接收到来自控制器SN = " & sn & " 的数据包..")

            Dim recordIndex As Long
            recordIndex = ByteToLong(watchingRecvBuff, 8, 4)
            If (recordIndex > watchingrecordIndex) Then
                watchingrecordIndex = recordIndex
               displayRecordInformation watchingRecvBuff
            End If
            
            Text1.SelLength = 1              '显示最后一行
            Text1.SelStart = Len(Text1.Text) '显示最后一行
        End If

        receivedByteCnt = receivedByteCnt + WGPacketSize
    Loop
End Sub


