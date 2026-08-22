'*
'* WGController32 2015-04-30 17:40:43 karl CSN  Chan Shonin $
'*
'* Doorbar controller Shortcast agreement Test cases
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
Imports System.Runtime.InteropServices


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
 
    End Sub
    Private Sub button2_Click(ByVal sender As Object, ByVal e As EventArgs) Handles button2.Click
        Dim ControllerIP As String = txtIP.Text
        Dim controllerSN As Long = Long.Parse(txtSN.Text)
        '1024Byte Command Test
        Dim ret As Integer = 0
        Dim success As Integer = 0
        '0 Failed, 1It means success.
        Dim command1024 As Byte() = New Byte(1024 - 1) {}
        'Create short message pkt
        Dim pkt As New WGPacketShort()
        pkt.iDevSn = controllerSN
        pkt.IP = ControllerIP
        '1.9	Extract Record Operation
        '1. Pass. 0xB0Command Fetch the earliest record index
        '2. Pass. 0xB0Command Get Last Record Index
        '3. Pass. 0xB4Command Get read record index numbers recordIndex
        '4. Pass. 0xB0Command Get a record of the given index number  FromrecordIndex + 1Start extracting records， Until the records are empty.
        '5. Pass. 0xB2Command Set a read record index number  Sets the value as the last read brush record index number
        'After three steps,， The entire extraction record is complete.
        Dim firstRecordIndex As Long = 0
        'First record index number
        Dim lastRecordIndex As Long = 0
        'Last record index number.
        Dim recordIndexGotToRead As Long = 0
        Dim recordIndexToGet As Long = 0
        log("1.9 Extract Record Operation" & Chr(9) & " Start...")
        pkt.Reset()
        pkt.functionID = &HB0
        'Take the earliest record index
        recordIndexToGet = 0
        LongToBytes(pkt.data, 0, recordIndexToGet)
        ret = pkt.run()
        success = 0
        If ret > 0 Then
            firstRecordIndex = byteToLong(pkt.recv, 8, 4)
            log(" Fetch the earliest record index" & Chr(9) & " =" + firstRecordIndex.ToString())
        End If
        If ret > 0 Then
            pkt.Reset()
            pkt.functionID = &HB0
            'Take Last Record Index
            recordIndexToGet = 4294967295
            LongToBytes(pkt.data, 0, recordIndexToGet)
            ret = pkt.run()
        End If
        If ret > 0 Then
            lastRecordIndex = byteToLong(pkt.recv, 8, 4)
            log(" Get Last Record Index" & Chr(9) & "  =" + lastRecordIndex.ToString())
        End If
        If ret > 0 Then
            pkt.Reset()
            pkt.functionID = &HB4
            'Get read record index numbers
            recordIndexToGet = 0
            LongToBytes(pkt.data, 0, recordIndexToGet)
            ret = pkt.run()
        End If
        If ret > 0 Then
            recordIndexGotToRead = byteToLong(pkt.recv, 8, 4)
            log("Get read record index numbers" & Chr(9) & "  =" + recordIndexGotToRead.ToString())
        End If
        Dim validRecordsCount As Long = 0
        'recordIndexGotToRead = 0;  //2015-11-05 21:31:05 Force all records
        If ret > 0 Then
            Dim recordIndexValidGet As Long = 0
            Dim recordIndexToGetStart As Long = recordIndexGotToRead + 1
            'Prepare record index to extract
            If recordIndexGotToRead > lastRecordIndex OrElse recordIndexGotToRead < firstRecordIndex Then
                'Beyond range Take index number for the first record
                recordIndexToGetStart = firstRecordIndex
            End If
            Dim recordIndexCurrent As Long
            Dim cnt As Integer = 0
            pkt.Reset()
            pkt.functionID = &HB0
            pkt.iDevSn = controllerSN
            Do
                Dim j As Integer = 0
                j = 0
                For j = 0 To 1023
                    'Restore
                    command1024(j) = 0
                Next
                recordIndexCurrent = recordIndexToGetStart
                j = 0
                While j < 1024
                    LongToBytes(pkt.data, 0, recordIndexToGetStart)
                    Dim cmd As Byte() = pkt.toByte()
                    Array.Copy(cmd, 0, command1024, j, 64)
                    recordIndexToGetStart += 1
                    cnt += 1
                    j = j + 64
                End While
                ret = pkt.run1024(command1024)
                success = 0
                If ret > 0 Then
                    j = 0
                    While j < 1024
                        success = 0
                        '12	Record type
                        '0=No record
                        '1=Brush Card Record
                        '2=Door Magnetic,button, Device startup, Remote Open Record
                        '3=Call the police.	1	
                        '0xFF=The record indicating the given index position has been overwritten.  Use the index.0, Retrieving index values from the earliest record
                        Dim recv As Byte() = New Byte(64) {}
                        Array.Copy(pkt.recv, j, recv, 0, 64)
                        Dim recordType As Integer = recv(12)
                        If recordType = 0 Then
                            success = 2
                            'No more records.
                            Exit While
                        End If
                        If recordType = 255 Then
                            'This index number is invalid
                            success = 0
                            Exit While
                        End If
                        success = 1
                        recordIndexValidGet = recordIndexCurrent
                        recordIndexCurrent += 1
                        validRecordsCount += 1
                        '
                        If validRecordsCount < 100 Then
                            '2015-11-05 14:59:20Show Before100individual, Too much shows slow processing. No analysis....
                            displayRecordInformation(recv)
                            '2015-06-09 20:01:21
                            If validRecordsCount = 99 Then
                                log(" To speed up extraction, Over100Shit.  Do not display recording information again.......")
                                Application.DoEvents()
                            End If
                            '.......Storage of records received
                            '*****
                            '###############
                        End If
                        j = j + 64
                    End While
                Else
                    'Ripping failed
                    Exit Do
                End If
                If success <> 1 Then
                    Exit Do
                End If
            Loop While cnt < 200000
            log("1.9 Full extraction successful." & Chr(9) & " Success... Number of valid records= " + validRecordsCount.ToString())
            If (success > 0) AndAlso validRecordsCount > 0 Then
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
                        log("1.9 Full extraction successful." & Chr(9) & " Success...")
                        success = 1
                    End If
                End If
            End If
        End If
        '1.21	Permissions added from childhood to larger[Function Number: 0x56] Applies to privileges1000, Less810,000 **********************************************************************************
        'This feature achieves Fully update all permissions, User does not have to empty permissions before. Just order the upload permissions from the first1In turn to last upload complete. If you interrupt., Still with the original authority.
        'Suggested number of privileges updated over50individual, Use this command
        'If the number of privileges exceeds8As soon as possible., If you interrupt., Permissions will be empty. That's why we have to upload the whole thing.
        log("1.21" & Chr(9) & "Permissions added from childhood to larger[Function Number: 0x56]" & Chr(9) & "Start...[Adopt1024Byte Command, Every upload16Permissions]")
        'Here.10000A card number is an example., Simplicit Sorting Here, Directly by50001Started.10000A card.. Store according to the number of card to be uploaded as required
        Dim cardCount As Integer = 1 * 10000
        ' 10000;  //2015-06-09 20:20:20 Total number of cards
        log(String.Format("       {0}Thousand powers...", cardCount / 10000))
        Dim cardArray As Long() = New Long(cardCount) {}
        Dim i As Integer
        For i = 0 To cardCount - 1
            cardArray(i) = 50001 + i
        Next
        Dim cardNOOfPrivilegeToGetlast As Long = 0
        Dim cardNOOfPrivilege As Long
        i = 0
        While i < cardCount
            Dim j As Integer = 0
            For j = 0 To 1023
                'Restore
                command1024(j) = 0
            Next
            j = 0
            While j < 1024
                If i >= cardCount Then
                    Exit While
                End If
                pkt.Reset()
                pkt.functionID = &H56
                cardNOOfPrivilege = cardArray(i)
                cardNOOfPrivilegeToGetlast = cardNOOfPrivilege
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
                Dim cmd As Byte() = pkt.toByte()
                Array.Copy(cmd, 0, command1024, j, 64)
                i += 1
                j = j + 64
            End While
            ret = pkt.run1024(command1024)
            '2015-11-04 19:15:48 
            success = 0
            If ret > 0 Then
                If pkt.recv(8) = 1 Then
                    success = 1
                End If
                If pkt.recv(8) = 225 Then
                    log("1.21" & Chr(9) & "Permissions added from childhood to larger[Function Number: 0x56]" & Chr(9) & " =0xE1 Which means the card number has not been sorted from a small to a large size....???")
                    success = 0
                    Exit While
                End If
            Else
                Exit While
            End If
        End While
        If success = 1 Then
            log("1.21" & Chr(9) & "Permissions added from childhood to larger[Function Number: 0x56]" & Chr(9) & " Success...")
        Else
            log("1.21" & Chr(9) & "Permissions added from childhood to larger[Function Number: 0x56]" & Chr(9) & " Failed...????")
        End If

        'The following section Read Operations for Permissions 
        '1.16  Access to specified index numbers[Function Number: 0x5C] **********************************************************************************
        'Read All Permissions
        pkt.Reset()
        pkt.functionID = &H5C
        pkt.iDevSn = controllerSN
        Dim maxCount As Long = 20 * 10000
        Dim cardArrayGet As Long() = New Long(maxCount) {}
        Dim QueryIndex As Long = 1
        'Index number(From1Start);
        LongToBytes(pkt.data, 0, QueryIndex)
        For i = 0 To maxCount - 1
            cardArrayGet(i) = 0
        Next
        log("Read All Permissions" & Chr(9) & " Start...[1024Byte Command]")
        Dim iCount As Long = 0
        For i = 0 To maxCount - 1
            Dim j As Integer = 0
            For j = 0 To 1023
                'Restore
                command1024(j) = 0
            Next
            j = 0
            While j < 1024
                LongToBytes(pkt.data, 0, QueryIndex)
                QueryIndex += 1
                'Index number(From1Start);
                Dim cmd As Byte() = pkt.toByte()
                Array.Copy(cmd, 0, command1024, j, 64)
                j = j + 64
            End While
            ret = pkt.run1024(command1024)
            success = 0
            If ret > 0 Then
                j = 0
                While j < 1024
                    success = 0
                    Dim cardNOOfPrivilegeToGet As Long = 0
                    cardNOOfPrivilegeToGet = byteToLong(pkt.recv, 8 + j, 4)
                    If &HFFFFFFFF = cardNOOfPrivilegeToGet Then
                        'FFFFFFFFResponse4294967295
                        'log("1.16      Can not open message: (Permissions deleted)");
                        'break;
                        success = 1
                    ElseIf cardNOOfPrivilegeToGet = 0 Then
                        'When no permission: (The card number is0)
                        'log("1.16       Can not open message: (The card number is0)--This index number is no longer valid.");
                        Exit While
                    Else
                        'Specific Permission Information...
                        '  log("1.16      Can not open message...");
                        ' log("1.16 Access to specified index numbers	 Success...");
                        cardArrayGet(iCount) = cardNOOfPrivilegeToGet
                        iCount += 1
                        success = 1
                        cardNOOfPrivilegeToGetlast = cardNOOfPrivilegeToGet
                    End If
                    j = j + 64
                End While
                If success = 0 Then
                    Exit For
                End If
            Else
                log("1.16     Problem...." + ret.ToString())
                Exit For
            End If
        Next
        log("Last read permission card number = " + cardNOOfPrivilegeToGetlast.ToString())
        log("Permissions extractediCount = " + iCount.ToString())
        '2015-11-04 19:59:50 Extract permissions
        log("1.16 Access to specified index numbers" & Chr(9) & " Success...")
        '**********************************************************************************
        'End  **********************************************************************************
        pkt.close()
        'Close communications
    End Sub
 
    ''' <summary>
    ''' Shortcasts
    ''' </summary>
    Class WGPacketShort
        Public Shared WGPacketSize As Integer = 64
        'Length of submission
        '2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
        Public Shared Type As Integer = 23
        '2015-04-29 22:22:50			//Type
        Public Shared ControllerPort As Integer = 60000
        'controller port
        Public Shared SpecialFlag As Long = 1437248085
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

        <DllImport("n3kWGCom.dll", EntryPoint:="ShortEncrypt", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)> _
        Private Shared Function ShortEncrypt(ByVal ptrCommand As IntPtr, ByVal ptrPassword As IntPtr) As Integer
        End Function
        <DllImport("n3kWGCom.dll", EntryPoint:="ShortDecrypt", CharSet:=CharSet.Auto, CallingConvention:=CallingConvention.StdCall)> _
        Public Shared Function ShortDecrypt(ByVal ptrCommand As IntPtr, ByVal ptrPassword As IntPtr) As Integer
        End Function
        Public Shared Function Encrypt(ByRef data As Byte(), ByVal key As Byte()) As Integer 'Encryption
            Dim pkt As IntPtr = Marshal.AllocHGlobal(64)
            Marshal.Copy(data, 0, pkt, 64)
            Dim commPassword As IntPtr = Marshal.AllocHGlobal(16)
            Marshal.Copy(key, 0, commPassword, 16)
            Dim ret As Integer = ShortEncrypt(pkt, commPassword)
            If ret > 0 Then
                Marshal.Copy(pkt, data, 0, 64)
            End If
            Marshal.FreeHGlobal(pkt)
            Marshal.FreeHGlobal(commPassword)
            Return ret
        End Function

        Public Shared Function Decrypt(ByRef data As Byte(), ByVal key As Byte()) As Integer
            '2015-09-28 15:12:12  Decrypt Data
            Dim pkt As IntPtr = Marshal.AllocHGlobal(64)
            Marshal.Copy(data, 0, pkt, 64)
            Dim commPassword As IntPtr = Marshal.AllocHGlobal(16)
            Marshal.Copy(key, 0, commPassword, 16)
            Dim ret As Integer = ShortDecrypt(pkt, commPassword)
            If ret > 0 Then
                 Marshal.Copy(pkt, data, 0, 64)
            End If
            Marshal.FreeHGlobal(pkt)
            Marshal.FreeHGlobal(commPassword)
            Return ret
        End Function

        Public Function run(ByVal commPassword As Byte()) As Integer
            '2015-10-28 10:16:38 Message-processing dispatch instructions Can not open message
            Dim buff As Byte() = toByte()
            Encrypt(buff, commPassword)
            Dim tries As Integer = 3
            Dim errcnt As Integer = 0
            controller.IP = IP
            controller.PORT = ControllerPort
            Do
                If controller.ShortPacketSend(buff, recv) < 0 Then
                    '2015-11-03 20:26:52 Enter Retry Return -1
                Else
                    Debug.WriteLine(System.BitConverter.ToString(recv))
                    If ((recv(0) And &H7F) = 23) Then
                        Decrypt(recv, commPassword)
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
                    Else
                        errcnt += 1
                    End If
                End If
            Loop While System.Math.Max(System.Threading.Interlocked.Decrement(tries), tries + 1) > 0
            'Try again three times.
            Return -1
        End Function


        Public Function run1024(ByVal buff As Byte()) As Integer
            '2015-11-05 14:50:45 1024Byte Command Send Command Can not open message 
            Return run1024(buff, Nothing)
        End Function

        'commPassword Do not use passwords when empty. The password must be16Bytes
        Public Function run1024(ByVal buff As Byte(), ByVal commPassword As Byte()) As Integer
            '2015-11-05 14:50:45 1024Byte Command Encrypted communications component Send Command Can not open message

            Dim sequenceIdSend As Long 'Send Serial Number
            For i As Integer = 0 To 3
                Dim lng As Long = buff(40 + i)
                sequenceIdSend += (lng << (8 * i))
            Next
            If commPassword IsNot Nothing Then
                'If Encryption
                Dim buffBk As Byte() = New Byte(64) {}
                Dim i As Integer
                i = 0
                While i < 1024
                    Array.Copy(buff, i, buffBk, 0, 64)
                    Encrypt(buffBk, commPassword)
                    Array.Copy(buffBk, 0, buff, i, 64)
                    i = i + 64
                End While
            End If
            Dim tries As Integer = 3
            Dim errcnt As Integer = 0
            controller.IP = IP
            controller.PORT = ControllerPort
            Do
                '2015-11-03 20:26:52 Enter Retry return -1;
                If controller.ShortPacketSend(buff, recv) < 0 Then
                Else
                    If (recv(0) And 127) = Type Then
                        If (commPassword IsNot Nothing) AndAlso recv.Length = 1024 Then
                            'If it's encrypted,
                            Dim buffBk As Byte() = New Byte(64) {}
                            Dim i As Integer = 0
                            While i < 1024
                                Array.Copy(recv, i, buffBk, 0, 64)
                                Decrypt(buffBk, commPassword)
                                Array.Copy(buffBk, 0, recv, i, 64)
                                i = i + 64
                            End While
                        End If
                    End If
                    'Water Stream
                    Dim sequenceIdReceived As Long = 0
                    For i As Integer = 0 To 3
                        Dim lng As Long = recv(40 + i)
                        sequenceIdReceived += (lng << (8 * i))
                    Next
                    If (recv(0) = Type) AndAlso (recv(1) = functionID) AndAlso (sequenceIdReceived = sequenceIdSend) Then
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

        'Set Communications Password[Function Number: 0xF0] **********************************************************************************
        Dim commPassword As Byte() = {&H11, &H22, &H33, &H44, &H55, &H66, &H77, &H88, &H99, &HAA, &HBB, &HCC, &HDD, &HEE, &HFF, &H0} '16Byte Password
        pkt.Reset()
        pkt.functionID = &HF0
        'Ideas against error
        LongToBytes(pkt.data, 0, WGPacketShort.SpecialFlag)
        For i As Integer = 0 To 15
            pkt.data(4 + i) = commPassword(i)
            pkt.data(36 + i) = commPassword(i)
        Next

        ret = pkt.run()
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                log("Communication password set successfully...")
                success = 1
            End If
        End If
        If success = 0 Then
            ret = pkt.run(commPassword)
            If (ret > 0) AndAlso (pkt.recv(8) = 1) Then
                log("Communication password set successfully...[Can not open message]")
                success = 1
            Else
                'return 0;
                log("Communication password setup failed...[Can not open message]")
            End If
        End If
        'Communication using coded communications

        '1.10	Open remote[Function Number: 0x40] **********************************************************************************
        Dim doorNO As Integer = 1
        pkt.Reset()
        pkt.functionID = &H40
        pkt.data(0) = (doorNO And 255)
        ret = pkt.run(commPassword)
        success = 0
        If ret > 0 Then
            If pkt.recv(8) = 1 Then
                'Open the door effectively......
                log("1.10 Open remote" & Chr(9) & " Success...[Can not open message]")
                success = 1
            End If
        Else
            log("1.10 Open remote" & Chr(9) & " Failed...[Can not open message]")
        End If

        'Clear the code.[Function Number: 0xF0] **********************************************************************************
        pkt.Reset()
        pkt.functionID = &HF0
        'Ideas against error
        LongToBytes(pkt.data, 0, WGPacketShort.SpecialFlag)
        For i As Integer = 0 To 15 'Empty Password
            pkt.data(4 + i) = 0
            pkt.data(36 + i) = 0
        Next

        ret = pkt.run(commPassword)
        If (ret > 0) AndAlso (pkt.recv(8) = 1) Then
            log("Communication code emptied....[Can not open message]")
            success = 1
        Else
            log("Communication password emptied...[Can not open message]")
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

