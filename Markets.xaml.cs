using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Net.Http;
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
using System.Text.Json;

namespace Individual_project_initial
{
    public partial class Markets : Page
    {
        public Markets()
        {
            InitializeComponent();
        }

        private async void SearchButtonClick(object sender, RoutedEventArgs e)
        {
            string search_query = SearchBox.Text;
            string response = await CallApi(search_query);

            if (!string.IsNullOrEmpty(response))
            {
                List<Equity> results = ParseResponse(response);
                SearchResult resultsPage = new SearchResult(results);
                NavigationService.Navigate(resultsPage);
            }
        }

        public async Task<string> CallApi(string search_query)
        {
            string key = "nWm0PZsupKKPYBuByJe99IurBFk8dOjy";
            string prefix = "https://financialmodelingprep.com/api/v3/search?query=";
            string url = $"{prefix}{search_query}&apikey={key}";
            return await queryLookup(url);
        }

        public async Task<string> queryLookup(string url)
        {
            try
            {
                HttpResponseMessage response = await ApiClient.Client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException e)
            {
                MessageBox.Show($"Request error: {e.Message}");
                return null;
            }
        }

        private List<Equity> ParseResponse(string response)
        {
            List<Equity> results = new List<Equity>();
            try
            {
                using JsonDocument document = JsonDocument.Parse(response);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        Equity equity = new Equity
                        {
                            symbol = item.GetProperty("symbol").GetString(),
                            name = item.GetProperty("name").GetString(),
                            currency = item.GetProperty("currency").GetString(),
                            exchangeFullName = item.GetProperty("stockExchange").GetString(),
                            exchangeShortName = item.GetProperty("exchangeShortName").GetString()
                        };
                        results.Add(equity);
                    }
                }
            }
            catch (JsonException ex)
            {
                MessageBox.Show($"Error parsing JSON: {ex.Message}");
            }
            return results;
        }
    }
    public class Equity
    {
        public string symbol { get; set; }
        public string name { get; set; }
        public string currency { get; set; }
        public string exchangeFullName { get; set; }
        public string exchangeShortName { get; set; }
    }
}
