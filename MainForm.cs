using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;
using Microsoft.Win32;

namespace click2call
{
	/// <summary>
	/// Originates an asterisk call to the given number from an internal
	/// /// phoneline.
	/// 
	/// If internal phoneline is not provided as an argument, it asks user to
	/// provide one and then saves it to registry so it won't annoy them again.
	/// 
	/// In order to remove the internal phoneline that is saved in the registry
	/// use the "c" command line parameter.
	/// </summary>
	public partial class MainForm : Form
	{
		SocketManager sm;
		const String ip = "asterisk ip";
		const int port = 5038;
		const String username = "your asterisk username";
		const String secret = "your asterisk secret";
		const String context = "your context";
		const String callerId = "Click 2 Call";

		const String subkeyPath = "Anpel\\Click2Call";
		const String subkeyName = "InternalPhoneLine";

		String originateRequest = "Action: Originate\r\n"
			+ "Callerid: " + callerId + "\r\n"
			+ "Context: " + context + "\r\n"
			+ "Priority: 1\r\n"
			+ "Async: yes\r\n";

		const String authenticationRequest = "Action: Login\r\n"
			+ "Username: " + username + "\r\n"
			+ "Secret: " + secret + "\r\n"
			+ "Events: off\r\n\r\n";
			
		/// <summary>
		/// Accepts 1 or 2 arguments, and then dials the number passed as arg[0]
		/// from the internal line passed as arg[1].
		/// 
		/// If arg[1] is ommited, it checks the registry for previously stored
		/// internal phoneline. If it is not found, it prompts the user to
		/// enter a line number and saves it to the registry.
		/// 
		/// If "c" is passed as arg[0] then the registry is cleared so that
		/// when the application is used again it will prompt the user again.
		/// </summary>
		/// <param name="args">The number to call and optionally the internal
		/// line to use</param>
		public MainForm(String[] args)
		{
			InitializeComponent();
			int internalPhoneLine = 0;
			
			// Check number of arguments
			if(args.Length == 0 || args.Length > 2)
			{
				Program.ExitApplication(111);
			}
			
			// Clear registry key if argument "c" is provided
			if(args[0] == "c")
			{
				Registry.CurrentUser.DeleteSubKey(subkeyPath);
				Program.ExitApplication(300);
			}
			
			// Check if phone line is passed as argument
			if( args.Length == 2 )
			{
				// Parse it as int
				if (!Int32.TryParse(args[1], out internalPhoneLine))
				{
					Program.ExitApplication(132);
				}
			} else {
				try
				{
					// Open subkey
					RegistryKey registryKey =
						Registry.CurrentUser.OpenSubKey(subkeyPath);
	
					// Check if key exists
					if(registryKey == null)
					{
						// Key does not exist, read user input
						String userInput = Interaction.InputBox(
							"Πληκτρολογήστε τον αριθμό του εσωτερικού σας τηλεφώνου:",
							"Πληροφορίες πρώτης εκτέλεσης"
						);
						
						// Parse user input as int
						int internalPhoneLineProvided;
						if (!Int32.TryParse(userInput, out internalPhoneLineProvided))
						{
							Program.ExitApplication(132);
						}
	
						// Create registry entry and save user input
						registryKey = Registry.CurrentUser.CreateSubKey(subkeyPath);
						registryKey.SetValue(subkeyName, internalPhoneLineProvided);
						internalPhoneLine = internalPhoneLineProvided;
					} else {
						// Key exists, read it and procceed
						internalPhoneLine = (int) registryKey.GetValue(subkeyName);
					}
				} catch(Exception) {
					Program.ExitApplication(133);
				}
			}

			// Instantiate Socket Manager
			sm = new SocketManager(ip, port);

			// Send Authenticate request
			String authenticationRequestResponse =
				sm.SendMessageAndGetResponse(authenticationRequest);

			if(!Regex.Match(
					authenticationRequestResponse,
					"Response: Success",
					RegexOptions.IgnoreCase
				).Success
			) {
				Program.ExitApplication(130);
			}

			// Forge and send originate request
			originateRequest += "Channel: SIP/" + internalPhoneLine + "\r\n";
			originateRequest += "Exten: " + args[0] + "\r\n\r\n";
			String originateRequestResponse =
				sm.SendMessageAndGetResponse(originateRequest);

			if (!Regex.Match(
				authenticationRequestResponse,
				"Response: Success",
				RegexOptions.IgnoreCase
				).Success
			) {
				Program.ExitApplication(131);
			}

			Program.SetExitCode(200);
		}
	}
}
