using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Threading;
using Newtonsoft.Json;
using Wpf.Ui.Controls;
using static PGas_v2._0._0.GlobalVariables;


namespace PGas_v2._0._0
{
    public partial class LoginWindow : FluentWindow
    {

        private DispatcherTimer loadingTimer;
        private int loadingStep = 0;

        private void StartLoadingAnimation()
        {
            LoadingTextBlock.Visibility = Visibility.Visible;
            loadingStep = 0;

            loadingTimer = new DispatcherTimer();
            loadingTimer.Interval = TimeSpan.FromMilliseconds(100);
            loadingTimer.Tick += (s, e) =>
            {
                loadingStep = (loadingStep + 1) % 4;
                switch (loadingStep)
                {
                    case 0:
                        LoadingTextBlock.Text = "Ɛ=D";
                        break;
                    case 1:
                        LoadingTextBlock.Text = "Ɛ==D";
                        break;
                    case 2:
                        LoadingTextBlock.Text = "Ɛ===D";
                        break;
                    case 3:
                        LoadingTextBlock.Text = "Ɛ==D";
                        break;
                }
            };
            loadingTimer.Start();
        }

        private void StopLoadingAnimation()
        {
            if (loadingTimer != null)
            {
                loadingTimer.Stop();
                LoadingTextBlock.Visibility = Visibility.Collapsed;
                LoadingTextBlock.Text = string.Empty;
            }
        }


        public LoginWindow()
        {
            InitializeComponent();
        }

        private void LoginBoxErrorHighlighting()
        {
            LoginBox.Background = new SolidColorBrush(Color.FromArgb(0x0F, 0xFF, 0x00, 0x00));
            LoginBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD0, 0x00, 0x00));
        }

        private void PasswordBoxErrorHighlighting()
        {
            PasswordBox.Background = new SolidColorBrush(Color.FromArgb(0x0F, 0xFF, 0x00, 0x00));
            PasswordBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xD0, 0x00, 0x00));
        }

        private void BoxesErrorHighlightting()
        {
            this.LoginBoxErrorHighlighting();
            this.PasswordBoxErrorHighlighting();
        }

        private void BoxesNormalizeHighlighting()
        {
            LoginBox.ClearValue(Control.BackgroundProperty);
            LoginBox.ClearValue(Control.BorderBrushProperty);

            PasswordBox.ClearValue(Control.BackgroundProperty);
            PasswordBox.ClearValue(Control.BorderBrushProperty);
        }

        private void EnabledSwitcher()
        {
            LoginBox.IsEnabled = !LoginBox.IsEnabled;
            PasswordBox.IsEnabled = !PasswordBox.IsEnabled;
            LoginButton.IsEnabled = !LoginButton.IsEnabled;
        }

        private void ShowErrorMessage(string error)
        {
            ErrorMessageTextBoxContainer.Visibility = Visibility.Visible;
            ErrorMessageTextBox.Text = error;
        }

        private void HideErrorMessage()
        {
            ErrorMessageTextBoxContainer.Visibility = Visibility.Collapsed;
        }

        private class AuthResponse
        {
            [JsonProperty("access")]
            public string AccessToken { get; set; }

            [JsonProperty("refresh")]
            public string RefreshToken { get; set; }

            public string ErrorMessage { get; set; }
        }


        private async Task<AuthResponse> LoginAsync(string username, string password)
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new
                {
                    username = username,
                    password = password
                };

                string json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(REST_API_URL, content);

                string responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    AuthResponse result = JsonConvert.DeserializeObject<AuthResponse>(responseContent);
                    return result;
                }
                else
                {
                    return new AuthResponse
                    {
                        ErrorMessage = ParseErrorMessage(responseContent)
                    };
                }
            }
        }

        private string ParseErrorMessage(string json)
        {
            try
            {
                var errorObj = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                return errorObj.Values.FirstOrDefault()?.ToString() ?? "Неизвестная ошибка";
            }
            catch
            {
                return "Ошибка при разборе сообщения: " + json;
            }
        }

        private async void EnterButton_Click(object sender, RoutedEventArgs e)
        {

            this.EnabledSwitcher();
            this.HideErrorMessage();
            this.StartLoadingAnimation();


            string username = LoginBox.Text;
            string password = PasswordBox.Password;

            if (String.IsNullOrEmpty(username) && String.IsNullOrEmpty(password))
            {
                this.ShowErrorMessage("Введите имя пользователя и пароль");
                this.EnabledSwitcher();
                this.BoxesErrorHighlightting();
                this.StopLoadingAnimation();
                return;
            }

            else if (String.IsNullOrEmpty(username))
            {
                this.StopLoadingAnimation();
                this.ShowErrorMessage("Введите имя пользователя");
                this.EnabledSwitcher();
                this.LoginBoxErrorHighlighting();
                return;
            }

            else if (String.IsNullOrEmpty(password))
            {
                this.StopLoadingAnimation();
                this.ShowErrorMessage("Введите пароль");
                this.EnabledSwitcher();
                this.PasswordBoxErrorHighlighting();
                return;
            }

            var result = await LoginAsync(username, password);
            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                this.StopLoadingAnimation();
                this.ShowErrorMessage(result.ErrorMessage);
                this.BoxesErrorHighlightting();
                this.EnabledSwitcher();
            }
            else
            {

                ACCESS_TOKEN = result.AccessToken;
                REFRESH_TOKEN = result.RefreshToken;

                SwitchWindowToMain();
            }
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

        private async void SwitchWindowToMain()
        {
            this.Close();
            var main = new MainWindow();

            await Task.Delay(100);

            main.Show();
        }


        private void DevEnter(object sender, RoutedEventArgs e)
        {
            SwitchWindowToMain();
        }

        private void PasswordBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.BoxesNormalizeHighlighting();
            //this.HideErrorMessage();
        }

        private void LoginBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.BoxesNormalizeHighlighting();
            //this.HideErrorMessage();
        }
    }
}
