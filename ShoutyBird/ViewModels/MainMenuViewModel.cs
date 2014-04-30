using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;

namespace ShoutyBird.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public RelayCommand NavigateToGame { get; set; }

        public MainMenuViewModel()
        {
            NavigateToGame = new RelayCommand(NavigateToGameExecute);
        }

        private void NavigateToGameExecute()
        {
            Messenger.Default.Send(new NavigationMessage(new Uri("/Game.xaml", UriKind.Relative)));
        }
    }
}
