using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Word2Vec.Net;

namespace Word2VecApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private string _binFile = "result_output";
        private string _searchText;
        private Distance _distance = new Distance("result_output");
        public MainWindow()
        {
            InitializeComponent();
        }


        private void SearchBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = SearchTxtBox.Text;
            var result = _distance.Search(text);

        }
    }
}
