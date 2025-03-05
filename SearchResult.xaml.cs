using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.Linq;
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
using static System.Net.WebRequestMethods;

namespace Individual_project_initial
{
    public partial class SearchResult : Page
    {
        private List<Equity> results;

        public SearchResult(List<Equity> results)
        {
            InitializeComponent();
            this.results = results;
            DisplayResults(results);
        }

        private void DisplayResults(List<Equity> results)
        {
            foreach (var result in results)
            {
                TextBlock symbolTextBlock = new TextBlock
                {
                    Text = $"Symbol: {result.symbol}"
                };

                TextBlock nameTextBlock = new TextBlock
                {
                    Text = $"Name: {result.name}"
                };

                TextBlock currencyTextBlock = new TextBlock
                {
                    Text = $"Currency: {result.currency}"
                };

                TextBlock exchangeTextBlock = new TextBlock
                {
                    Text = $"Exchange: {result.exchangeShortName}"
                };

                TextBlock exchangeFullNameTextBlock = new TextBlock
                {
                    Text = $"Exchange Full Name: {result.exchangeFullName}"
                };

                Button moreInfoButton = new Button
                {
                    Content = "More Info",
                    Tag = result.symbol
                };
                moreInfoButton.Click += MoreInfoButton_Click;

                ResultsStackPanel.Children.Add(symbolTextBlock);
                ResultsStackPanel.Children.Add(nameTextBlock);
                ResultsStackPanel.Children.Add(currencyTextBlock);
                ResultsStackPanel.Children.Add(exchangeTextBlock);
                ResultsStackPanel.Children.Add(exchangeFullNameTextBlock);
                ResultsStackPanel.Children.Add(moreInfoButton);

                //return result.symbol;
            }
        }

        private void MoreInfoButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            string symbol = button.Tag.ToString();
            Equity equity = results.Find(e => e.symbol == symbol);
            DisplayInfo displayInfoPage = new DisplayInfo(equity);
            NavigationService.Navigate(displayInfoPage);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessResult processResult = new ProcessResult();
            processResult.processSearchInfo();
        }
    }
    class ProcessResult
    {
        public void processSearchInfo() 
        {
            
        }
    }
}
