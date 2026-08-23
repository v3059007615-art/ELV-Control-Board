using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using WG3000_COMM.Core;
using System.Net;

namespace WGController32_CSharp
{
    public partial class dfrmTCPIPConfigure : Form
    {
        public dfrmTCPIPConfigure()
        {
            InitializeComponent();
        }

        public string strSN = "";
        public string strMac = "";
        public string strIP = "";
        public string strMask = "";
        public string strGateway = "";
        public string strPCAddr = "";



        //            'EnteredIPFirst byte is not required00, The last byte is not allowed255
        public Boolean isIPAddress(string ipstr)
        {
            Boolean ret = false;
            try
            {
                if (string.IsNullOrEmpty(ipstr))
                {
                }
                else
                {

                    string[] strIPInput = ipstr.Split('.');
                    if (strIPInput.Length == 4)
                    {
                        int itemp;
                        ret = true;
                        for (int i = 0; i <= 3; i++)
                        {
                            //'Value0Present.255
                            if (!int.TryParse(strIPInput[i], out itemp))
                            {
                                ret = false;

                                break;
                            }

                            if (!((itemp >= 0) && (itemp <= 255)))
                            {
                                ret = false;
                                break;
                            }
                        }
                        if (int.Parse(strIPInput[0]) == 0) // 'The first value cannot be0 
                        {
                            ret = false;

                        }
                        else if (int.Parse(strIPInput[3]) == 255) //The last value cannot be255 
                        {
                            ret = false;

                        }
                    }
                }
            }
            catch
            {
                ret = false;
            }
            finally
            {
            }
            return ret;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            //New Value
            int itemp;
            if (this.txtf_ControllerSN.ReadOnly == false)
            {
                this.txtf_ControllerSN.Text = this.txtf_ControllerSN.Text.Trim();
                if (!int.TryParse(this.txtf_ControllerSN.Text, out itemp))
                {
                    MessageBox.Show("Controller SN  Wrong");
                    return;
                }

            }

            this.txtf_IP.Text = this.txtf_IP.Text.Replace(" ", ""); // 'Exclude Space
            if (!isIPAddress(this.txtf_IP.Text))
            {
                MessageBox.Show("IP  Wrong");
                return;
            }

            this.txtf_mask.Text = this.txtf_mask.Text.Replace(" ", ""); // 'Exclude Space
            if (!isIPAddress(this.txtf_mask.Text))
            {
                MessageBox.Show("mask  Wrong");
                return;
            }

            this.txtf_gateway.Text = this.txtf_gateway.Text.Replace(" ", ""); // 'Exclude Space
            if (!string.IsNullOrEmpty(this.txtf_gateway.Text))
            {
                if (!isIPAddress(this.txtf_gateway.Text))
                {
                    MessageBox.Show("gateway  Wrong");
                    return;
                }
            }

            this.txtHostIP.Text = this.txtHostIP.Text.Replace(" ", ""); // 'Exclude Space
            if (!string.IsNullOrEmpty(this.txtHostIP.Text))
            {
                if (!isIPAddress(this.txtHostIP.Text))
                {
                    MessageBox.Show("HostIP  Wrong");
                    return;
                }
            }

            strSN = this.txtf_ControllerSN.Text;
            strMac = this.txtf_MACAddr.Text;
            strIP = this.txtf_IP.Text;
            strMask = this.txtf_mask.Text;
            strGateway = this.txtf_gateway.Text;

            //ModifyIP
            int ret = 0;
            //Create short message pkt

            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = long.Parse(strSN); // controllerSN;
            //Set up the receiver server
            if (this.chkEditDateServer.Checked)
            {
                pkt.Reset();
                pkt.functionID = 0x90;
                if (!string.IsNullOrEmpty(this.txtHostIP.Text))
                {
                    IPAddress adr = IPAddress.Parse(this.txtHostIP.Text);
                    Array.Copy(adr.GetAddressBytes(), 0, pkt.data, 0, 4);  //New ReceiverIP
                }
                int port = int.Parse(this.txtPortShort.Text);
                pkt.data[4] = (byte)(port & 0xff);         //Newport
                pkt.data[5] = (byte)((port >> 8) & 0xff);

                pkt.data[6] = (byte)(this.nudCycle.Value); // Newcycle
                ret = pkt.run(strPCAddr);
                if (ret == 1)
                {
                    //ModifyOK
                }
                else
                {
                    MessageBox.Show("Failed to modify receiver...");
                    return;
                }
            }




            //ModifyIP
            pkt.Reset();
            pkt.functionID = 0x96;
            IPAddress adrA = IPAddress.Parse(this.txtf_IP.Text);
            Array.Copy(adrA.GetAddressBytes(), 0, pkt.data, 0, 4);  //NewIP
            adrA = IPAddress.Parse(this.txtf_mask.Text);
            Array.Copy(adrA.GetAddressBytes(), 0, pkt.data, 4, 4);  //New Mask
            if (!string.IsNullOrEmpty(strGateway))
            {
                adrA = IPAddress.Parse(this.txtf_gateway.Text);
                Array.Copy(adrA.GetAddressBytes(), 0, pkt.data, 8, 4);  //New Gateway
            }
            if (this.optDhcp.Checked)
            {
                //Select Dynamic AcquireIP DHCP thenIPSet As00
                pkt.data[0] = 0x0;
                pkt.data[1] = 0x0;
                pkt.data[2] = 0x0;
                pkt.data[3] = 0x0;
            }
            pkt.data[12] = 0x55;
            pkt.data[13] = 0xaa;
            pkt.data[14] = 0xaa;
            pkt.data[15] = 0x55;
            ret = pkt.run(strPCAddr);
            //if (ret == 1)
            //{
            //    //ModifyOK
            //}
            //else
            //{
            //    MessageBox.Show("ModifyIPFailed...");
            //    return;
            //}


            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// 4Byte to Integer(Down front., Behind you.)
        /// </summary>
        /// <param name="buff">Bytes</param>
        /// <param name="start">Start Indexing Post(From0Start counting.)</param>
        /// <param name="len">Length</param>
        /// <returns>Integer</returns>
        long byteToLong(byte[] buff, int start, int len)
        {
            long val = 0;
            for (int i = 0; i < len && i < 4; i++)
            {
                long lng = buff[i + start];
                val += (lng << (8 * i));
            }
            return val;
        }

        string dataServerShortIP = "";
        int dataServerShortPort = 61005; // int.Parse(this.txtPort.Text);
        int dataServerShortCycle = 4;  //2015-06-14 08:23:17 Introduction of cycle delivery
        int[] dataServerShortOption = new int[8]; //2017-09-08 12:43:53 8Options Special

        void getdeviceNetInfo()
        {
            int ret = 0;
            //Create short message pkt

            WGPacketShort pkt = new WGPacketShort();
            pkt.iDevSn = long.Parse(strSN); // controllerSN;
            //pkt.IP = strIP; // ControllerIP;

            //	Read the receiver server.IPand Port **********************************************************************************
            pkt.Reset();
            pkt.functionID = 0x92;
            ret = pkt.run(strPCAddr);
            if (ret == 1)
            {
                dataServerShortIP = string.Format("{0}.{1}.{2}.{3}", pkt.recv[8], pkt.recv[9], pkt.recv[10], pkt.recv[11]);
                dataServerShortPort = pkt.recv[12] + (pkt.recv[13] << 8);
                dataServerShortCycle = pkt.recv[14];
                for (int i = 0; i < 8; i++)
                {
                    dataServerShortOption[i] = pkt.recv[15 + i];
                }

                this.txtHostIP.Text = dataServerShortIP;
                this.txtPortShort.Text = dataServerShortPort.ToString();
                this.nudCycle.Value = dataServerShortCycle;

            }
            if (ret == 1)
            {
                pkt.Reset();
                pkt.functionID = 0xF4;
                pkt.data[0] = 0x55;
                pkt.data[1] = 0xaa;
                pkt.data[2] = 0xaa;
                pkt.data[3] = 0x55;
                pkt.data[4] = 0x92; pkt.data[5] = 0x00; pkt.data[6] = 0x00;
                ret = pkt.run(strPCAddr);
            }
            if (ret == 1)
            {

                if (pkt.recv[14] == 0xA5)
                {
                    this.optDhcp.Checked = true;
                }
                else
                {
                    this.optSetIP.Checked = true;
                }

            }
            this.grpIP.Enabled = this.optSetIP.Checked; //2017-09-08 13:18:15
        }
        private void dfrmTCPIPConfigure_Load(object sender, EventArgs e)
        {
            this.txtf_ControllerSN.Text = strSN;
            this.txtf_MACAddr.Text = strMac;
            this.txtf_IP.Text = strIP;
            this.txtf_mask.Text = strMask;
            this.txtf_gateway.Text = strGateway;

            if (this.txtf_IP.Text == "255.255.255.255")  //When the system isFFTime, Change to Default
            {
                this.txtf_IP.Text = "192.168.0.0";
            }
            if (this.txtf_mask.Text == "255.255.255.255")  //When the system isFFTime, Change to Default
            {
                this.txtf_mask.Text = "255.255.255.0";
            }
            if (this.txtf_gateway.Text == "255.255.255.255")  //When the system isFFTime, Change to Default
            {
                this.txtf_gateway.Text = "";
            }
            if (this.txtf_gateway.Text == "0.0.0.0")
            {
                this.txtf_gateway.Text = "";
            }

            //Get additional information about the controller
            getdeviceNetInfo();
        }

        private void optSetIP_CheckedChanged(object sender, EventArgs e)
        {
            this.grpIP.Enabled = this.optSetIP.Checked; //2017-09-08 13:18:15
        }
    }
}
