//using Asterisk.NET.Manager;
//using Asterisk.NET.Manager.Event;
using AsterNET.Manager;
using AsterNET.Manager.Event;

using ITLGMCA_CALL.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.OleDb;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace ITLGMCA_CALL
{
	public class FormMain : Form
	{
        class MySettings : MyAppSettings<MySettings>
        {
            public string MedialogConnectionString = "Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;";
            public string MedialogQuery = "select replace(dbo.PreparePhone(phone,'',''),'+7','8') as tel ,system as name from us_calls_sources";
            //public string Address="192.168.1.1";
			//public string Port="5038";
            //public string User="medialog";
            public string password="";
            public string CallerNameColumn = "";
            public bool Replase8 = false;
            public string IgnoreChannel1String = "";
            public string IgnoreChannel2String = "";
            public bool DebugFile = false;
            //public string phone_number="";
            //public int phone_length=4;
        }
        
        MySettings settings;
        
		private string address;

		private int port;

		private string user;

		private string password;

		private string phone_number;

		private int phonelength;

		private bool autoconnect;

		private string test1 = "reterdsx834";

		private string test2 = "sdsduo322389dx";

		private string test3 = "7887943%%932kdf";

		private List<string> answered = new List<string>();

        private Dictionary<string, string> channels;    //новые каналы из атс

        private Dictionary<string, string> tel_dic;     //телефонный справочник из базы SQL

        private string medialog_file="MCA_CALL.INI";

        private int events_count;
        private int events_completed;

		private ManagerConnection manager;

		private IContainer components;
		private GroupBox groupBox1;
		private Button SaveOptions;
		private Label label1;
		private TextBox PhonetextBox;
		private Button btnDisconnect;
		private Button btnConnect;
		private Label label4;
		private Label label3;
		private TextBox tbUser;
		private TextBox tbPassword;
		private Label label2;
		private TextBox tbPort;
		private Label lable1;
		private TextBox tbAddress;
		private NotifyIcon notifyIcon1;
		private CheckBox AutoConnectcheckBox;
		private Label label5;
		private TextBox NumLenghtextBox;
		private Button button2;
		private Button button1;
		private SplitContainer splitContainer1;
        private PictureBox pictureBox1;
		private Label label6;

		public FormMain()
		{
			this.InitializeComponent();
		}

		private void btnConnect_Click(object sender, EventArgs e)
		{
			this.Connect();
		}

		private void btnDisconnect_Click(object sender, EventArgs e)
		{
			this.btnConnect.Enabled = true;
			if (this.manager != null)
			{
				this.manager.Logoff();
				this.manager = null;
			}
			this.btnDisconnect.Enabled = false;
            update_notifyIcon_text();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			RegistryKey registryKey = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
			registryKey.SetValue("ITLGMCA_CALL", Application.ExecutablePath);
			registryKey.Close();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			RegistryKey registryKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\", true);
			registryKey.DeleteValue("ITLGMCA_CALL", false);
			registryKey.Close();
		}

		private void Connect()
		{
			this.address = this.tbAddress.Text;
			this.port = int.Parse(this.tbPort.Text);
			this.user = this.tbUser.Text;
			this.password = this.tbPassword.Text;
			this.phone_number = this.PhonetextBox.Text;
			this.phonelength = int.Parse(this.NumLenghtextBox.Text);
			this.btnConnect.Enabled = false;
			this.manager = new ManagerConnection(this.address, this.port, this.user, this.password);
			this.manager.UnhandledEvent += new ManagerEventHandler(this.manager_Events);
			this.manager.ConnectionState += new ConnectionStateEventHandler(this.reconnect_event);
			this.manager.Hangup += new HangupEventHandler(this.hangup);
            //this.manager.UseASyncEvents = true;     //check this
            this.manager.PingInterval = 60000;
            this.manager.ReconnectIntervalFast = 2000;  //Fast Reconnect interval in milliseconds
            this.manager.DefaultResponseTimeout=4000;
			try
			{
				this.manager.Login();
				this.manager.FireAllEvents = true;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				MessageBox.Show(string.Concat("Error connect\n", exception.Message, exception.ToString()));
				this.manager.Logoff();
				base.Close();
			}
			this.btnDisconnect.Enabled = true;
            update_notifyIcon_text();

            log("My number=" + phone_number);

            if(this.channels==null)
                this.channels=new Dictionary<string,string>();
            else
                channels.Clear();

            events_count = 0;
            events_completed = 0;
		}

		public static string Decrypt(string cipherText, string password, string salt = "Kosher", string hashAlgorithm = "SHA1", int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
		{
			if (string.IsNullOrEmpty(cipherText))
			{
				return "";
			}
			byte[] bytes = Encoding.ASCII.GetBytes(initialVector);
			byte[] numArray = Encoding.ASCII.GetBytes(salt);
			byte[] numArray1 = Convert.FromBase64String(cipherText);
			PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(password, numArray, hashAlgorithm, passwordIterations);
			byte[] bytes1 = passwordDeriveByte.GetBytes(keySize / 8);
			RijndaelManaged rijndaelManaged = new RijndaelManaged()
			{
				Mode = CipherMode.CBC
			};
			byte[] numArray2 = new byte[(int)numArray1.Length];
			int num = 0;
			using (ICryptoTransform cryptoTransform = rijndaelManaged.CreateDecryptor(bytes1, bytes))
			{
				using (MemoryStream memoryStream = new MemoryStream(numArray1))
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
					{
						num = cryptoStream.Read(numArray2, 0, (int)numArray2.Length);
						memoryStream.Close();
						cryptoStream.Close();
					}
				}
			}
			rijndaelManaged.Clear();
			return Encoding.UTF8.GetString(numArray2, 0, num);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.components != null)
			{
				this.components.Dispose();
			}
			base.Dispose(disposing);
		}

		public static string Encrypt(string plainText, string password, string salt = "Kosher", string hashAlgorithm = "SHA1", int passwordIterations = 2, string initialVector = "OFRna73m*aze01xY", int keySize = 256)
		{
			if (string.IsNullOrEmpty(plainText))
			{
				return "";
			}
			byte[] bytes = Encoding.ASCII.GetBytes(initialVector);
			byte[] numArray = Encoding.ASCII.GetBytes(salt);
			byte[] bytes1 = Encoding.UTF8.GetBytes(plainText);
			PasswordDeriveBytes passwordDeriveByte = new PasswordDeriveBytes(password, numArray, hashAlgorithm, passwordIterations);
			byte[] numArray1 = passwordDeriveByte.GetBytes(keySize / 8);
			RijndaelManaged rijndaelManaged = new RijndaelManaged()
			{
				Mode = CipherMode.CBC
			};
			byte[] array = null;
			using (ICryptoTransform cryptoTransform = rijndaelManaged.CreateEncryptor(numArray1, bytes))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
					{
						cryptoStream.Write(bytes1, 0, (int)bytes1.Length);
						cryptoStream.FlushFinalBlock();
						array = memoryStream.ToArray();
						memoryStream.Close();
						cryptoStream.Close();
					}
				}
			}
			rijndaelManaged.Clear();
			return Convert.ToBase64String(array);
		}

		[DllImport("user32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
		public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.None, ExactSpelling = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AttachConsole();

		private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
		{
			if (this.manager != null)
			{
				this.manager.Logoff();
				this.manager = null;
			}
			Application.Exit();
		}

		private void FormMain_Load(object sender, EventArgs e)
		{
            if(Environment.GetCommandLineArgs().Length>1 && !AttachConsole()) AllocConsole();

            settings = MySettings.Load();

			if (File.Exists("ITLGMCA_CALL.CONF"))
			{
				StreamReader streamReader = File.OpenText("ITLGMCA_CALL.CONF");
				this.tbAddress.Text = streamReader.ReadLine();
				this.tbPort.Text = streamReader.ReadLine();
				this.tbUser.Text = streamReader.ReadLine();
				this.tbPassword.Text = FormMain.Decrypt(streamReader.ReadLine(), FormMain.Encrypt(this.test1, this.test2, "Kosher", "SHA1", 2, "OFRna73m*aze01xY", 256), "Kosher", "SHA1", 2, "OFRna73m*aze01xY", 256);
				this.PhonetextBox.Text = streamReader.ReadLine();
				this.NumLenghtextBox.Text = streamReader.ReadLine();
				this.AutoConnectcheckBox.Checked = Convert.ToBoolean(streamReader.ReadLine());
				streamReader.Close();
				if (this.AutoConnectcheckBox.Checked)
				{
					this.Connect();
					base.WindowState = FormWindowState.Minimized;
				}
			}

            if (!string.IsNullOrEmpty(settings.MedialogConnectionString) && !string.IsNullOrEmpty(settings.MedialogQuery))
            {
                tel_dic = GetData();
                log("Loaded " + tel_dic.Count + " elements");
            }
            update_notifyIcon_text();

		}

        private void update_notifyIcon_text()
        {
            notifyIcon1.Text = "MCA ";
            if(this.manager!=null && this.manager.IsConnected())
                notifyIcon1.Text += phone_number;

            if (tel_dic != null && tel_dic.Count > 0)
                notifyIcon1.Text += " В справочнике " + tel_dic.Count.ToString() + " номеров";
        }

		private void FormMain_SizeChanged(object sender, EventArgs e)
		{
			if (FormWindowState.Minimized == base.WindowState)
			{
				this.notifyIcon1.Visible = true;
				base.Hide();
			}
		}

		private void hangup(object sender, HangupEvent e)
		{
			if (this.answered.Contains(e.UniqueId))
			{
				this.answered.RemoveAt(this.answered.IndexOf(e.UniqueId));
			}
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.AutoConnectcheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.NumLenghtextBox = new System.Windows.Forms.TextBox();
            this.SaveOptions = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.PhonetextBox = new System.Windows.Forms.TextBox();
            this.btnDisconnect = new System.Windows.Forms.Button();
            this.btnConnect = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbUser = new System.Windows.Forms.TextBox();
            this.tbPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbPort = new System.Windows.Forms.TextBox();
            this.lable1 = new System.Windows.Forms.Label();
            this.tbAddress = new System.Windows.Forms.TextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label6 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.AutoConnectcheckBox);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.NumLenghtextBox);
            this.groupBox1.Controls.Add(this.SaveOptions);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.PhonetextBox);
            this.groupBox1.Controls.Add(this.btnDisconnect);
            this.groupBox1.Controls.Add(this.btnConnect);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.tbUser);
            this.groupBox1.Controls.Add(this.tbPassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.tbPort);
            this.groupBox1.Controls.Add(this.lable1);
            this.groupBox1.Controls.Add(this.tbAddress);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(319, 204);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Parameters connection";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(212, 122);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(101, 23);
            this.button2.TabIndex = 17;
            this.button2.Text = "RemoveAutorun";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(212, 97);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(101, 23);
            this.button1.TabIndex = 16;
            this.button1.Text = "AutoRun";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // AutoConnectcheckBox
            // 
            this.AutoConnectcheckBox.AutoSize = true;
            this.AutoConnectcheckBox.Location = new System.Drawing.Point(94, 180);
            this.AutoConnectcheckBox.Name = "AutoConnectcheckBox";
            this.AutoConnectcheckBox.Size = new System.Drawing.Size(88, 17);
            this.AutoConnectcheckBox.TabIndex = 15;
            this.AutoConnectcheckBox.Text = "AutoConnect";
            this.AutoConnectcheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 158);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(59, 13);
            this.label5.TabIndex = 14;
            this.label5.Text = "NumLengh";
            // 
            // NumLenghtextBox
            // 
            this.NumLenghtextBox.Location = new System.Drawing.Point(94, 155);
            this.NumLenghtextBox.Name = "NumLenghtextBox";
            this.NumLenghtextBox.Size = new System.Drawing.Size(100, 20);
            this.NumLenghtextBox.TabIndex = 13;
            // 
            // SaveOptions
            // 
            this.SaveOptions.Location = new System.Drawing.Point(213, 174);
            this.SaveOptions.Name = "SaveOptions";
            this.SaveOptions.Size = new System.Drawing.Size(101, 23);
            this.SaveOptions.TabIndex = 12;
            this.SaveOptions.Text = "SaveOptions";
            this.SaveOptions.UseVisualStyleBackColor = true;
            this.SaveOptions.Click += new System.EventHandler(this.SaveOptions_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 130);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Phone number";
            // 
            // PhonetextBox
            // 
            this.PhonetextBox.Location = new System.Drawing.Point(94, 124);
            this.PhonetextBox.Name = "PhonetextBox";
            this.PhonetextBox.Size = new System.Drawing.Size(100, 20);
            this.PhonetextBox.TabIndex = 10;
            this.PhonetextBox.Text = "3016";
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.Enabled = false;
            this.btnDisconnect.Location = new System.Drawing.Point(212, 45);
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(101, 23);
            this.btnDisconnect.TabIndex = 9;
            this.btnDisconnect.Text = "Disconnect";
            this.btnDisconnect.UseVisualStyleBackColor = true;
            this.btnDisconnect.Click += new System.EventHandler(this.btnDisconnect_Click);
            // 
            // btnConnect
            // 
            this.btnConnect.Location = new System.Drawing.Point(212, 19);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(101, 23);
            this.btnConnect.TabIndex = 8;
            this.btnConnect.Text = "Connect";
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 102);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Password";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "User";
            // 
            // tbUser
            // 
            this.tbUser.Location = new System.Drawing.Point(94, 71);
            this.tbUser.Name = "tbUser";
            this.tbUser.Size = new System.Drawing.Size(100, 20);
            this.tbUser.TabIndex = 5;
            this.tbUser.Text = "demo";
            // 
            // tbPassword
            // 
            this.tbPassword.Location = new System.Drawing.Point(94, 97);
            this.tbPassword.Name = "tbPassword";
            this.tbPassword.PasswordChar = '*';
            this.tbPassword.Size = new System.Drawing.Size(100, 20);
            this.tbPassword.TabIndex = 7;
            this.tbPassword.Text = "demo";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port";
            // 
            // tbPort
            // 
            this.tbPort.Location = new System.Drawing.Point(94, 45);
            this.tbPort.Name = "tbPort";
            this.tbPort.Size = new System.Drawing.Size(100, 20);
            this.tbPort.TabIndex = 3;
            this.tbPort.Text = "5038";
            // 
            // lable1
            // 
            this.lable1.AutoSize = true;
            this.lable1.Location = new System.Drawing.Point(6, 24);
            this.lable1.Name = "lable1";
            this.lable1.Size = new System.Drawing.Size(29, 13);
            this.lable1.TabIndex = 0;
            this.lable1.Text = "Host";
            // 
            // tbAddress
            // 
            this.tbAddress.Location = new System.Drawing.Point(94, 19);
            this.tbAddress.Name = "tbAddress";
            this.tbAddress.Size = new System.Drawing.Size(100, 20);
            this.tbAddress.TabIndex = 1;
            this.tbAddress.Text = "10.10.254.2";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "ITLGMCA_CALL";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label6);
            this.splitContainer1.Panel1.Controls.Add(this.pictureBox1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer1.Size = new System.Drawing.Size(319, 316);
            this.splitContainer1.SplitterDistance = 108;
            this.splitContainer1.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Dock = System.Windows.Forms.DockStyle.Right;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label6.Location = new System.Drawing.Point(234, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(85, 72);
            this.label6.TabIndex = 1;
            this.label6.Text = "Medialog to\r\nAsterisk\r\nConnector\r\n20170301";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Image = global::ITLGMCA_CALL.Properties.Resources.itlogic_alpha_small;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(319, 108);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(319, 316);
            this.Controls.Add(this.splitContainer1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FormMain";
            this.Text = "ITLGMCA_CALL";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormMain_FormClosed);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.SizeChanged += new System.EventHandler(this.FormMain_SizeChanged);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

		}


		private void manager_Events(object sender, ManagerEvent e)
        {
            if(events_count!= events_completed)
                log("WARNING! events_count<>events_completed");

            events_count++;
#if DEBUG
                //System.Console.WriteLine("{0} EVENT: {1}", DateTime.Now.ToString(), e.GetType().Name);
         /*    if (e.GetType().Name == "NewStateEvent")
             {
                 NewStateEvent ev = (NewStateEvent)e;
                 log("CallerId="+ ev.CallerId);
                 log("CallerIdNum="+ev.CallerIdNum);
                 log("CallerIdLineName="+ ev.CallerIdName);
                 //log("ConnectedLinenum"+ ev.Connectedlinenum);
                 //log("ConnectedLineName="+ ev.ConnectedLineName);
                 log("Channel="+ ev.Channel);
             }
             else
         */
            if (e.GetType().Name == "BridgeEvent")
                {
                    log("DEBUG EVENT: "+ e.GetType().Name);
                    BridgeEvent bridgeEvent = (BridgeEvent)e;
                    log(
                        "BridgeEvent.CallerId1=" + bridgeEvent.CallerId1+"\n"+
                        "BridgeEvent.CallerId2=" + bridgeEvent.CallerId2+"\n"+
                       // "BridgeEvent.Channel=" + bridgeEvent.Channel+"\n"+
                        "BridgeEvent.Channel1=" + bridgeEvent.Channel1+"\n"+
                        "BridgeEvent.Channel2=" + bridgeEvent.Channel2 + "\n" +
                       // "BridgeEvent.Reason=" + bridgeEvent.Reason + "\n" +
                       // "BridgeEvent.Response=" + bridgeEvent.Response + "\n" +
                        "BridgeEvent.UniqueId1=" + bridgeEvent.UniqueId1 + "\n" +
                        "BridgeEvent.UniqueId2=" + bridgeEvent.UniqueId2 //+ "\n"
                       //+ "BridgeEvent.Server=" + bridgeEvent.Server
                        ,false);

                    string exten;
                    if (channels.TryGetValue(bridgeEvent.Channel1, out exten))
                    {
                        log("callednum exten="+exten+" from chanell "+ bridgeEvent.Channel1,false);
                    }
                    else
                        log("callednum from chanell "+bridgeEvent.Channel1+" not found",false);
                }
            if (e.GetType().Name == "BridgeEnterEvent")
            {
                log("DEBUG EVENT: " + e.GetType().Name);
                BridgeEnterEvent bridgeEvent = (BridgeEnterEvent)e;
                log(
                    "BridgeEnterEvent.CallerIdNum=" + bridgeEvent.CallerIdNum + "\n" +
                    "BridgeEnterEvent.CallerIdName=" + bridgeEvent.CallerIdName + "\n" +
                    "BridgeEnterEvent.ConnectedLineNum=" + bridgeEvent.ConnectedLineNum + "\n" +
                    "BridgeEnterEvent.ConnectedLineName=" + bridgeEvent.ConnectedLineName + "\n" +
                    "BridgeEnterEvent.Channel=" + bridgeEvent.Channel + "\n" +

                    "BridgeEnterEvent.UniqueId=" + bridgeEvent.UniqueId + "\n" +
                    "BridgeEnterEvent.BridgeUniqueId=" + bridgeEvent.BridgeUniqueId //+ "\n"
                    , false);

                string exten;
                if (channels.TryGetValue(bridgeEvent.Channel, out exten))
                {
                    log("callednum exten=" + exten + " from chanell " + bridgeEvent.Channel, false);
                }
                else
                    log("callednum from chanell " + bridgeEvent.Channel + " not found", false);
            }

       /*         else if (e.GetType().Name == "NewChannelEvent")
                {
                    log("EVENT: "+ e.GetType().Name);
                    NewChannelEvent ev = (NewChannelEvent)e;
                    log(
                        "NewChannelEvent.CallerId="+ ev.CallerId + "\n" +
                        "NewChannelEvent.CallerIdnum="+ ev.CallerIdNum + "\n" +
                        "NewChannelEvent.CallerIdName=" + ev.CallerIdName + "\n" +
                    //"NewChannelEvent.ConnectedLinenum="+ ev.Connectedlinenum);
                    //"NewChannelEvent.ConnectedLineName="+ ev.ConnectedLineName);
                        "NewChannelEvent.Channel="+ ev.Channel
                    );

                }
                else if (e.GetType().Name == "UnlinkEvent")
                {
                    UnlinkEvent ev = (UnlinkEvent)e;
                    log("UnlinkEvent.Channel1="+ ev.Channel1);
                }
                  */          
#endif
            if (e.GetType().Name == "NewChannelEvent")
            {
                NewChannelEvent ev= (NewChannelEvent)e;
                string exten;
                if (ev.Attributes.TryGetValue("exten", out exten))
                {
                    if (exten.Length>0 && !channels.ContainsKey(ev.Channel))
                        channels.Add(ev.Channel, exten);

                    log("NewChannelEvent.exten=" + exten + " for " + ev.Channel);
                }
                else
                    log("NewChannelEvent no exten attr for " + ev.Channel);  
            }
       /*     else if (e.GetType().Name == "UnlinkEvent")
            {
                UnlinkEvent ev = (UnlinkEvent)e;
                channels.Remove(ev.Channel1);
            }
         */   else if (e.GetType().Name == "BridgeEvent")
            {
                  /* old Asterisk version */
                BridgeEvent bridgeEvent = (BridgeEvent)e;
                //----------
                string exten;
                channels.TryGetValue(bridgeEvent.Channel1, out exten);
                log("Checking BridgeEvent.chanell=" + bridgeEvent.Channel1 + "\texten=" + exten + "\t" + bridgeEvent.CallerId2);
                //----------


                if (bridgeEvent.CallerId2 == this.phone_number && bridgeEvent.CallerId1.Length >= this.phonelength && bridgeEvent.BridgeState == BridgeEvent.BridgeStates.BRIDGE_STATE_LINK && !this.answered.Contains(bridgeEvent.UniqueId2))
                {
                    //
                    log("Processing bridgeEvent...", false);

                    if (settings.IgnoreChannel1String.Length > 0 && bridgeEvent.Channel1.Contains(settings.IgnoreChannel1String))
                    {
                        log("ignoring chanell \"" + bridgeEvent.Channel1 + "\" with mask \"" + settings.IgnoreChannel1String + "\"", false);
                        return;
                    }
                    if (settings.IgnoreChannel2String.Length > 0 && bridgeEvent.Channel2.Contains(settings.IgnoreChannel2String))
                    {
                        log("ignoring chanel2 \"" + bridgeEvent.Channel2 + "\" with mask \"" + settings.IgnoreChannel2String + "\"", false);
                        return;
                    }

                    this.answered.Add(bridgeEvent.UniqueId2);

                    List<string> contents = new List<string>();

                    contents.Add("PHONE=" + bridgeEvent.CallerId1);
                    contents.Add("CALL_UID=" + bridgeEvent.UniqueId1);
                    contents.Add("CallerId2=" + bridgeEvent.CallerId2);
                    contents.Add("Channel=" + bridgeEvent.Channel);
                    contents.Add("Channel1=" + bridgeEvent.Channel1);
                    contents.Add("Channel2=" + bridgeEvent.Channel2);
                    contents.Add("Reason=" + bridgeEvent.Reason);
                    contents.Add("Response=" + bridgeEvent.Response);
                    contents.Add("UniqueId1=" + bridgeEvent.UniqueId1);
                    contents.Add("UniqueId2=" + bridgeEvent.UniqueId2);
                    contents.Add("Server=" + bridgeEvent.Server);

                    string callednum = "";
                    /* вызываемый номер, отрезка из строки вида SIP/123456-00003f97 */
                    callednum = bridgeEvent.Channel1.IndexOf('-') > 0 ? bridgeEvent.Channel1.Split('-')[0] : bridgeEvent.Channel1;
                    callednum = callednum.IndexOf('/') > 0 ? callednum.Split('/')[1] : callednum;

                    //string exten;
                    channels.TryGetValue(bridgeEvent.Channel1, out exten);

                    if (exten != null && exten.Length > 0)
                    {
                        log("callednum from chanell exten " + exten, false);
                        callednum = exten;
                    }
                    else
                        log("callednum from bridgeevent " + bridgeEvent.Channel1, false);

                    //замена 8495 1234567 на 7495 1234567
                    if (settings.Replase8 && callednum.StartsWith("8") && callednum.Length == 11)
                        callednum = "7" + callednum.Remove(0, 1);

                    contents.Add("callednum=" + callednum);

                    if (tel_dic != null && !string.IsNullOrEmpty(settings.CallerNameColumn))
                    {
                        /* поиск номера в справочнике */
                        string CalledName;
                        if (tel_dic.TryGetValue(callednum, out CalledName))
                            contents.Add(settings.CallerNameColumn + "=" + CalledName);
                    }

                    log(" !!! SENDING EVENT:");
                    for (int i = 0; i < contents.Count; i++)
                        log(contents[i], false);

                    try
                    {
                        File.WriteAllLines(medialog_file, contents.ToArray(), Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Cannot write file " + medialog_file, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        log("Cannot write file " + medialog_file+ ": " + ex.Message);
                    }

                    IntPtr intPtr = FormMain.FindWindow("TfMain", null);
                    if ((IntPtr)0 != intPtr)
                    {
                        FormMain.SendMessage(intPtr, 1803, (IntPtr)0, (IntPtr)0);
                    }
                }
                else
                {
                    string str =
                        (bridgeEvent.CallerId1.Length >= this.phonelength ? "" : "CallerId1.Length is low") +
                        (bridgeEvent.BridgeState == BridgeEvent.BridgeStates.BRIDGE_STATE_LINK ? "" : ("BRIDGE_STATE_LINK!=" + bridgeEvent.BridgeState.ToString())) +
                        (this.answered.Contains(bridgeEvent.UniqueId2) ? (bridgeEvent.UniqueId2 + " DUP!") : "");
                    if (str.Length > 0)
                        log(str);
                }
            }
            else if (e.GetType().Name == "BridgeEnterEvent")
            {
                BridgeEnterEvent BridgeEnterEvent = (BridgeEnterEvent)e;
                //----------
                string exten;
                channels.TryGetValue(BridgeEnterEvent.Channel, out exten);
                log("Checking BridgeEnterEvent.chanel=" + BridgeEnterEvent.Channel + "\texten=" + exten + "\t" + BridgeEnterEvent.ConnectedLineNum);
                //----------

                if (BridgeEnterEvent.ConnectedLineNum == this.phone_number && BridgeEnterEvent.CallerIdNum.Length >= this.phonelength /*&& Event.BridgeState == BridgeEvent.BridgeStates.BRIDGE_STATE_LINK*/ && !this.answered.Contains(BridgeEnterEvent.UniqueId))
                {
                    //
                    log("Processing BridgeEnterEvent...", false);

                    if (settings.IgnoreChannel1String.Length > 0 && BridgeEnterEvent.Channel.Contains(settings.IgnoreChannel1String))
                    {
                        log("ignoring chanell \"" + BridgeEnterEvent.Channel + "\" with mask \"" + settings.IgnoreChannel1String + "\"", false);
                        return;
                    }
                    /*
                    if (settings.IgnoreChannel2String.Length > 0 && bridgeEvent.Channel2.Contains(settings.IgnoreChannel2String))
                    {
                        log("ignoring chanel2 \"" + bridgeEvent.Channel2 + "\" with mask \"" + settings.IgnoreChannel2String + "\"", false);
                        return;
                    }
                    */
                    this.answered.Add(BridgeEnterEvent.UniqueId);

                    List<string> contents = new List<string>();
                    /*
    "BridgeEnterEvent.CallerIdNum=" + bridgeEvent.CallerIdNum + "\n" +
    "BridgeEnterEvent.CallerIdName=" + bridgeEvent.CallerIdName + "\n" +
    "BridgeEnterEvent.ConnectedLineNum=" + bridgeEvent.ConnectedLineNum + "\n" +
    "BridgeEnterEvent.ConnectedLineName=" + bridgeEvent.ConnectedLineName + "\n" +
    "BridgeEnterEvent.Channel=" + bridgeEvent.Channel + "\n" +

    "BridgeEvent.UniqueId1=" + bridgeEvent.UniqueId + "\n" +
    "BridgeEvent.BridgeUniqueId=" + bridgeEvent.BridgeUniqueId //+ "\n"
 * */
                    contents.Add("PHONE=" + BridgeEnterEvent.CallerIdNum);
                    contents.Add("CALL_UID=" + BridgeEnterEvent.UniqueId);
                    contents.Add("CallerId2=" + BridgeEnterEvent.ConnectedLineNum);

                    contents.Add("CallerIdNum=" + BridgeEnterEvent.CallerIdNum);
                    contents.Add("CallerIdName=" + BridgeEnterEvent.CallerIdName);
                    contents.Add("ConnectedLineNum=" + BridgeEnterEvent.ConnectedLineNum);
                    contents.Add("ConnectedLineName=" + BridgeEnterEvent.ConnectedLineName);

                    contents.Add("Channel=" + BridgeEnterEvent.Channel);
                    contents.Add("UniqueId=" + BridgeEnterEvent.UniqueId);
                    contents.Add("Server=" + BridgeEnterEvent.Server);
                    contents.Add("BridgeUniqueId=" + BridgeEnterEvent.BridgeUniqueId);

                    string callednum = "";
                    /* вызываемый номер, отрезка из строки вида SIP/123456-00003f97 */
                    callednum = BridgeEnterEvent.Channel.IndexOf('-') > 0 ? BridgeEnterEvent.Channel.Split('-')[0] : BridgeEnterEvent.Channel;
                    callednum = callednum.IndexOf('/') > 0 ? callednum.Split('/')[1] : callednum;

                    //string exten;
                    channels.TryGetValue(BridgeEnterEvent.Channel, out exten);

                    if (exten != null && exten.Length > 0)
                    {
                        log("callednum from chanell exten " + exten, false);
                        callednum = exten;
                    }
                    else
                        log("callednum from bridgeevent " + BridgeEnterEvent.Channel, false);

                    //замена 8495 1234567 на 7495 1234567
                    if (settings.Replase8 && callednum.StartsWith("8") && callednum.Length == 11)
                        callednum = "7" + callednum.Remove(0, 1);

                    contents.Add("callednum=" + callednum);

                    if (tel_dic != null && !string.IsNullOrEmpty(settings.CallerNameColumn))
                    {
                        /* поиск номера в справочнике */
                        string CalledName;
                        if (tel_dic.TryGetValue(callednum, out CalledName))
                            contents.Add(settings.CallerNameColumn + "=" + CalledName);
                    }

                    log(" !!! SENDING EVENT:");
                    for (int i = 0; i < contents.Count; i++)
                        log(contents[i], false);

                    try
                    {
                        File.WriteAllLines(medialog_file, contents.ToArray(), Encoding.Default);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Cannot write file " + medialog_file, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        log("Cannot write file " + medialog_file + ": " + ex.Message);
                    }

                    IntPtr intPtr = FormMain.FindWindow("TfMain", null);
                    if ((IntPtr)0 != intPtr)
                    {
                        FormMain.SendMessage(intPtr, 1803, (IntPtr)0, (IntPtr)0);
                    }
                }
                else
                {
                    string str =
                        (BridgeEnterEvent.CallerIdNum.Length >= this.phonelength ? "" : "CallerIdNum.Length is low") +
                        /*(bridgeEvent.BridgeState == BridgeEvent.BridgeStates.BRIDGE_STATE_LINK ? "" : ("BRIDGE_STATE_LINK!=" + bridgeEvent.BridgeState.ToString())) +*/
                        (this.answered.Contains(BridgeEnterEvent.UniqueId) ? (BridgeEnterEvent.UniqueId + " DUP!") : "");
                    if (str.Length > 0)
                        log(str);
                }
            }

            events_completed++;
        }

		private void notifyIcon1_DoubleClick(object sender, EventArgs e)
		{
			base.Show();
			base.WindowState = FormWindowState.Normal;
			this.notifyIcon1.Visible = false;
		}

		private void reconnect_event(object sender, ManagerEvent e)
		{
            log("RECONNECT EVENT: "+ e.GetType().Name);

			if (!this.manager.IsConnected())
			{
				this.manager.Login();
			}
		}

		private void SaveOptions_Click(object sender, EventArgs e)
		{
			StreamWriter streamWriter = File.CreateText("ITLGMCA_CALL.CONF");
			streamWriter.WriteLine(this.tbAddress.Text);
			streamWriter.WriteLine(this.tbPort.Text);
			streamWriter.WriteLine(this.tbUser.Text);
			streamWriter.WriteLine(FormMain.Encrypt(this.tbPassword.Text, FormMain.Encrypt(this.test1, this.test2, "Kosher", "SHA1", 2, "OFRna73m*aze01xY", 256), "Kosher", "SHA1", 2, "OFRna73m*aze01xY", 256));
			streamWriter.WriteLine(this.PhonetextBox.Text);
			streamWriter.WriteLine(this.NumLenghtextBox.Text);
			streamWriter.WriteLine(this.AutoConnectcheckBox.Checked.ToString());
			streamWriter.Close();

            settings.Save();
		}

        private Dictionary<string, string> GetData()
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            //OleDbConnection conn = new  OleDbConnection ("Provider=sqloledb;Data Source=myServerAddress;Initial Catalog=myDataBase;Integrated Security=SSPI;");
            //OleDbConnection conn = new OleDbConnection("Provider=sqloledb;Data Source=dev-srv\\sqlexpress;Initial Catalog=medialog_750;User Id=sa;Password=1;");
            //OleDbCommand comm = new OleDbCommand("select replace(dbo.PreparePhone(phone,'',''),'+7','8') as tel ,system as name from z_source",conn);

            string connstring;
            connstring = settings.MedialogConnectionString;
            if(!string.IsNullOrEmpty(settings.password))
                connstring+=";Password="+Decrypt(settings.password, FormMain.Encrypt(this.test1, this.test2));

            //System.Console.WriteLine("connect SQL: {0} ", connstring);

            OleDbConnection conn=null;
            OleDbCommand comm=null;

            try
            {
                conn = new OleDbConnection(connstring);
                comm = new OleDbCommand(settings.MedialogQuery, conn);

                conn.Open();

                OleDbDataReader sr = comm.ExecuteReader();

                if (sr.HasRows)
                {
                    while (sr.Read())
                    {
                        //dict.Add(sr["tel"].ToString(), sr["name"].ToString());
                        
                        dict.Add(sr.GetString(0), sr.GetValue(1).ToString());
                    }

                    if (string.IsNullOrEmpty(settings.CallerNameColumn))
                        settings.CallerNameColumn = sr.GetName(1);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,"Error connect to SQL",MessageBoxButtons.OK,MessageBoxIcon.Error);
                log("Error connect to SQL: "+ ex.Message);
            }

            if(conn!=null && conn.State==System.Data.ConnectionState.Open)
                 conn.Close();

            return dict;
        }

        public void log(string str, bool with_time=true)
        {
            if (with_time)
            {
                string time = DateTime.Now.ToString();
                str = time + ": " + str;
            }

            Console.WriteLine(str);

            if (settings.DebugFile)
            {
                try
                {
                    System.IO.File.AppendAllText("log.txt", str + "\n", Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
	}
}