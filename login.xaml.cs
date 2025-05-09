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
        private void ChangeServer_Btn(object sender, RoutedEventArgs e)
        {
            this.Hide();
            EstablishConnection establishConnection = new EstablishConnection();
            establishConnection.Show();
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
                        string data_query = "SELECT user_pk FROM user_information WHERE username = @username";
                        using (var command = new NpgsqlCommand(data_query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);
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
                        string query = "SELECT password FROM user_information WHERE username = @username";
                        using (var command = new NpgsqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@username", username);

                            object result = command.ExecuteScalar();
                            if (result == null || result == DBNull.Value)
                            {
                                return false;
                            }

                            string storedHash = result.ToString();
                            return BCrypt.Net.BCrypt.Verify(password, storedHash);
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

        private void CreateAccount_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            Register registerWindow = new Register();
            registerWindow.Show();
        }

        public static int GetOwner()
        {
            return owner;
        }
    }
}