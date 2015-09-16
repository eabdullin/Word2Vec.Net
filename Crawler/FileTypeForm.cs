using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Crawler
{
	/// <summary>
	/// Summary description for FileTypeForm.
	/// </summary>
	public class FileTypeForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox textBoxTypeDescription;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		public System.Windows.Forms.NumericUpDown numericUpDownMaxSize;
		public System.Windows.Forms.NumericUpDown numericUpDownMinSize;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public FileTypeForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
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
			this.buttonOK = new System.Windows.Forms.Button();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.textBoxTypeDescription = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.numericUpDownMaxSize = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.numericUpDownMinSize = new System.Windows.Forms.NumericUpDown();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSize)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinSize)).BeginInit();
			this.SuspendLayout();
			// 
			// buttonOK
			// 
			this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(344, 8);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.TabIndex = 4;
			this.buttonOK.Text = "OK";
			// 
			// buttonCancel
			// 
			this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(344, 32);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.TabIndex = 3;
			this.buttonCancel.Text = "Cancel";
			// 
			// textBoxTypeDescription
			// 
			this.textBoxTypeDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.textBoxTypeDescription.BackColor = System.Drawing.Color.WhiteSmoke;
			this.textBoxTypeDescription.Location = new System.Drawing.Point(16, 24);
			this.textBoxTypeDescription.Name = "textBoxTypeDescription";
			this.textBoxTypeDescription.Size = new System.Drawing.Size(312, 20);
			this.textBoxTypeDescription.TabIndex = 8;
			this.textBoxTypeDescription.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(112, 16);
			this.label1.TabIndex = 6;
			this.label1.Text = "MIME Type:";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(80, 16);
			this.label2.TabIndex = 5;
			this.label2.Text = "Minimum size:";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 80);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(80, 16);
			this.label3.TabIndex = 5;
			this.label3.Text = "Maimum size:";
			// 
			// numericUpDownMaxSize
			// 
			this.numericUpDownMaxSize.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownMaxSize.Location = new System.Drawing.Point(96, 80);
			this.numericUpDownMaxSize.Maximum = new System.Decimal(new int[] {
																				 100000000,
																				 0,
																				 0,
																				 0});
			this.numericUpDownMaxSize.Name = "numericUpDownMaxSize";
			this.numericUpDownMaxSize.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownMaxSize.TabIndex = 9;
			this.numericUpDownMaxSize.Tag = "";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(168, 80);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(32, 16);
			this.label5.TabIndex = 5;
			this.label5.Text = "(KB)";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(168, 56);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(32, 16);
			this.label6.TabIndex = 5;
			this.label6.Text = "(KB)";
			// 
			// numericUpDownMinSize
			// 
			this.numericUpDownMinSize.BackColor = System.Drawing.Color.WhiteSmoke;
			this.numericUpDownMinSize.Location = new System.Drawing.Point(96, 56);
			this.numericUpDownMinSize.Maximum = new System.Decimal(new int[] {
																				 100000000,
																				 0,
																				 0,
																				 0});
			this.numericUpDownMinSize.Name = "numericUpDownMinSize";
			this.numericUpDownMinSize.Size = new System.Drawing.Size(64, 20);
			this.numericUpDownMinSize.TabIndex = 9;
			this.numericUpDownMinSize.Tag = "";
			// 
			// FileTypeForm
			// 
			this.AcceptButton = this.buttonOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.buttonCancel;
			this.ClientSize = new System.Drawing.Size(434, 119);
			this.Controls.Add(this.numericUpDownMaxSize);
			this.Controls.Add(this.textBoxTypeDescription);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.buttonOK);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.numericUpDownMinSize);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FileTypeForm";
			this.Text = "Edit file type";
			this.Load += new System.EventHandler(this.FileTypeForm_Load);
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxSize)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownMinSize)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void FileTypeForm_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}
