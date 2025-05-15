using System.Windows;
using System.Windows.Media;

namespace PGas_v2._0._0
{
    public class ButtonBlueUnderline : Wpf.Ui.Controls.Button
    {
        public ButtonBlueUnderline()
        {
            this.Height = 38;
            this.Width = 92;
            this.FontSize = 24;
            this.HorizontalAlignment = HorizontalAlignment.Center;
            this.Margin = new Thickness(0, 20, 0, 0);

            // BorderBrush
            this.BorderBrush = CreateBorderBrush("#18FFFFFF", "#FF0078D7");

            // MouseOverBorderBrush
            this.MouseOverBorderBrush = CreateBorderBrush("#18FFFFFF", "#FF0072CA");

            // PressedBorderBrush
            this.PressedBorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0.5, 0),
                EndPoint = new Point(0.5, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString("#12FFFFFF"), 0.84),
                    new GradientStop((Color)ColorConverter.ConvertFromString("#FF0064B3"), 0.853)
                }
            };
        }

        private LinearGradientBrush CreateBorderBrush(string color1, string color2)
        {
            return new LinearGradientBrush
            {
                EndPoint = new Point(0, 3),
                MappingMode = BrushMappingMode.Absolute,
                RelativeTransform = new TransformGroup
                {
                    Children = new TransformCollection
                    {
                        new ScaleTransform { CenterX = 0.5, CenterY = 0.5 },
                        new SkewTransform { CenterX = 0.5, CenterY = 0.5 },
                        new RotateTransform { Angle = -180, CenterX = 0.5, CenterY = 0.5 }
                    }
                },
                GradientStops = new GradientStopCollection
                {
                    new GradientStop((Color)ColorConverter.ConvertFromString(color1), 1),
                    new GradientStop((Color)ColorConverter.ConvertFromString(color2), 0.99)
                }
            };
        }
    }
}
