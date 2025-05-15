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
using System.Windows.Shapes;
using System.Drawing;
using System.Runtime.Remoting.Channels;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using static PGas_v2._0._0.GlobalVariables;
using Wpf.Ui.Controls;
using System.Security.Principal;

namespace PGas_v2._0._0
{
    public partial class MainWindow : FluentWindow
    {
        private List<UserAccount> allAccounts;

        public MainWindow()
        {
            InitializeComponent();

            allAccounts = new List<UserAccount>
            {
            new UserAccount { Service = "Google", Url = "https://google.com", Login = "login1login@pok.df", Password = "pis123", AddvancedOptions = null, Icon = "/resources/google_logo.png" },
            new UserAccount { Service = "Google", Url = "https://google.com", Login = "jozi@google.com", Password = "pis123", AddvancedOptions = null, Icon = "/resources/google_logo.png" },
            new UserAccount { Service = "GitHub", Url = "https://gihub.com", Login = "login1nelogin", Password = "passosp12", AddvancedOptions = null, Icon = "/resources/github_logo.png" },
            new UserAccount { Service = "OK", Url = "https://ok.ru", Login = "loginperelogin341", Password = "#dsf$df", AddvancedOptions = null, Icon = "/resources/ok_logo.png" },
            new UserAccount { Service = "ВК", Url = "https://vk.ru", Login = "login666devil", Password = "DFsdfqwerty123456", AddvancedOptions = null, Icon = "/resources/vk_logo.png" },
            new UserAccount { Service = "KakaFonia", Url = "https://kakafonia.ru", Login = "loginPoxui", Password = "kafka23324#", AddvancedOptions = null, Icon = "/resources/blank_logo.png" }
    };

            AccountsList.ItemsSource = allAccounts;
        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
            
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
                DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            var box = sender as AutoSuggestBox;

            if (e.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            string query = box.Text.ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                AccountsList.ItemsSource = allAccounts;
            }
            else
            {
                var filtered = allAccounts.Where(acc =>
                    (!string.IsNullOrEmpty(acc.Service) && acc.Service.ToLower().Contains(query)) ||
                    (!string.IsNullOrEmpty(acc.Login) && acc.Login.ToLower().Contains(query))
                ).ToList();

                AccountsList.ItemsSource = filtered;
            }
        }




        private void AutoSuggestBox_SuggestionChosen(object sender, AutoSuggestBoxSuggestionChosenEventArgs e)
        {
            if (e.SelectedItem != null)
            {
                var box = sender as AutoSuggestBox;
                box.Text = e.SelectedItem.ToString();
            }
        }

        private void AccountsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AccountsList.SelectedItem is UserAccount selectedAccount)
            {
                ServiceNameTextBox.Text = selectedAccount.Service;
                UrlTextBox.Text = selectedAccount.Url;
                LoginTextBox.Text = selectedAccount.Login;
                PasswordBox.Password = selectedAccount.Password;
            }
        }
    }
}