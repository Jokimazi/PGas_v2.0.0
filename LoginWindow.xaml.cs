using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
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
        private readonly ApiService APIService;

        private Thread animationThread;

        private bool isAnimating = false;

        private UseMode USE_MODE { get; set; } = UseMode.UserMode;

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
            StartLogoAnimation();


            string username = LoginBox.Text;
            string password = PasswordBox.Password;

            if (String.IsNullOrEmpty(username) && String.IsNullOrEmpty(password))
            {
                ShowErrorMessage("Введите имя пользователя и пароль");
                EnabledSwitcher();
                BoxesErrorHighlightting();
                StopLogoAnimation();
                return;
            }

            else if (String.IsNullOrEmpty(username))
            {
                StopLogoAnimation();
                ShowErrorMessage("Введите имя пользователя");
                EnabledSwitcher();
                LoginBoxErrorHighlighting();
                return;
            }

            else if (String.IsNullOrEmpty(password))
            {
                StopLogoAnimation();
                ShowErrorMessage("Введите пароль");
                EnabledSwitcher();
                    PasswordBoxErrorHighlighting();
                return;
            }

            var result = await APIService.AuthAsync(username, password);

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                StopLogoAnimation();
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



        private void StartLogoAnimation()
        {
            isAnimating = true;

            animationThread = new Thread(() =>
            {
                int frame = 0;
                int totalFrames = 22;

                while (isAnimating)
                {
                    string frameNumber = frame.ToString("D5");
                    string path = $"pack://application:,,,/Resources/pgas_logo_animation_frames/PGas_new_ai_logo_edited_2_smooth_2_{frameNumber}.png";

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.UriSource = new Uri(path, UriKind.Absolute);
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            Logo.Source = bitmap;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка загрузки кадра {frameNumber}: {ex.Message}");
                        }
                    });

                    frame = (frame + 1) % totalFrames;
                    Thread.Sleep(30);
                }
            });

            animationThread.IsBackground = true;
            animationThread.Start();
        }

        private void StopLogoAnimation()
        {
            isAnimating = false;
            animationThread?.Join();

            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri("pack://application:,,,/resources/pgas_logo/PGas_new_ai_logo_edited_2_smooth_2.png", UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    Logo.Source = bitmap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при установке стандартного логотипа: {ex.Message}");
                }
            });
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
