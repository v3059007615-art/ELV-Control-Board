package org.wiegand.at8000;

import java.net.InetSocketAddress;
import java.util.LinkedList;
import java.util.Queue;

import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.future.ConnectFuture;
import org.apache.mina.core.service.IoConnector;
import org.apache.mina.core.session.IoSession;
import org.apache.mina.transport.socket.nio.NioDatagramConnector;

public class WgUdpCommShort { //Shortcast agreement

	public static final int  WGPacketSize = 64;             //Length of submission
	public static final byte Type = 0x17; //2015-04-30 08:50:29 0x19;					//Type
	public static final int  ControllerPort = 60000;        //controller port
	public static final long SpecialFlag = 0x55AAAA55;      //Special identification Prevent mishandling

    public static byte[] longToByte(long number) {   
	     byte[] b = new byte[8];   
	     for (int i = 0; i < 8; i++) {   
		       b[i] = (byte) (number % 256);   
		       number >>= 8;   
		     }   
	     return b;   
	    }
	 
	 //Will be markedbtConvert to UnsignedintType Data 
	 public static int getIntByByte(byte bt)  //bt Convert to Unsignedint
	{
	    if (bt <0)
	    {
	    	return (bt+256);
	    }
	    else
	    {
	    	return bt;
	    }
	}
	 
	//Convert from byte to longType Data, Maximum length is8Bytes Low in front, High in the back....
			//bytlen (1--8), Return outside this range -1
	public static long getLongByByte(byte[] data,int startIndex,int bytlen)
    {
    	long ret =-1;
		if ((bytlen >=1) && (bytlen <=8))
		{
    		ret = getIntByByte(data[startIndex + bytlen-1]);
    		for (int i=1; i<bytlen; i++)
    		{
    			ret <<=8;
    			ret += getIntByByte(data[startIndex + bytlen-1-i]);
    		}
		}
    	return ret;
    }
			
				
	public byte	 functionID;		    //Function Number
	public long	 iDevSn;                //Device serial number 4Bytes
	public byte[]  data= new byte[56];              //56Byte Data [Fluid]

	private static long _Global_xid = 0;
    protected long _xid = 0; //2011-5-12 15:28:37
    void GetNewXid()  //2011-1-10 14:22:16 Get NewXid
    {
        _Global_xid++;
        _xid = _Global_xid; //New Value
    }
    static long getXidOfCommand(byte[] cmd) //Fetching commandsxid
    {
        long ret = -1;
        if (cmd.length >= WGPacketSize)
        {
            ret = getLongByByte(cmd, 40, 4);
        }
        return ret;
    }

	   public WgUdpCommShort()
		{
			Reset();
		}
		public void Reset()  //Data Reunification
		{
			for(int i=0; i<data.length; i++)
			{
				data[i] =0;
			}
		}
		public byte[] toByte() //Generate64Bytes package
		{
			byte[] buff =new byte[WGPacketSize];
				for(int i=0; i<data.length; i++)
				{
					buff[i] =0;
				}
				buff[0] = Type;
				buff[1] = functionID;
		        System.arraycopy(longToByte(iDevSn), 0, buff, 4, 4);
		        System.arraycopy(data, 0, buff, 8, data.length);

		        GetNewXid();
		        System.arraycopy(longToByte(_xid), 0, buff, 40, 4);
               return buff;
		}
		

	Queue<byte[]> queue;
	IoConnector connector; // = new NioDatagramConnector();
	ConnectFuture connFuture; 
	public  void CommOpen(String ip, int port)
	{
		queue = new LinkedList<byte[]>();
		connector = new NioDatagramConnector();
		connector.setHandler(new WgUdpCommShortHandler(queue));
		connFuture = connector.connect(new InetSocketAddress(ip,	port));
	}
	
	//Open communication connection
	public  void CommOpen(String ip)
	{
		CommOpen(ip,ControllerPort);  //2013-11-06 14:16:52 Default is60000
	}
	
	//Close communication connection
	public  void CommClose()
	{
		IoSession  session = connFuture.getSession();
	   	if (session !=null)
	   	{
	   		session.close(true);
	   	}
         connector.dispose();
	}
	
	//Run Access to communication data
	//When failed, Back null, Otherwise64Byte Data
	public byte[] run()
	{
		return getInfo(iDevSn,toByte());
	}
	
	//By designationsnandcommand Getting data
    public  byte[] getInfo( long sn, byte[] command) 
    {
		byte[] bytCommand = command;
        IoBuffer b;
		IoSession session = connFuture.getSession();
		Boolean bSent =false;
		if (session !=null)
		{
			if (session.isConnected())
			{
				b = IoBuffer.allocate(bytCommand.length);
				b.put(bytCommand);
				b.flip();
				session.write(b);
				bSent =true; 
			}
		}
		
         int  bSuccess = 0; 
		 int tries = 3;
		 long xid = getXidOfCommand(bytCommand) ;   
		 byte[] bytget=null;
         while ((tries--) > 0)  
         {
				long startTicks = java.util.Calendar.getInstance().getTimeInMillis(); // DateTime.Now.Ticks;
				long CommTimeoutMsMin = 300;
			    long endTicks = startTicks + CommTimeoutMsMin; 
		       if (startTicks > endTicks)
		       {
		    	   //System.out.println("Timeout");
		    	   try {
		  				Thread.sleep(30);
		  			} catch (InterruptedException e) {
		  				e.printStackTrace();
		  			}
		    	   continue; 
		       }
		       long startIndex = 0;
		       while (endTicks > java.util.Calendar.getInstance().getTimeInMillis())
		       {
		    	   if (!bSent)  //I didn't send it.....
		    	   {
		    		   session = connFuture.getSession();
		    		   if (session !=null)
		    			{
		    				if (session.isConnected())
		    				{
		    					b = IoBuffer.allocate(bytCommand.length);
		    					b.put(bytCommand);
		    					b.flip();
		    					session.write(b);
		    					bSent =true; 
		    				}
		    			}
		    	   }
		           if (!queue.isEmpty())
		           {
		        		synchronized(queue)
		        		{
		                		bytget= queue.poll();
		        		}
		                if ((bytget[0]== bytCommand[0]) //Align type
									&& (bytget[1]== bytCommand[1]) //Function numbers are consistent
									&& (xid == getXidOfCommand(bytget)) )  //Serial number corresponding
					    {
		                   bSuccess = 1;
		                   break; // return ret;
		                }
		                else
		                {
		                	//System.out.printf("Invalid package xid=%d\r\n", WgUdpComm.getXidOfCommand(bytget));
		                }
		           }
		           else
		           {
		               if ((startTicks + 1) < java.util.Calendar.getInstance().getTimeInMillis()) 
		               {
		               }
		               else if (startIndex > 10)
		               {
		                   try {
		       				Thread.sleep(30);
		       			} catch (InterruptedException e) {
		       				e.printStackTrace();
		       			}
		               }
		               else
		               {
		                   startIndex++;
		                   try {
		       				Thread.sleep(1);
		       				} catch (InterruptedException e) {
		       				e.printStackTrace();
		       				}
		               }
		           }
		
		       }
		       if (bSuccess > 0)
		       {
		    	   break;
		       }
		       else
		       {
		    	  // System.out.println("Try again....");
		    	session = connFuture.getSession();
		   		if (session !=null)
		   		{
		   			if (session.isConnected())
		   			{
		   				b = IoBuffer.allocate(bytCommand.length);
				   		b.put(bytCommand);
			            b.flip();
		   				session.write(b);
		   			}
		   		}
		       }
         }
         
         if (bSuccess > 0)
         {
      	   //System.out.println("Communications Success");
       	    return  bytget;
         }
         else
         {
      	  //System.out.println("Communications Failed....");
         }
         return null;
 	}

}
