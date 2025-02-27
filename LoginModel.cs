/*using System;
using System.Windows;
using MySql.Data.MySqlClient;

namespace Individual_project_initial
{
    public class LoginModel
    {
        private string? username { get; set; }
        private string? password { get; set; }
        public string Username
        {
            get => username;
            set
            {
                username = value;
                OnPropertyChanged(nameof(Username));
            }
        }
        public string Password
        {
            get => password;
            set
            {
                password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        private void verify_user
        {
            public bool ValidateUser(string username, string password)
        {
            string query = "SELECT PasswordHash FROM Users WHERE Username = @Username";
            using (MySqlConnection conn = new MySqlConnection(localConnection))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    var storedHash = cmd.ExecuteScalar() as string;
                    if (storedHash != null)
                    {
                        return VerifyPassword(password, storedHash);
                    }
                }
            }

        }
        }
    }
    }
}*/