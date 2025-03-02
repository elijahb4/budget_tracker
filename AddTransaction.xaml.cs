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
    public partial class AddTransaction : Page
    {
        public AddTransaction()
        {
            InitializeComponent();
            DataContext = this;
            LoadComboBox();
        }
        private void LoadComboBox()
        {
            int owner = GetLoginOwner();
            List<string> options = GetComboBoxOptions(owner);
            if (options.Count == 0)
            {
                MessageBox.Show("No options found for the ComboBox.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                AccountComboBox.ItemsSource = options;
                Console.WriteLine("ComboBox options loaded successfully.");
            }
        }
        public List<string> GetComboBoxOptions(int owner)
        {
            List<string> accountOptions = new List<string>();

            try
            {
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT AccountNickname FROM liquid_accounts WHERE Owner = @Owner";

                        using (var command = new MySqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Owner", owner);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    string accountName = reader.GetString(0);
                                    accountOptions.Add(accountName);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading account types: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return accountOptions;
        }

        public string selectedAccountType { get; set; }
        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            
            
            DateTime transactionDate = dateComboBox.SelectedDate.Value;
            string time = timeBox.Text;
            string transactionSum = transactionSumBox.Text;
            string account = AccountComboBox.Text;
            string note = noteBox.Text;

            //DateTime transactionTime = ;
        }

        private int GetLoginOwner()
        {
            return Login.GetOwner();
        }
    }
}