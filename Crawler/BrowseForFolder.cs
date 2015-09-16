using System;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace LiteLib
{
	public class BrowseForFolderClass : FolderNameEditor
	{
		FolderNameEditor.FolderBrowser browser = new FolderNameEditor.FolderBrowser();
		public string DirectoryPath
		{
			get	{	return browser.DirectoryPath;	}
		}
		
		public string Title
		{
			set	{	browser.Description = value;	}
		}

		public DialogResult ShowDialog()
		{
			return browser.ShowDialog();
		}
	}
}
