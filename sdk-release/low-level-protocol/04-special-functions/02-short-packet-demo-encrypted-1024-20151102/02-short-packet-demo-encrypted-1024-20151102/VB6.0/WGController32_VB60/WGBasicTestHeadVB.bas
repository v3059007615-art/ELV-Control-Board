Attribute VB_Name = "Module1"
'2014-09-20 18:43:19 NewWG VBCases
'Functions to be called to consider delay , It's also available for timing.
Public Declare Function GetTickCount Lib "kernel32" () As Long
Public Declare Sub Sleep Lib "kernel32" (ByVal dwMilliseconds As Long)

'2015-11-02 14:38:02 Increase Fax code
Public Declare Function ShortEncrypt Lib "n3kWGCom.dll" (ByVal ptrCommand As Long, ByVal ptrPassword As Long) As Long 'Encryption
Public Declare Function ShortDecrypt Lib "n3kWGCom.dll" (ByVal ptrCommand As Long, ByVal ptrPassword As Long) As Long 'Decrypt

'Sub timeDelay(ByVal DTms As Long)   'Here.msUnit
'    Dim T As Long
'    T = GetTickCount()
'    Do
'        DoEvents()
'    Loop Until GetTickCount - T >= DT
'End Sub


'Other Organiser0
Public Function arrayReset(ByRef arrbyte() As Byte, ByVal length As Integer) As Integer
    Dim i As Integer
    For i = 0 To length - 1
        arrbyte(i) = 0
    Next i
    arrayReset = 1
End Function

'Integer to Bytes (4Bytes)
Public Function IntToByte(ByVal value As Long, ByRef arrbyte() As Byte, ByVal start As Integer, ByVal length As Integer)
    Dim i As Integer
    Dim val As Long
    Dim validLen As Integer
    validLen = length
    val = value
    For i = 0 To validLen - 1
        If (value = &HFFFFFFFF) Then
            arrbyte(i + start) = &HFF
        Else
            arrbyte(i + start) = val Mod 256&
            val = (val - (val Mod 256&)) / 256&
        End If
    Next i
    IntToByte = 1
End Function

'Integer to Bytes (8Bytes)
Public Function DoubleToByte(ByVal value As Double, ByRef arrbyte() As Byte, ByVal start As Integer, ByVal length As Integer)
    Dim i As Integer
    Dim val As Double
    Dim validLen As Integer
    validLen = length
    val = value
    For i = 0 To validLen - 1
        If (value = &HFFFFFFFF) Then
            arrbyte(i + start) = &HFF
        Else
            arrbyte(i + start) = val Mod 256&
            val = (val - (val Mod 256&)) / 256&
        End If
    Next i
    DoubleToByte = 1
End Function

'Byte to Integer(4Bytes)
Public Function ByteToLong(ByRef arrbyte() As Byte, ByVal start As Integer, ByVal length As Integer) As Long
    Dim i As Integer
    Dim val As Long
    Dim validLen As Integer
    validLen = length
    val = 0
    For i = validLen - 1 To 0 Step -1
        val = val * 256&
        val = val + arrbyte(i + start)
    Next i
    ByteToLong = val
End Function

'Byte to Integer(8Bytes)
Public Function ByteToDouble(ByRef arrbyte() As Byte, ByVal start As Integer, ByVal length As Integer) As Double
    Dim i As Integer
    Dim val As Double
    Dim validLen As Integer
    validLen = length
    val = 0
    For i = validLen - 1 To 0 Step -1
        val = val * 256&
        val = val + arrbyte(i + start)
    Next i
    ByteToDouble = val
End Function

Public Function GetFromBCD(ByVal val As Integer) As Byte 'AccessHexValue, Mainly used in date time format
    GetFromBCD = ((val - (val Mod 16)) / 16) * 10 + (val Mod 16)
End Function


'Convert controller time format toWINDOWS Long Format
Public Function getMsDate(ByVal yearH, ByVal yearL, ByVal month, ByVal day, ByVal hour, ByVal minute, ByVal second) As Date
    Dim i As Long, strTime As String
    i = GetFromBCD(yearH)
    i = i * 100
    i = i + GetFromBCD(yearL)
    strTime = Trim(Str$(i)) & "-"    'Year
    i = GetFromBCD(month)
    strTime = strTime & Trim(Str$(i)) & "-"       'Month
    i = GetFromBCD(day)
    strTime = strTime & Trim(Str$(i)) & " "       'Day
    i = GetFromBCD(hour)
    strTime = strTime & Trim(Str$(i)) & ":"       'Time
    i = GetFromBCD(minute)
    strTime = strTime & Trim(Str$(i)) & ":"       'min
    i = GetFromBCD(second)
    strTime = strTime & Trim(Str$(i))        'sec
    If Not IsDate(strTime) Then strTime = "2000-1-1 0:0:0" 'If not time format,By default2000Year1Month1Day 0Time0min0sec Granted
    getMsDate = strTime
End Function

'AccessBCDValue, Mainly used in date time format
Public Function GetHex(ByVal val As Integer) As Byte
    GetHex = ((val Mod 10) + (((val - (val Mod 10)) / 10) Mod 10) * 16)
End Function
