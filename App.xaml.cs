using System.Configuration;
using System.Data;
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
}