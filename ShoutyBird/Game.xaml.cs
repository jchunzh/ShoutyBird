using System.Windows.Controls;
using ShoutyBird.ViewModels;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for Game.xaml
    /// </summary>
    public partial class Game : Page
    {
        public Game()
        {
            InitializeComponent();
            this.DataContext = new MainViewModel(640, 800);
            Display.Focus();
        }
    }
}
