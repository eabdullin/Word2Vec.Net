using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Word2Vec.Net;
using Word2Vec.Net.Analytics;

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
      Dictionary<string,double> result = null;
      switch (Type)
      {
        case "Analogy":
          result = _analogy.Search(searchTextBox.Text.Split(new[]{" "}, StringSplitOptions.RemoveEmptyEntries));
          break;
        case "Distance":
          result = _distance.Search(searchTextBox.Text);
          break;
        default:
          return;
      }
      foreach (var bestWord in result.OrderByDescending(x=>x.Value))
        resultsListBox.Items.Add($"{bestWord.Key}\t\t{bestWord.Value}");
    }

    private void SetFileName()
    {
      _analogy = new WordAnalogy(_fileName){ MinimumDistance = 0.0 };
      _distance = new Distance(_fileName);
    }
  }
}