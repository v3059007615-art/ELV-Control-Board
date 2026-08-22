using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace WGController32_CSharp
{
    /// <summary>
    /// Shortcasts
    /// </summary>
    public class WGPacketShort
        : IDisposable
    {

        /// <summary>
        /// Release resources1
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)  //2017-09-12 09:42:30 Additional Releases
        {
            if (disposing)
            {
                // dispose managed resources
                // newFile.Close();
                if (controller != null)
                {
                    controller.Dispose();
                }
            }
            // free native resources
        }
        /// <summary>
        /// Release resources
        /// </summary>
        public void Dispose()  //2017-09-12 09:42:37
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public const int WGPacketSize = 64;			    //Length of submission
        //2015-04-29 22:22:41 const static unsigned char	 Type = 0x19;					//Type
        public const int Type = 0x17;		//2015-04-29 22:22:50			//Type
        public const int ControllerPort = 60000;        //controller port
        public const long SpecialFlag = 0x55AAAA55;     //Special identification Prevent mishandling

        public int functionID;		                     //Function Number
        public long iDevSn;                              //Device serial number 4Bytes, 9Digits
        public string IP;                                //The controller.IPAddress

        public byte[] data = new byte[56];               //56Byte Data [Fluid]
        public byte[] recv = new byte[WGPacketSize];     //Data received

        public WGPacketShort()
        {
            Reset();
        }
        public void Reset()  //Data Reunification
        {
            for (int i = 0; i < 56; i++)
            {
                data[i] = 0;
            }
        }
        static long sequenceId;     //Serial number	
        public byte[] toByte() //Generate64Bytes package
        {
            byte[] buff = new byte[WGPacketSize];
            sequenceId++;

            buff[0] = (byte)Type;
            buff[1] = (byte)functionID;
            Array.Copy(System.BitConverter.GetBytes(iDevSn), 0, buff, 4, 4);
            Array.Copy(data, 0, buff, 8, data.Length);
            Array.Copy(System.BitConverter.GetBytes(sequenceId), 0, buff, 40, 4);
            return buff;
        }

        WG3000_COMM.Core.wgMjController controller = new WG3000_COMM.Core.wgMjController();
        public int run()  //Send Command Can not open message
        {
            byte[] buff = toByte();

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 Enter Retry return -1;
                }
                else
                {
                    //Water Stream
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //Align type
                        && (recv[1] == functionID)              //Function numbers are consistent
                        && (sequenceIdReceived == sequenceId))  //Serial number corresponding
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //Try again three times.

            return -1;
        }
        public int run(string pcIPAddress)  //Send Command Can not open message  AssignPCYes.IPAddress
        {
            byte[] buff = toByte();

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv, pcIPAddress) < 0)
                {
                    //2015-11-03 20:26:52 Enter Retry return -1;
                }
                else
                {
                    //Water Stream
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //Align type
                        && (recv[1] == functionID)              //Function numbers are consistent
                        && (sequenceIdReceived == sequenceId))  //Serial number corresponding
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //Try again three times.

            return -1;
        }



        //Encrypt the call dynamic library
        [DllImport("n3kWGCom.dll", EntryPoint = "ShortEncrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int ShortEncrypt(IntPtr command, IntPtr password);

        [DllImport("n3kWGCom.dll", EntryPoint = "ShortDecrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        static extern int ShortDecrypt(IntPtr command, IntPtr password);

        public static int Encrypt(ref byte[] command, byte[] password)  //2015-09-28 13:19:12 2013-4-2_07:31:32 Encrypt data
        {
            IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

            Marshal.Copy(command, 0, pkt, 64);

            IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
            Marshal.Copy(password, 0, commPassword, 16);

            int ret = ShortEncrypt(pkt, commPassword);
            if (ret > 0)
            {
                Marshal.Copy(pkt, command, 0, 64);  //Copy it.
            }
            Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
            Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
            return ret;

        }
        public static int Decrypt(ref byte[] command, byte[] password)  //2015-09-28 15:12:12  Decrypt Data
        {
            IntPtr pkt = Marshal.AllocHGlobal((Int32)64);

            Marshal.Copy(command, 0, pkt, 64);

            IntPtr commPassword = Marshal.AllocHGlobal((Int32)16);
            Marshal.Copy(password, 0, commPassword, 16);


            int ret = ShortDecrypt(pkt, commPassword);
            if (ret > 0)
            {
                Marshal.Copy(pkt, command, 0, 64);  //Copy it.
            }
            Marshal.FreeHGlobal(pkt); //2014-01-02 13:20:43 Release Memory
            Marshal.FreeHGlobal(commPassword); //2014-01-02 13:20:43 Release Memory
            return ret;
        }

        public int run(byte[] commPassword)  //2015-10-28 10:16:38 Message-processing dispatch instructions Can not open message
        {
            byte[] buff = toByte();
            Encrypt(ref buff, commPassword);

            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 Enter Retry return -1;
                }
                else
                {
                    if ((recv[0] & 0x7F) == Type)
                    {
                        Decrypt(ref recv, commPassword);

                        //Water Stream
                        long sequenceIdReceived = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            long lng = recv[40 + i];
                            sequenceIdReceived += (lng << (8 * i));
                        }

                        if ((recv[0] == Type)                       //Align type
                            && (recv[1] == functionID)              //Function numbers are consistent
                            && (sequenceIdReceived == sequenceId))  //Serial number corresponding
                        {
                            return 1;
                        }
                        else
                        {
                            errcnt++;
                        }
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //Try again three times.

            return -1;
        }

        public int run1024(byte[] buff)  //2015-11-05 14:50:45 1024Byte Command Send Command Can not open message 
        {
            return run1024(buff, null);
        }


        //commPassword Do not use passwords when empty. The password must be16Bytes
        public int run1024(byte[] buff, byte[] commPassword)  //2015-11-05 14:50:45 1024Byte Command Encrypted communications component Send Command Can not open message 
        {
            long sequenceIdSend = 0;
            for (int i = 0; i < 4; i++)
            {
                long lng = buff[40 + i];
                sequenceIdSend += (lng << (8 * i));
            }
            if (commPassword != null)
            {
                //If Encryption
                byte[] buffBk = new byte[64];
                for (int i = 0; i < 1024; i = i + 64)
                {
                    Array.Copy(buff, i, buffBk, 0, 64);
                    Encrypt(ref buffBk, commPassword);
                    Array.Copy(buffBk, 0, buff, i, 64);
                }
            }
            int tries = 3;
            int errcnt = 0;
            controller.IP = IP;
            controller.PORT = ControllerPort;
            do
            {
                if (controller.ShortPacketSend(buff, ref recv) < 0)
                {
                    //2015-11-03 20:26:52 Enter Retry return -1;
                }
                else
                {
                    if ((recv[0] & 0x7F) == Type)
                    {
                        if ((commPassword != null) && recv.Length == 1024)
                        {
                            //If it's encrypted,
                            byte[] buffBk = new byte[64];
                            for (int i = 0; i < 1024; i = i + 64)
                            {
                                Array.Copy(recv, i, buffBk, 0, 64);
                                Decrypt(ref buffBk, commPassword);
                                Array.Copy(buffBk, 0, recv, i, 64);
                            }
                        }
                    }
                    //Water Stream
                    long sequenceIdReceived = 0;
                    for (int i = 0; i < 4; i++)
                    {
                        long lng = recv[40 + i];
                        sequenceIdReceived += (lng << (8 * i));
                    }

                    if ((recv[0] == Type)                       //Align type
                        && (recv[1] == functionID)              //Function numbers are consistent
                        && (sequenceIdReceived == sequenceIdSend)  //Serial number corresponding
                    )
                    {
                        return 1;
                    }
                    else
                    {
                        errcnt++;
                    }
                }
            } while (tries-- > 0); //Try again three times.

            return -1;
        }
        
        /// <summary>
        /// It's the last running water.
        /// </summary>
        /// <returns></returns>
        public static long sequenceIdSent()// 
        {
            return sequenceId; // It's the last running water.
        }
        /// <summary>
        /// Close
        /// </summary>
        public void close()
        {
            controller.Dispose();
        }
    }
}
