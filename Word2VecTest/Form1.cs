using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Word2Vec.Net;

namespace Word2VecTest
{
    public partial class Form1 : Form
    {
        private string _fileName;
        private string Type = "Distance";
        public Form1()
        {
            InitializeComponent();
        }

        private void SetFileName()
        {
            _analogy = new WordAnalogy(_fileName);
            _distance = new Distance(_fileName);
        }
        private Distance _distance;
        private WordAnalogy _analogy;
        private void searchBtn_Click(object sender, EventArgs e)
        {
            
            resultsListBox.Items.Clear();
            BestWord[] result = new BestWord[0];
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
            {
                resultsListBox.Items.Add(String.Format("{0}\t\t{1}", bestWord.Word, bestWord.Distance));
            }
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

        private void distanceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analogyToolStripMenuItem.Checked = false;
            distanceToolStripMenuItem.Checked = true;
            Type = "Distance";
        }

        private void analogyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            analogyToolStripMenuItem.Checked = true;
            distanceToolStripMenuItem.Checked = false;
            Type = "Analogy";
        }
    }
}
