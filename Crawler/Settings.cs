using System;
using System.IO;
using System.Xml;
using System.Windows.Forms;


namespace LiteLib
{
	public class Settings
	{
		public string this[string name]
		{
			get
			{
				try
				{
					return (string)Application.UserAppDataRegistry.GetValue(name);
				}
				catch(Exception)
				{
					return null;
				}
			}
			set
			{
				try
				{
					Application.UserAppDataRegistry.SetValue(name, value);
				}
				catch(Exception)
				{
				}
			}
		}
		// get functions
		public static string GetValue(string name)
		{
			return (string)Application.UserAppDataRegistry.GetValue(name);
		}
		public static string GetValue(string name, string defaultValue)
		{
			return (string)Application.UserAppDataRegistry.GetValue(name, defaultValue);
		}
		public static int GetValue(string name, int defaultValue)
		{
			string str = GetValue(name, defaultValue.ToString());
			if(str != "")
				return int.Parse(str);
			return -1;
		}
		public static bool GetValue(string name, bool defaultValue)
		{
			return GetValue(name, defaultValue.ToString()).ToLower() == "true";
		}
		// set functions
		public static void SetValue(string name, string value)
		{
			Application.UserAppDataRegistry.SetValue(name, value);
		}
		public static void SetValue(string name, bool value)
		{
			SetValue(name, value.ToString());
		}

		public static void SetValue(Form form)
		{
			Control ctrl = null;
			while((ctrl = form.GetNextControl(ctrl, true)) != null)
				Settings.SetValue(ctrl);
		}
		public static void SetValue(Control ctrl)
		{
			string Name = ctrl.FindForm().Name+'-'+ctrl.Name;
			if(ctrl.Tag != null && ctrl.Tag.ToString().Trim().Length > 0)
				switch(ctrl.GetType().Name)
				{
					case "TextBox":
						Settings.SetValue((string)ctrl.Tag, ctrl.Text);
						break;
					case "CheckBox":
						CheckBox check = (CheckBox)ctrl;
						Settings.SetValue((string)ctrl.Tag, check.Checked);
						break;
					case "RadioButton":
						RadioButton radio = (RadioButton)ctrl;
						Settings.SetValue((string)ctrl.Tag, radio.Checked);
						break;
					case "NumericUpDown":
						NumericUpDown numeric = (NumericUpDown)ctrl;
						Settings.SetValue((string)ctrl.Tag, numeric.Value.ToString());
						break;
					case "TabControl":
						TabControl tab = (TabControl)ctrl;
						Settings.SetValue((string)ctrl.Tag, tab.SelectedIndex.ToString());
						break;
					case "ComboBox":
						ComboBox combo = (ComboBox)ctrl;
						try
						{
							string fileName = null;
							XmlDocument doc = null;
							XmlNode element = GetCtrlNode(ctrl, ref fileName, ref doc);
							element.RemoveAll();
							foreach (string item in combo.Items)
							{
								XmlNode node = doc.CreateNode(XmlNodeType.Element, "Item", "");
								node.InnerText = item;
								element.AppendChild(node);
							}
							doc.Save(fileName);
						}
						catch(XmlException)
						{
						}
						Settings.SetValue(Name, combo.Text);
						break;
					case "ListView":
						ListView list = (ListView)ctrl;
						try
						{
							string fileName = null;
							XmlDocument doc = null;
							XmlNode element = GetCtrlNode(ctrl, ref fileName, ref doc);
							element.RemoveAll();
							foreach (ListViewItem viewItem in list.Items)
							{
								XmlNode node = doc.CreateNode(XmlNodeType.Element, "Item", "");
								if(list.CheckBoxes == true)
								{
									XmlAttribute attribute = doc.CreateAttribute("Checked");
									attribute.Value = viewItem.Checked.ToString();
									node.Attributes.Append(attribute);
								}
								string InnerText = "";
								foreach(ListViewItem.ListViewSubItem subItem in viewItem.SubItems)
									InnerText += subItem.Text+'\t';
								node.InnerText = InnerText.TrimEnd('\t');
								element.AppendChild(node);
							}
							doc.Save(fileName);
						}
						catch(XmlException)
						{
						}
						break;
				}
		}
		
		static XmlNode GetCtrlNode(Control ctrl)
		{
			string fileName = null;
			XmlDocument doc = null;
			return GetCtrlNode(ctrl, ref fileName, ref doc);
		}

		static XmlNode GetCtrlNode(Control ctrl, ref string fileName, ref XmlDocument doc)
		{
			fileName = Application.StartupPath+'\\'+(string)ctrl.Tag+".xml";
			doc = new XmlDocument();
			XmlNode node = null;
			string Name = ctrl.FindForm().Name+'-'+ctrl.Name;
			if(File.Exists(fileName))
			{
				doc.Load(fileName);
				node = doc.DocumentElement.SelectSingleNode(Name);
			}
			else
				doc.LoadXml("<Settings>\r\n</Settings>");
			if(node == null)
			{
				node = doc.CreateNode(XmlNodeType.Element, Name, "");
				doc.DocumentElement.AppendChild(node);
			}
			return node;
		}

		public static void GetValue(Form form)
		{
			Control ctrl = null;
			while((ctrl = form.GetNextControl(ctrl, true)) != null)
				Settings.GetValue(ctrl);
		}
		public static void GetValue(Control ctrl)
		{
			string Name = ctrl.FindForm().Name+'-'+ctrl.Name;
			if(ctrl.Tag != null && ctrl.Tag.ToString().Trim().Length > 0)
				switch(ctrl.GetType().Name)
				{
					case "TextBox":
						ctrl.Text = Settings.GetValue((string)ctrl.Tag, ctrl.Text);
						break;
					case "CheckBox":
						CheckBox check = (CheckBox)ctrl;
						check.Checked = Settings.GetValue((string)ctrl.Tag, check.Checked);
						break;
					case "RadioButton":
						RadioButton radio = (RadioButton)ctrl;
						radio.Checked = Settings.GetValue((string)ctrl.Tag, radio.Checked);
						break;
					case "NumericUpDown":
						NumericUpDown numeric = (NumericUpDown)ctrl;
						numeric.Value = decimal.Parse(Settings.GetValue((string)ctrl.Tag, numeric.Value.ToString()));
						break;
					case "TabControl":
						TabControl tab = (TabControl)ctrl;
						tab.SelectedIndex = int.Parse(Settings.GetValue((string)ctrl.Tag, tab.SelectedIndex.ToString()));
						break;
					case "ComboBox":
						ComboBox combo = (ComboBox)ctrl;
						try
						{
							XmlNode element = GetCtrlNode(ctrl);
							if(element != null && element.ChildNodes.Count > 0)
							{
								combo.Items.Clear();
								foreach(XmlNode node in element.ChildNodes)
									combo.Items.Add(node.InnerText);
							}
						}
						catch(XmlException)
						{
						}
						combo.Text = Settings.GetValue(Name, combo.Text);
						break;
					case "ListView":
						ListView list = (ListView)ctrl;
						try
						{
							XmlNode element = GetCtrlNode(ctrl);
							if(element != null && element.ChildNodes.Count > 0)
							{
								list.Items.Clear();
								foreach(XmlNode node in element.ChildNodes)
								{
									string[] items = node.InnerText.Split('\t');
									ListViewItem viewItem = list.Items.Add(items[0]);
									for(int nIndex = 1; nIndex < items.Length; nIndex++)
										viewItem.SubItems.Add(items[nIndex]);
									if(list.CheckBoxes == true)
									{
										XmlAttribute attribute  = node.Attributes["Checked"];
										if(attribute != null)
											viewItem.Checked = attribute.Value.ToLower() == "true";
									}
								}
							}
						}
						catch(XmlException)
						{
						}
						break;
				}
		}
	}
}