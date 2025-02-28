using IndividualProjectInitial;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Interaction logic for AddAccount.xaml
    /// </summary>
    public partial class AddAccount : Page
    {
        public AddAccount()
        {
            InitializeComponent();

            var viewModel = new UserModel();
            this.DataContext = viewModel;
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = this.DataContext as UserModel;
            int owner = 0;
            if (viewModel != null)
            {
                string id = viewModel.UserInstance.Username;
                owner = int.Parse(id);
            }
            string institutionName = institutionNameTextBox.Text;
            string accountName = accountNameTextBox.Text;
            string accountNumber = accountNumberTextBox.Text;
            string sortCode = sortCodeTextBox.Text;
            string iban = ibanTextBox.Text;
            string bic = bicTextBox.Text;
            string reference = referenceTextBox.Text;
            string startingBalance = balanceTextBox.Text;
            string accountType = AccountTypeComboBox.Text;
            string currency = currencyTextBox.Text;

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = @"INSERT INTO liquid_accounts (InstitutionName, AccountNickname, AccountNumber, SortCode, IBAN, BIC, Reference, Balance, Owner, Currency)
                    VALUES (@InstitutionName, @AccountNickname, @AccountNumber, @SortCode, @IBAN, @BIC, @Reference, @Balance, @Owner, @Currency)";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@InstitutionName", institutionName);
                            command.Parameters.AddWithValue("@AccountNickname", accountName);
                            command.Parameters.AddWithValue("@AccountNumber", accountNumber);
                            command.Parameters.AddWithValue("@SortCode", sortCode);
                            command.Parameters.AddWithValue("@IBAN", iban);
                            command.Parameters.AddWithValue("@BIC", bic);
                            command.Parameters.AddWithValue("@Reference", reference);
                            command.Parameters.AddWithValue("@Balance", startingBalance);
                            command.Parameters.AddWithValue("@Owner", owner);
                            command.Parameters.AddWithValue("@Currency", currency);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
    }
}
