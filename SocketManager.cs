/*
 * User: apelekoudas
 * Date: 27/10/2016
 * Time: 1:49 μμ
 */
using System;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Text.RegularExpressions;

namespace click2call
{
	/// <summary>
	/// Description of SocketManager.
	/// </summary>
	public class SocketManager
	{		
		/// <summary>
		/// The socket object we will use for sending and receiving data
		/// </summary>
		Socket socket;
		
		/// <summary>
		/// Creates a new SocketManager Object and attempts to connect to the 
		/// given socket.
		/// </summary>
	    /// <param name="hostIp">The remote host IP</param>	
	    /// <param name="port">THe port on the remote host</param>
		public SocketManager(String hostIp, int port)
		{
			IPAddress ipAddress;
			if(!IPAddress.TryParse(hostIp, out ipAddress))
			{
				Program.ExitApplication(113);
			}
			socket = CreateSocket(ipAddress, port);
		}
		
		/// <summary>
		/// Send a message to a socket and read the response until two empty lines
		/// are found - "\r\n\r\n".
		/// </summary>
		/// <param name="message">The message to send to the socket server</param>
		/// <returns>The server response</returns>
		public String SendMessageAndGetResponse(String message)
		{
			String response = "";
			try
			{
				socket.Send(Encoding.ASCII.GetBytes(message));
				
				int bytesRead = 0;
				do
            	{
	                byte[] buffer = new byte[1024];
	                bytesRead = socket.Receive(buffer);
	
	                String line = Encoding.ASCII.GetString(buffer, 0, bytesRead);
	                response += line;
	                if (Regex.Match(line, "\r\n\r\n", RegexOptions.IgnoreCase).Success)
	                {
	                	break;
	                }
	            } while (bytesRead != 0);
			} catch(ArgumentNullException){
				Program.ExitApplication(111);
			} catch(SocketException){
				Program.ExitApplication(121);
			} catch(ObjectDisposedException){
				Program.ExitApplication(122);
			}
            return response;
		}
		
		
		/// <summary>
	    /// Creates a socket connection to ip:port 
	    /// </summary>
	    /// <param name="ipAddress">The remote host IP address</param>	
	    /// <param name="port">THe port on the remote host</param>
	    /// <returns>Socket object</returns>
		Socket CreateSocket(IPAddress ipAddress, int port)
		{
			Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
            	s.Connect(new IPEndPoint(ipAddress, port));
			} catch(ArgumentNullException) {
				Program.ExitApplication(120);
			} catch(SocketException) {
				Program.ExitApplication(121);
			} catch(ObjectDisposedException) {
				Program.ExitApplication(122);
			} catch(InvalidOperationException) {
				Program.ExitApplication(123);
			}
            return s;
		}
	}
}
