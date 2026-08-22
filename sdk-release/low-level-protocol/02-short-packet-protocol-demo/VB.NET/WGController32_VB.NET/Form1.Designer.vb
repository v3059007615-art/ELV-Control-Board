<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form Rewrite Dispose，To clear the list of components。
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Windows Form designer required
    Private components As System.ComponentModel.IContainer

    'Attention.: The following process Windows Form designer required
    'Available Windows The form designer modified it.。
    'Do not modify it with the code editor。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.button3 = New System.Windows.Forms.Button()
        Me.button2 = New System.Windows.Forms.Button()
        Me.txtWatchServerPort = New System.Windows.Forms.TextBox()
        Me.label3 = New System.Windows.Forms.Label()
        Me.txtIP = New System.Windows.Forms.TextBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.txtSN = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.txtInfo = New System.Windows.Forms.TextBox()
        Me.button1 = New System.Windows.Forms.Button()
        Me.txtWatchServerIP = New System.Windows.Forms.TextBox()
        Me.label5 = New System.Windows.Forms.Label()
        Me.Button4 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'button3
        '
        Me.button3.Location = New System.Drawing.Point(497, 12)
        Me.button3.Name = "button3"
        Me.button3.Size = New System.Drawing.Size(129, 23)
        Me.button3.TabIndex = 5
        Me.button3.Text = "Search Controller"
        Me.button3.UseVisualStyleBackColor = True
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(497, 87)
        Me.button2.Name = "button2"
        Me.button2.Size = New System.Drawing.Size(75, 23)
        Me.button2.TabIndex = 6
        Me.button2.Text = "Stop"
        Me.button2.UseVisualStyleBackColor = True
        '
        'txtWatchServerPort
        '
        Me.txtWatchServerPort.Location = New System.Drawing.Point(408, 87)
        Me.txtWatchServerPort.Name = "txtWatchServerPort"
        Me.txtWatchServerPort.Size = New System.Drawing.Size(50, 21)
        Me.txtWatchServerPort.TabIndex = 4
        Me.txtWatchServerPort.Text = "61005"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Location = New System.Drawing.Point(295, 90)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(107, 12)
        Me.label3.TabIndex = 16
        Me.label3.Text = "Watch Server Port"
        '
        'txtIP
        '
        Me.txtIP.Location = New System.Drawing.Point(358, 31)
        Me.txtIP.Name = "txtIP"
        Me.txtIP.Size = New System.Drawing.Size(100, 21)
        Me.txtIP.TabIndex = 2
        Me.txtIP.Text = "192.168.168.123"
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(269, 34)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(83, 12)
        Me.label2.TabIndex = 14
        Me.label2.Text = "Controller IP"
        '
        'txtSN
        '
        Me.txtSN.Location = New System.Drawing.Point(358, 4)
        Me.txtSN.Name = "txtSN"
        Me.txtSN.Size = New System.Drawing.Size(100, 21)
        Me.txtSN.TabIndex = 1
        Me.txtSN.Text = "229999901"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(269, 7)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(83, 12)
        Me.label1.TabIndex = 12
        Me.label1.Text = "Controller SN"
        '
        'txtInfo
        '
        Me.txtInfo.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.txtInfo.Location = New System.Drawing.Point(29, 114)
        Me.txtInfo.Multiline = True
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both
        Me.txtInfo.Size = New System.Drawing.Size(559, 553)
        Me.txtInfo.TabIndex = 7
        '
        'button1
        '
        Me.button1.Location = New System.Drawing.Point(29, 12)
        Me.button1.Name = "button1"
        Me.button1.Size = New System.Drawing.Size(173, 23)
        Me.button1.TabIndex = 0
        Me.button1.Text = "1. Test Basic Function"
        Me.button1.UseVisualStyleBackColor = True
        '
        'txtWatchServerIP
        '
        Me.txtWatchServerIP.Location = New System.Drawing.Point(358, 58)
        Me.txtWatchServerIP.Name = "txtWatchServerIP"
        Me.txtWatchServerIP.Size = New System.Drawing.Size(100, 21)
        Me.txtWatchServerIP.TabIndex = 3
        Me.txtWatchServerIP.Text = "192.168.168.101"
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Location = New System.Drawing.Point(257, 61)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(95, 12)
        Me.label5.TabIndex = 21
        Me.label5.Text = "Watch Server IP"
        '
        'Button4
        '
        Me.Button4.Location = New System.Drawing.Point(497, 56)
        Me.Button4.Name = "Button4"
        Me.Button4.Size = New System.Drawing.Size(75, 23)
        Me.Button4.TabIndex = 22
        Me.Button4.Text = "Only Watch"
        Me.Button4.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(627, 692)
        Me.Controls.Add(Me.Button4)
        Me.Controls.Add(Me.txtWatchServerIP)
        Me.Controls.Add(Me.label5)
        Me.Controls.Add(Me.button3)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.txtWatchServerPort)
        Me.Controls.Add(Me.label3)
        Me.Controls.Add(Me.txtIP)
        Me.Controls.Add(Me.label2)
        Me.Controls.Add(Me.txtSN)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.txtInfo)
        Me.Controls.Add(Me.button1)
        Me.Name = "Form1"
        Me.Text = "Form1-VB.NET V2.6.2"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents button3 As System.Windows.Forms.Button
    Private WithEvents button2 As System.Windows.Forms.Button
    Private WithEvents txtWatchServerPort As System.Windows.Forms.TextBox
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents txtIP As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents txtSN As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents txtInfo As System.Windows.Forms.TextBox
    Private WithEvents button1 As System.Windows.Forms.Button
    Private WithEvents txtWatchServerIP As System.Windows.Forms.TextBox
    Private WithEvents label5 As System.Windows.Forms.Label
    Friend WithEvents Button4 As System.Windows.Forms.Button

End Class
