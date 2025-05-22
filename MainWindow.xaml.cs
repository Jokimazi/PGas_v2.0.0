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
using Wpf.Ui.Controls;
using System.Security.Principal;
using System.Windows.Media.TextFormatting;
using System.Diagnostics.Tracing;
using System.Windows.Controls.Primitives;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Windows.Media.Animation;

namespace PGas_v2._0._0
{
    public partial class MainWindow : FluentWindow
    {
        public List<UserAccount> AllAccounts { get; set; }

        public List<ServiceItems.ServiceItem> AllServices { get; set; }

        public UseMode USE_MODE { get; set; }

        private readonly ApiService APIService;

        public enum UseMode
        {
            DevMode,
            UserMode
        }

        public enum PasswordStrength
        {
            Weak,
            Medium,
            Strong
        }

        public MainWindow(UseMode usemode)
        {
            InitializeComponent();

            USE_MODE = usemode;
                
            AllServices = new ServiceItems().AllServices;
            ServiceComboBox.ItemsSource = AllServices;
            ServiceComboBox.SelectedItem = AllServices.FirstOrDefault(s => s.Name == "Другой сервис");

            if (USE_MODE == UseMode.DevMode)
            {
                var UA = new UserAccount();

                AllAccounts = new List<UserAccount>
                {
                    UA.CreateUser("Google", "https://google.com", "login1login@pok.df", "pis123" ),
                    UA.CreateUser("Google", "https://google.com", "jozi@google.com", "pis123" ),
                    UA.CreateUser("GitHub", "https://gihub.com", "login1nelogin", "passosp12" ),
                    UA.CreateUser("OK", "https://ok.ru", "loginperelogin341", "#dsf$df" ),
                    UA.CreateUser("VK", "https://vk.ru", "login666devil", "DFsdfqwerty123456" ),
                    UA.CreateUser("KakaFonia", "https://kakafonia.ru", "loginPoxui", "kafka23324#" ),
                    UA.CreateUser("Googor", "https://lox.pidor", "loginPoxui", "kafka23324#" ),
                    UA.CreateUser("Algol", "https://algol.net", "loginPoxui", "kafka23324#" ),
                };

                AllAccounts = AllAccounts.OrderBy(s => s.Service).ThenBy(s => s.Service.ToLower()).ToList();
                AccountsList.ItemsSource = AllAccounts;
            }
            else
            {
                APIService = new ApiService(ApiService.ApiServiceMode.DataInteraction);

                UpdateAllAccountsList();
            }
        }

        private async void UpdateAllAccountsList()
        {
            AllAccounts = await APIService.GetDataAsync();

            AllAccounts = AllAccounts.OrderBy(s => s.Service).ThenBy(s => s.Service.ToLower()).ToList();
            AccountsList.ItemsSource = AllAccounts;
        }

        public static PasswordStrength CheckPasswordStrength(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return PasswordStrength.Weak;

            int score = 0;

            if (password.Length >= 8)
                score++;
            if (password.Length >= 10)
                score++;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);
            if (hasLetter && hasDigit)
                score++;

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            if (hasUpper && hasLower)
                score++;

            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));
            if (hasSpecial)
                score++;

            if (score <= 2)
                return PasswordStrength.Weak;
            else if (score == 3 || score == 4)
                return PasswordStrength.Medium;
            else
                return PasswordStrength.Strong;
        }



        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MaxMinWinButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HideButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 1)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    var mousePosition = e.GetPosition(this);
                    double percentX = mousePosition.X / this.ActualWidth;

                    Point screenPoint = PointToScreen(mousePosition);

                    this.WindowState = WindowState.Normal;

                    this.Left = screenPoint.X - this.ActualWidth * percentX;
                    this.Top = 0;

                    DragMove();
                }
                else
                {
                    DragMove();
                }
            }
        }



        private async void GoToLogin_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            var login = new LoginWindow();

            await Task.Delay(100);

            login.Show();
        }




        private async void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if (USE_MODE == UseMode.UserMode)
            {
                UserAccount selected = AccountsList.SelectedItem as UserAccount;

                int id = selected.Id;

                if(await APIService.DeleteDataAsync(id))
                {
                    UpdateAllAccountsList();
                }
            }
        }
        
        private async void AddAccount_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SaveChange_Click(object sender, RoutedEventArgs e)
        {

        }




        private void ServiceComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ServiceComboBox.SelectedItem is ServiceItems.ServiceItem selected)
            {
                var SelectedItem = AccountsList.SelectedItem as UserAccount;

                if (SelectedItem == null || selected.Name == SelectedItem.Service)
                {
                    SaveButton.IsEnabled = false;
                }
                else
                {
                    SaveButton.IsEnabled = true;
                }

                if (selected.IsCustom)
                {
                    UnotherServiceTextBox.Visibility = Visibility.Visible;
                    UrlTextBox.IsReadOnly = false;
                    UrlTextBox.Text = null;
                }
                else
                {
                    UnotherServiceTextBox.Visibility = Visibility.Collapsed;
                    UrlTextBox.IsReadOnly = true;
                    UrlTextBox.Text = selected.Url;
                }
            }
        }

        private void UnotherServiceTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var SelectedItem = AccountsList.SelectedItem as UserAccount;

            if (SelectedItem == null || UnotherServiceTextBox.Text == SelectedItem.Service)
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }        
        
        private void PasswordBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var SelectedItem = AccountsList.SelectedItem as UserAccount;

            string password = PasswordBox.Password;

            if (SelectedItem == null || password == SelectedItem.Password)
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }

            PasswordStrength strength = CheckPasswordStrength(password);

            switch (strength)
            {
                case PasswordStrength.Weak:
                    FirstSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xF1, 0x00, 0x00));
                    SecondSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                    ThirdSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                    break;
                case PasswordStrength.Medium:
                    FirstSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFD, 0xFF, 0x00));
                    SecondSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xFD, 0xFF, 0x00));
                    ThirdSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x80, 0x80, 0x80));
                    break;
                case PasswordStrength.Strong:
                    FirstSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0xC5, 0x00));
                    SecondSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0xC5, 0x00));
                    ThirdSegmentPasswordDifficultyBar.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0x18, 0xC5, 0x00));
                    break;
            }
        }

        private void UrlTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var SelectedItem = AccountsList.SelectedItem as UserAccount;

            if (SelectedItem == null || UrlTextBox.Name == SelectedItem.Url)
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }
        private void LoginTextBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var SelectedItem = AccountsList.SelectedItem as UserAccount;

            if (SelectedItem == null || LoginTextBox.Text == SelectedItem.Login)
            {
                SaveButton.IsEnabled = false;
            }
            else
            {
                SaveButton.IsEnabled = true;
            }
        }

        private void AutoSuggestBox_TextChanged(object sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            var box = sender as AutoSuggestBox;

            if (e.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
                return;

            string query = box.Text.ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                AccountsList.ItemsSource = AllAccounts;
            }
            else
            {
                var filtered = AllAccounts.Where(acc =>
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
                var matchingService = AllServices.FirstOrDefault(s => s.Name == selectedAccount.Service);

                ServiceComboBox.SelectedItem = matchingService ?? AllServices.FirstOrDefault();

                UnotherServiceTextBox.Text = selectedAccount.Service;
                UrlTextBox.Text = selectedAccount.Url;
                LoginTextBox.Text = selectedAccount.Login;
                PasswordBox.Password = selectedAccount.Password;
            }
        }
    }
}