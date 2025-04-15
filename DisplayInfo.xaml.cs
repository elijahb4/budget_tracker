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
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Individual_project_initial
{
    public partial class DisplayInfo : Page
    {
        public TextWrapping TextWrapping { get; set; }
        private List<CompanyProfile> companyProfiles;
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
            companyProfiles = ParseResponse(response);
            OutputResult();
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
                            Price = item.GetProperty("price").GetDecimal().ToString(),
                            Beta = item.GetProperty("beta").GetDecimal().ToString(),
                            volAvg = item.GetProperty("volAvg").GetDecimal().ToString(),
                            mktCap = item.GetProperty("mktCap").GetDecimal().ToString(),
                            lastDiv = item.GetProperty("lastDiv").GetDecimal().ToString(),
                            range = item.GetProperty("range").GetString(),
                            changes = item.GetProperty("changes").GetDecimal().ToString(),
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
                            dcfDiff = item.GetProperty("dcfDiff").GetDecimal().ToString(),
                            dcf = item.GetProperty("dcf").GetDecimal().ToString(),

                            ipoDate = item.GetProperty("ipoDate").GetString()

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
        public void OutputResult()
        {
            if (companyProfiles != null && companyProfiles.Count > 0)
            {
                foreach (var profile in companyProfiles)
                {
                    // Define a dictionary to map property names to their values
                    var properties = new Dictionary<string, string>
                    {
                    { "Symbol", profile.Symbol },
                    { "Company Name", profile.companyName },
                    { "Price", profile.Price },
                    { "Industry", profile.industry },
                    { "Currency", profile.currency },
                    { "Exchange", profile.exchange },
                    { "CEO", profile.ceo },
                    { "Sector", profile.sector },
                    { "Country", profile.country },
                    { "Full-Time Employees", profile.fullTimeEmployees },
                    { "Phone", profile.phone },
                    { "Address", profile.address },
                    { "City", profile.city },
                    { "State", profile.state },
                    { "Zip", profile.zip },
                    { "Website", profile.website },
                    { "Description", profile.description }
                    };


                    // Loop through the dictionary to create TextBlocks
                    foreach (var property in properties)
                    {
                        TextBlock textBlock = new TextBlock
                        {
                            Text = $"{property.Key}: {property.Value}",
                            TextWrapping = TextWrapping.Wrap
                        };
                        ResultsStackPanel.Children.Add(textBlock);
                    }

                    // Optionally, add a separator or other UI elements between profiles
                    ResultsStackPanel.Children.Add(new Separator());
                }
            }
        }
    }

    public class CompanyProfile
    {
        public required string Symbol { get; set; }
        public required string Price { get; set; }
        public required string Beta { get; set; }
        public required string volAvg { get; set; }
        public required string mktCap { get; set; }
        public required string lastDiv { get; set; }
        public required string range { get; set; }
        public required string changes { get; set; }
        public required string companyName { get; set; }
        public required string currency { get; set; }
        public required string cik { get; set; }
        public required string isin { get; set; }
        public required string cusip { get; set; }
        public required string exchange { get; set; }
        public required string exchangeShortName { get; set; }
        public required string industry { get; set; }
        public required string website { get; set; }
        public required string description { get; set; }
        public required string ceo { get; set; }
        public required string sector { get; set; }
        public required string country { get; set; }
        public required string fullTimeEmployees { get; set; }
        public required string phone { get; set; }
        public required string address { get; set; }
        public required string city { get; set; }
        public required string state { get; set; }
        public required string zip { get; set; }
        public required string dcfDiff { get; set; }
        public required string dcf { get; set; }
        //public required string image { get; set; }
        public required string ipoDate { get; set; }
        //public required string defaultImage { get; set; }
    }
}
