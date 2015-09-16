using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Crawler
{
	/// <summary>
	/// Summary description for AboutForm.
	/// </summary>
	public class AboutForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button IDOK;

		public AboutForm()
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
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.IDOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(280, 16);
			this.label1.TabIndex = 0;
			this.label1.Text = "Simple Net Crawler 1.00";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(24, 40);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(280, 16);
			this.label2.TabIndex = 0;
			this.label2.Text = "Developed by Hatem Mostafa";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(24, 64);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(280, 16);
			this.label3.TabIndex = 0;
			this.label3.Text = "Copyright (C) 2006";
			// 
			// IDOK
			// 
			this.IDOK.Location = new System.Drawing.Point(328, 16);
			this.IDOK.Name = "IDOK";
			this.IDOK.TabIndex = 1;
			this.IDOK.Text = "OK";
			this.IDOK.Click += new System.EventHandler(this.IDOK_Click);
			// 
			// AboutForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(432, 96);
			this.Controls.Add(this.IDOK);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "AboutForm";
			this.Opacity = 0.95;
			this.ShowInTaskbar = false;
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "About Net Crawler";
			this.Load += new System.EventHandler(this.AboutForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void AboutForm_Load(object sender, System.EventArgs e)
		{
		}

		private void IDOK_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}
	}
}
