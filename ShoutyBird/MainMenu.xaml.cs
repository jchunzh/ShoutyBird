using System.Windows.Controls;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using System.Windows.Navigation;

namespace ShoutyBird
{
    /// <summary>
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : Page
    {
        public MainMenu()
        {
            InitializeComponent();
            Messenger.Default.Register<NavigationMessage>(this, NavigationMessageRecieved);
        }

        private void NavigationMessageRecieved(NavigationMessage obj)
        {
            NavigationService navService = NavigationService.GetNavigationService(this);
            navService.Navigate(obj.DestinationUri);
        }
    }
}
