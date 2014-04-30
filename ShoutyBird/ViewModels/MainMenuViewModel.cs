using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;

namespace ShoutyBird.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        public RelayCommand NavigateToGame { get; set; }
        public RelayCommand NavigateToAudio { get; set; }

        public MainMenuViewModel()
        {
            NavigateToGame = new RelayCommand(NavigateToGameExecute);
            NavigateToAudio = new RelayCommand(NavigateToAudioExecute);
        }

        private void NavigateToGameExecute()
        {
            Messenger.Default.Send(new NavigationMessage(typeof(GameViewModel)));
        }

        private void NavigateToAudioExecute()
        {
            Messenger.Default.Send(new NavigationMessage(typeof(AudioViewModel)));
        }
    }
}
