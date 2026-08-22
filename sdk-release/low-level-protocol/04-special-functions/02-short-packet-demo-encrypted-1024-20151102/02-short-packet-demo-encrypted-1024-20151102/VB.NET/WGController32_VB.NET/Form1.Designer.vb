<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form 重写 Dispose，以清理组件列表。
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

    'Windows 窗体设计器所必需的
    Private components As System.ComponentModel.IContainer

    '注意: 以下过程是 Windows 窗体设计器所必需的
    '可以使用 Windows 窗体设计器修改它。
    '不要使用代码编辑器修改它。
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.txtIP = New System.Windows.Forms.TextBox()
        Me.label2 = New System.Windows.Forms.Label()
        Me.txtSN = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.txtInfo = New System.Windows.Forms.TextBox()
        Me.button1 = New System.Windows.Forms.Button()
        Me.button2 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
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
        Me.button1.Size = New System.Drawing.Size(222, 23)
        Me.button1.TabIndex = 0
        Me.button1.Text = "1. Test Encrypt Function(加密通信)"
        Me.button1.UseVisualStyleBackColor = True
        '
        'button2
        '
        Me.button2.Location = New System.Drawing.Point(29, 67)
        Me.button2.Name = "button2"
        Me.button2.Size = New System.Drawing.Size(470, 23)
        Me.button2.TabIndex = 15
        Me.button2.Text = "2 1024-Bytes Command (1024字节指令实现 提取记录 上传权限 读取权限)"
        Me.button2.UseVisualStyleBackColor = True
        '
        'Form1
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(627, 692)
        Me.Controls.Add(Me.button2)
        Me.Controls.Add(Me.txtIP)
        Me.Controls.Add(Me.label2)
        Me.Controls.Add(Me.txtSN)
        Me.Controls.Add(Me.label1)
        Me.Controls.Add(Me.txtInfo)
        Me.Controls.Add(Me.button1)
        Me.Name = "Form1"
        Me.Text = "Form1-VB.NET V2.6.3"
        Me.ResumeLayout(false)
        Me.PerformLayout

End Sub
    Private WithEvents txtIP As System.Windows.Forms.TextBox
    Private WithEvents label2 As System.Windows.Forms.Label
    Private WithEvents txtSN As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents txtInfo As System.Windows.Forms.TextBox
    Private WithEvents button1 As System.Windows.Forms.Button
    Private WithEvents button2 As System.Windows.Forms.Button

End Class
