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

namespace Individual_project_initial
{
    /// <summary>
    /// Interaction logic for AddTransaction.xaml
    /// </summary>
    public partial class AddTransaction : Page
    {
        public AddTransaction()
        {
            InitializeComponent();
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // Does not exist in the current context = name to add in xaml page
            //DateTime transactionTime = transactionTimeBox.Text;
            //decimal transactionSum = transactionSumBox.Text;
            string account = accountBox.Text;
            string note = noteBox.Text;
        }
    }
}
