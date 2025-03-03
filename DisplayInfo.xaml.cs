using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
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
    /// Interaction logic for DisplayInfo.xaml
    /// </summary>
    public partial class DisplayInfo : Page
    {
        public DisplayInfo(Equity? equity)
        {
            InitializeComponent();
            DisplayInformation(equity);
            
        }
        private async void DisplayInformation(Equity? equity)
        {
            this.DataContext = equity;
            string search_query = equity.symbol;
            string response = await CallApi(search_query);
            List<CompanyProfile> results = ParseResponse(response);
        }
        public async Task<string> CallApi(string search_query)
        {
            string key = "nWm0PZsupKKPYBuByJe99IurBFk8dOjy";
            string prefix = "https://financialmodelingprep.com/api/v3/profile/";
            string url = $"{prefix}{search_query}?apikey={key}";
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

        private List<CompanyProfile> ParseResponse(string response)
        {
            List<CompanyProfile> results = new List<CompanyProfile>();
            try
            {
                using JsonDocument document = JsonDocument.Parse(response);
                JsonElement root = document.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement item in root.EnumerateArray())
                    {
                        CompanyProfile profile = new()
                        {
                            Symbol = item.GetProperty("symbol").GetString(),
                            Price = item.GetProperty("price").GetString(),
                            Beta = item.GetProperty("beta").GetString(),
                            volAvg = item.GetProperty("volAvg").GetString(),
                            mktCap = item.GetProperty("mktCap").GetString(),
                            lastDiv = item.GetProperty("lastDiv").GetString(),
                            range = item.GetProperty("range").GetString(),
                            changes = item.GetProperty("changes").GetString(),
                            companyName = item.GetProperty("companyName").GetString(),
                            currency = item.GetProperty("currency").GetString(),
                            cik = item.GetProperty("cik").GetString(),
                            isin = item.GetProperty("isin").GetString(),
                            cusip = item.GetProperty("cusip").GetString(),
                            exchange = item.GetProperty("exchange").GetString(),
                            exchangeShortName = item.GetProperty("exchangeShortName").GetString(),
                            industry = item.GetProperty("industry").GetString(),
                            website = item.GetProperty("website").GetString(),
                            description = item.GetProperty("description").GetString(),
                            ceo = item.GetProperty("ceo").GetString(),
                            sector = item.GetProperty("sector").GetString(),
                            country = item.GetProperty("country").GetString(),
                            fullTimeEmployees = item.GetProperty("fullTimeEmployees").GetString(),
                            phone = item.GetProperty("phone").GetString(),
                            address = item.GetProperty("address").GetString(),
                            city = item.GetProperty("city").GetString(),
                            state = item.GetProperty("state").GetString(),
                            zip = item.GetProperty("zip").GetString(),
                            dcfDiff = item.GetProperty("dcfDiff").GetString(),
                            dcf = item.GetProperty("dcf").GetString(),
                            image = item.GetProperty("image").GetString(),
                            ipoDate = item.GetProperty("ipoDate").GetString(),
                            defaultImage = item.GetProperty("defaultImage").GetString()
                        };
                        results.Add(profile);
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
    public class CompanyProfile
    {
        public string Symbol { get; set; }
        public string Price { get; set; }
        public string Beta { get; set; }
        public string volAvg { get; set; }
        public string mktCap { get; set; }
        public string lastDiv { get; set; }
        public string range { get; set; }
        public string changes { get; set; }
        public string companyName { get; set; }
        public string currency { get; set; }
        public string cik { get; set; }
        public string isin { get; set; }
        public string cusip { get; set; }
        public string exchange { get; set; }
        public string exchangeShortName { get; set; }
        public string industry { get; set; }
        public string website { get; set; }
        public string description { get; set; }
        public string ceo { get; set; }
        public string sector { get; set; }
        public string country { get; set; }
        public string fullTimeEmployees { get; set; }
        public string phone { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip { get; set; }
        public string dcfDiff { get; set; }
        public string dcf { get; set; }
        public string image { get; set; }
        public string ipoDate { get; set; }
        public string defaultImage { get; set; }
    }
}
