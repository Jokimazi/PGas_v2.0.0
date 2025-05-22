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


namespace PGas_v2._0._0
{
    public partial class LoginWindow : FluentWindow
    {

        private DispatcherTimer loadingTimer;

        private int loadingStep = 0;

        private readonly ApiService APIService;

        private UseMode USE_MODE { get; set; } = UseMode.DevMode;

        private enum UseMode
        {
            DevMode,
            UserMode
        }




        public LoginWindow()
        {
            InitializeComponent();

            APIService = new ApiService(ApiService.ApiServiceMode.Authentication);

            if (USE_MODE == UseMode.DevMode)
            {
                DevEnter.Visibility = Visibility.Visible;
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

        private void DevEnter_Click(object sender, RoutedEventArgs e)
        {
            GoToMain(UseMode.DevMode);
        }

        private async void GoToMain(UseMode usemode, string access = null, string refresh = null)
        {
            this.Close();

            MainWindow main;

            if (usemode == UseMode.DevMode) 
            { 
                main = new MainWindow(MainWindow.UseMode.DevMode);
            } 
            else
            {
                main = new MainWindow(MainWindow.UseMode.UserMode);
            }

            await Task.Delay(100);

            main.Show();
        }

        private async void EnterButton_Click(object sender, RoutedEventArgs e)
        {

            EnabledSwitcher();
            HideErrorMessage();
            StartLoadingAnimation();


            string username = LoginBox.Text;
            string password = PasswordBox.Password;

            if (String.IsNullOrEmpty(username) && String.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Введите имя пользователя и пароль");
                EnabledSwitcher();
                BoxesErrorHighlightting();
                StopLoadingAnimation();
                return;
            }

            else if (String.IsNullOrEmpty(username))
            {
                StopLoadingAnimation();
                ShowErrorMessage("Введите имя пользователя");
                EnabledSwitcher();
                LoginBoxErrorHighlighting();
                return;
            }

            else if (String.IsNullOrEmpty(password))
            {
                StopLoadingAnimation();
                ShowErrorMessage("Введите пароль");
                EnabledSwitcher();
                    PasswordBoxErrorHighlighting();
                return;
            }

            var result = await APIService.AuthAsync(username, password);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                StopLoadingAnimation();
                ShowErrorMessage(result.ErrorMessage);
                BoxesErrorHighlightting();
                EnabledSwitcher();
            }
            else
            {
                App.ACCESS_TOKEN = result.AccessToken;
                App.REFRESH_TOKEN = result.RefreshToken;

                GoToMain(UseMode.UserMode);
            }
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
