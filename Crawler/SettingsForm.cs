using System;
using System.Drawing;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using LiteLib;

namespace Crawler
{
	/// <summary>
	/// Summary description for SettingsForm.
	/// </summary>
	public class SettingsForm : System.Windows.Forms.Form
	{
		public int SelectedIndex = -1;
		private System.Windows.Forms.TabControl tabControlSettings;
		private System.Windows.Forms.Button buttonSettingsOK;
		private System.Windows.Forms.Button buttonSettingsCancel;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.CheckBox checkBoxSettingsUseWindowsDefaultCodePage;
		private System.Windows.Forms.ComboBox comboBoxSettingsCodePage;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.NumericUpDown numericUpDownRunThreadsCount;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericUpDownSleepTime;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.NumericUpDown numericUpDown1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericUpDownWebDepth;
		private System.Windows.Forms.Label labelWebDepth;
		private System.Windows.Forms.CheckBox checkBoxKeepURLServer;
		private System.Windows.Forms.TabPage tabPageFileMatches;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.ListView listViewFileMatches;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button buttonAddExt;
		private System.Windows.Forms.Button buttonEditExt;
		private System.Windows.Forms.Button buttonDeleteExt;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Button buttonDownloadFolderBrowse;
		private System.Windows.Forms.TextBox textBoxDownloadFolder;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.NumericUpDown numericUpDown2;
		private System.Windows.Forms.TabPage tabPageAdvanced;
		private System.Windows.Forms.TabPage tabPageOutput;
		private System.Windows.Forms.TabPage tabPageConnections;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox textBoxExcludePages;
		private System.Windows.Forms.TextBox textBoxExcludeFiles;
		private System.Windows.Forms.TextBox textBoxExcludeURLs;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.NumericUpDown numericUpDownRequests;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.ColumnHeader columnHeaderMatchText;
		private System.Windows.Forms.ColumnHeader columnHeaderMatchName;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.CheckBox checkBox1;
		private System.Windows.Forms.CheckBox checkBox2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SettingsForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "text/richtext",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "text/html",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "audio/x-aiff",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "audio/basic",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "audio/wav",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "image/gif",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "image/jpeg",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "image/pjpeg",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem(new string[] {
																													 "image/tiff",
																													 "0",
																													 "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem10 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/x-png",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem11 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/x-xbitmap",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem12 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/bmp",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem13 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/x-jg",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem14 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/x-emf",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem15 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "image/x-wmf",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem16 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "video/avi",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem17 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "video/mpeg",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem18 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/postscript",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem19 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/base64",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem20 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/macbinhex40",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem21 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/pdf",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem22 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/x-compressed",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem23 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/x-zip-compressed",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem24 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/x-gzip-compressed",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem25 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/java",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Windows.Forms.ListViewItem listViewItem26 = new System.Windows.Forms.ListViewItem(new string[] {
																													  "application/x-msdownload",
																													  "0",
																													  "0"}, -1, System.Drawing.SystemColors.WindowText, System.Drawing.Color.WhiteSmoke, new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))));
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(SettingsForm));
			this.tabControlSettings = new System.Windows.Forms.TabControl();
			this.tabPageFileMatches = new System.Windows.Forms.TabPage();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.buttonAddExt = new System.Windows.Forms.Button();
			this.label9 = new System.Windows.Forms.Label();
			this.listViewFileMatches = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
			this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
			this.buttonEditExt = new System.Windows.Forms.Button();
			this.buttonDeleteExt = new System.Windows.Forms.Button();
			this.tabPageOutput = new System.Windows.Forms.TabPage();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonDownloadFolderBrowse = new System.Windows.Forms.Button();
			this.label10 = new System.Windows.Forms.Label();
			this.textBoxDownloadFolder = new System.Windows.Forms.TextBox();
			this.numericUpDownRequests = new System.Windows.Forms.NumericUpDown();
			this.label20 = new System.Windows.Forms.Label();
			this.label21 = new System.Windows.Forms.Label();
			this.tabPageConnections = new System.Windows.Forms.TabPage();
			this.checkBox2 = new System.Windows.Forms.CheckBox();
			this.numericUpDownRunThreadsCount = new System.Windows.Forms.NumericUpDown();
			this.label3 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.numericUpDownSleepTime = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.labelWebDepth = new System.Windows.Forms.Label();
			this.numericUpDownWebDepth = new System.Windows.Forms.NumericUpDown();
			this.checkBoxKeepURLServer = new System.Windows.Forms.CheckBox();
			this.label12 = new System.Windows.Forms.Label();
			this.label13 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
			this.tabPageAdvanced = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.textBoxExcludePages = new System.Windows.Forms.TextBox();
			this.label17 = new System.Windows.Forms.Label();
			this.textBoxExcludeFiles = new System.Windows.Forms.TextBox();
			this.label18 = new System.Windows.Forms.Label();
			this.textBoxExcludeURLs = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.checkBoxSettingsUseWindowsDefaultCodePage = new System.Windows.Forms.CheckBox();
			this.comboBoxSettingsCodePage = new System.Windows.Forms.ComboBox();
			this.buttonSettingsOK = new System.Windows.Forms.Button();
			this.buttonSettingsCancel = new System.Windows.Forms.Button();
			this.columnHeaderMatchText = new System.Windows.Forms.ColumnHeader();
			this.columnHeaderMatchName = new System.Windows.Forms.ColumnHeader();
			this.tabControlSettings.SuspendLayout();
			this.tabPageFileMatches.SuspendLayout();
			this.tabPageOutput.SuspendLayout();
			this.groupBox3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRequests)).BeginInit();
			this.tabPageConnections.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunThreadsCount)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepTime)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWebDepth)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
			this.tabPageAdvanced.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControlSettings
			// 
			this.tabControlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tabControlSettings.Controls.Add(this.tabPageFileMatches);
			this.tabControlSettings.Controls.Add(this.tabPageOutput);
			this.tabControlSettings.Controls.Add(this.tabPageConnections);
			this.tabControlSettings.Controls.Add(this.tabPageAdvanced);
			this.tabControlSettings.Location = new System.Drawing.Point(0, 0);
			this.tabControlSettings.Name = "tabControlSettings";
			this.tabControlSettings.SelectedIndex = 0;
			this.tabControlSettings.Size = new System.Drawing.Size(466, 274);
			this.tabControlSettings.TabIndex = 1;
			this.tabControlSettings.Tag = "Settings tab";
			// 
			// tabPageFileMatches
			// 
			this.tabPageFileMatches.Controls.Add(this.checkBox1);
			this.tabPageFileMatches.Controls.Add(this.buttonAddExt);
			this.tabPageFileMatches.Controls.Add(this.label9);
			this.tabPageFileMatches.Controls.Add(this.listViewFileMatches);
			this.tabPageFileMatches.Controls.Add(this.buttonEditExt);
			this.tabPageFileMatches.Controls.Add(this.buttonDeleteExt);
			this.tabPageFileMatches.Location = new System.Drawing.Point(4, 22);
			this.tabPageFileMatches.Name = "tabPageFileMatches";
			this.tabPageFileMatches.Size = new System.Drawing.Size(458, 248);
			this.tabPageFileMatches.TabIndex = 4;
			this.tabPageFileMatches.Text = "MIME types";
			// 
			// checkBox1
			// 
			this.checkBox1.Checked = true;
			this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.checkBox1.Location = new System.Drawing.Point(16, 8);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(168, 24);
			this.checkBox1.TabIndex = 8;
			this.checkBox1.Tag = "Allow all MIME types";
			this.checkBox1.Text = "Allow all MIME types";
			// 
			// buttonAddExt
			// 
			this.buttonAddExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonAddExt.Location = new System.Drawing.Point(24, 216);
			this.buttonAddExt.Name = "buttonAddExt";
			this.buttonAddExt.Size = new System.Drawing.Size(75, 22);
			this.buttonAddExt.TabIndex = 1;
			this.buttonAddExt.Text = "&Add...";
			this.buttonAddExt.Click += new System.EventHandler(this.buttonAddExt_Click);
			// 
			// label9
			// 
			this.label9.Location = new System.Drawing.Point(16, 32);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(100, 16);
			this.label9.TabIndex = 7;
			this.label9.Text = "MIME Types: ";
			// 
			// listViewFileMatches
			// 
			this.listViewFileMatches.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listViewFileMatches.BackColor = System.Drawing.Color.WhiteSmoke;
			this.listViewFileMatches.CheckBoxes = true;
			this.listViewFileMatches.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								  this.columnHeader1,
																								  this.columnHeader2,
																								  this.columnHeader3});
			this.listViewFileMatches.FullRowSelect = true;
			this.listViewFileMatches.GridLines = true;
			listViewItem1.Checked = true;
			listViewItem1.StateImageIndex = 1;
			listViewItem1.Tag = "";
			listViewItem2.Checked = true;
			listViewItem2.StateImageIndex = 1;
			listViewItem2.Tag = "";
			listViewItem3.Checked = true;
			listViewItem3.StateImageIndex = 1;
			listViewItem3.Tag = "";
			listViewItem4.Checked = true;
			listViewItem4.StateImageIndex = 1;
			listViewItem4.Tag = "";
			listViewItem5.Checked = true;
			listViewItem5.StateImageIndex = 1;
			listViewItem5.Tag = "";
			listViewItem6.Checked = true;
			listViewItem6.StateImageIndex = 1;
			listViewItem6.Tag = "";
			listViewItem7.Checked = true;
			listViewItem7.StateImageIndex = 1;
			listViewItem7.Tag = "";
			listViewItem8.Checked = true;
			listViewItem8.StateImageIndex = 1;
			listViewItem8.Tag = "";
			listViewItem9.Checked = true;
			listViewItem9.StateImageIndex = 1;
			listViewItem9.Tag = "";
			listViewItem10.Checked = true;
			listViewItem10.StateImageIndex = 1;
			listViewItem10.Tag = "";
			listViewItem11.Checked = true;
			listViewItem11.StateImageIndex = 1;
			listViewItem11.Tag = "";
			listViewItem12.Checked = true;
			listViewItem12.StateImageIndex = 1;
			listViewItem12.Tag = "";
			listViewItem13.Checked = true;
			listViewItem13.StateImageIndex = 1;
			listViewItem13.Tag = "";
			listViewItem14.Checked = true;
			listViewItem14.StateImageIndex = 1;
			listViewItem14.Tag = "";
			listViewItem15.Checked = true;
			listViewItem15.StateImageIndex = 1;
			listViewItem15.Tag = "";
			listViewItem16.Checked = true;
			listViewItem16.StateImageIndex = 1;
			listViewItem16.Tag = "";
			listViewItem17.Checked = true;
			listViewItem17.StateImageIndex = 1;
			listViewItem17.Tag = "";
			listViewItem18.Checked = true;
			listViewItem18.StateImageIndex = 1;
			listViewItem18.Tag = "";
			listViewItem19.Checked = true;
			listViewItem19.StateImageIndex = 1;
			listViewItem19.Tag = "";
			listViewItem20.Checked = true;
			listViewItem20.StateImageIndex = 1;
			listViewItem20.Tag = "";
			listViewItem21.Checked = true;
			listViewItem21.StateImageIndex = 1;
			listViewItem21.Tag = "";
			listViewItem22.Checked = true;
			listViewItem22.StateImageIndex = 1;
			listViewItem22.Tag = "";
			listViewItem23.Checked = true;
			listViewItem23.StateImageIndex = 1;
			listViewItem23.Tag = "";
			listViewItem24.Checked = true;
			listViewItem24.StateImageIndex = 1;
			listViewItem24.Tag = "";
			listViewItem25.Checked = true;
			listViewItem25.StateImageIndex = 1;
			listViewItem25.Tag = "";
			listViewItem26.Checked = true;
			listViewItem26.StateImageIndex = 1;
			listViewItem26.Tag = "";
			this.listViewFileMatches.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																								listViewItem1,
																								listViewItem2,
																								listViewItem3,
																								listViewItem4,
																								listViewItem5,
																								listViewItem6,
																								listViewItem7,
																								listViewItem8,
																								listViewItem9,
																								listViewItem10,
																								listViewItem11,
																								listViewItem12,
																								listViewItem13,
																								listViewItem14,
																								listViewItem15,
																								listViewItem16,
																								listViewItem17,
																								listViewItem18,
																								listViewItem19,
																								listViewItem20,
																								listViewItem21,
																								listViewItem22,
																								listViewItem23,
																								listViewItem24,
																								listViewItem25,
																								listViewItem26});
			this.listViewFileMatches.Location = new System.Drawing.Point(16, 48);
			this.listViewFileMatches.MultiSelect = false;
			this.listViewFileMatches.Name = "listViewFileMatches";
			this.listViewFileMatches.Size = new System.Drawing.Size(426, 160);
			this.listViewFileMatches.TabIndex = 0;
			this.listViewFileMatches.Tag = "Settings";
			this.listViewFileMatches.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Category";
			this.columnHeader1.Width = 201;
			// 
			// columnHeader2
			// 
			this.columnHeader2.Text = "Min";
			this.columnHeader2.Width = 50;
			// 
			// columnHeader3
			// 
			this.columnHeader3.Text = "Max";
			this.columnHeader3.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.columnHeader3.Width = 50;
			// 
			// buttonEditExt
			// 
			this.buttonEditExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonEditExt.Location = new System.Drawing.Point(104, 216);
			this.buttonEditExt.Name = "buttonEditExt";
			this.buttonEditExt.Size = new System.Drawing.Size(75, 22);
			this.buttonEditExt.TabIndex = 2;
			this.buttonEditExt.Text = "&Edit...";
			this.buttonEditExt.Click += new System.EventHandler(this.buttonEditExt_Click);
			// 
			// buttonDeleteExt
			// 
			this.buttonDeleteExt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.buttonDeleteExt.Location = new System.Drawing.Point(184, 216);
			this.buttonDeleteExt.Name = "buttonDeleteExt";
			this.buttonDeleteExt.Size = new System.Drawing.Size(75, 22);
			this.buttonDeleteExt.TabIndex = 3;
			this.buttonDeleteExt.Text = "&Delete";
			this.buttonDeleteExt.Click += new System.EventHandler(this.buttonDeleteExt_Click);
			// 
			// tabPageOutput
			// 
			this.tabPageOutput.Controls.Add(this.groupBox3);
			this.tabPageOutput.Location = new System.Drawing.Point(4, 22);
			this.tabPageOutput.Name = "tabPageOutput";
			this.tabPageOutput.Size = new System.Drawing.Size(458, 248);
			this.tabPageOutput.TabIndex = 2;
			this.tabPageOutput.Text = "Output";
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.buttonDownloadFolderBrowse);
			this.groupBox3.Controls.Add(this.label10);
			this.groupBox3.Controls.Add(this.textBoxDownloadFolder);
			this.groupBox3.Controls.Add(this.numericUpDownRequests);
			this.groupBox3.Controls.Add(this.label20);
			this.groupBox3.Controls.Add(this.label21);
			this.groupBox3.Location = new System.Drawing.Point(16, 8);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(360, 224);
			this.groupBox3.TabIndex = 7;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Target";
			// 
			// buttonDownloadFolderBrowse
			// 
			this.buttonDownloadFolderBrowse.Location = new System.Drawing.Point(304, 32);
			this.buttonDownloadFolderBrowse.Name = "buttonDownloadFolderBrowse";
			this.buttonDownloadFolderBrowse.Size = new System.Drawing.Size(24, 20);
			this.buttonDownloadFolderBrowse.TabIndex = 5;
			this.buttonDownloadFolderBrowse.Text = "...";
			this.buttonDownloadFolderBrowse.Click += new System.EventHandler(this.buttonDownloadFolderBrowse_Click);
			// 
			// label10
			// 
			this.label10.Location = new System.Drawing.Point(16, 32);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(96, 16);
			this.label10.TabIndex = 4;
			this.label10.Text = "Download folder:";
			// 
			// textBoxDownloadFolder
			// 
			this.textBoxDownloadFolder.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxDownloadFolder.Location = new System.Drawing.Point(136, 32);
			this.textBoxDownloadFolder.Name = "textBoxDownloadFolder";
			this.textBoxDownloadFolder.Size = new System.Drawing.Size(160, 20);
			this.textBoxDownloadFolder.TabIndex = 4;
			this.textBoxDownloadFolder.Tag = "Download folder";
			this.textBoxDownloadFolder.Text = "";
			// 
			// numericUpDownRequests
			// 
			this.numericUpDownRequests.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownRequests.Location = new System.Drawing.Point(136, 64);
			this.numericUpDownRequests.Maximum = new System.Decimal(new int[] {
																				  1000,
																				  0,
																				  0,
																				  0});
			this.numericUpDownRequests.Name = "numericUpDownRequests";
			this.numericUpDownRequests.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownRequests.TabIndex = 10;
			this.numericUpDownRequests.Tag = "View last requests count";
			this.numericUpDownRequests.Value = new System.Decimal(new int[] {
																				20,
																				0,
																				0,
																				0});
			// 
			// label20
			// 
			this.label20.Location = new System.Drawing.Point(32, 64);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(64, 16);
			this.label20.TabIndex = 12;
			this.label20.Text = "View last";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(208, 64);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(64, 16);
			this.label21.TabIndex = 11;
			this.label21.Tag = "";
			this.label21.Text = "(Requests)";
			// 
			// tabPageConnections
			// 
			this.tabPageConnections.Controls.Add(this.checkBox2);
			this.tabPageConnections.Controls.Add(this.numericUpDownRunThreadsCount);
			this.tabPageConnections.Controls.Add(this.label3);
			this.tabPageConnections.Controls.Add(this.label2);
			this.tabPageConnections.Controls.Add(this.numericUpDownSleepTime);
			this.tabPageConnections.Controls.Add(this.label4);
			this.tabPageConnections.Controls.Add(this.numericUpDown1);
			this.tabPageConnections.Controls.Add(this.label5);
			this.tabPageConnections.Controls.Add(this.label6);
			this.tabPageConnections.Controls.Add(this.label11);
			this.tabPageConnections.Controls.Add(this.labelWebDepth);
			this.tabPageConnections.Controls.Add(this.numericUpDownWebDepth);
			this.tabPageConnections.Controls.Add(this.checkBoxKeepURLServer);
			this.tabPageConnections.Controls.Add(this.label12);
			this.tabPageConnections.Controls.Add(this.label13);
			this.tabPageConnections.Controls.Add(this.label14);
			this.tabPageConnections.Controls.Add(this.numericUpDown2);
			this.tabPageConnections.Location = new System.Drawing.Point(4, 22);
			this.tabPageConnections.Name = "tabPageConnections";
			this.tabPageConnections.Size = new System.Drawing.Size(458, 248);
			this.tabPageConnections.TabIndex = 3;
			this.tabPageConnections.Text = "Connections";
			// 
			// checkBox2
			// 
			this.checkBox2.Checked = true;
			this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.checkBox2.Location = new System.Drawing.Point(16, 152);
			this.checkBox2.Name = "checkBox2";
			this.checkBox2.Size = new System.Drawing.Size(136, 24);
			this.checkBox2.TabIndex = 8;
			this.checkBox2.Tag = "Keep connection alive";
			this.checkBox2.Text = "Keep connection alive";
			// 
			// numericUpDownRunThreadsCount
			// 
			this.numericUpDownRunThreadsCount.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownRunThreadsCount.Location = new System.Drawing.Point(248, 8);
			this.numericUpDownRunThreadsCount.Minimum = new System.Decimal(new int[] {
																						 1,
																						 0,
																						 0,
																						 0});
			this.numericUpDownRunThreadsCount.Name = "numericUpDownRunThreadsCount";
			this.numericUpDownRunThreadsCount.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownRunThreadsCount.TabIndex = 0;
			this.numericUpDownRunThreadsCount.Tag = "Threads count";
			this.numericUpDownRunThreadsCount.Value = new System.Decimal(new int[] {
																					   10,
																					   0,
																					   0,
																					   0});
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 8);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 0;
			this.label3.Text = "Threads count:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 32);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(224, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Thread sleep time when refs queue empty:";
			// 
			// numericUpDownSleepTime
			// 
			this.numericUpDownSleepTime.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownSleepTime.Location = new System.Drawing.Point(248, 32);
			this.numericUpDownSleepTime.Maximum = new System.Decimal(new int[] {
																				   60,
																				   0,
																				   0,
																				   0});
			this.numericUpDownSleepTime.Minimum = new System.Decimal(new int[] {
																				   1,
																				   0,
																				   0,
																				   0});
			this.numericUpDownSleepTime.Name = "numericUpDownSleepTime";
			this.numericUpDownSleepTime.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownSleepTime.TabIndex = 1;
			this.numericUpDownSleepTime.Tag = "Sleep fetch time";
			this.numericUpDownSleepTime.Value = new System.Decimal(new int[] {
																				 2,
																				 0,
																				 0,
																				 0});
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(16, 56);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(232, 16);
			this.label4.TabIndex = 0;
			this.label4.Text = "Thread sleep time between two connections:";
			// 
			// numericUpDown1
			// 
			this.numericUpDown1.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDown1.Location = new System.Drawing.Point(248, 56);
			this.numericUpDown1.Maximum = new System.Decimal(new int[] {
																		   60,
																		   0,
																		   0,
																		   0});
			this.numericUpDown1.Name = "numericUpDown1";
			this.numericUpDown1.Size = new System.Drawing.Size(64, 20);
			this.numericUpDown1.TabIndex = 2;
			this.numericUpDown1.Tag = "Sleep connect time";
			this.numericUpDown1.Value = new System.Decimal(new int[] {
																		 1,
																		 0,
																		 0,
																		 0});
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(320, 32);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(64, 16);
			this.label5.TabIndex = 0;
			this.label5.Text = "(Seconds)";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(320, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(64, 16);
			this.label6.TabIndex = 0;
			this.label6.Text = "(Seconds)";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(320, 8);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(64, 16);
			this.label11.TabIndex = 0;
			this.label11.Tag = "";
			this.label11.Text = "(Threads)";
			// 
			// labelWebDepth
			// 
			this.labelWebDepth.Location = new System.Drawing.Point(16, 104);
			this.labelWebDepth.Name = "labelWebDepth";
			this.labelWebDepth.Size = new System.Drawing.Size(216, 16);
			this.labelWebDepth.TabIndex = 1;
			this.labelWebDepth.Text = "Navigate through pages to a depth of";
			// 
			// numericUpDownWebDepth
			// 
			this.numericUpDownWebDepth.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownWebDepth.Location = new System.Drawing.Point(248, 104);
			this.numericUpDownWebDepth.Maximum = new System.Decimal(new int[] {
																				  20,
																				  0,
																				  0,
																				  0});
			this.numericUpDownWebDepth.Name = "numericUpDownWebDepth";
			this.numericUpDownWebDepth.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownWebDepth.TabIndex = 5;
			this.numericUpDownWebDepth.Tag = "Web depth";
			this.numericUpDownWebDepth.Value = new System.Decimal(new int[] {
																				3,
																				0,
																				0,
																				0});
			// 
			// checkBoxKeepURLServer
			// 
			this.checkBoxKeepURLServer.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.checkBoxKeepURLServer.Location = new System.Drawing.Point(16, 128);
			this.checkBoxKeepURLServer.Name = "checkBoxKeepURLServer";
			this.checkBoxKeepURLServer.Size = new System.Drawing.Size(144, 16);
			this.checkBoxKeepURLServer.TabIndex = 7;
			this.checkBoxKeepURLServer.Tag = "Keep same URL server";
			this.checkBoxKeepURLServer.Text = "Keep same URL server";
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(320, 104);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(64, 16);
			this.label12.TabIndex = 0;
			this.label12.Text = "(Pages)";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(320, 80);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(64, 16);
			this.label13.TabIndex = 0;
			this.label13.Text = "(Seconds)";
			// 
			// label14
			// 
			this.label14.Location = new System.Drawing.Point(16, 80);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(216, 16);
			this.label14.TabIndex = 1;
			this.label14.Text = "Connection timeout:";
			// 
			// numericUpDown2
			// 
			this.numericUpDown2.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDown2.Location = new System.Drawing.Point(248, 80);
			this.numericUpDown2.Maximum = new System.Decimal(new int[] {
																		   60,
																		   0,
																		   0,
																		   0});
			this.numericUpDown2.Minimum = new System.Decimal(new int[] {
																		   1,
																		   0,
																		   0,
																		   0});
			this.numericUpDown2.Name = "numericUpDown2";
			this.numericUpDown2.Size = new System.Drawing.Size(64, 20);
			this.numericUpDown2.TabIndex = 3;
			this.numericUpDown2.Tag = "Request timeout";
			this.numericUpDown2.Value = new System.Decimal(new int[] {
																		 20,
																		 0,
																		 0,
																		 0});
			// 
			// tabPageAdvanced
			// 
			this.tabPageAdvanced.Controls.Add(this.groupBox2);
			this.tabPageAdvanced.Controls.Add(this.groupBox1);
			this.tabPageAdvanced.Location = new System.Drawing.Point(4, 22);
			this.tabPageAdvanced.Name = "tabPageAdvanced";
			this.tabPageAdvanced.Size = new System.Drawing.Size(458, 248);
			this.tabPageAdvanced.TabIndex = 1;
			this.tabPageAdvanced.Text = "Advanced";
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.textBoxExcludePages);
			this.groupBox2.Controls.Add(this.label17);
			this.groupBox2.Controls.Add(this.textBoxExcludeFiles);
			this.groupBox2.Controls.Add(this.label18);
			this.groupBox2.Controls.Add(this.textBoxExcludeURLs);
			this.groupBox2.Controls.Add(this.label19);
			this.groupBox2.Location = new System.Drawing.Point(16, 88);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(424, 152);
			this.groupBox2.TabIndex = 5;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Exclusions";
			// 
			// textBoxExcludePages
			// 
			this.textBoxExcludePages.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxExcludePages.Location = new System.Drawing.Point(8, 40);
			this.textBoxExcludePages.Name = "textBoxExcludePages";
			this.textBoxExcludePages.Size = new System.Drawing.Size(408, 20);
			this.textBoxExcludePages.TabIndex = 0;
			this.textBoxExcludePages.Tag = "Exclude words";
			this.textBoxExcludePages.Text = "";
			// 
			// label17
			// 
			this.label17.Location = new System.Drawing.Point(8, 24);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(408, 16);
			this.label17.TabIndex = 5;
			this.label17.Text = "Exclude pages contain these words: (use semicolon separator)";
			// 
			// textBoxExcludeFiles
			// 
			this.textBoxExcludeFiles.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxExcludeFiles.Location = new System.Drawing.Point(8, 120);
			this.textBoxExcludeFiles.Name = "textBoxExcludeFiles";
			this.textBoxExcludeFiles.Size = new System.Drawing.Size(408, 20);
			this.textBoxExcludeFiles.TabIndex = 2;
			this.textBoxExcludeFiles.Tag = "Exclude files";
			this.textBoxExcludeFiles.Text = "";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(8, 104);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(408, 16);
			this.label18.TabIndex = 5;
			this.label18.Text = "Exclude files with these extensions from parsing: (use semicolon separator)";
			// 
			// textBoxExcludeURLs
			// 
			this.textBoxExcludeURLs.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxExcludeURLs.Location = new System.Drawing.Point(8, 80);
			this.textBoxExcludeURLs.Name = "textBoxExcludeURLs";
			this.textBoxExcludeURLs.Size = new System.Drawing.Size(408, 20);
			this.textBoxExcludeURLs.TabIndex = 1;
			this.textBoxExcludeURLs.Tag = "Exclude Hosts";
			this.textBoxExcludeURLs.Text = ".org; .gov;";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(8, 64);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(408, 16);
			this.label19.TabIndex = 5;
			this.label19.Text = "Exclude Hosts contain these patterns: (ex: .org; .gov; .net)";
			// 
			// groupBox1
			// 
			this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.groupBox1.Controls.Add(this.checkBoxSettingsUseWindowsDefaultCodePage);
			this.groupBox1.Controls.Add(this.comboBoxSettingsCodePage);
			this.groupBox1.Location = new System.Drawing.Point(16, 8);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(426, 72);
			this.groupBox1.TabIndex = 4;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Code page";
			// 
			// checkBoxSettingsUseWindowsDefaultCodePage
			// 
			this.checkBoxSettingsUseWindowsDefaultCodePage.Checked = true;
			this.checkBoxSettingsUseWindowsDefaultCodePage.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxSettingsUseWindowsDefaultCodePage.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.checkBoxSettingsUseWindowsDefaultCodePage.Location = new System.Drawing.Point(16, 48);
			this.checkBoxSettingsUseWindowsDefaultCodePage.Name = "checkBoxSettingsUseWindowsDefaultCodePage";
			this.checkBoxSettingsUseWindowsDefaultCodePage.Size = new System.Drawing.Size(144, 16);
			this.checkBoxSettingsUseWindowsDefaultCodePage.TabIndex = 3;
			this.checkBoxSettingsUseWindowsDefaultCodePage.Tag = "Use windows default code page";
			this.checkBoxSettingsUseWindowsDefaultCodePage.Text = "Use windows default";
			this.checkBoxSettingsUseWindowsDefaultCodePage.CheckedChanged += new System.EventHandler(this.checkBoxSettingsUseWindowsDefaultCodePage_CheckedChanged);
			// 
			// comboBoxSettingsCodePage
			// 
			this.comboBoxSettingsCodePage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxSettingsCodePage.BackColor = System.Drawing.Color.WhiteSmoke;
			this.comboBoxSettingsCodePage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBoxSettingsCodePage.Items.AddRange(new object[] {
																		  "Arabic (1256)",
																		  "Baltic (1257)",
																		  "Chinese (Taiwan, Hong Kong) (950)",
																		  "Cyrillic (1251)",
																		  "Greek (1253)",
																		  "Hebrew (1255)",
																		  "Japanese (932)",
																		  "Korean Extended Wansung (949)",
																		  "Latin 1 (1252)",
																		  "Latin 2 (1250)",
																		  "Latin 5 (1254)",
																		  "PRC GBK (XGB) (936)",
																		  "Thai (874)",
																		  "Viet Nam (1258)"});
			this.comboBoxSettingsCodePage.Location = new System.Drawing.Point(16, 24);
			this.comboBoxSettingsCodePage.MaxDropDownItems = 20;
			this.comboBoxSettingsCodePage.Name = "comboBoxSettingsCodePage";
			this.comboBoxSettingsCodePage.Size = new System.Drawing.Size(394, 21);
			this.comboBoxSettingsCodePage.Sorted = true;
			this.comboBoxSettingsCodePage.TabIndex = 0;
			this.comboBoxSettingsCodePage.Tag = "Settings";
			// 
			// buttonSettingsOK
			// 
			this.buttonSettingsOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSettingsOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonSettingsOK.Location = new System.Drawing.Point(290, 282);
			this.buttonSettingsOK.Name = "buttonSettingsOK";
			this.buttonSettingsOK.TabIndex = 1;
			this.buttonSettingsOK.Text = "OK";
			this.buttonSettingsOK.Click += new System.EventHandler(this.buttonSettingsOK_Click);
			// 
			// buttonSettingsCancel
			// 
			this.buttonSettingsCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonSettingsCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonSettingsCancel.Location = new System.Drawing.Point(370, 282);
			this.buttonSettingsCancel.Name = "buttonSettingsCancel";
			this.buttonSettingsCancel.TabIndex = 1;
			this.buttonSettingsCancel.Text = "Cancel";
			// 
			// columnHeaderMatchText
			// 
			this.columnHeaderMatchText.Text = "Description";
			this.columnHeaderMatchText.Width = 300;
			// 
			// columnHeaderMatchName
			// 
			this.columnHeaderMatchName.Text = "Match";
			this.columnHeaderMatchName.Width = 90;
			// 
			// SettingsForm
			// 
			this.AcceptButton = this.buttonSettingsOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonSettingsCancel;
			this.ClientSize = new System.Drawing.Size(466, 311);
			this.Controls.Add(this.buttonSettingsOK);
			this.Controls.Add(this.tabControlSettings);
			this.Controls.Add(this.buttonSettingsCancel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "SettingsForm";
			this.ShowInTaskbar = false;
			this.Text = "Settings";
			this.Load += new System.EventHandler(this.SettingsForm_Load);
			this.tabControlSettings.ResumeLayout(false);
			this.tabPageFileMatches.ResumeLayout(false);
			this.tabPageOutput.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRequests)).EndInit();
			this.tabPageConnections.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRunThreadsCount)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownSleepTime)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownWebDepth)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
			this.tabPageAdvanced.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void SettingsForm_Load(object sender, System.EventArgs e)
		{
			Settings.GetValue(this);
			this.comboBoxSettingsCodePage.Enabled = this.checkBoxSettingsUseWindowsDefaultCodePage.Checked == false;

			if(this.textBoxDownloadFolder.Text == "")
				this.textBoxDownloadFolder.Text = System.Environment.CurrentDirectory;

			if(SelectedIndex != -1)
				this.tabControlSettings.SelectedIndex = SelectedIndex;
		}

		private void buttonSettingsOK_Click(object sender, System.EventArgs e)
		{
			Settings.SetValue(this);
		}

		private void checkBoxSettingsUseWindowsDefaultCodePage_CheckedChanged(object sender, System.EventArgs e)
		{
			this.comboBoxSettingsCodePage.Enabled = this.checkBoxSettingsUseWindowsDefaultCodePage.Checked == false;
		}

		private void buttonAddExt_Click(object sender, System.EventArgs e)
		{
			FileTypeForm form = new FileTypeForm();
			if(form.ShowDialog() == DialogResult.OK)
			{
				ListViewItem item = this.listViewFileMatches.Items.Add(form.textBoxTypeDescription.Text);
				item.SubItems.Add(form.numericUpDownMinSize.Value.ToString());
				item.SubItems.Add(form.numericUpDownMaxSize.Value.ToString());
			}
		}

		private void buttonEditExt_Click(object sender, System.EventArgs e)
		{
			if(this.listViewFileMatches.SelectedItems.Count == 0)
				return;
			ListViewItem item = this.listViewFileMatches.SelectedItems[0];
			FileTypeForm form = new FileTypeForm();
			form.textBoxTypeDescription.Text = item.Text;
			if(item.SubItems.Count <= 1)
				item.SubItems.Add("0");
			form.numericUpDownMinSize.Value = int.Parse(item.SubItems[1].Text);
			if(item.SubItems.Count <= 2)
				item.SubItems.Add("0");
			form.numericUpDownMaxSize.Value = int.Parse(item.SubItems[2].Text);

			if(form.ShowDialog() == DialogResult.OK)
			{
				item.Text = form.textBoxTypeDescription.Text;
				item.SubItems[1].Text = form.numericUpDownMinSize.Value.ToString();
				item.SubItems[2].Text = form.numericUpDownMaxSize.Value.ToString();
			}
		}

		private void buttonDeleteExt_Click(object sender, System.EventArgs e)
		{
			if(this.listViewFileMatches.SelectedItems.Count == 0)
				return;
			ListViewItem item = this.listViewFileMatches.SelectedItems[0];
			item.Remove();
		}

		private void buttonDownloadFolderBrowse_Click(object sender, System.EventArgs e)
		{
			BrowseForFolderClass form = new BrowseForFolderClass();
			form.Title = "Select folder to save crawled files";
			if(form.ShowDialog() == DialogResult.OK)
				this.textBoxDownloadFolder.Text = form.DirectoryPath;
		}

	}
}
