using System.Configuration;
using System.Data;
using System.Net.Http;
using System.Windows;
using static System.Net.Mime.MediaTypeNames;

namespace Individual_project_initial
{
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            
        }
    }

    public static class ApiClient
    {
        public static readonly HttpClient Client = new HttpClient();
    }
}
