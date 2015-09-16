// inventor name: Hatem Mostafa
// Date: 19/3/2006

using System;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Linq;
using System.Resources;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Web;
using LiteLib;


namespace Crawler
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class CrawlerForm : System.Windows.Forms.Form
	{
		// unique Uri's queue
		private Queue queueURLS;
		// thread that take the browse editor text to parse it
		private Thread threadParse;
		// binary tree to keep unique Uri's
		private SortTree urlStorage;
		// Performance Counter to measure CPU usage
		private System.Diagnostics.PerformanceCounter cpuCounter; 
		// Performance Counter to measure memory usage
		private System.Diagnostics.PerformanceCounter ramCounter; 
		
		// number of bytes downloaded
		private int nByteCount;
		private int ByteCount
		{
			get	{	return nByteCount;	}
			set
			{
				nByteCount = value;
                this.Invoke((MethodInvoker)delegate
                {
                    this.statusBarPanelByteCount.Text = Commas(nByteCount / 1024 + 1) + " KB";
                });
				
			}
		}

		// number of errors during the download process
		private int nErrorCount;
		private int ErrorCount
		{
			get	{	return nErrorCount;	}
			set
			{
				nErrorCount = value;

			    this.Invoke((MethodInvoker) delegate
			    {
			        this.statusBarPanelErrors.Text = Commas(nErrorCount) + " errors";
			    });
			}
		}

		// number of Uri's found
		private int nURLCount;
		private int URLCount
		{
			get	{	return nURLCount;	}
			set
			{
				nURLCount = value;
                this.Invoke((MethodInvoker)delegate
                {
                    this.statusBarPanelURLs.Text = Commas(nURLCount) + " URL found";
                });
				
			}
		}

		// available memory
		private float nFreeMemory;
		private float FreeMemory
		{
			get	{	return nFreeMemory;	}
			set
			{
				nFreeMemory = value;
                this.Invoke((MethodInvoker)delegate
                {
                    this.statusBarPanelMem.Text = nFreeMemory + " Mb Available";
                });
				
			}
		}

		// CPU usage
		private int nCPUUsage;
		private int CPUUsage
		{
			get	{	return nCPUUsage;	}
			set
			{
				nCPUUsage = value;
                this.Invoke((MethodInvoker)delegate
                {
                    this.statusBarPanelCPU.Text = "CPU usage " + nCPUUsage + "%";

                    Icon icon = Icon.FromHandle(((Bitmap)imageListPercentage.Images[value / 10]).GetHicon());
                    this.statusBarPanelCPU.Icon = icon;
                });
				
			}
		}

		// download folder
		private string strDownloadfolder;
		private string Downloadfolder
		{
			get	{	return strDownloadfolder;	}
			set
			{
				strDownloadfolder = value;
				strDownloadfolder = strDownloadfolder.TrimEnd('\\');
			}
		}
		
		// number of files downloaded
		private int nFileCount;
		private int FileCount
		{
			get	{	return nFileCount;	}
			set
			{
				nFileCount = value;
			    this.Invoke((MethodInvoker) delegate
			    {
			        this.statusBarPanelFiles.Text = Commas(nFileCount) + " file(s) downloaded";
			    });

			}
		}
		
		// threads array
		private Thread[] threadsRun;

		// number of running threads
		private int nThreadCount;
		private int ThreadCount
		{
			get	{	return nThreadCount;	}
			set
			{
				Monitor.Enter(this.listViewThreads);
				try
				{
					for(int nIndex = 0; nIndex < value; nIndex ++)
					{
						// check if thread not created or not suspended
						if(threadsRun[nIndex] == null || threadsRun[nIndex].ThreadState != ThreadState.Suspended)
						{	
							// create new thread
							threadsRun[nIndex] = new Thread(new ThreadStart(ThreadRunFunction));
							// set thread name equal to its index
							threadsRun[nIndex].Name = nIndex.ToString();
							// start thread working function
							threadsRun[nIndex].Start();
							// check if thread dosn't added to the view
							if(nIndex == this.listViewThreads.Items.Count)
							{
								// add a new line in the view for the new thread
								ListViewItem item = this.listViewThreads.Items.Add((nIndex+1).ToString(), 0);
								string[] subItems = { "", "", "", "0", "0%" };
								item.SubItems.AddRange(subItems);
							}
						}
						// check if the thread is suspended
						else	if(threadsRun[nIndex].ThreadState == ThreadState.Suspended)
						{
							// get thread item from the list
							ListViewItem item = this.listViewThreads.Items[nIndex];
							item.ImageIndex = 1;
							item.SubItems[2].Text = "Resume";
							// resume the thread
							threadsRun[nIndex].Resume();
						}
					}
					// change thread value
					nThreadCount = value;
				}
				catch(Exception)
				{
				}
				Monitor.Exit(this.listViewThreads);
			}
		}

		// MIME types string
		private string strMIMETypes = GetMIMETypes();
		private string MIMETypes
		{
			get	{	return strMIMETypes;	}
			set	{	strMIMETypes = value;	}
		}

		// encoding text that includes all settings types in one string
		private Encoding encoding = GetTextEncoding();
		private Encoding TextEncoding
		{
			get	{	return encoding;	}
			set	{	encoding = value;	}
		}

		// timeout of sockets send and receive
		private int nRequestTimeout;
		private int RequestTimeout
		{
			get	{	return nRequestTimeout;	}
			set	{	nRequestTimeout = value;	}
		}

		// the time that each thread sleeps when the refs queue empty
		private int nSleepFetchTime;
		private int SleepFetchTime
		{
			get	{	return nSleepFetchTime;	}
			set	{	nSleepFetchTime = value;	}
		}		
		
		// List of a user defined list of restricted words to enable user to prevent any bad pages 
		private string[] strExcludeWords;
		private string[] ExcludeWords
		{
			get	{	return strExcludeWords;	}
			set	{	strExcludeWords = value;	}
		}

		// List of a user defined list of restricted files extensions to avoid paring non-text data 
		private string[] strExcludeFiles;
		private string[] ExcludeFiles
		{
			get	{	return strExcludeFiles;	}
			set	{	strExcludeFiles = value;	}
		}

		// List of a user defined list of restricted hosts extensions to avoid blocking by these hosts
		private string[] strExcludeHosts;
		private string[] ExcludeHosts
		{
			get	{	return strExcludeHosts;	}
			set	{	strExcludeHosts = value;	}
		}
		
		// the number of requests to keep in the requests view for review requests details
		private int nLastRequestCount;
		private int LastRequestCount
		{
			get	{	return nLastRequestCount;	}
			set	{	nLastRequestCount = value;	}
		}
		
		// the time that each thread sleep after handling any request, 
		// which is very important value to prevent Hosts from blocking the crawler due to heavy load
		private int nSleepConnectTime;
		private int SleepConnectTime
		{
			get	{	return nSleepConnectTime;	}
			set	{	nSleepConnectTime = value;	}
		}

		// represents the depth of navigation in the crawling process
		private int nWebDepth;
		private int WebDepth
		{
			get	{	return nWebDepth;	}
			set	{	nWebDepth = value;	}
		}

		// MIME types are the types that are supported to be downloaded by the crawler 
		// and the crawler includes a default types to be used. 
		private bool bAllMIMETypes;
		private bool AllMIMETypes
		{
			get	{	return bAllMIMETypes;	}
			set	{	bAllMIMETypes = value;	}
		}		

		// to limit crawling process to the same host of the original URL
		private bool bKeepSameServer;
		private bool KeepSameServer
		{
			get	{	return bKeepSameServer;	}
			set	{	bKeepSameServer = value;	}
		}		
		
		// means keep socket connection opened for subsequent requests to avoid reconnect time
		private bool bKeepAlive;
		private bool KeepAlive
		{
			get	{	return bKeepAlive;	}
			set	{	bKeepAlive = value;	}
		}			
		
		// flag to be used to stop all running threads when user request to stop
		bool ThreadsRunning;

		private System.Windows.Forms.MenuItem menuItemFile;
		private System.Windows.Forms.MenuItem menuItemExit;
		private System.Windows.Forms.MenuItem menuItemOptions;
		private System.Windows.Forms.MenuItem menuItemSettings;
		private System.Windows.Forms.MainMenu mainMenu;
		private System.Windows.Forms.ToolBar toolBarMain;
		private System.Windows.Forms.ImageList imageList2;
		private System.ComponentModel.IContainer components;

		private System.Windows.Forms.StatusBar statusBar;
		private System.Windows.Forms.ToolBarButton toolBarButtonPause;
		private System.Windows.Forms.ToolBarButton toolBarButtonStop;
		private System.Windows.Forms.ToolBarButton toolBarButton1;
		private System.Windows.Forms.ToolBarButton toolBarButtonDeleteAll;
		private System.Windows.Forms.ToolBarButton toolBarButton2;
		private System.Windows.Forms.ToolBarButton toolBarButtonSettings;
		private System.Windows.Forms.MenuItem menuItemAbout;
		private System.Windows.Forms.ToolBar toolBarWeb;
		private System.Windows.Forms.TabControl tabControlRightView;
		private System.Windows.Forms.ComboBox comboBoxWeb;
		private System.Windows.Forms.TabPage tabPageThreads;
		private System.Windows.Forms.ListView listViewThreads;
		private System.Windows.Forms.ColumnHeader columnHeaderTHreadID;
		private System.Windows.Forms.ColumnHeader columnHeaderThreadURL;
		private System.Windows.Forms.ColumnHeader columnHeaderThreadBytes;
		private System.Windows.Forms.ColumnHeader columnHeaderThreadPersentage;
		private System.Windows.Forms.ColumnHeader columnHeaderThreadDepth;
		private System.Windows.Forms.StatusBarPanel statusBarPanelMem;
		private System.Windows.Forms.ColumnHeader columnHeaderThreadAction;	
		private System.Windows.Forms.StatusBarPanel statusBarPanelByteCount;
		private System.Windows.Forms.ImageList imageList3;
		private System.Windows.Forms.ToolBarButton toolBarButton4;
		private System.Windows.Forms.Button buttonGo;
		private System.Windows.Forms.ImageList imageList4;
		private System.Windows.Forms.ToolBarButton toolBarButtonContinue;
		private System.Windows.Forms.TabPage tabPageErrors;
		private System.Windows.Forms.ColumnHeader columnHeaderErrorID;
		private System.Windows.Forms.ColumnHeader columnHeaderErrorDescription;
		private System.Windows.Forms.ColumnHeader columnHeaderErrorItem;
		private System.Windows.Forms.Splitter splitter3;
		private System.Windows.Forms.TextBox textBoxErrorDescription;
		private System.Windows.Forms.ListView listViewErrors;
		private System.Windows.Forms.ContextMenu contextMenuBrowse;
		private System.Windows.Forms.MenuItem menuItemBrowseHttp;
		private System.Windows.Forms.StatusBarPanel statusBarPanelErrors;
		private System.Windows.Forms.ToolBarButton toolBarButtonBrowse;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ColumnHeader columnHeaderDate;
		private System.Windows.Forms.Timer timerMem;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem menuItemFileMatches;
		private System.Windows.Forms.MenuItem menuItemOutput;
		private System.Windows.Forms.MenuItem menuItemConnections;
		private System.Windows.Forms.MenuItem menuItemHelp;
		private System.Windows.Forms.ContextMenu contextMenuNavigate;
		private System.Windows.Forms.ContextMenu contextMenuSettings;
		private System.Windows.Forms.MenuItem menuItemSettingsFileMatches;
		private System.Windows.Forms.MenuItem menuItemSettingsOutput;
		private System.Windows.Forms.MenuItem menuItemSettingsConnections;
		private System.Windows.Forms.MenuItem menuItemCopy;
		private System.Windows.Forms.MenuItem menuItemPaste;
		private System.Windows.Forms.MenuItem menuItemCut;
		private System.Windows.Forms.MenuItem menuItemDelete;
		private System.Windows.Forms.MenuItem menuItemSettingsAdvanced;
		private System.Windows.Forms.MenuItem menuItemAdvanced;
		private System.Windows.Forms.TabPage tabPageRequests;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.ListView listViewRequests;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.TextBox textBoxRequest;
		private System.Windows.Forms.MenuItem menuItemHttp;
		private System.Windows.Forms.StatusBarPanel statusBarPanelCPU;
		private System.Windows.Forms.Timer timerConnectionInfo;
		private System.Windows.Forms.ImageList imageListPercentage;
		private System.Windows.Forms.StatusBarPanel statusBarPanelInfo;
		private System.Windows.Forms.StatusBarPanel statusBarPanelFiles;
		private System.Windows.Forms.StatusBarPanel statusBarPanelURLs;
		private System.Windows.Forms.MenuItem menuItem5;

		public CrawlerForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.urlStorage = new SortTree();
			this.threadsRun = new Thread[200];
			this.queueURLS = new Queue();
			this.cpuCounter = new System.Diagnostics.PerformanceCounter(); 
			this.ramCounter = new System.Diagnostics.PerformanceCounter("Memory", "Available MBytes"); 

			this.cpuCounter.CategoryName = "Processor"; 
			this.cpuCounter.CounterName = "% Processor Time"; 
			this.cpuCounter.InstanceName = "_Total"; 

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			this.StopParsing();

			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );

			System.Environment.Exit(0);
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(CrawlerForm));
			this.mainMenu = new System.Windows.Forms.MainMenu();
			this.menuItemFile = new System.Windows.Forms.MenuItem();
			this.menuItemExit = new System.Windows.Forms.MenuItem();
			this.menuItemOptions = new System.Windows.Forms.MenuItem();
			this.menuItemSettings = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.menuItemFileMatches = new System.Windows.Forms.MenuItem();
			this.menuItemOutput = new System.Windows.Forms.MenuItem();
			this.menuItemConnections = new System.Windows.Forms.MenuItem();
			this.menuItemAdvanced = new System.Windows.Forms.MenuItem();
			this.menuItemHelp = new System.Windows.Forms.MenuItem();
			this.menuItemAbout = new System.Windows.Forms.MenuItem();
			this.toolBarMain = new System.Windows.Forms.ToolBar();
			this.toolBarButtonContinue = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonPause = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonStop = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton1 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonDeleteAll = new System.Windows.Forms.ToolBarButton();
			this.toolBarButton2 = new System.Windows.Forms.ToolBarButton();
			this.toolBarButtonSettings = new System.Windows.Forms.ToolBarButton();
			this.contextMenuSettings = new System.Windows.Forms.ContextMenu();
			this.menuItemSettingsFileMatches = new System.Windows.Forms.MenuItem();
			this.menuItemSettingsOutput = new System.Windows.Forms.MenuItem();
			this.menuItemSettingsConnections = new System.Windows.Forms.MenuItem();
			this.menuItemSettingsAdvanced = new System.Windows.Forms.MenuItem();
			this.imageList2 = new System.Windows.Forms.ImageList(this.components);
			this.statusBar = new System.Windows.Forms.StatusBar();
			this.statusBarPanelInfo = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelURLs = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelFiles = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelByteCount = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelErrors = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelCPU = new System.Windows.Forms.StatusBarPanel();
			this.statusBarPanelMem = new System.Windows.Forms.StatusBarPanel();
			this.toolBarWeb = new System.Windows.Forms.ToolBar();
			this.toolBarButtonBrowse = new System.Windows.Forms.ToolBarButton();
			this.contextMenuBrowse = new System.Windows.Forms.ContextMenu();
			this.menuItemBrowseHttp = new System.Windows.Forms.MenuItem();
			this.menuItemHttp = new System.Windows.Forms.MenuItem();
			this.menuItem5 = new System.Windows.Forms.MenuItem();
			this.imageList4 = new System.Windows.Forms.ImageList(this.components);
			this.tabControlRightView = new System.Windows.Forms.TabControl();
			this.tabPageThreads = new System.Windows.Forms.TabPage();
			this.listViewThreads = new System.Windows.Forms.ListView();
			this.columnHeaderTHreadID = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderThreadDepth = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderThreadAction = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderThreadURL = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderThreadBytes = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderThreadPersentage = new System.Windows.Forms.ColumnHeader();
			this.imageList3 = new System.Windows.Forms.ImageList(this.components);
			this.tabPageRequests = new System.Windows.Forms.TabPage();
			this.textBoxRequest = new System.Windows.Forms.TextBox();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.listViewRequests = new System.Windows.Forms.ListView();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
			this.tabPageErrors = new System.Windows.Forms.TabPage();
			this.textBoxErrorDescription = new System.Windows.Forms.TextBox();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.listViewErrors = new System.Windows.Forms.ListView();
			this.columnHeaderErrorID = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderDate = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderErrorItem = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderErrorDescription = new System.Windows.Forms.ColumnHeader();
			this.comboBoxWeb = new System.Windows.Forms.ComboBox();
			this.contextMenuNavigate = new System.Windows.Forms.ContextMenu();
			this.menuItemCut = new System.Windows.Forms.MenuItem();
			this.menuItemCopy = new System.Windows.Forms.MenuItem();
			this.menuItemPaste = new System.Windows.Forms.MenuItem();
			this.menuItemDelete = new System.Windows.Forms.MenuItem();
			this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
			this.buttonGo = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.timerMem = new System.Windows.Forms.Timer(this.components);
			this.imageListPercentage = new System.Windows.Forms.ImageList(this.components);
			this.timerConnectionInfo = new System.Windows.Forms.Timer(this.components);
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelInfo)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelURLs)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelFiles)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelByteCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelErrors)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelCPU)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelMem)).BeginInit();
			this.tabControlRightView.SuspendLayout();
			this.tabPageThreads.SuspendLayout();
			this.tabPageRequests.SuspendLayout();
			this.tabPageErrors.SuspendLayout();
			this.SuspendLayout();
			// 
			// mainMenu
			// 
			this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItemFile,
																					 this.menuItemOptions,
																					 this.menuItemHelp});
			// 
			// menuItemFile
			// 
			this.menuItemFile.Index = 0;
			this.menuItemFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemExit});
			this.menuItemFile.Text = "&File";
			// 
			// menuItemExit
			// 
			this.menuItemExit.Index = 0;
			this.menuItemExit.Shortcut = System.Windows.Forms.Shortcut.CtrlX;
			this.menuItemExit.Text = "E&xit";
			this.menuItemExit.Click += new System.EventHandler(this.menuItemExit_Click);
			// 
			// menuItemOptions
			// 
			this.menuItemOptions.Index = 1;
			this.menuItemOptions.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.menuItemSettings,
																							this.menuItem1,
																							this.menuItemFileMatches,
																							this.menuItemOutput,
																							this.menuItemConnections,
																							this.menuItemAdvanced});
			this.menuItemOptions.Text = "&Options";
			// 
			// menuItemSettings
			// 
			this.menuItemSettings.Index = 0;
			this.menuItemSettings.Text = "&Settings...";
			this.menuItemSettings.Click += new System.EventHandler(this.menuItemSettings_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 1;
			this.menuItem1.Text = "-";
			// 
			// menuItemFileMatches
			// 
			this.menuItemFileMatches.Index = 2;
			this.menuItemFileMatches.Text = "&MIME types...";
			this.menuItemFileMatches.Click += new System.EventHandler(this.menuItemFileMatches_Click);
			// 
			// menuItemOutput
			// 
			this.menuItemOutput.Index = 3;
			this.menuItemOutput.Text = "&Output...";
			this.menuItemOutput.Click += new System.EventHandler(this.menuItemOutput_Click);
			// 
			// menuItemConnections
			// 
			this.menuItemConnections.Index = 4;
			this.menuItemConnections.Text = "&Connections...";
			this.menuItemConnections.Click += new System.EventHandler(this.menuItemConnections_Click);
			// 
			// menuItemAdvanced
			// 
			this.menuItemAdvanced.Index = 5;
			this.menuItemAdvanced.Text = "&Advanced...";
			this.menuItemAdvanced.Click += new System.EventHandler(this.menuItemAdvanced_Click);
			// 
			// menuItemHelp
			// 
			this.menuItemHelp.Index = 2;
			this.menuItemHelp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.menuItemAbout});
			this.menuItemHelp.Text = "&Help";
			// 
			// menuItemAbout
			// 
			this.menuItemAbout.Index = 0;
			this.menuItemAbout.Text = "&About...";
			this.menuItemAbout.Click += new System.EventHandler(this.menuItemAbout_Click);
			// 
			// toolBarMain
			// 
			this.toolBarMain.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBarMain.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						   this.toolBarButtonContinue,
																						   this.toolBarButtonPause,
																						   this.toolBarButtonStop,
																						   this.toolBarButton1,
																						   this.toolBarButtonDeleteAll,
																						   this.toolBarButton2,
																						   this.toolBarButtonSettings});
			this.toolBarMain.ButtonSize = new System.Drawing.Size(16, 16);
			this.toolBarMain.DropDownArrows = true;
			this.toolBarMain.ImageList = this.imageList2;
			this.toolBarMain.Location = new System.Drawing.Point(0, 0);
			this.toolBarMain.Name = "toolBarMain";
			this.toolBarMain.ShowToolTips = true;
			this.toolBarMain.Size = new System.Drawing.Size(680, 28);
			this.toolBarMain.TabIndex = 0;
			this.toolBarMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarMain_ButtonClick);
			// 
			// toolBarButtonContinue
			// 
			this.toolBarButtonContinue.Enabled = false;
			this.toolBarButtonContinue.ImageIndex = 0;
			this.toolBarButtonContinue.ToolTipText = "Coninue parsing process";
			// 
			// toolBarButtonPause
			// 
			this.toolBarButtonPause.Enabled = false;
			this.toolBarButtonPause.ImageIndex = 1;
			this.toolBarButtonPause.ToolTipText = "Pause parsing process";
			// 
			// toolBarButtonStop
			// 
			this.toolBarButtonStop.ImageIndex = 2;
			this.toolBarButtonStop.ToolTipText = "Stop parsing process";
			// 
			// toolBarButton1
			// 
			this.toolBarButton1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButtonDeleteAll
			// 
			this.toolBarButtonDeleteAll.ImageIndex = 3;
			this.toolBarButtonDeleteAll.ToolTipText = "Delete all results";
			// 
			// toolBarButton2
			// 
			this.toolBarButton2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
			// 
			// toolBarButtonSettings
			// 
			this.toolBarButtonSettings.DropDownMenu = this.contextMenuSettings;
			this.toolBarButtonSettings.ImageIndex = 4;
			this.toolBarButtonSettings.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.toolBarButtonSettings.ToolTipText = "Show settings form";
			// 
			// contextMenuSettings
			// 
			this.contextMenuSettings.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.menuItemSettingsFileMatches,
																								this.menuItemSettingsOutput,
																								this.menuItemSettingsConnections,
																								this.menuItemSettingsAdvanced});
			// 
			// menuItemSettingsFileMatches
			// 
			this.menuItemSettingsFileMatches.Index = 0;
			this.menuItemSettingsFileMatches.Text = "&MIME types...";
			this.menuItemSettingsFileMatches.Click += new System.EventHandler(this.menuItemSettingsFileMatches_Click);
			// 
			// menuItemSettingsOutput
			// 
			this.menuItemSettingsOutput.Index = 1;
			this.menuItemSettingsOutput.Text = "&Output...";
			this.menuItemSettingsOutput.Click += new System.EventHandler(this.menuItemSettingsOutput_Click);
			// 
			// menuItemSettingsConnections
			// 
			this.menuItemSettingsConnections.Index = 2;
			this.menuItemSettingsConnections.Text = "&Connections...";
			this.menuItemSettingsConnections.Click += new System.EventHandler(this.menuItemSettingsConnections_Click);
			// 
			// menuItemSettingsAdvanced
			// 
			this.menuItemSettingsAdvanced.Index = 3;
			this.menuItemSettingsAdvanced.Text = "&Advanced...";
			this.menuItemSettingsAdvanced.Click += new System.EventHandler(this.menuItemSettingsAdvanced_Click);
			// 
			// imageList2
			// 
			this.imageList2.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList2.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList2.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList2.ImageStream")));
			this.imageList2.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// statusBar
			// 
			this.statusBar.Location = new System.Drawing.Point(0, 291);
			this.statusBar.Name = "statusBar";
			this.statusBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						 this.statusBarPanelInfo,
																						 this.statusBarPanelURLs,
																						 this.statusBarPanelFiles,
																						 this.statusBarPanelByteCount,
																						 this.statusBarPanelErrors,
																						 this.statusBarPanelCPU,
																						 this.statusBarPanelMem});
			this.statusBar.ShowPanels = true;
			this.statusBar.Size = new System.Drawing.Size(680, 22);
			this.statusBar.TabIndex = 1;
			this.statusBar.Text = "Ready";
			// 
			// statusBarPanelInfo
			// 
			this.statusBarPanelInfo.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statusBarPanelInfo.ToolTipText = "View total parsed uris";
			this.statusBarPanelInfo.Width = 393;
			// 
			// statusBarPanelURLs
			// 
			this.statusBarPanelURLs.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this.statusBarPanelURLs.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.statusBarPanelURLs.ToolTipText = "View unique hits count";
			this.statusBarPanelURLs.Width = 10;
			// 
			// statusBarPanelFiles
			// 
			this.statusBarPanelFiles.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this.statusBarPanelFiles.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.statusBarPanelFiles.ToolTipText = "View total hits count";
			this.statusBarPanelFiles.Width = 10;
			// 
			// statusBarPanelByteCount
			// 
			this.statusBarPanelByteCount.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this.statusBarPanelByteCount.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.statusBarPanelByteCount.ToolTipText = "View total bytes of parsed items";
			this.statusBarPanelByteCount.Width = 10;
			// 
			// statusBarPanelErrors
			// 
			this.statusBarPanelErrors.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
			this.statusBarPanelErrors.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
			this.statusBarPanelErrors.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarPanelErrors.Icon")));
			this.statusBarPanelErrors.ToolTipText = "View errors count";
			this.statusBarPanelErrors.Width = 31;
			// 
			// statusBarPanelCPU
			// 
			this.statusBarPanelCPU.Icon = ((System.Drawing.Icon)(resources.GetObject("statusBarPanelCPU.Icon")));
			this.statusBarPanelCPU.ToolTipText = "CPU usage";
			this.statusBarPanelCPU.Width = 110;
			// 
			// statusBarPanelMem
			// 
			this.statusBarPanelMem.ToolTipText = "Available memory";
			// 
			// toolBarWeb
			// 
			this.toolBarWeb.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.toolBarWeb.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																						  this.toolBarButtonBrowse});
			this.toolBarWeb.ButtonSize = new System.Drawing.Size(50, 26);
			this.toolBarWeb.DropDownArrows = true;
			this.toolBarWeb.ImageList = this.imageList4;
			this.toolBarWeb.Location = new System.Drawing.Point(0, 28);
			this.toolBarWeb.Name = "toolBarWeb";
			this.toolBarWeb.ShowToolTips = true;
			this.toolBarWeb.Size = new System.Drawing.Size(680, 27);
			this.toolBarWeb.TabIndex = 2;
			this.toolBarWeb.ButtonDropDown += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBarWeb_ButtonDropDown);
			// 
			// toolBarButtonBrowse
			// 
			this.toolBarButtonBrowse.DropDownMenu = this.contextMenuBrowse;
			this.toolBarButtonBrowse.ImageIndex = 0;
			this.toolBarButtonBrowse.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.toolBarButtonBrowse.ToolTipText = "Browse text sources";
			// 
			// contextMenuBrowse
			// 
			this.contextMenuBrowse.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							  this.menuItemBrowseHttp});
			// 
			// menuItemBrowseHttp
			// 
			this.menuItemBrowseHttp.Index = 0;
			this.menuItemBrowseHttp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							   this.menuItemHttp,
																							   this.menuItem5});
			this.menuItemBrowseHttp.Text = "&Http(s)";
			// 
			// menuItemHttp
			// 
			this.menuItemHttp.Index = 0;
			this.menuItemHttp.Text = "&http://";
			this.menuItemHttp.Click += new System.EventHandler(this.menuItemHttp_Click);
			// 
			// menuItem5
			// 
			this.menuItem5.Index = 1;
			this.menuItem5.Text = "-";
			// 
			// imageList4
			// 
			this.imageList4.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList4.ImageSize = new System.Drawing.Size(58, 15);
			this.imageList4.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList4.ImageStream")));
			this.imageList4.TransparentColor = System.Drawing.Color.Teal;
			// 
			// tabControlRightView
			// 
			this.tabControlRightView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlRightView.Controls.Add(this.tabPageThreads);
			this.tabControlRightView.Controls.Add(this.tabPageRequests);
			this.tabControlRightView.Controls.Add(this.tabPageErrors);
			this.tabControlRightView.ImageList = this.imageList3;
			this.tabControlRightView.Location = new System.Drawing.Point(0, 56);
			this.tabControlRightView.Name = "tabControlRightView";
			this.tabControlRightView.SelectedIndex = 0;
			this.tabControlRightView.ShowToolTips = true;
			this.tabControlRightView.Size = new System.Drawing.Size(680, 231);
			this.tabControlRightView.TabIndex = 7;
			this.tabControlRightView.Tag = "Main Tab";
			this.tabControlRightView.SelectedIndexChanged += new System.EventHandler(this.tabControlRightView_SelectedIndexChanged);
			// 
			// tabPageThreads
			// 
			this.tabPageThreads.Controls.Add(this.listViewThreads);
			this.tabPageThreads.ImageIndex = 6;
			this.tabPageThreads.Location = new System.Drawing.Point(4, 23);
			this.tabPageThreads.Name = "tabPageThreads";
			this.tabPageThreads.Size = new System.Drawing.Size(672, 204);
			this.tabPageThreads.TabIndex = 3;
			this.tabPageThreads.Text = "Threads";
			this.tabPageThreads.ToolTipText = "View working threads status";
			// 
			// listViewThreads
			// 
			this.listViewThreads.BackColor = System.Drawing.Color.WhiteSmoke;
			this.listViewThreads.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							  this.columnHeaderTHreadID,
																							  this.columnHeaderThreadDepth,
																							  this.columnHeaderThreadAction,
																							  this.columnHeaderThreadURL,
																							  this.columnHeaderThreadBytes,
																							  this.columnHeaderThreadPersentage});
			this.listViewThreads.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewThreads.FullRowSelect = true;
			this.listViewThreads.GridLines = true;
			this.listViewThreads.HideSelection = false;
			this.listViewThreads.Location = new System.Drawing.Point(0, 0);
			this.listViewThreads.MultiSelect = false;
			this.listViewThreads.Name = "listViewThreads";
			this.listViewThreads.Size = new System.Drawing.Size(672, 204);
			this.listViewThreads.SmallImageList = this.imageList3;
			this.listViewThreads.TabIndex = 0;
			this.listViewThreads.View = System.Windows.Forms.View.Details;
			this.listViewThreads.SelectedIndexChanged += new System.EventHandler(this.listViewThreads_SelectedIndexChanged);
			// 
			// columnHeaderTHreadID
			// 
			this.columnHeaderTHreadID.Text = "ID";
			this.columnHeaderTHreadID.Width = 40;
			// 
			// columnHeaderThreadDepth
			// 
			this.columnHeaderThreadDepth.Text = "Depth";
			this.columnHeaderThreadDepth.Width = 43;
			// 
			// columnHeaderThreadAction
			// 
			this.columnHeaderThreadAction.Text = "Action";
			// 
			// columnHeaderThreadURL
			// 
			this.columnHeaderThreadURL.Text = "Uri";
			this.columnHeaderThreadURL.Width = 300;
			// 
			// columnHeaderThreadBytes
			// 
			this.columnHeaderThreadBytes.Text = "Bytes";
			this.columnHeaderThreadBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeaderThreadBytes.Width = 70;
			// 
			// columnHeaderThreadPersentage
			// 
			this.columnHeaderThreadPersentage.Text = "%";
			this.columnHeaderThreadPersentage.Width = 40;
			// 
			// imageList3
			// 
			this.imageList3.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList3.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList3.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList3.ImageStream")));
			this.imageList3.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// tabPageRequests
			// 
			this.tabPageRequests.Controls.Add(this.textBoxRequest);
			this.tabPageRequests.Controls.Add(this.splitter1);
			this.tabPageRequests.Controls.Add(this.listViewRequests);
			this.tabPageRequests.ImageIndex = 8;
			this.tabPageRequests.Location = new System.Drawing.Point(4, 23);
			this.tabPageRequests.Name = "tabPageRequests";
			this.tabPageRequests.Size = new System.Drawing.Size(672, 204);
			this.tabPageRequests.TabIndex = 5;
			this.tabPageRequests.Text = "Requests";
			// 
			// textBoxRequest
			// 
			this.textBoxRequest.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxRequest.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxRequest.Location = new System.Drawing.Point(0, 155);
			this.textBoxRequest.Multiline = true;
			this.textBoxRequest.Name = "textBoxRequest";
			this.textBoxRequest.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxRequest.Size = new System.Drawing.Size(672, 49);
			this.textBoxRequest.TabIndex = 5;
			this.textBoxRequest.Text = "";
			this.textBoxRequest.WordWrap = false;
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 152);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(672, 3);
			this.splitter1.TabIndex = 4;
			this.splitter1.TabStop = false;
			// 
			// listViewRequests
			// 
			this.listViewRequests.BackColor = System.Drawing.Color.WhiteSmoke;
			this.listViewRequests.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							   this.columnHeader2,
																							   this.columnHeader3,
																							   this.columnHeader4});
			this.listViewRequests.Dock = System.Windows.Forms.DockStyle.Top;
			this.listViewRequests.FullRowSelect = true;
			this.listViewRequests.GridLines = true;
			this.listViewRequests.HideSelection = false;
			this.listViewRequests.Location = new System.Drawing.Point(0, 0);
			this.listViewRequests.MultiSelect = false;
			this.listViewRequests.Name = "listViewRequests";
			this.listViewRequests.Size = new System.Drawing.Size(672, 152);
			this.listViewRequests.TabIndex = 3;
			this.listViewRequests.View = System.Windows.Forms.View.Details;
			this.listViewRequests.SelectedIndexChanged += new System.EventHandler(this.listViewRequests_SelectedIndexChanged);
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Date";
			this.columnHeader2.Width = 140;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Request";
			this.columnHeader3.Width = 400;
			// 
			// columnHeader4
			// 
			this.columnHeader4.Text = "Description";
			this.columnHeader4.Width = 0;
			// 
			// tabPageErrors
			// 
			this.tabPageErrors.Controls.Add(this.textBoxErrorDescription);
			this.tabPageErrors.Controls.Add(this.splitter3);
			this.tabPageErrors.Controls.Add(this.listViewErrors);
			this.tabPageErrors.ImageIndex = 7;
			this.tabPageErrors.Location = new System.Drawing.Point(4, 23);
			this.tabPageErrors.Name = "tabPageErrors";
			this.tabPageErrors.Size = new System.Drawing.Size(672, 204);
			this.tabPageErrors.TabIndex = 4;
			this.tabPageErrors.Text = "Errors";
			this.tabPageErrors.ToolTipText = "View reported errors";
			// 
			// textBoxErrorDescription
			// 
			this.textBoxErrorDescription.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxErrorDescription.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxErrorDescription.Location = new System.Drawing.Point(0, 155);
			this.textBoxErrorDescription.Multiline = true;
			this.textBoxErrorDescription.Name = "textBoxErrorDescription";
			this.textBoxErrorDescription.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.textBoxErrorDescription.Size = new System.Drawing.Size(672, 49);
			this.textBoxErrorDescription.TabIndex = 2;
			this.textBoxErrorDescription.Text = "";
			this.textBoxErrorDescription.WordWrap = false;
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter3.Location = new System.Drawing.Point(0, 152);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(672, 3);
			this.splitter3.TabIndex = 1;
			this.splitter3.TabStop = false;
			// 
			// listViewErrors
			// 
			this.listViewErrors.BackColor = System.Drawing.Color.WhiteSmoke;
			this.listViewErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																							 this.columnHeaderErrorID,
																							 this.columnHeaderDate,
																							 this.columnHeaderErrorItem,
																							 this.columnHeaderErrorDescription});
			this.listViewErrors.Dock = System.Windows.Forms.DockStyle.Top;
			this.listViewErrors.FullRowSelect = true;
			this.listViewErrors.GridLines = true;
			this.listViewErrors.HideSelection = false;
			this.listViewErrors.Location = new System.Drawing.Point(0, 0);
			this.listViewErrors.MultiSelect = false;
			this.listViewErrors.Name = "listViewErrors";
			this.listViewErrors.Size = new System.Drawing.Size(672, 152);
			this.listViewErrors.TabIndex = 0;
			this.listViewErrors.View = System.Windows.Forms.View.Details;
			this.listViewErrors.SelectedIndexChanged += new System.EventHandler(this.listViewErrors_SelectedIndexChanged);
			// 
			// columnHeaderErrorID
			// 
			this.columnHeaderErrorID.Text = "ID";
			// 
			// columnHeaderDate
			// 
			this.columnHeaderDate.Text = "Date";
			this.columnHeaderDate.Width = 160;
			// 
			// columnHeaderErrorItem
			// 
			this.columnHeaderErrorItem.Text = "Error";
			this.columnHeaderErrorItem.Width = 343;
			// 
			// columnHeaderErrorDescription
			// 
			this.columnHeaderErrorDescription.Text = "Description";
			this.columnHeaderErrorDescription.Width = 0;
			// 
			// comboBoxWeb
			// 
			this.comboBoxWeb.AllowDrop = true;
			this.comboBoxWeb.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxWeb.BackColor = System.Drawing.Color.WhiteSmoke;
			this.comboBoxWeb.ContextMenu = this.contextMenuNavigate;
			this.comboBoxWeb.ItemHeight = 13;
			this.comboBoxWeb.Location = new System.Drawing.Point(80, 29);
			this.comboBoxWeb.MaxDropDownItems = 20;
			this.comboBoxWeb.Name = "comboBoxWeb";
			this.comboBoxWeb.Size = new System.Drawing.Size(544, 21);
			this.comboBoxWeb.TabIndex = 9;
			this.comboBoxWeb.Tag = "Settings";
			this.comboBoxWeb.Text = "http://";
			this.comboBoxWeb.KeyDown += new System.Windows.Forms.KeyEventHandler(this.comboBoxWeb_KeyDown);
			// 
			// contextMenuNavigate
			// 
			this.contextMenuNavigate.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.menuItemCut,
																								this.menuItemCopy,
																								this.menuItemPaste,
																								this.menuItemDelete});
			// 
			// menuItemCut
			// 
			this.menuItemCut.Index = 0;
			this.menuItemCut.Text = "Cu&t";
			this.menuItemCut.Click += new System.EventHandler(this.menuItemCut_Click);
			// 
			// menuItemCopy
			// 
			this.menuItemCopy.Index = 1;
			this.menuItemCopy.Text = "&Copy";
			this.menuItemCopy.Click += new System.EventHandler(this.menuItemCopy_Click);
			// 
			// menuItemPaste
			// 
			this.menuItemPaste.Index = 2;
			this.menuItemPaste.Text = "&Paste";
			this.menuItemPaste.Click += new System.EventHandler(this.menuItemPaste_Click);
			// 
			// menuItemDelete
			// 
			this.menuItemDelete.Index = 3;
			this.menuItemDelete.Text = "&Delete";
			this.menuItemDelete.Click += new System.EventHandler(this.menuItemDelete_Click);
			// 
			// toolBarButton4
			// 
			this.toolBarButton4.Text = "Go";
			// 
			// buttonGo
			// 
			this.buttonGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonGo.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.buttonGo.ImageIndex = 0;
			this.buttonGo.ImageList = this.imageList1;
			this.buttonGo.Location = new System.Drawing.Point(632, 30);
			this.buttonGo.Name = "buttonGo";
			this.buttonGo.Size = new System.Drawing.Size(40, 22);
			this.buttonGo.TabIndex = 10;
			this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(27, 15);
			this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			this.imageList1.TransparentColor = System.Drawing.Color.Teal;
			// 
			// timerMem
			// 
			this.timerMem.Enabled = true;
			this.timerMem.Interval = 2000;
			this.timerMem.Tick += new System.EventHandler(this.timerMem_Tick);
			// 
			// imageListPercentage
			// 
			this.imageListPercentage.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageListPercentage.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListPercentage.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListPercentage.ImageStream")));
			this.imageListPercentage.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// timerConnectionInfo
			// 
			this.timerConnectionInfo.Enabled = true;
			this.timerConnectionInfo.Interval = 15000;
			this.timerConnectionInfo.Tick += new System.EventHandler(this.timerConnectionInfo_Tick);
			// 
			// CrawlerForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(680, 313);
			this.Controls.Add(this.buttonGo);
			this.Controls.Add(this.comboBoxWeb);
			this.Controls.Add(this.tabControlRightView);
			this.Controls.Add(this.toolBarWeb);
			this.Controls.Add(this.statusBar);
			this.Controls.Add(this.toolBarMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mainMenu;
			this.Name = "CrawlerForm";
			this.Text = "Net Crawler";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.CrawlerForm_Closing);
			this.Load += new System.EventHandler(this.CrawlerForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelInfo)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelURLs)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelFiles)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelByteCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelErrors)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelCPU)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.statusBarPanelMem)).EndInit();
			this.tabControlRightView.ResumeLayout(false);
			this.tabPageThreads.ResumeLayout(false);
			this.tabPageRequests.ResumeLayout(false);
			this.tabPageErrors.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new CrawlerForm());
		}

		private void CrawlerForm_Load(object sender, System.EventArgs e)
		{
			Settings.GetValue(this);

			InitValues();

			this.statusBarPanelInfo.Text = InternetGetConnectedStateString();
		}

		[DllImport("wininet")]
		public static extern int InternetGetConnectedState(ref int lpdwFlags, int dwReserved);
		[DllImport("wininet")]
		public static extern int InternetAutodial(int dwFlags, int hwndParent);
		int nFirstTimeCheckConnection = 0;
		string InternetGetConnectedStateString()
		{
			string strState = "";
			try
			{
				int nState = 0;
				// check internet connection state
				if(InternetGetConnectedState(ref nState, 0) == 0)
					return "You are currently not connected to the internet";
				if((nState & 1) == 1)
					strState = "Modem connection";
				else	if((nState & 2) == 2)
					strState = "LAN connection";
				else	if((nState & 4) == 4)
					strState = "Proxy connection";
				else	if((nState & 8) == 8)
					strState = "Modem is busy with a non-Internet connection";
				else	if((nState & 0x10) == 0x10)
					strState = "Remote Access Server is installed";
				else	if((nState & 0x20) == 0x20)
					return "Offline";
				else	if((nState & 0x40) == 0x40)
					return "Internet connection is currently configured";
					
				// get current machine IP
				IPHostEntry he = Dns.Resolve(Dns.GetHostName());
				strState += ",  Machine IP: "+he.AddressList[0].ToString();
			}
			catch
			{
			}
			return strState;
		}
		void ConnectionInfo()
		{
			try
			{
				int nState = 0;
				if(InternetGetConnectedState(ref nState, 0) == 0)
				{
					if(nFirstTimeCheckConnection++ == 0)
						// ask for dial up or DSL connection
						if(InternetAutodial(1, 0) != 0)
							// check internet connection state again
							InternetGetConnectedState(ref nState, 0);
				}
				if((nState & 2) == 2 || (nState & 4) == 4)
					// reset to reask for connection agina
					nFirstTimeCheckConnection = 0;
			}
			catch
			{
			}
			this.statusBarPanelInfo.Text = InternetGetConnectedStateString();
		}

		private void menuItemExit_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		private void menuItemSettings_Click(object sender, System.EventArgs e)
		{
			ShowSettings(-1);
		}

		void ShowSettings(int nSelectedIndex)
		{
			SettingsForm form = new SettingsForm();
			form.SelectedIndex = nSelectedIndex;
			if(form.ShowDialog(this) == DialogResult.OK)
			{
				ThreadCount = Settings.GetValue("Threads count", 10);
				InitValues();
			}
		}

		void InitValues()
		{
			WebDepth = Settings.GetValue("Web depth", 3);
			RequestTimeout = Settings.GetValue("Request timeout", 20);
			SleepFetchTime = Settings.GetValue("Sleep fetch time", 2);
			SleepConnectTime = Settings.GetValue("Sleep connect time", 1);
			KeepSameServer = Settings.GetValue("Keep same URL server", false);
			AllMIMETypes = Settings.GetValue("Allow all MIME types", true);
			KeepAlive = Settings.GetValue("Keep connection alive", true);
			ExcludeHosts = Settings.GetValue("Exclude Hosts", ".org; .gov;").Replace("*", "").ToLower().Split(';');
			ExcludeWords = Settings.GetValue("Exclude words", "").Split(';');
			ExcludeFiles = Settings.GetValue("Exclude files", "").Replace("*", "").ToLower().Split(';');
			LastRequestCount = Settings.GetValue("View last requests count", 20);
			Downloadfolder = Settings.GetValue("Download folder", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal));
			MIMETypes = GetMIMETypes();
			TextEncoding = GetTextEncoding();
		}

		static Encoding GetTextEncoding()
		{
			Encoding code;
			if(Settings.GetValue("Use windows default code page", true) == true)
				code = Encoding.Default;
			else
			{
				string strCodePage = Settings.GetValue("Settings code page");
				Regex reg = new Regex(@"\([0-9]*\)");
				strCodePage = reg.Match(strCodePage).Value;
				code = Encoding.GetEncoding(int.Parse(strCodePage.Trim('(', ')')));
			}
			return code;
		}
		
		// construct MIME types string from settings xml file
		static string GetMIMETypes()
		{
			string str = "";
			// check for settings xml file existence
			if(File.Exists(Application.StartupPath+"\\Settings.xml"))
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(Application.StartupPath+"\\Settings.xml");
				XmlNode element = doc.DocumentElement.SelectSingleNode("SettingsForm-listViewFileMatches");
				if(element != null)
				{
					for(int n = 0; n < element.ChildNodes.Count; n++)
					{
						XmlNode xmlnode  = element.ChildNodes[n];
						XmlAttribute attribute  = xmlnode.Attributes["Checked"];
						if(attribute == null || attribute.Value.ToLower() != "true")
							continue;
						string[] items = xmlnode.InnerText.Split('\t');
						if(items.Length > 1)
						{
							str += items[0];
							if(items.Length > 2)
								str += '['+items[1]+','+items[2]+']';
							str += ';';
						}
					}
				}
			}
			return str;
		}

		private void buttonGo_Click(object sender, System.EventArgs e)
		{
			StartParsing();
		}

		void StartParsing()
		{			
			this.buttonGo.Enabled = false;
			// insert combo text in the combo list
			if(this.comboBoxWeb.FindStringExact(this.comboBoxWeb.Text) < 0)
				this.comboBoxWeb.Items.Insert(0, this.comboBoxWeb.Text);

			if(threadParse == null || threadParse.ThreadState != ThreadState.Suspended)
			{
				this.urlStorage.Clear();
				// start parsing thread
				threadParse = new Thread(new ThreadStart(RunParser));
				threadParse.Start();
			}

			// update running threads
			ThreadCount = Settings.GetValue("Threads count", 10);

			this.toolBarButtonContinue.Enabled = false;
			this.toolBarButtonPause.Enabled = true;
		}

		void ContinueParsing()
		{
			if(threadParse == null)
				return;
			if(threadParse.ThreadState == ThreadState.Suspended)
				threadParse.Resume();

			// update runnning threads
			ThreadCount = Settings.GetValue("Threads count", 10);

			this.toolBarButtonContinue.Enabled = false;
			this.toolBarButtonPause.Enabled = true;
		}

		// pause all working threads
		void PauseParsing()
		{
			if(threadParse.ThreadState == ThreadState.Running)
				threadParse.Suspend();
			for(int n = 0; n < ThreadCount; n++)
			{
				Thread thread = this.threadsRun[n];
				if(thread.ThreadState == ThreadState.Running)
					thread.Suspend();
			}
			this.toolBarButtonContinue.Enabled = true;
			this.toolBarButtonPause.Enabled = false;
		}
		
		// abort all working threads
		void StopParsing()
		{
			this.queueURLS.Clear();
			ThreadsRunning = false;
			Thread.Sleep(500);
			try
			{
				if(threadParse.ThreadState == ThreadState.Suspended)
					threadParse.Resume();
				threadParse.Abort();
			}
			catch(Exception)
			{
			}
			Monitor.Enter(this.listViewThreads);
			for(int n = 0; n < ThreadCount; n++)
			{
				try
				{
					Thread thread = this.threadsRun[n];
				    this.Invoke((MethodInvoker) delegate
				    {
				        ListViewItem itemLog = this.listViewThreads.Items[int.Parse(thread.Name)];
				        itemLog.SubItems[2].Text = "Stop";
				        itemLog.BackColor = Color.WhiteSmoke;
				        itemLog.ImageIndex = 3;
				        if (thread != null && thread.IsAlive)
				        {
				            if (thread.ThreadState == ThreadState.Suspended)
				                thread.Resume();
				            thread.Abort();
				        }

				    });

				}
				catch(Exception)
				{
				}
			}
			Monitor.Exit(this.listViewThreads);
			this.toolBarButtonContinue.Enabled = true;
			this.toolBarButtonPause.Enabled = false;
			this.buttonGo.Enabled = true;

			this.queueURLS.Clear();
			this.urlStorage.Clear();
		}

		void ThreadRunFunction()
		{
			WebClient client = new WebClient();
			while(ThreadsRunning && int.Parse(Thread.CurrentThread.Name) < this.ThreadCount)
			{
				MyUri uri = DequeueUri();
				if(uri != null)
				{
					if(SleepConnectTime > 0)
						Thread.Sleep(SleepConnectTime*1000);
					ParseUri(uri, client);
				}
				else
					Thread.Sleep(SleepFetchTime*1000);
			}

			Monitor.Enter(this.listViewThreads);
			try
			{
			    ListViewItem item = null;
			    string threadName = Thread.CurrentThread.Name;
			    this.Invoke((MethodInvoker) delegate
			    {
                    item = this.listViewThreads.Items[int.Parse(threadName)];
                    if (!ThreadsRunning)
                        item.SubItems[2].Text = "Stop";
                    item.ImageIndex = 0;
			    });

			}
			catch(Exception)
			{
			}
			Monitor.Exit(this.listViewThreads);
		}

		// push uri to the queue
		bool EnqueueUri(MyUri uri, bool bCheckRepetition)
		{
			// add the uri to the binary tree to check if it is duplicated or not
			if(bCheckRepetition == true && AddURL(ref uri) == false)
				return false;

			Monitor.Enter(queueURLS);
			try
			{
				// add the uri to the queue
				queueURLS.Enqueue(uri);
			}
			catch(Exception)
			{
			}
			Monitor.Exit(queueURLS);

			return true;
		}

		// pop uri from the queue
		MyUri DequeueUri()
		{
			Monitor.Enter(queueURLS);
			MyUri uri = null;
			try
			{
                if (queueURLS.Count > 0)
				    uri = (MyUri)queueURLS.Dequeue();
			}
			catch(Exception)
			{
			}
			Monitor.Exit(queueURLS);
			return uri;
		}

		void RunParser()
		{
			ThreadsRunning = true;
			try
			{
				string strUri = null ;//= this.comboBoxWeb.Text.Trim()
			    this.Invoke((MethodInvoker) delegate
			    {
			        strUri = this.comboBoxWeb.Text.Trim();

			    });
				if(Directory.Exists(strUri) == true)
					ParseFolder(strUri, 0);
				else
				{
					if(File.Exists(strUri) == false)
					{
						Normalize(ref strUri);
						
                        this.Invoke((MethodInvoker)delegate
                        {
                            this.comboBoxWeb.Text = strUri;
                        });
					}
					MyUri uri = new MyUri(strUri);
					this.EnqueueUri(uri, false);
				}
			}
			catch(Exception e)
			{
				LogError(this.comboBoxWeb.Text, e.Message);
				return;
			}


            this.Invoke((MethodInvoker)delegate
            {
                toolBarButtonContinue.Enabled = false;
                buttonGo.Enabled = true;
            });
		}
		
		private void Normalize(ref string strURL)
		{
            if (!strURL.StartsWith("http://") && !strURL.StartsWith("https://"))
				strURL = "http://"+strURL;
			if(strURL.IndexOf("/", 8) == -1)
				strURL += '/';
		}

		bool AddURL(ref MyUri uri)
		{
			foreach(string str in ExcludeHosts)
				if(str.Trim().Length > 0 && uri.Host.ToLower().IndexOf(str.Trim()) != -1)
				{
					LogError(uri.AbsoluteUri, "\r\nHost excluded as it includes reserved pattern ("+str+")");
					return false;
				}
			Monitor.Enter(urlStorage);
			bool bNew = false;
			try
			{
				string strURL = uri.AbsoluteUri;
				bNew = urlStorage.Add(ref strURL).Count == 1;
			}
			catch(Exception)
			{
			}
			Monitor.Exit(urlStorage);
			
			return bNew;
		}

		void LogCell(ref ListViewItem itemLog, int nCell, string str)
		{
			Monitor.Enter(this.listViewThreads);
			try
			{
				itemLog.SubItems[nCell].Text = str;
			}
			catch(Exception)
			{
			}
			Monitor.Exit(this.listViewThreads);
		}

	    private WebClient _webClient = new WebClient();
		void ParseUri(MyUri uri, WebClient client)
		{
            if(client == null) throw new ArgumentNullException("client");
			string strStatus = "";
			// check if connection is kept alive from previous connections or not
            if (client != null && client.IsBusy)
				strStatus += "Connection live to: "+uri.Host+"\r\n\r\n";
			else
				strStatus += "Connecting: "+uri.Host+"\r\n\r\n";

			ListViewItem itemLog = null;
			Monitor.Enter(this.listViewThreads);
			try
			{	// update thread information in the threads view list
			    string threadName = Thread.CurrentThread.Name;
			    this.Invoke((MethodInvoker) delegate
			    {
                    itemLog = this.listViewThreads.Items[int.Parse(threadName)];
			        int nDepth = uri.Depth;
			        itemLog.SubItems[1].Text = nDepth.ToString();
			        itemLog.ImageIndex = 1;
			        itemLog.BackColor = Color.WhiteSmoke;
			        // initialize status to Connect
			        itemLog.SubItems[2].Text = "Connect";
			        itemLog.ForeColor = Color.Red;
			        itemLog.SubItems[3].Text = uri.AbsoluteUri;
			        itemLog.SubItems[4].Text = "";
			        itemLog.SubItems[5].Text = "";
			    });

			}
			catch(Exception)
			{
			}
			Monitor.Exit(this.listViewThreads);

			try
			{

				// retrieve response from web request
                

				
				// check for response extention
				string[] ExtArray = { ".gif", ".jpg", ".css", ".zip", ".exe"	};
				bool bParse = true;
				foreach(string ext in ExtArray)
					if(uri.AbsoluteUri.ToLower().EndsWith(ext))
					{
						bParse = false;
						break;
					}
				foreach(string ext in ExcludeFiles)
					if(ext.Trim().Length > 0 && uri.AbsoluteUri.ToLower().EndsWith(ext))
					{
						bParse = false;
						break;
					}

				// construct path in the hard disk
				string strLocalPath = HttpUtility.UrlDecode(uri.LocalPath);
				// check if the path ends with / to can crate the file on the HD 
				if(strLocalPath.EndsWith("/") || !uri.Segments.Last().Contains("."))
					// check if there is no query like (.asp?i=32&j=212)
					if(uri.Query == "")
						// add a default name for / ended pathes
						strLocalPath += "default.html";
				// check if the uri includes a query string
				if(uri.Query != "")
					// construct the name from the query hash value to be the same if we download it again
					strLocalPath += uri.Query.GetHashCode()+".html";
				// construct the full path folder
                string basePath = this.Downloadfolder + "\\" + uri.Host + HttpUtility.UrlDecode(uri.AbsolutePath.Replace(":", "_").Replace(uri.Segments.Last(), ""));
				// check if the folder not found
				if(Directory.Exists(basePath) == false)
					// create the folder
					Directory.CreateDirectory(basePath);
				// construct the full path name of the file
                string PathName = this.Downloadfolder + "\\" + uri.Host + strLocalPath.Replace("%20", " ").Replace(":", "_");
				// open the output file

			    this.Invoke((MethodInvoker) delegate
			    {
                    itemLog.SubItems[2].Text = "Download";
                    itemLog.ForeColor = Color.Black;
			    });
				
				// receive response buffer
				string strResponse = client.DownloadString(uri);
                // update status text with the request and response headers
                strStatus += client.Headers.ToString() + client.ResponseHeaders.ToString();

                // check for allowed MIME types
                if (AllMIMETypes == false && client.ResponseHeaders["ContentType"] != null && MIMETypes.Length > 0)
                {
                    string strContentType = client.ResponseHeaders["ContentType"].ToLower();
                    int nExtIndex = strContentType.IndexOf(';');
                    if (nExtIndex != -1)
                        strContentType = strContentType.Substring(0, nExtIndex);
                    if (strContentType.IndexOf('*') == -1 && (nExtIndex = MIMETypes.IndexOf(strContentType)) == -1)
                    {
                        LogError(uri.AbsoluteUri, strStatus + "\r\nUnlisted Content-Type (" + strContentType + "), check settings.");
                        return;
                    }
                    // find numbers
                    Match match = new Regex(@"\d+").Match(MIMETypes, nExtIndex);
                    int nMin = int.Parse(match.Value) * 1024;
                    match = match.NextMatch();
                    int nMax = int.Parse(match.Value) * 1024;
                    var contentLength = int.Parse(client.ResponseHeaders["Content-Length"]);
                    if (nMin < nMax && (contentLength < nMin || contentLength > nMax))
                    {
                        LogError(uri.AbsoluteUri, strStatus + "\r\nContentLength limit error (" + contentLength + ")");
                        return;
                    }
                }
                FileStream streamOut = File.Open(PathName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			    byte[] responseBytes = Encoding.UTF8.GetBytes(strResponse);
                streamOut.Write(responseBytes, 0, responseBytes.Length);
				streamOut.Close();
				
				if(client.IsBusy)
					strStatus += "Connection kept alive to be used in subpages.\r\n";
				else
				{
					strStatus += "Connection closed.\r\n";
				}
				// update status
                strStatus += Commas(responseBytes.Length) + " bytes, downloaded to \"" + PathName + "\"\r\n";
				// increment total file count
				FileCount++;
				// increment total bytes count
                ByteCount += responseBytes.Length;

				if(ThreadsRunning == true && bParse == true && uri.Depth < WebDepth)
				{
					strStatus += "\r\nParsing page ...\r\n";

					// check for restricted words
					foreach(string strExcludeWord in ExcludeWords)
						if(strExcludeWord.Trim().Length > 0 && strResponse.IndexOf(strExcludeWord) != -1)
						{
							LogError(uri.AbsoluteUri, strStatus+"\r\nPage includes reserved word ("+strExcludeWord+")");
							EraseItem(itemLog);
							File.Delete(PathName);
							return;
						}			

					// parse the page to search for refs
					string strRef = @"(href|HREF|src|SRC)[ ]*=[ ]*[""'][^""'#>]+[""']";
					MatchCollection matches = new Regex(strRef).Matches(strResponse);
					strStatus += "Found: "+matches.Count+" ref(s)\r\n";
					URLCount += matches.Count;
					foreach(Match match in matches)
					{
						strRef = match.Value.Substring(match.Value.IndexOf('=')+1).Trim('"', '\'', '#', ' ', '>');
						try
						{
							if(strRef.IndexOf("..") != -1 || strRef.StartsWith("/") == true || strRef.StartsWith("http://") == false)
								strRef = new Uri(uri, strRef).AbsoluteUri;
							Normalize(ref strRef);
							MyUri newUri = new MyUri(strRef);
							if(newUri.Scheme != Uri.UriSchemeHttp && newUri.Scheme != Uri.UriSchemeHttps)
								continue;
							if(newUri.Host != uri.Host && KeepSameServer == true)
								continue;
							newUri.Depth = uri.Depth+1;
							if(this.EnqueueUri(newUri, true) == true)
								strStatus += newUri.AbsoluteUri+"\r\n";
						}
						catch(Exception)
						{
						}		
					}
				}
				LogUri(uri.AbsoluteUri, strStatus);
			}
			catch(Exception e)
			{
				LogError(uri.AbsoluteUri, strStatus+e.Message);
			}
			finally
			{
				EraseItem(itemLog);
			}
		}
        private ulong GetWordHash(string word)
        {
            int a;
            ulong hash = 0;
            for (a = 0; a < word.Length; a++) hash = hash * 257 + word[a];
            return hash;
        }

		void EraseItem(ListViewItem item)
		{
		    if (item != null)
		    {
                Monitor.Enter(this.listViewThreads);
                try
                {
                    this.Invoke((MethodInvoker) delegate
                    {
                        item.SubItems[1].Text = "";
                        item.ImageIndex = 0;
                        item.BackColor = Color.WhiteSmoke;
                        item.ForeColor = Color.Black;
                        item.SubItems[2].Text = "";
                        item.SubItems[3].Text = "";
                        item.SubItems[4].Text = "";
                        item.SubItems[5].Text = "";
                    });
                    
                }
                finally
                {
                    Monitor.Exit(this.listViewThreads);
                }
		    }
		}

		void LogUri(string strHead, string strBody)
		{
			if(LastRequestCount > 0)
			{
				Monitor.Enter(this.listViewRequests);
				try
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        ListViewItem item = this.listViewRequests.Items.Insert(0, DateTime.Now.ToString());
                        item.SubItems.AddRange(new String[] { strHead, strBody });
                        while (this.listViewRequests.Items.Count > LastRequestCount)
                            this.listViewRequests.Items.RemoveAt(this.listViewRequests.Items.Count - 1);
                    });
				
					
				}
				catch(Exception)
				{
				}
				Monitor.Exit(this.listViewRequests);
			}
		}

		void LogError(string strHead, string strBody)
		{
			Monitor.Enter(this.listViewErrors);
			try
			{
                this.Invoke((MethodInvoker)delegate
                {
                    ListViewItem item = this.listViewErrors.Items.Add((ErrorCount + 1).ToString());
                    item.SubItems.AddRange(new String[] { DateTime.Now.ToString(), strHead, strBody });
                    while (this.listViewErrors.Items.Count > 1000)
                        this.listViewErrors.Items.RemoveAt(0);
                });
				
			}
			catch(Exception)
			{
			}
			Monitor.Exit(this.listViewErrors);
			ErrorCount++;
		}

		void ParseFolder(string folderName, int nDepth)
		{
			DirectoryInfo dir = new DirectoryInfo(folderName);
			FileInfo[] fia = dir.GetFiles("*.txt");
			foreach(FileInfo f in fia)
			{
				if(ThreadsRunning == false)
					break;
				MyUri uri = new MyUri(f.FullName);
				uri.Depth = nDepth;
				this.EnqueueUri(uri, true);
			}
			
			DirectoryInfo[] dia = dir.GetDirectories();
			foreach(DirectoryInfo d in dia)
			{
				if(ThreadsRunning == false)
					break;
				ParseFolder(d.FullName, nDepth+1);
			}
		}

		private void toolBarMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			if(e.Button == this.toolBarButtonContinue)
				ContinueParsing();
			else	if(e.Button == this.toolBarButtonPause)
				PauseParsing();
			else	if(e.Button == this.toolBarButtonStop)
				StopParsing();
			else	if(e.Button == this.toolBarButtonDeleteAll)
				DeleteAllItems();
			else	if(e.Button == this.toolBarButtonSettings)
				ShowSettings(-1);
		}
		
		void DeleteAllItems()
		{
			if(MessageBox.Show(this, "Do you want to delete all?", "Verify", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				this.listViewErrors.Items.Clear();
				this.listViewRequests.Items.Clear();
				this.urlStorage = new SortTree();
				this.URLCount = 0;
				this.FileCount = 0;
				this.ByteCount = 0;
				this.ErrorCount = 0;
			}
		}

		private void menuItemListDeleteAll_Click(object sender, System.EventArgs e)
		{
			DeleteAllItems();		
		}

		private void tabControlRightView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		}

		string Commas(int nNum)
		{
			string str = nNum.ToString();
			int nIndex = str.Length;
			while(nIndex > 3)
			{
				str = str.Insert(nIndex-3, ",");
				nIndex -= 3;
			}
			return str;
		}

		private void CrawlerForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Settings.SetValue(this);
		}

		private void menuItemAbout_Click(object sender, System.EventArgs e)
		{
			AboutForm form = new AboutForm();
			form.ShowDialog();
		}

		private void comboBoxWeb_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Return)
			{
				if(threadParse == null || threadParse.ThreadState != ThreadState.Running)
					StartParsing();
			}
		}

		private void listViewErrors_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(this.listViewErrors.SelectedItems.Count == 0)
				return;
			ListViewItem item = this.listViewErrors.SelectedItems[0];
			this.textBoxErrorDescription.Text = item.SubItems[3].Text;
		}

		private void timerMem_Tick(object sender, System.EventArgs e)
		{
			FreeMemory = ramCounter.NextValue();
			CPUUsage = (int)cpuCounter.NextValue();
		}

		private void menuItemFileMatches_Click(object sender, System.EventArgs e)
		{
			ShowSettings(0);
		}

		private void menuItemOutput_Click(object sender, System.EventArgs e)
		{
			ShowSettings(1);
		}

		private void menuItemConnections_Click(object sender, System.EventArgs e)
		{
			ShowSettings(2);
		}

		private void menuItemAdvanced_Click(object sender, System.EventArgs e)
		{
			ShowSettings(3);
		}

		private void menuItemSettingsFileMatches_Click(object sender, System.EventArgs e)
		{
			ShowSettings(0);
		}

		private void menuItemSettingsOutput_Click(object sender, System.EventArgs e)
		{
			ShowSettings(1);
		}

		private void menuItemSettingsConnections_Click(object sender, System.EventArgs e)
		{
			ShowSettings(2);
		}

		private void menuItemSettingsAdvanced_Click(object sender, System.EventArgs e)
		{
			ShowSettings(3);
		}

		private void menuItemCut_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(this.comboBoxWeb.SelectedText);
			this.comboBoxWeb.SelectedText = "";
		}

		private void menuItemCopy_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(this.comboBoxWeb.SelectedText);
		}

		private void menuItemPaste_Click(object sender, System.EventArgs e)
		{
			IDataObject obj = Clipboard.GetDataObject();
			if(obj != null)
				this.comboBoxWeb.SelectedText = obj.GetData("System.String").ToString();
		}

		private void menuItemDelete_Click(object sender, System.EventArgs e)
		{
			this.comboBoxWeb.SelectedText = "";
		}

		private void listViewRequests_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(this.listViewRequests.SelectedItems.Count == 0)
				return;
			ListViewItem item = this.listViewRequests.SelectedItems[0];
			if(item.SubItems.Count > 2)
				this.textBoxRequest.Text = item.SubItems[2].Text;
		}

		private void menuItemHttp_Click(object sender, System.EventArgs e)
		{
			this.comboBoxWeb.Text = "http://";
		}

		private void menuItemFileBrowse_Click(object sender, System.EventArgs e)
		{
			OnFileBrowse();
		}
		
		void OnFileBrowse()
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Title = "Select file to parse";
			dlg.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
			if(dlg.ShowDialog() == DialogResult.OK)
				this.comboBoxWeb.Text = dlg.FileName;
		}

		private void toolBarWeb_ButtonDropDown(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			this.menuItemBrowseHttp = new System.Windows.Forms.MenuItem();
			e.Button.DropDownMenu = new System.Windows.Forms.ContextMenu();
			e.Button.DropDownMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							  this.menuItemBrowseHttp});
			// 
			// menuItemBrowseHttp
			// 
			this.menuItemBrowseHttp.Index = 0;
			this.menuItemBrowseHttp.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							   this.menuItemHttp,
																							   this.menuItem5});
			this.menuItemBrowseHttp.Text = "&Http";

			foreach (string item in this.comboBoxWeb.Items)
			{
				try
				{
					Uri uri = new Uri(item);
					MenuItem mi = new MenuItem(item, new System.EventHandler(OnBrowse));
					if(File.Exists(item) == true)
						e.Button.DropDownMenu.MenuItems[2].MenuItems.Add(mi);
					else	if(Directory.Exists(item) == true)
						e.Button.DropDownMenu.MenuItems[1].MenuItems.Add(mi);
					else	if(uri.Scheme == Uri.UriSchemeHttp)
						e.Button.DropDownMenu.MenuItems[0].MenuItems.Add(mi);
				}
				catch(Exception)
				{
				}
			}
		}
		private void OnBrowse(object sender, System.EventArgs e)
		{
			this.comboBoxWeb.Text = ((MenuItem)sender).Text;
		}

		private void menuItemListClear_Click(object sender, System.EventArgs e)
		{
			switch(this.tabControlRightView.SelectedTab.Text)
			{
				case "Threads":
					this.listViewThreads.Items.Clear();
					break;
				case "Requests":
					this.listViewRequests.Items.Clear();
					break;
				case "Errors":
					this.listViewErrors.Items.Clear();
					break;
			}
		}

		private void timerConnectionInfo_Tick(object sender, System.EventArgs e)
		{
			ConnectionInfo();
		}

		private void listViewThreads_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}


	}

	public class MyUri : System.Uri
	{
		public MyUri(string uriString):base(uriString)
		{
		}
		public int Depth;
	}

	public class MyWebRequest 
	{
		public MyWebRequest(Uri uri, bool bKeepAlive)
		{
			Headers = new WebHeaderCollection();
			RequestUri = uri;
			Headers["Host"] = uri.Host;
			KeepAlive = bKeepAlive;
			if(KeepAlive)
				Headers["Connection"] = "Keep-Alive";
			Method = "GET";
		}
		public static MyWebRequest Create(Uri uri, MyWebRequest AliveRequest, bool bKeepAlive)
		{
			if( bKeepAlive &&
				AliveRequest != null &&
				AliveRequest.response != null &&
				AliveRequest.response.KeepAlive && 
				AliveRequest.response.socket.Connected && 
				AliveRequest.RequestUri.Host == uri.Host)
			{
				AliveRequest.RequestUri = uri;
				return AliveRequest;
			}
			return new MyWebRequest(uri, bKeepAlive);
		}
		public MyWebResponse GetResponse()
		{
			if(response == null || response.socket == null || response.socket.Connected == false)
			{
				response = new MyWebResponse();
				response.Connect(this);
				response.SetTimeout(Timeout);
			}
			response.SendRequest(this);
			response.ReceiveHeader();
			return response;
		}

		public int Timeout;
		public WebHeaderCollection Headers;
		public string Header;
		public Uri RequestUri;
		public string Method;
		public MyWebResponse response;
		public bool KeepAlive;
	}
	public class MyWebResponse
	{
	    private HttpWebRequest _httpWebRequest;
		public MyWebResponse()
		{
		}
		public void Connect(MyWebRequest request)
		{
		    _httpWebRequest = (HttpWebRequest) WebRequest.Create(request.RequestUri);
			ResponseUri = request.RequestUri;
			
            //socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //IPEndPoint remoteEP = new IPEndPoint(Dns.Resolve(ResponseUri.Host).AddressList[0], ResponseUri.Port);
            //socket.Connect(remoteEP);			
		}
		public void SendRequest(MyWebRequest request)
		{
			ResponseUri = request.RequestUri;
		    _httpWebRequest.Headers = request.Headers;
			request.Header = request.Method+" "+ResponseUri.PathAndQuery+" HTTP/1.1\r\n"+request.Headers;
			socket.Send(Encoding.ASCII.GetBytes(request.Header));
		}
		public void SetTimeout(int Timeout)
		{
		    _httpWebRequest.Timeout = Timeout*1000;
		    //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, Timeout*1000);
		    //socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, Timeout*1000);
		}
		public void ReceiveHeader()
		{
			Header = "";
			Headers = new WebHeaderCollection();

			byte[] bytes = new byte[10];
			while(socket.Receive(bytes, 0, 1, SocketFlags.None) > 0)
			{
				Header += Encoding.ASCII.GetString(bytes, 0, 1);
				if(bytes[0] == '\n' && Header.EndsWith("\r\n\r\n"))
					break;
			}
			MatchCollection matches = new Regex("[^\r\n]+").Matches(Header.TrimEnd('\r', '\n'));
			for(int n = 1; n < matches.Count; n++)
			{
				string[] strItem = matches[n].Value.Split(new char[] { ':' }, 2);
				if(strItem.Length > 0)
					Headers[strItem[0].Trim()] = strItem[1].Trim();
			}
			// check if the page should be transfered to another location
			if( matches.Count > 0 && (
				matches[0].Value.IndexOf(" 302 ") != -1 || 
				matches[0].Value.IndexOf(" 301 ") != -1))
				// check if the new location is sent in the "location" header
				if(Headers["Location"] != null)
				{
					try		{	ResponseUri = new Uri(Headers["Location"]);		}
					catch	{	ResponseUri = new Uri(ResponseUri, Headers["Location"]);		}
				}
			ContentType = Headers["Content-Type"];
			if(Headers["Content-Length"] != null)
				ContentLength = int.Parse(Headers["Content-Length"]);
			KeepAlive = (Headers["Connection"] != null && Headers["Connection"].ToLower() == "keep-alive") ||
						(Headers["Proxy-Connection"] != null && Headers["Proxy-Connection"].ToLower() == "keep-alive");
		}
		public void Close()
		{
			socket.Close();
		}
		public Uri ResponseUri;
		public string ContentType;
		public int ContentLength;
		public WebHeaderCollection Headers; 
		public string Header;
		public Socket socket;
		public bool KeepAlive;
	}
}
