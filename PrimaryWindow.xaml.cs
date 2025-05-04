using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Navigation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Individual_project_initial
{
    public partial class PrimaryWindow : Window
    {
        public PrimaryWindow()
        {
            InitializeComponent();
        }

        private void GoToHome_Click(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("Home.xaml", UriKind.Relative));
        }
        private void GoToAccounts_Click(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("Accounts.xaml", UriKind.Relative));
        }
        private void GoToMarkets_Click(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("Markets.xaml", UriKind.Relative));
        }
        private void GoToSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.NavigationService.Navigate(new Uri("Settings.xaml", UriKind.Relative));
        }
    }
}
