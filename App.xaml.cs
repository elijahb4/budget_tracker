using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;

namespace Individual_project_initial
{
    public partial class App : Application
    {
        public void PerformDatabaseOperation()
        {
            using (var dbHelper = new DatabaseHelper())
            {
                using (var connection = dbHelper.GetConnection())
                {

                }
            }
        }
    }
    public static class ApiClient
    {
        public static readonly HttpClient Client = new HttpClient();
    }
}