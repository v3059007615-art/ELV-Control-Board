package org.wiegand.TestZone;

import java.util.Queue;

import org.apache.mina.core.buffer.IoBuffer;
import org.apache.mina.core.service.IoHandlerAdapter;
import org.apache.mina.core.session.IdleStatus;
import org.apache.mina.core.session.IoSession;
import org.wiegand.at8000.WgUdpCommShort;


    
    /**
     * Class the extends IoHandlerAdapter in order to properly handle
     * connections and the data the connections send
     *
     * @author <a href="http://mina.apache.org" mce_href="http://mina.apache.org">Apache MINA Project</a>
     */
    public class WatchingShortHandler extends IoHandlerAdapter {

    	private Queue<byte[]> queue;
        public WatchingShortHandler(Queue<byte[]> queue) {
    		super();
    		this.queue = queue;
    	}
        /**
         * Unusual to close.session
         */
        @Override
        public void exceptionCaught(IoSession session, Throwable cause)
                throws Exception {
            cause.printStackTrace();
            session.close(true);
        }

        /**
         * The server received a message.
         */
        @Override
        public void messageReceived(IoSession session, Object message)
                throws Exception {

        	IoBuffer io = (IoBuffer) message;
    		if (io.hasRemaining())
    		{
    			byte[] validBytes = new byte[io.remaining()];
    			io.get(validBytes,0,io.remaining());
    			if (validBytes.length == WgUdpCommShort.WGPacketSize)
    			{
    				 synchronized (queue)
    		         {
      				   queue.offer(validBytes);
    		         }
    			}
    			else
    			{
    				//System.out.print("Invalid package received: ????\r\n");
    			}
    			//System.out.println("");
    		}
        }

        @Override
        public void sessionClosed(IoSession session) throws Exception {
//            System.out.println("Serverend Closesession...");
        }

        @Override
        public void sessionCreated(IoSession session) throws Exception {
//            System.out.println("Serverend successfully createdsession...");
        }

        @Override
        public void sessionIdle(IoSession session, IdleStatus status)
                throws Exception {
           //  System.out.println("Session idle...");
        }

        @Override
        public void sessionOpened(IoSession session) throws Exception {
//            System.out.println("Server-end successfully opened onesession...");
        }
    }
