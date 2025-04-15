using IndividualProjectInitial;
using Npgsql;
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
                        string data_query = "SELECT user_pk FROM user_information WHERE username = @username AND password = @password";
                        using (var command = new NpgsqlCommand(data_query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@password", password);
                            using (var reader = command.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    owner = reader.GetInt32(reader.GetOrdinal("user_pk"));
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
                using (var dbHelper = new DatabaseHelper())
                {
                    using (var connection = dbHelper.GetConnection())
                    {
                        string query = "SELECT COUNT(*) FROM user_information WHERE username = @username AND password = @password";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);
                            command.Parameters.AddWithValue("@password", password);
                            int count = Convert.ToInt32(command.ExecuteScalar());
                            return count > 0;
                        }
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