/**
* $Id: 2015-08-24 15:49:39 Karl CSN 陈绍宁 $
*
* 门禁控制器 短报文协议 测试案例(只适用于V6.56或以上版本)
* V1.0 版本  2015-08-24 15:50:17
*            基本功能:     远程开门
*            系统要求: android版本为2.3.1或以上
*            权限要求: 要在 AndroidMainfest.xml中增加如下权限
*                <uses-permission android:name="android.permission.INTERNET"></uses-permission> 
*            案例控制器: 驱动V6.56或以上; IP设为 192.168.168.123; 电脑IP在同一网段.
*
*            
*/


package com.adct.wgremoteopendoor;

import java.io.IOException;
import java.net.DatagramPacket;
import java.net.DatagramSocket;
import java.net.InetAddress;
import java.net.SocketException;
import java.net.UnknownHostException;



import android.app.Activity;
import android.os.Bundle;
import android.os.StrictMode;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.TextView;

public class MainActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
							
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		//must manually  add StrictMode
		StrictMode.setThreadPolicy(new StrictMode.ThreadPolicy.Builder()
				.detectDiskReads().detectDiskWrites().detectNetwork()
				.penaltyLog() 
				.build());
		(findViewById(R.id.button1)).setOnClickListener(listener);
	}

	private OnClickListener listener = new OnClickListener() {
		@Override
		public void onClick(View v) {
			View btn = v;
			switch (btn.getId()) {
			case R.id.button1:
				//Controller IP:192.168.168.123, Mask:255.255.255.0; 
				//PC IP: 192.168.168.120 or IP in the same local net
				if (RemoteOpenDoorIP(1,"192.168.168.123") >0)
			    //if (RemoteOpenDoorIP(1,"10.0.1.123") >0)
				{
					((TextView) findViewById(R.id.textView1)).setText("Successful!");
				}
				else
				{
					((TextView) findViewById(R.id.textView1)).setText("Failed.");
				}
				break;
			default:
				break;
			}
		}
	};
	
	private static int tag = 1;

	private int RemoteOpenDoorIP(int doorNO, String IP) {
		int ret = -13; 
		byte content[] = null;
		    DatagramPacket snddataPacket;
		try {
			int controllerPort = 60000;
			byte[] byteCmd = new byte[] { (byte) 0x17, (byte) 0x40,
					(byte) 0x00, (byte) 0x00, (byte) 0xff, (byte) 0xff,
					(byte) 0xff, (byte) 0xff, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00, (byte) 0x00, (byte) 0x00,
					(byte) 0x00, (byte) 0x00 };

			
			byteCmd[8] = (byte) doorNO;

			byteCmd[40] = (byte) (tag & 0xff);
			byteCmd[41] = (byte) ((tag >> 8) & 0xff);
			byteCmd[42] = (byte) ((tag >> 16) & 0xff);
			byteCmd[43] = (byte) ((tag >> 24) & 0xff); 

			content = null;
			DatagramSocket dataSocket = new DatagramSocket();
			dataSocket.setSoTimeout(1000); 

			snddataPacket = new DatagramPacket((byteCmd), byteCmd.length,
					InetAddress.getByName(IP), controllerPort);
			dataSocket.send(snddataPacket);

			byte recvDataByte[] = new byte[64];

			Thread.sleep(200); 
			DatagramPacket dataPacket = new DatagramPacket(recvDataByte,
					recvDataByte.length);
			dataSocket.receive(dataPacket);

			content = dataPacket.getData();
			dataSocket.close();
		} catch (NumberFormatException exNum) {
			exNum.printStackTrace();
		}

		catch (UnknownHostException e1) {
			e1.printStackTrace();
			return ret; 
		} catch (SocketException e) {
			e.printStackTrace();
		} catch (IOException e) {
			e.printStackTrace();
		} catch (InterruptedException e) {
			e.printStackTrace();
		} finally {

		}

		if ((content != null) && (content.length == 64)) {
			ret = content[8];
		} else {
			ret = -13; 
		}
		tag++;

		return ret; // null;
	}

	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			return true;
		}
		return super.onOptionsItemSelected(item);
	}
}
