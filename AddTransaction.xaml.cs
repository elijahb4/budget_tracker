using System;
using MySql.Data.MySqlClient;
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
using IndividualProjectInitial;

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

            

            var viewModel = new UserModel();
            this.DataContext = viewModel;

            int id = viewModel.UserInstance.Id;
            string output = id.ToString();
            //useridconfirm.Text = output;

        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as UserModel;
            if (viewModel != null)
            {
                string id = viewModel.UserInstance.Username;
                int owner = int.Parse(id);
            }
            DateTime transactionDate = dateComboBox.SelectedDate.Value;
            string time = timeBox.Text;
            string transactionSum = transactionSumBox.Text;
            string account = accountBox.Text;
            string note = noteBox.Text;

            //DateTime transactionTime = ;
        }
    }
}