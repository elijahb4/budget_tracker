using IndividualProjectInitial;
using MySql.Data.MySqlClient;
using System.Windows;

namespace Individual_project_initial
{
    partial class Login : Window
    {
        static int owner;
        public Login()
        {
            InitializeComponent();
            var viewModel = new UserModel();
            this.DataContext = viewModel;
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Password;
            int id = 0;
            string email = "";

            if (AuthenticateUser(username, password, id))
            {
                MessageBox.Show("Login successful", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string data_query = "SELECT user_id FROM user_information WHERE username = @username AND password = @password";
                        using (MySqlCommand command = new MySqlCommand(data_query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@password", password);
                            using (MySqlDataReader reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    owner = reader.GetInt32("user_id");
                                }
                            }
                        }
                    }
                }
                var viewModel = this.DataContext as UserModel;
                if (viewModel == null)
                {
                    viewModel.SetUserInstance(new User
                    {
                        Id = id,
                        Username = username,
                        Email = email,
                        Password = password
                    });
                }
                this.Hide();
                PrimaryWindow primaryWindow = new PrimaryWindow();
                primaryWindow.Show();
            }
            else
            {
                MessageBox.Show("Invalid username or password", "Login Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private bool AuthenticateUser(string username, string password, int owner)
        {
            try
            {
                // Use your existing database connection service here
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT COUNT(*) FROM user_information WHERE username = @username AND password = @password";
                        MySqlCommand cmd = new MySqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0; // If count > 0, user exists
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database error: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
        public static int GetOwner()
        {
            return owner;
        }
    }
}