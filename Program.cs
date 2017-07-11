using System;
using System.Windows.Forms;

namespace click2call
{
	internal sealed class Program
	{
		/**
		 * Exit code reference:
		 * 
		 * 0   : Default
		 * 111 : Invalid arguments
		 * 113 : Could not parse IP address string
		 * 120 : Could not create IPEndPoint with given ip and port
		 * 121 : Error trying to access the socket
		 * 122 : The socket has been closed
		 * 123 : The socket is listening
		 * 130 : Invalid asterisk credentials
		 * 131 : Could not originate call
		 * 132 : Invalid internal phoneline user input
		 * 133 : Could not create registry key
		 * 132 : Invalid internal command line argument
		 * 200 : OK / Dialing
		 * 300 : Registry cleared
		 */
		static int exitCode = 0;

		public static void ExitApplication(int exitCode)
		{
			MessageBox.Show("Error attempting to originate call. Code " +
				exitCode,
				"Error",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error
			);
			Program.exitCode = exitCode;
			Environment.Exit(exitCode);
		}

		public static void SetExitCode(int exitCode)
		{
			Program.exitCode = exitCode;
		}
		
		[STAThread]
		private static int Main(string[] args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			MainForm mainForm = new MainForm(args);
			return exitCode;
		}
		
	}
}
