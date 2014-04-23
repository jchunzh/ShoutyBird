using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class UnitUserControl: UserControl
    {
        public UnitUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CanvasColorProperty = DependencyProperty.Register("CanvasColor", typeof (Color),
            typeof (UnitUserControl), new PropertyMetadata(default(Color)));

        public static readonly DependencyProperty BackgroundBrushProperty = DependencyProperty.Register("BackgroundBrush", typeof(Brush),
            typeof (UnitUserControl), new PropertyMetadata(default(Brush)));

        public Brush BackgroundBrush
        {
            get { return (Brush) GetValue(BackgroundBrushProperty); }
            set { SetValue(BackgroundBrushProperty, value);}
        }

        public Color CanvasColor
        {
            get { return (Color)GetValue(CanvasColorProperty); }
            set { SetValue(CanvasColorProperty, value); }
        }
    }
}
