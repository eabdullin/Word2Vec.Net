using System;
using System.Windows.Forms;
using Word2Vec.Net;

namespace Word2VecTest
{
  public partial class Form1 : Form
  {
    private WordAnalogy _analogy;
    private Distance _distance;
    private string _fileName;
    private string Type = "Distance";

    public Form1() { InitializeComponent(); }

    private void analogyToolStripMenuItem_Click(object sender, EventArgs e)
    {
      analogyToolStripMenuItem.Checked = true;
      distanceToolStripMenuItem.Checked = false;
      Type = "Analogy";
    }

    private void distanceToolStripMenuItem_Click(object sender, EventArgs e)
    {
      analogyToolStripMenuItem.Checked = false;
      distanceToolStripMenuItem.Checked = true;
      Type = "Distance";
    }

    private void openToolStripMenuItem_Click(object sender, EventArgs e)
    {
      var result = openFileDialog.ShowDialog();
      if (result == DialogResult.OK)
      {
        _fileName = openFileDialog.FileName;
        SetFileName();
      }
    }

    private void searchBtn_Click(object sender, EventArgs e)
    {
      resultsListBox.Items.Clear();
      var result = new BestWord[0];
      switch (Type)
      {
        case "Analogy":
          result = _analogy.Search(searchTextBox.Text);
          break;
        case "Distance":
          result = _distance.Search(searchTextBox.Text);
          break;
        default:
          break;
      }
      foreach (var bestWord in result)
        resultsListBox.Items.Add(string.Format("{0}\t\t{1}", bestWord.Word, bestWord.Distance));
    }

    private void SetFileName()
    {
      _analogy = new WordAnalogy(_fileName);
      _distance = new Distance(_fileName);
    }
  }
}