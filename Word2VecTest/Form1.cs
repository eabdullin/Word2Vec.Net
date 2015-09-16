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
        public Form1()
        {
            InitializeComponent();
        }

        private Distance _distance = new Distance("..\\..\\..\\Data\\result_kazakh_150.bin");
        private void searchBtn_Click(object sender, EventArgs e)
        {
            resultsListBox.Items.Clear();
            var result = _distance.Search(searchTextBox.Text);
            foreach (var bestWord in result)
            {
                resultsListBox.Items.Add(String.Format("{0}\t\t{1}", bestWord.Word, bestWord.Distance));
            }
        }
    }
}
