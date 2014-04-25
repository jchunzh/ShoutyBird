using System.Windows;
using System.Windows.Controls;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class ScoreUserControl : UserControl
    {
        public ScoreUserControl()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register(
            "Score", typeof (int), typeof (ScoreUserControl), new PropertyMetadata(default(int)));

        public int Score
        {
            get { return (int) GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }
    }
}
