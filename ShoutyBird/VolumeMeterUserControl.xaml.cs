using System.Windows;
using System.Windows.Controls;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for VolumeMeterUserControl.xaml
    /// </summary>
    public partial class VolumeMeterUserControl : UserControl
    {
        public VolumeMeterUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MeterHeightFactorProperty = DependencyProperty.Register(
            "MeterHeightFactor", typeof (double), typeof (VolumeMeterUserControl), new PropertyMetadata(default(double)));

        public double MeterHeightFactor
        {
            get { return (double) GetValue(MeterHeightFactorProperty); }
            set { SetValue(MeterHeightFactorProperty, value); }
        }
    }
}
