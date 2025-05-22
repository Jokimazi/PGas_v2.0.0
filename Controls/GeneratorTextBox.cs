using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf.Ui.Controls;

namespace PGas_v2._0._0.Controls
{
    public class GeneratorTextBox : Wpf.Ui.Controls.TextBox
    {
        static GeneratorTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GeneratorTextBox),
                new FrameworkPropertyMetadata(typeof(GeneratorTextBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (GetTemplateChild("PART_GenerateButton") is Button generateButton)
            {
                generateButton.Click += (s, e) =>
                {
                    this.Text = "Сгенерировано!";
                };
            }
        }
    }

}
