using System;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;

namespace LiteLib
{
	/// <summary>
	/// Summary description for SortTree.
	/// </summary>
	public class SortTree
	{
		public SortTreeNode Root;
		public int Count;
		public bool Modified;

		public SortTree()
		{
		}
		public void Clear()
		{
			Root = null;
			Count = 0;
			Modified = false;
		}
		private SortTreeNode Add(string strText, int nCount, object Tag)
		{
			SortTreeNode node = Add(ref strText);
			node.Count = nCount;
			node.Tag = Tag;
			return node;
		}

		public SortTreeNode Add(ref string str)
		{
			SortTreeNode node;
			if(Root == null)
			{
				Root = new SortTreeNode();
				node = Root;
			}
			else	
			{
				node = Root;
				while(true)
				{
					if(node.Text == str)
					{
						node.Count++;
						return node;
					}
					if(node.Text.CompareTo(str) > 0)
					{	// add the node at the small branch
						if(node.Small == null)
						{
							node.Small = new SortTreeNode();
							node.Small.Parent = node;
							node = node.Small;
							break;
						}
						node = node.Small;
					}
					else
					{	// add the node at the great branch
						if(node.Great == null)
						{
							node.Great = new SortTreeNode();
							node.Great.Parent = node;
							node = node.Great;
							break;
						}
						node = node.Great;
					}
				}	
			}
			node.Text = str;
			node.ID = this.Count++;
			node.Count++;
			Modified = true;
			
			return node;
		}
		
		public void Save(string fileName, System.Text.Encoding code)
		{
			FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
			StreamWriter writer = new StreamWriter(stream, code);
			
			BitArray bitArray = new BitArray(this.Count, false);
			SortTreeNode node = this.Root;
			int nCount = this.Count;
			while(nCount > 0)
			{
				if(node.Small != null && bitArray.Get(node.Small.ID) == false)
					node = node.Small;
				else	if(bitArray.Get(node.ID) == false)
					OutNode(node, writer, bitArray, ref nCount);
				else	if(node.Great != null && bitArray.Get(node.Great.ID) == false)
					node = node.Great;
				else
				{
					if(bitArray.Get(node.ID) == false)
						OutNode(node, writer, bitArray, ref nCount);
					node = node.Parent;
				}
			}
			writer.Close();
			stream.Close();
			
			Modified = false;
		}

		public void Open(string fileName, System.Text.Encoding code, ListView list)
		{
			FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			StreamReader reader = new StreamReader(stream, code);

			SortTreeNode node = null;
			int nTagCount = 0;
			ArrayList array = new ArrayList();
			string str;
			while((str = reader.ReadLine()) != null)
			{
				string[] strCols = str.Split('\t');
				for(int n = 0; n < strCols.Length; n++)
				{
					string strCol = strCols[n];
					if(n == 0)
					{
						if(strCol != "")
						{
							node = new SortTreeNode();
							node.Text = strCol;
							array.Add(node);
							nTagCount = 0;
						}
					}
					else	if(n == 1)
					{
						try
						{
							if(strCol != "")
								node.Count = int.Parse(strCol);
						}
						catch(Exception e)
						{
							MessageBox.Show(e.Message);
						}
					}
					else
					{
						if(nTagCount++ > 0)
							node.Tag += "\r\n";
						node.Tag += strCol;
					}
				}
			}
			reader.Close();
			stream.Close();

			Random rand = new Random();
			while(array.Count > 0)
			{
				int nIndex = rand.Next(array.Count);
				node = (SortTreeNode)array[nIndex];
				array.RemoveAt(nIndex);
				node = this.Add(node.Text, node.Count, node.Tag);
				if(list != null)
				{
					ListViewItem item = list.Items.Add((node.ID+1).ToString());
					item.SubItems.Add(node.Text);
					item.SubItems.Add(node.Count.ToString());
					item.Tag = node;
				}
			}
		}
		
		bool OutNode(SortTreeNode node, StreamWriter writer, BitArray bits, ref int nCount)
		{
			if(node == null || bits.Get(node.ID) == true)
				return false;
			string str = node.Text + '\t' + node.Count.ToString() + '\t';
			if(node.Tag != null)
				str += node.Tag.ToString().Replace("\r\n", "\r\n\t\t");
			writer.WriteLine(str.TrimEnd('\t'));
			
			bits.Set(node.ID, true);
			nCount--;
			
			return true;
		}
	}
	public class SortTreeNode
	{
		public SortTreeNode Parent;
		public SortTreeNode Small;
		public SortTreeNode Great;
		public string Text;
		public int Count;
		public int ID;

		public object Tag;
	}

	/// <summary>
	/// Summary description for HashTree.
	/// </summary>
	public class HashTree
	{
		public HashTreeNode Root;
		public bool Modified;
		public int Count;

		public HashTree()
		{
		}
		public void Clear()
		{
			Root = null;
			Count = 0;
			Modified = false;
		}
		public bool Add(ref string str)
		{
			int Code = str.GetHashCode();
			HashTreeNode node;
			if(Root == null)
			{
				Root = new HashTreeNode();
				node = Root;
			}
			else	
			{
				node = Root;
				while(true)
				{
					if(node.Code == Code)
						return false;
					if(Code < node.Code)
					{	// add the node at the small branch
						if(node.Small == null)
						{
							node.Small = new HashTreeNode();
							node = node.Small;
							break;
						}
						node = node.Small;
					}
					else
					{	// add the node at the great branch
						if(node.Great == null)
						{
							node.Great = new HashTreeNode();
							node = node.Great;
							break;
						}
						node = node.Great;
					}
				}	
			}
			node.Code = Code;
			this.Modified = true;
			this.Count++;			
			
			return true;
		}		

		public void Open(string fileName, System.Text.Encoding code)
		{
			FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			StreamReader reader = new StreamReader(stream, code);

			string str;
			while((str = reader.ReadLine()) != null)
			{
				string[] strCols = str.Split('\t');
				if(strCols.Length > 0)
					this.Add(ref strCols[0]);
			}
			reader.Close();
			stream.Close();
		}
	}

	public class HashTreeNode
	{
		public HashTreeNode Small;
		public HashTreeNode Great;
		public int Code;
	}
}
