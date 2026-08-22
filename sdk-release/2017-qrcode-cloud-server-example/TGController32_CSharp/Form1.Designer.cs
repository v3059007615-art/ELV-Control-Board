namespace WGController32_CSharp
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtSN = new System.Windows.Forms.TextBox();
            this.txtIP = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtWatchServerPort = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtWatchServerIP = new System.Windows.Forms.TextBox();
            this.button4 = new System.Windows.Forms.Button();
            this.btnGetController = new System.Windows.Forms.Button();
            this.btnRemoteOpenDoor1 = new System.Windows.Forms.Button();
            this.btnQR1 = new System.Windows.Forms.Button();
            this.btnQR2 = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.btnQRRestore = new System.Windows.Forms.Button();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.btnGetDriverVersion = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(44, 91);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(173, 71);
            this.button1.TabIndex = 2;
            this.button1.Text = "1. Test Basic Function\r\n1.  Test Basic Functions\r\n    (Question/At school./Permissions/Records)";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInfo.Location = new System.Drawing.Point(44, 193);
            this.txtInfo.Multiline = true;
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtInfo.Size = new System.Drawing.Size(1022, 525);
            this.txtInfo.TabIndex = 7;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(286, 31);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "Controller SN\r\n Control serial number";
            // 
            // txtSN
            // 
            this.txtSN.Location = new System.Drawing.Point(390, 28);
            this.txtSN.Name = "txtSN";
            this.txtSN.Size = new System.Drawing.Size(100, 21);
            this.txtSN.TabIndex = 1;
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(390, 63);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(100, 21);
            this.txtIP.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(286, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 24);
            this.label2.TabIndex = 4;
            this.label2.Text = "Controller IP\r\ncontrollerIPAddress";
            // 
            // txtWatchServerPort
            // 
            this.txtWatchServerPort.Location = new System.Drawing.Point(390, 141);
            this.txtWatchServerPort.Name = "txtWatchServerPort";
            this.txtWatchServerPort.Size = new System.Drawing.Size(50, 21);
            this.txtWatchServerPort.TabIndex = 4;
            this.txtWatchServerPort.Text = "61005";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(277, 138);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 24);
            this.label3.TabIndex = 6;
            this.label3.Text = "Watch Server Port\r\nReceive server port number";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(515, 135);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(216, 23);
            this.button2.TabIndex = 5;
            this.button2.Text = "Stop Stop surveillance or testing";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(515, 28);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(216, 49);
            this.button3.TabIndex = 3;
            this.button3.Text = "Search Controller\r\nSearch controller Change controller'sIPConfigure";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(277, 105);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 24);
            this.label5.TabIndex = 4;
            this.label5.Text = "Watch Server IP\r\nReceiving ServersIPAddress";
            // 
            // txtWatchServerIP
            // 
            this.txtWatchServerIP.Location = new System.Drawing.Point(390, 102);
            this.txtWatchServerIP.Name = "txtWatchServerIP";
            this.txtWatchServerIP.Size = new System.Drawing.Size(100, 21);
            this.txtWatchServerIP.TabIndex = 3;
            this.txtWatchServerIP.Text = "192.168.168.101";
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(515, 102);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(216, 23);
            this.button4.TabIndex = 4;
            this.button4.Text = "Only Watch Only open receiving server";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // btnGetController
            // 
            this.btnGetController.Location = new System.Drawing.Point(27, 5);
            this.btnGetController.Name = "btnGetController";
            this.btnGetController.Size = new System.Drawing.Size(223, 23);
            this.btnGetController.TabIndex = 0;
            this.btnGetController.Text = "Get controllerSNandIP  [LAN only]";
            this.btnGetController.UseVisualStyleBackColor = true;
            this.btnGetController.Click += new System.EventHandler(this.btnGetController_Click);
            // 
            // btnRemoteOpenDoor1
            // 
            this.btnRemoteOpenDoor1.Location = new System.Drawing.Point(44, 34);
            this.btnRemoteOpenDoor1.Name = "btnRemoteOpenDoor1";
            this.btnRemoteOpenDoor1.Size = new System.Drawing.Size(173, 23);
            this.btnRemoteOpenDoor1.TabIndex = 1;
            this.btnRemoteOpenDoor1.Text = "Remotely open1Door.";
            this.btnRemoteOpenDoor1.UseVisualStyleBackColor = true;
            this.btnRemoteOpenDoor1.Click += new System.EventHandler(this.btnRemoteOpenDoor1_Click);
            // 
            // btnQR1
            // 
            this.btnQR1.Location = new System.Drawing.Point(15, 37);
            this.btnQR1.Name = "btnQR1";
            this.btnQR1.Size = new System.Drawing.Size(194, 23);
            this.btnQR1.TabIndex = 8;
            this.btnQR1.Text = "Serial1 Simulation1Door in. Passage";
            this.btnQR1.UseVisualStyleBackColor = true;
            this.btnQR1.Click += new System.EventHandler(this.btnQRFunction_Click);
            // 
            // btnQR2
            // 
            this.btnQR2.Location = new System.Drawing.Point(15, 66);
            this.btnQR2.Name = "btnQR2";
            this.btnQR2.Size = new System.Drawing.Size(194, 23);
            this.btnQR2.TabIndex = 9;
            this.btnQR2.Text = "Serial2 Simulation2Door No. Passage";
            this.btnQR2.UseVisualStyleBackColor = true;
            this.btnQR2.Click += new System.EventHandler(this.btnQRFunction_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(775, 28);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(272, 147);
            this.tabControl1.TabIndex = 10;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.btnQRRestore);
            this.tabPage1.Controls.Add(this.btnQR1);
            this.tabPage1.Controls.Add(this.btnQR2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(264, 121);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "QR Two-dimensional code.";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 12);
            this.label4.TabIndex = 11;
            this.label4.Text = "Take two-door two-way controller, for example.";
            // 
            // btnQRRestore
            // 
            this.btnQRRestore.Location = new System.Drawing.Point(15, 94);
            this.btnQRRestore.Name = "btnQRRestore";
            this.btnQRRestore.Size = new System.Drawing.Size(194, 23);
            this.btnQRRestore.TabIndex = 10;
            this.btnQRRestore.Text = "Unstring All";
            this.btnQRRestore.UseVisualStyleBackColor = true;
            this.btnQRRestore.Click += new System.EventHandler(this.btnQRFunction_Click);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(264, 121);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // btnGetDriverVersion
            // 
            this.btnGetDriverVersion.Location = new System.Drawing.Point(44, 63);
            this.btnGetDriverVersion.Name = "btnGetDriverVersion";
            this.btnGetDriverVersion.Size = new System.Drawing.Size(173, 23);
            this.btnGetDriverVersion.TabIndex = 11;
            this.btnGetDriverVersion.Text = "Get controller Driver Version";
            this.btnGetDriverVersion.UseVisualStyleBackColor = true;
            this.btnGetDriverVersion.Click += new System.EventHandler(this.btnGetDriverVersion_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1124, 730);
            this.Controls.Add(this.btnGetDriverVersion);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnRemoteOpenDoor1);
            this.Controls.Add(this.btnGetController);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.txtWatchServerPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtWatchServerIP);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtSN);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1-C# V3.6.3";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox txtInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtSN;
        private System.Windows.Forms.TextBox txtIP;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtWatchServerPort;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtWatchServerIP;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button btnGetController;
        private System.Windows.Forms.Button btnRemoteOpenDoor1;
        private System.Windows.Forms.Button btnQR1;
        private System.Windows.Forms.Button btnQR2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button btnQRRestore;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnGetDriverVersion;
    }
}