'*
'* WGController32 2015-04-30 17:40:43 karl CSN  陈绍宁 $
'*
'* 门禁控制器 短报文协议 测试案例
'* V2.6 版本  2015-11-03 20:25:53 V6.60驱动版本 通信密码测试.  
'*                               修改通信的重试操作
'* V2.5 版本  2015-04-29 20:41:30 采用 V6.56驱动版本 型号由0x19改为0x17
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
    '2015-05-05 17:35:07 停止接收服务器
    Private bStopBasicFunction As Boolean = False
    '2015-06-10 09:04:52 基本测试
    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As FormClosingEventArgs)
        bStopWatchServer = True
        bStopBasicFunction = True
        '2015-06-10 09:04:52 基本测试
    End Sub
    Private Sub button1_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button1.Click
        Me.txtInfo.Text = ""
        '停止接收服务器标识 
        bStopWatchServer = True
        bStopBasicFunction = False
        '2015-06-10 09:04:52 基本测试
        ''    '本案例未作搜索控制器  及 设置IP的工作  (直接由IP设置工具来完成)
        ''    '本案例中测试说明
        ''    '控制器SN  = 229999901
        ''    '控制器IP  = 192.168.168.123
        ''    '电脑  IP  = 192.168.168.101
        ''    '用于作为接收服务器的IP (本电脑IP 192.168.168.101), 接收服务器端口 (61005)
        '基本功能测试
        'txtSN.Text 控制器9位数的序列SN
        'txtIP.Text 控制器IP地址, 缺省采用192.168.168.123  [可以采用 Search Controller 修改控制器IP]
        testBasicFunction(txtIP.Text, Long.Parse(txtSN.Text))
        'txtWatchServerIP.Text  接收服务器的IP,缺省采用电脑IP 192.168.168.101 [也可以采用 Search Controller 修改设置]
        'txtWatchServerPort.Text  接收服务器的PORT, 缺省 61005
        testWatchingServer(txtIP.Text, Long.Parse(txtSN.Text), txtWatchServerIP.Text, Integer.Parse(Me.txtWatchServerPort.Text))
        '接收服务器设置
        bStopWatchServer = False
        WatchingServerRuning(txtWatchServerIP.Text, Integer.Parse(Me.txtWatchServerPort.Text))
        '服务器运行....
        bStopWatchServer = True
    End Sub
    Private Sub button2_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button2.Click
        bStopWatchServer = True
        bStopBasicFunction = True
        '2015-06-10 09:04:52 基本测试
    End Sub
    Private Sub button3_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button3.Click
        '2015-05-05 17:35:35 搜索控制器
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
        '服务器运行....
        bStopWatchServer = True
    End Sub
    ''' <summary>
    ''' 短报文
    ''' </summary>
    Class WGPacketShort
        Public Shared WGPacketSize As Integer = 64
        '报文长度
        '2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//类型
        Public Shared Type As Integer = &H17
        '2015-04-29 22:22:50			//类型
        Public Shared ControllerPort As Integer = 60000
        '控制器端口
        Public Shared SpecialFlag As Long = &H55AAAA55
        '特殊标识 防止误操作
        Public functionID As Integer
        '功能号
        Public iDevSn As Long
        '设备序列号 4字节, 9位数
        Public IP As String
        '控制器的IP地址
        Public data As Byte() = New Byte(56 - 1) {}
        '56字节的数据 [含流水号]
        Public recv As Byte() = New Byte(WGPacketSize - 1) {}
        '接收到的数据
        Public Sub New()
            Reset()
        End Sub
        Public Sub Reset()
            '数据复位
            For i As Integer = 0 To 55
                data(i) = 0
            Next
        End Sub
        Shared sequenceId As Long
        '序列号	
        Public Function toByte() As Byte()
            '生成64字节指令包
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
            '发送指令 接收返回信息
            Dim buff As Byte() = toByte()
            Dim tries As Integer = 3
            Dim errcnt As Integer = 0
            controller.IP = IP
            controller.PORT = ControllerPort
            Do
                If controller.ShortPacketSend(buff, recv) < 0 Then
                    '2015-11-03 20:26:52 进入重试 Return -1
                Else
                    '流水号
                    Dim sequenceIdReceived As Long = 0
                    For i As Integer = 0 To 3
                        Dim lng As Long = recv(40 + i)
                        sequenceIdReceived += (lng << (8 * i))
                    Next
                        If (recv(0) = Type) AndAlso (recv(1) = functionID) AndAlso (sequenceIdReceived = sequenceId) Then
                            '类型一致
                            '功能号一致
                            '序列号对应
                            Return 1
                        Else
                            errcnt += 1
                        End If
                End If
            Loop While System.Math.Max(System.Threading.Interlocked.Decrement(tries), tries + 1) > 0
            '重试三次
            Return -1
        End Function
        ''' <summary>
        ''' 最后发出的流水号
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function sequenceIdSent() As Long
            ' 
            Return sequenceId
            ' 最后发出的流水号
        End Function
        ''' <summary>
        ''' 关闭
        ''' </summary>
        Public Sub close()
            controller.Dispose()
        End Sub
    End Class
    Private Sub log(ByVal info As String)
        '日志信息
        'txtInfo.Text += String.Format("{0}" & Chr(13) & "" & Chr(10) & "", info)
        'txtInfo.AppendText(String.Format("{0}" & Chr(13) & "" & Chr(10) & "", info))
        txtInfo.AppendText(String.Format("{0} {1}" & Chr(13) & "" & Chr(10) & "", Date.Now.ToString("HH:mm:ss"), info)) '2015-11-03 21:04:24 时间点
        txtInfo.ScrollToCaret()  '滚动到光标处
        Application.DoEvents()
    End Sub
    ''' <summary>
    ''' 4字节转成整型数(低位前, 高位后)
    ''' </summary>
    ''' <param name="buff">字节数组</param>
    ''' <param name="start">起始索引位(从0开始计)</param>
    ''' <param name="len">长度</param>
    ''' <returns>整型数</returns>
    Private Function byteToLong(ByVal buff As Byte(), ByVal start As Integer, ByVal len As Integer) As Long
        Dim val As Long = 0
        Dim i As Integer = 0
        While i < len AndAlso i < 4
            '2015-06-10 10:29:42 增加 (long)
            Dim lng As Long = buff(i + start)
            val += (lng << (8 * i))
            i += 1
        End While
        Return val
    End Function
    ''' <summary>
    ''' 整型数转换为4字节数组
    ''' </summary>
    ''' <param name="outBytes">数组</param>
    ''' <param name="startIndex">起始索引位(从0开始计)</param>
    ''' <param name="val">数值</param>
    Private Sub LongToBytes(ByRef outBytes As Byte(), ByVal startIndex As Integer, ByVal val As Long)
        Array.Copy(System.BitConverter.GetBytes(val), 0, outBytes, startIndex, 4)
    End Sub
    ''' <summary>
    ''' 获取Hex值, 主要用于日期时间格式
    ''' </summary>
    ''' <param name="val">数值</param>
    ''' <returns>Hex值</returns>
    Private Function GetHex(ByVal val As Integer) As Integer
        Return ((val Mod 10) + (((val - (val Mod 10)) / 10) Mod 10) * 16)
    End Function
   

    ''' <summary>
    ''' 显示记录信息
    ''' </summary>
    ''' <param name="recv"></param>
    ''' <remarks></remarks>
    Private Sub displayRecordInformation(ByRef recv() As Byte)
        '8-11	记录的索引号
        '(=0表示没有记录)	4	0x00000000
        Dim recordIndex As Integer = 0
        recordIndex = (byteToLong(recv, 8, 4))
        '12	记录类型**********************************************
        '0=无记录
        '1=刷卡记录
        '2=门磁,按钮, 设备启动, 远程开门记录
        '3=报警记录	1	
        '0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
        Dim recordType As Integer = recv(12)
        '13	有效性(0 表示不通过, 1表示通过)	1	
        Dim recordValid As Integer = recv(13)
        '14	门号(1,2,3,4)	1	
        Dim recordDoorNO As Integer = recv(14)
        '15	进门/出门(1表示进门, 2表示出门)	1	0x01
        Dim recordInOrOut As Integer = recv(15)
        '16-19	卡号(类型是刷卡记录时)
        '或编号(其他类型记录)	4	
        Dim recordCardNO As Long = 0
        recordCardNO = (byteToLong(recv, 16, 4))
        '20-26	刷卡时间:
        '年月日时分秒 (采用BCD码)见设置时间部分的说明
        Dim recordTime As String = "2000-01-01 00:00:00"
        recordTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", recv(20), recv(21), recv(22), recv(23), recv(24), _
            recv(25), recv(26))
        '2012.12.11 10:49:59	7	
        '27	记录原因代码(可以查 “刷卡记录说明.xls”文件的ReasonNO)
        '处理复杂信息才用	1	
        Dim reason As Integer = recv(27)
        '0=无记录
        '1=刷卡记录
        '2=门磁,按钮, 设备启动, 远程开门记录
        '3=报警记录	1	
        '0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
        If recordType = 0 Then
            log(String.Format("索引位={0}  无记录", recordIndex))
        ElseIf recordType = 255 Then
            log(String.Format(" 指定索引位的记录已被覆盖掉了,请使用索引0, 取回最早一条记录的索引值"))
        ElseIf recordType = 1 Then
            '2015-06-10 08:49:31 显示记录类型为卡号的数据
            '卡号
            log(String.Format("索引位={0}  ", recordIndex))
            log(String.Format("  卡号 = {0}", recordCardNO))
            log(String.Format("  门号 = {0}", recordDoorNO))
            log(String.Format("  进出 = {0}", IIf(recordInOrOut = 1, "进门", "出门")))
            log(String.Format("  有效 = {0}", IIf(recordValid = 1, "通过", "禁止")))
            log(String.Format("  时间 = {0}", recordTime))
            log(String.Format("  原因 = {0}", getReasonDetailChinese(reason)))
        ElseIf recordType = 2 Then
            '其他处理
            '门磁,按钮, 设备启动, 远程开门记录
            log(String.Format("索引位={0}  非刷卡记录", recordIndex))
            log(String.Format("  编号 = {0}", recordCardNO))
            log(String.Format("  门号 = {0}", recordDoorNO))
            log(String.Format("  时间 = {0}", recordTime))
            log(String.Format("  原因 = {0}", getReasonDetailChinese(reason)))
        ElseIf recordType = 3 Then
            '其他处理
            '报警记录
            log(String.Format("索引位={0}  报警记录", recordIndex))
            log(String.Format("  编号 = {0}", recordCardNO))
            log(String.Format("  门号 = {0}", recordDoorNO))
            log(String.Format("  时间 = {0}", recordTime))
            log(String.Format("  原因 = {0}", getReasonDetailChinese(reason)))
        End If
    End Sub

    '记录原因 (类型中 SwipePass 表示通过; SwipeNOPass表示禁止通过; ValidEvent 有效事件(如按钮 门磁 超级密码开门); Warn 报警事件)
    '代码  类型   英文描述  中文描述
    Private RecordDetails As String() = {
"1", "SwipePass", "Swipe", "刷卡开门",
"2", "SwipePass", "Swipe Close", "刷卡关",
"3", "SwipePass", "Swipe Open", "刷卡开",
"4", "SwipePass", "Swipe Limited Times", "刷卡开门(带限次)",
"5", "SwipeNOPass", "Denied Access: PC Control", "刷卡禁止通过: 电脑控制",
"6", "SwipeNOPass", "Denied Access: No PRIVILEGE", "刷卡禁止通过: 没有权限",
"7", "SwipeNOPass", "Denied Access: Wrong PASSWORD", "刷卡禁止通过: 密码不对",
"8", "SwipeNOPass", "Denied Access: AntiBack", "刷卡禁止通过: 反潜回",
"9", "SwipeNOPass", "Denied Access: More Cards", "刷卡禁止通过: 多卡",
"10", "SwipeNOPass", "Denied Access: First Card Open", "刷卡禁止通过: 首卡",
"11", "SwipeNOPass", "Denied Access: Door Set NC", "刷卡禁止通过: 门为常闭",
"12", "SwipeNOPass", "Denied Access: InterLock", "刷卡禁止通过: 互锁",
"13", "SwipeNOPass", "Denied Access: Limited Times", "刷卡禁止通过: 受刷卡次数限制",
"14", "SwipeNOPass", "Denied Access: Limited Person Indoor", "刷卡禁止通过: 门内人数限制",
"15", "SwipeNOPass", "Denied Access: Invalid Timezone", "刷卡禁止通过: 卡过期或不在有效时段",
"16", "SwipeNOPass", "Denied Access: In Order", "刷卡禁止通过: 按顺序进出限制",
"17", "SwipeNOPass", "Denied Access: SWIPE GAP LIMIT", "刷卡禁止通过: 刷卡间隔约束",
"18", "SwipeNOPass", "Denied Access", "刷卡禁止通过: 原因不明",
"19", "SwipeNOPass", "Denied Access: Limited Times", "刷卡禁止通过: 刷卡次数限制",
"20", "ValidEvent", "Push Button", "按钮开门",
"21", "ValidEvent", "Push Button Open", "按钮开",
"22", "ValidEvent", "Push Button Close", "按钮关",
"23", "ValidEvent", "Door Open", "门打开[门磁信号]",
"24", "ValidEvent", "Door Closed", "门关闭[门磁信号]",
"25", "ValidEvent", "Super Password Open Door", "超级密码开门",
"26", "ValidEvent", "Super Password Open", "超级密码开",
"27", "ValidEvent", "Super Password Close", "超级密码关",
"28", "Warn", "Controller Power On", "控制器上电",
"29", "Warn", "Controller Reset", "控制器复位",
"30", "Warn", "Push Button Invalid: Disable", "按钮不开门: 按钮禁用",
"31", "Warn", "Push Button Invalid: Forced Lock", "按钮不开门: 强制关门",
"32", "Warn", "Push Button Invalid: Not On Line", "按钮不开门: 门不在线",
"33", "Warn", "Push Button Invalid: InterLock", "按钮不开门: 互锁",
"34", "Warn", "Threat", "胁迫报警",
"35", "Warn", "Threat Open", "胁迫报警开",
"36", "Warn", "Threat Close", "胁迫报警关",
"37", "Warn", "Open too long", "门长时间未关报警[合法开门后]",
"38", "Warn", "Forced Open", "强行闯入报警",
"39", "Warn", "Fire", "火警",
"40", "Warn", "Forced Close", "强制关门",
"41", "Warn", "Guard Against Theft", "防盗报警",
"42", "Warn", "7*24Hour Zone", "烟雾煤气温度报警",
"43", "Warn", "Emergency Call", "紧急呼救报警",
"44", "RemoteOpen", "Remote Open Door", "操作员远程开门",
"45", "RemoteOpen", "Remote Open Door By USB Reader", "发卡器确定发出的远程开门"
                                        }
    Private Function getReasonDetailChinese(ByVal Reason As Integer) As String
        '中文
        If Reason > 45 Then
            Return ""
        End If
        If Reason <= 0 Then
            Return ""
        End If
        Return RecordDetails((Reason - 1) * 4 + 3)
        '中文信息
    End Function
    Private Function getReasonDetailEnglish(ByVal Reason As Integer) As String
        '英文描述
        If Reason > 45 Then
            Return ""
        End If
        If Reason <= 0 Then
            Return ""
        End If
        Return RecordDetails((Reason - 1) * 4 + 2)
        '英文信息
    End Function
    ''' <summary>
    ''' 基本功能测试
    ''' </summary>
    ''' <param name="ControllerIP">控制器IP地址</param>
    ''' <param name="controllerSN"> 控制器序列号</param>
    ''' <returns>小于或等于0 失败, 1表示成功</returns>
    Private Function testBasicFunction(ByVal ControllerIP As String, ByVal controllerSN As Long) As Integer
        Dim ret As Integer = 0
        Dim success As Integer = 0
        '0 失败, 1表示成功
        '创建短报文 pkt
        Dim pkt As New WGPacketShort()
        pkt.iDevSn = controllerSN
        pkt.IP = ControllerIP
        '1.4	查询控制器状态[功能号: 0x20](实时监控用) **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H20
        ret = pkt.run()
        success = 0
        If ret = 1 Then
            '读取信息成功...
            success = 1
            log("1.4 查询控制器状态 成功...")
            '	  	最后一条记录的信息		
            displayRecordInformation(pkt.recv)
            '2015-06-09 20:01:21
            '	其他信息		
            Dim doorStatus As Integer() = New Integer(4) {}
            '28	1号门门磁(0表示关上, 1表示打开)	1	0x00
            doorStatus(1 - 1) = pkt.recv(28)
            '29	2号门门磁(0表示关上, 1表示打开)	1	0x00
            doorStatus(2 - 1) = pkt.recv(29)
            '30	3号门门磁(0表示关上, 1表示打开)	1	0x00
            doorStatus(3 - 1) = pkt.recv(30)
            '31	4号门门磁(0表示关上, 1表示打开)	1	0x00
            doorStatus(4 - 1) = pkt.recv(31)
            Dim pbStatus As Integer() = New Integer(4) {}
            '32	1号门按钮(0表示松开, 1表示按下)	1	0x00
            pbStatus(1 - 1) = pkt.recv(32)
            '33	2号门按钮(0表示松开, 1表示按下)	1	0x00
            pbStatus(2 - 1) = pkt.recv(33)
            '34	3号门按钮(0表示松开, 1表示按下)	1	0x00
            pbStatus(3 - 1) = pkt.recv(34)
            '35	4号门按钮(0表示松开, 1表示按下)	1	0x00
            pbStatus(4 - 1) = pkt.recv(35)
            '36	故障号
            '等于0 无故障
            '不等于0, 有故障(先重设时间, 如果还有问题, 则要返厂家维护)	1	
            Dim errCode As Integer = pkt.recv(36)
            '37	控制器当前时间
            '时	1	0x21
            '38	分	1	0x30
            '39	秒	1	0x58
            '40-43	流水号	4	
            Dim sequenceId As Long = 0
            sequenceId = byteToLong(pkt.recv, 40, 4)
            '48
            '特殊信息1(依据实际使用中返回)
            '键盘按键信息	1	
            '49	继电器状态	1	 [0表示门上锁, 1表示门开锁. 正常门上锁时, 值为0000]
            Dim relayStatus As Integer = pkt.recv(49)
            '一号门 开锁
            If (relayStatus And 1) > 0 Then
                '一号门 上锁
            Else
            End If
            '二号门 开锁
            If (relayStatus And 2) > 0 Then
                '二号门 上锁
            Else
            End If
            '三号门 开锁
            If (relayStatus And 4) > 0 Then
                '三号门 上锁
            Else
            End If
            '四号门 开锁
            If (relayStatus And 8) > 0 Then
                '四号门 上锁
            Else
            End If
            '50	门磁状态的8-15bit位[火警/强制锁门]
            'Bit0  强制锁门
            'Bit1  火警		
            Dim otherInputStatus As Integer = pkt.recv(50)
            '强制锁门
            If (otherInputStatus And 1) > 0 Then
            End If
            '火警
            If (otherInputStatus And 2) > 0 Then
            End If
            '51	V5.46版本支持 控制器当前年	1	0x13
            '52	V5.46版本支持 月	1	0x06
            '53	V5.46版本支持 日	1	0x22
            Dim controllerTime As String = "2000-01-01 00:00:00"
            '控制器当前时间
            controllerTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", 32, pkt.recv(51), pkt.recv(52), pkt.recv(53), pkt.recv(37), _
             pkt.recv(38), pkt.recv(39))
        Else
            log("1.4 查询控制器状态 失败?????...")
            Return -1
        End If
        '1.5	读取日期时间(功能号: 0x32) **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H32
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim controllerTime As String = "2000-01-01 00:00:00"
            '控制器当前时间
            controllerTime = String.Format("{0:X2}{1:X2}-{2:X2}-{3:X2} {4:X2}:{5:X2}:{6:X2}", pkt.recv(8), pkt.recv(9), pkt.recv(10), pkt.recv(11), pkt.recv(12), _
             pkt.recv(13), pkt.recv(14))
            log("1.5 读取日期时间 成功...")
            success = 1
        End If
        '1.6	设置日期时间[功能号: 0x30] **********************************************************************************
        '按电脑当前时间校准控制器.....
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
                log("1.6 设置日期时间 成功...")
                success = 1
            End If
        End If
        '1.7	获取指定索引号的记录[功能号: 0xB0] **********************************************************************************
        '(取索引号 0x00000001的记录)
        Dim recordIndexToGet As Long = 0
        pkt.Reset()
        pkt.functionID = &HB0
        pkt.iDevSn = controllerSN
        '	(特殊
        '如果=0, 则取回最早一条记录信息
        '如果=0xffffffff则取回最后一条记录的信息)
        '记录索引号正常情况下是顺序递增的, 最大可达0xffffff = 16,777,215 (超过1千万) . 由于存储空间有限, 控制器上只会保留最近的20万个记录. 当索引号超过20万后, 旧的索引号位的记录就会被覆盖, 所以这时查询这些索引号的记录, 返回的记录类型将是0xff, 表示不存在了.
        recordIndexToGet = 1
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 获取索引为1号记录的信息 成功...")
            '	  	索引为1号记录的信息		
            displayRecordInformation(pkt.recv)
            success = 1
        End If
        '. 发出报文 (取最早的一条记录 通过索引号 0x00000000) [此指令适合于 刷卡记录超过20万时环境下使用]
        pkt.Reset()
        pkt.functionID = &HB0
        recordIndexToGet = 0
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 获取最早一条记录的信息 成功...")
            '	  	最早一条记录的信息		
            displayRecordInformation(pkt.recv)
            success = 1
        End If
        '发出报文 (取最新的一条记录 通过索引 0xffffffff)
        pkt.Reset()
        pkt.functionID = &HB0
        recordIndexToGet = 4294967295
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.7 获取最新一条记录的信息 成功...")
            '	  	最新一条记录的信息		
            displayRecordInformation(pkt.recv)
            '2015-06-09 20:01:21
            success = 1
        End If
        '    '1.8	设置已读取过的记录索引号[功能号: 0xB2] **********************************************************************************
        '    pkt.Reset()
        '    pkt.functionID = &HB2
        '    ' (设为已读取过的记录索引号为5)
        '    Dim recordIndexGot As Integer = 5
        '    LongToBytes(pkt.data, 0, recordIndexGot)

        '    '12	标识(防止误设置)	1	0x55 [固定]
        '    LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
        '    ret = pkt.run()
        '    success = 0
        '    If ret > 0 Then
        '        If pkt.recv(8) = 1 Then
        '            log("1.8 设置已读取过的记录索引号 成功...")
        '            success = 1
        '        End If
        '    End If

        '    '1.9	获取已读取过的记录索引号[功能号: 0xB4] **********************************************************************************
        '    pkt.Reset()
        '    pkt.functionID = &HB4
        '    Dim recordIndexGotToRead As Integer = 0
        '    ret = pkt.run()
        '    success = 0
        '    If ret > 0 Then
        '        recordIndexGotToRead = (byteToLong(pkt.recv, 8, 4))
        '        log("1.9 获取已读取过的记录索引号 成功...")
        '        success = 1
        '    End If


        ''1.8	设置已读取过的记录索引号[功能号: 0xB2] **********************************************************************************
        ''恢复已提取过的记录, 为1.9的完整提取操作作准备-- 实际使用中, 在出现问题时才恢复, 正常不用恢复...
        'pkt.Reset()
        'pkt.functionID = &HB2
        '' (设为已读取过的记录索引号为0)
        'Dim recordIndexGot As Integer = 0
        'LongToBytes(pkt.data, 0, recordIndexGot)
        ''12	标识(防止误设置)	1	0x55 [固定]
        'LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
        'ret = pkt.run()
        'success = 0
        'If ret > 0 Then
        '    If pkt.recv(8) = 1 Then
        '        log("1.8 设置已读取过的记录索引号 成功...")
        '        success = 1
        '    End If
        'End If


        '1.9	提取记录操作
        '1. 通过 0xB4指令 获取已读取过的记录索引号 recordIndex
        '2. 通过 0xB0指令 获取指定索引号的记录  从recordIndex + 1开始提取记录， 直到记录为空为止
        '3. 通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
        '经过上面三个步骤， 整个提取记录的操作完成
        log("1.9 提取记录操作	 开始...")
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
                    '2015-06-10 09:08:14 停止
                    Return 0
                End If
                LongToBytes(pkt.data, 0, recordIndexToGetStart)
                ret = pkt.run()
                success = 0
                If ret > 0 Then
                    success = 1
                    '12	记录类型
                    '0=无记录
                    '1=刷卡记录
                    '2=门磁,按钮, 设备启动, 远程开门记录
                    '3=报警记录	1	
                    '0xFF=表示指定索引位的记录已被覆盖掉了.  请使用索引0, 取回最早一条记录的索引值
                    Dim recordType As Integer = pkt.recv(12)
                    If recordType = 0 Then
                        '没有更多记录
                        Exit Do
                    End If
                    If recordType = 255 Then
                        '此索引号无效  重新设置索引值
                        '取最早一条记录的索引位
                        pkt.Reset()
                        pkt.functionID = &HB0
                        recordIndexToGet = 0
                        LongToBytes(pkt.data, 0, recordIndexToGet)
                        ret = pkt.run()
                        success = 0
                        If ret > 0 Then
                            log("1.7 获取最早一条记录的信息 成功...")
                            recordIndexGotToRead = (byteToLong(pkt.recv, 8, 4))
                            recordIndexToGetStart = recordIndexGotToRead
                            Continue Do
                        End If
                        success = 0
                        Exit Do
                    End If
                    recordIndexValidGet = recordIndexToGetStart
                    '2015-06-09 20:01:21
                    '.......对收到的记录作存储处理
                    '*****
                    '###############
                    displayRecordInformation(pkt.recv)
                Else
                    '提取失败
                    Exit Do
                End If
                recordIndexToGetStart += 1
            Loop While System.Math.Max(System.Threading.Interlocked.Increment(cnt), cnt - 1) < 200000
            If success > 0 Then
                '通过 0xB2指令 设置已读取过的记录索引号  设置的值为最后读取到的刷卡记录索引号
                pkt.Reset()
                pkt.functionID = &HB2
                LongToBytes(pkt.data, 0, recordIndexValidGet)
                '12	标识(防止误设置)	1	0x55 [固定]
                LongToBytes(pkt.data, 4, WGPacketShort.SpecialFlag)
                ret = pkt.run()
                success = 0
                If ret > 0 Then
                    If pkt.recv(8) = 1 Then
                        '完全提取成功....
                        log("1.9 完全提取成功  成功...")
                        success = 1
                    End If
                End If
            End If
        End If
        '1.10	远程开门[功能号: 0x40] **********************************************************************************
        Dim doorNO As Integer = 1
        pkt.Reset()
        pkt.functionID = &H40
        pkt.data(0) = (doorNO And 255)
        '2013-11-03 20:56:33
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                '有效开门.....
                log("1.10 远程开门 成功...")
                success = 1
            End If
        End If
        '1.11	权限添加或修改[功能号: 0x50] **********************************************************************************
        '增加卡号0D D7 37 00, 通过当前控制器的所有门
        pkt.Reset()
        pkt.functionID = &H50
        '0D D7 37 00 要添加或修改的权限中的卡号 = 0x0037D70D = 3659533 (十进制)
        Dim cardNOOfPrivilege As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilege)
        '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
        pkt.data(4) = 32
        pkt.data(5) = 16
        pkt.data(6) = 1
        pkt.data(7) = 1
        '20 29 12 31 截止日期:  2029年12月31日
        pkt.data(8) = 32
        pkt.data(9) = 41
        pkt.data(10) = 18
        pkt.data(11) = 49
        '01 允许通过 一号门 [对单门, 双门, 四门控制器有效] 
        pkt.data(12) = 1
        '01 允许通过 二号门 [对双门, 四门控制器有效]
        pkt.data(13) = 1
        '如果禁止2号门, 则只要设为 0x00
        '01 允许通过 三号门 [对四门控制器有效]
        pkt.data(14) = 1
        '01 允许通过 四号门 [对四门控制器有效]
        pkt.data(15) = 1
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                '这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
                log("1.11 权限添加或修改  成功...")
                success = 1
            End If
        End If
        '1.12	权限删除(单个删除)[功能号: 0x52] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H52
        pkt.iDevSn = controllerSN
        '要删除的权限卡号0D D7 37 00  = 0x0037D70D = 3659533 (十进制)
        Dim cardNOOfPrivilegeToDelete As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilegeToDelete)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                '这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器不会动作.
                log("1.12 权限删除(单个删除)  成功...")
                success = 1
            End If
        End If
        '1.13	权限清空(全部清掉)[功能号: 0x54] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H54
        pkt.iDevSn = controllerSN
        LongToBytes(pkt.data, 0, WGPacketShort.SpecialFlag)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                '这时清空成功
                log("1.13 权限清空(全部清掉)  成功...")
                success = 1
            End If
        End If
        '1.14	权限总数读取[功能号: 0x58] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H58
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim privilegeCount As Integer = 0
            privilegeCount = (byteToLong(pkt.recv, 8, 4))
            log("1.14 权限总数读取  成功...")
            success = 1
        End If
        '再次添加为查询操作 1.11	权限添加或修改[功能号: 0x50] **********************************************************************************
        '增加卡号0D D7 37 00, 通过当前控制器的所有门
        pkt.Reset()
        pkt.functionID = &H50
        '0D D7 37 00 要添加或修改的权限中的卡号 = 0x0037D70D = 3659533 (十进制)
        cardNOOfPrivilege = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilege)
        '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
        pkt.data(4) = 32
        pkt.data(5) = 16
        pkt.data(6) = 1
        pkt.data(7) = 1
        '20 29 12 31 截止日期:  2029年12月31日
        pkt.data(8) = 32
        pkt.data(9) = 41
        pkt.data(10) = 18
        pkt.data(11) = 49
        '01 允许通过 一号门 [对单门, 双门, 四门控制器有效] 
        pkt.data(12) = 1
        '01 允许通过 二号门 [对双门, 四门控制器有效]
        pkt.data(13) = 1
        '如果禁止2号门, 则只要设为 0x00
        '01 允许通过 三号门 [对四门控制器有效]
        pkt.data(14) = 1
        '01 允许通过 四号门 [对四门控制器有效]
        pkt.data(15) = 1
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                '这时 刷卡号为= 0x0037D70D = 3659533 (十进制)的卡, 1号门继电器动作.
                log("1.11 权限添加或修改  成功...")
                success = 1
            End If
        End If
        '1.15	权限查询[功能号: 0x5A] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H5A
        pkt.iDevSn = controllerSN
        ' (查卡号为 0D D7 37 00的权限)
        Dim cardNOOfPrivilegeToQuery As Long = 3659533
        LongToBytes(pkt.data, 0, cardNOOfPrivilegeToQuery)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim cardNOOfPrivilegeToGet As Long = 0
            cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4)
            If cardNOOfPrivilegeToGet = 0 Then
                '没有权限时: (卡号部分为0)
                log("1.15      没有权限信息: (卡号部分为0)")
            Else
                '具体权限信息...
                log("1.15     有权限信息...")
            End If
            log("1.15 权限查询  成功...")
            success = 1
        End If
        '1.16  获取指定索引号的权限[功能号: 0x5C] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H5C
        pkt.iDevSn = controllerSN
        Dim QueryIndex As Long = 1
        '索引号(从1开始);
        LongToBytes(pkt.data, 0, QueryIndex)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            Dim cardNOOfPrivilegeToGet As Long = 0
            cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8, 4)
            If 4294967295 = cardNOOfPrivilegeToGet Then
                'FFFFFFFF对应于4294967295
                log("1.16      没有权限信息: (权限已删除)")
            ElseIf cardNOOfPrivilegeToGet = 0 Then
                '没有权限时: (卡号部分为0)
                log("1.16       没有权限信息: (卡号部分为0)--此索引号之后没有权限了")
            Else
                '具体权限信息...
                log("1.16      有权限信息...")
            End If
            log("1.16 获取指定索引号的权限  成功...")
            success = 1
        End If
        '1.17	设置门控制参数(在线/延时) [功能号: 0x80] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H80
        '(设置2号门 在线  开门延时 3秒)
        pkt.data(0) = 2
        '2号门
        pkt.data(1) = 3
        '在线
        pkt.data(2) = 3
        '开门延时
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.data(0) = pkt.recv(8) AndAlso pkt.data(1) = pkt.recv(9) AndAlso pkt.data(2) = pkt.recv(10) Then
                '成功时, 返回值与设置一致
                log("1.17 设置门控制参数 成功...")
                success = 1
                '失败
            Else
            End If
        End If


        '1.21	权限按从小到大顺序添加[功能号: 0x56] 适用于权限数过1000, 少于8万 **********************************************************************************
        '此功能实现 完全更新全部权限, 用户不用清空之前的权限. 只是将上传的权限顺序从第1个依次到最后一个上传完成. 如果中途中断的话, 仍以原权限为主
        '建议权限数更新超过50个, 即可使用此指令
        log("1.21 权限按从小到大顺序添加[功能号: 0x56]开始...")
        log("       1万条权限...")
        '以10000个卡号为例, 此处简化的排序, 直接是以50001开始的10000个卡. 用户按照需要将要上传的卡号排序存放

        Dim cardCount As Integer = 10000
        '2015-06-09 20:20:20 卡总数量
        Dim cardArray As Long() = New Long(cardCount - 1) {}
        For i As Integer = 0 To cardCount - 1
            cardArray(i) = 50001 + i
        Next
        For i As Integer = 0 To cardCount - 1
            If bStopBasicFunction Then
                '2015-06-10 09:08:14 停止
                Return 0
            End If
            pkt.Reset()
            pkt.functionID = &H56
            cardNOOfPrivilege = cardArray(i)
            LongToBytes(pkt.data, 0, cardNOOfPrivilege)
            '其他参数简化时 统一, 可以依据每个卡的不同进行修改
            '20 10 01 01 起始日期:  2010年01月01日   (必须大于2001年)
            pkt.data(4) = 32
            pkt.data(5) = 16
            pkt.data(6) = 1
            pkt.data(7) = 1
            '20 29 12 31 截止日期:  2029年12月31日
            pkt.data(8) = 32
            pkt.data(9) = 41
            pkt.data(10) = 18
            pkt.data(11) = 49
            '01 允许通过 一号门 [对单门, 双门, 四门控制器有效] 
            pkt.data(12) = 1
            '01 允许通过 二号门 [对双门, 四门控制器有效]
            pkt.data(13) = 1
            '如果禁止2号门, 则只要设为 0x00
            '01 允许通过 三号门 [对四门控制器有效]
            pkt.data(14) = 1
            '01 允许通过 四号门 [对四门控制器有效]
            pkt.data(15) = 1
            LongToBytes(pkt.data, 32 - 8, cardCount)
            '总的权限数
            LongToBytes(pkt.data, 35 - 8, i + 1)
            '当前权限的索引位(从1开始)
            ret = pkt.run()
            success = 0
            If ret > 0 Then
                If pkt.recv(8) = 1 Then
                    success = 1
                End If
                If pkt.recv(8) = &HE1 Then
                    log("1.21权限按从小到大顺序添加[功能号: 0x56] =0xE1 表示卡号没有从小到大排序...???")
                    success = 0
                    Exit For
                End If
            Else
                Exit For
            End If
        Next
        If success = 1 Then
            log("1.21权限按从小到大顺序添加[功能号: 0x56] 成功...")
        Else
            log("1.21权限按从小到大顺序添加[功能号: 0x56] 失败...????")
        End If
        '其他指令  **********************************************************************************
        ' **********************************************************************************
        '结束  **********************************************************************************
        pkt.close()
        '关闭通信
        Return success
    End Function
    ''' <summary>
    ''' 接收服务器设置测试
    ''' </summary>
    ''' <param name="ControllerIP">被设置的控制器IP地址</param>
    ''' <param name="controllerSN">被设置的控制器序列号</param>
    ''' <param name="watchServerIP">要设置的服务器IP</param>
    ''' <param name="watchServerPort">要设置的端口</param>
    ''' <returns>0 失败, 1表示成功</returns>
    Private Function testWatchingServer(ByVal ControllerIP As String, ByVal controllerSN As Long, ByVal watchServerIP As String, ByVal watchServerPort As Integer) As Integer
        '接收服务器测试 -- 设置
        Dim ret As Integer = 0
        Dim success As Integer = 0
        '0 失败, 1表示成功
        Dim pkt As New WGPacketShort()
        pkt.iDevSn = controllerSN
        pkt.IP = ControllerIP
        '1.18	设置接收服务器的IP和端口 [功能号: 0x90] **********************************************************************************
        '(如果不想让控制器发出数据, 只要将接收服务器的IP设为0.0.0.0 就行了)
        '接收服务器的端口: 61005
        '每隔5秒发送一次: 05
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
        '接收服务器的端口: 61005
        pkt.data(4) = (((watchServerPort And 255)))
        pkt.data(5) = (((watchServerPort >> 8) And 255))
        '每隔5秒发送一次: 05 (定时上传信息的周期为5秒 [正常运行时每隔5秒发送一次  有刷卡时立即发送])
        pkt.data(6) = 5
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                log("1.18 设置接收服务器的IP和端口  成功...")
                success = 1
            End If
        End If
        '1.19	读取接收服务器的IP和端口 [功能号: 0x92] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &H92
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            log("1.19 读取接收服务器的IP和端口  成功...")
            success = 1
        End If
        pkt.close()
        Return success
    End Function
    ''' <summary>
    ''' 打开接收服务器接收数据 (注意防火墙 要允许此端口的所有包进入才行)
    ''' </summary>
    ''' <param name="watchServerIP">接收服务器IP(一般是当前电脑IP)</param>
    ''' <param name="watchServerPort">接收服务器端口</param>
    ''' <returns>1 表示成功,否则失败</returns>
    Private Function WatchingServerRuning(ByVal watchServerIP As String, ByVal watchServerPort As Integer) As Integer
        '注意防火墙 要允许此端口的所有包进入才行
        Try
            Dim udpserver As New WG3000_COMM.Core.wgUdpServerCom(watchServerIP, watchServerPort)
            If Not udpserver.IsWatching() Then
                log("进入接收服务器监控状态....失败")
                Return -1
            End If
            log("进入接收服务器监控状态....")
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
                        log(String.Format("接收到来自控制器SN = {0} 的数据包.." & Chr(13) & "" & Chr(10) & "", sn))
                        recordIndexGet = byteToLong(buff, 8, 4)
                        If recordIndex < recordIndexGet Then
                            recordIndex = recordIndexGet
                            displayRecordInformation(buff)
                        End If
                    End If
                Else
                    System.Threading.Thread.Sleep(10)
                    ''延时10ms
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

