using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Wpf.Ui;
using Wpf.Ui.Appearance;

namespace PGas_v2._0._0
{
    public partial class App : Application
    {
        public static string ACCESS_TOKEN { get; set; }
        public static string REFRESH_TOKEN { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ApplicationAccentColorManager.Apply(Color.FromRgb(0, 120, 215));
        }
    }
}
