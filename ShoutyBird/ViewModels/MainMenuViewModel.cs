using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;

namespace ShoutyBird.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private bool _isVisible;
        public RelayCommand NavigateToGame { get; set; }
        public RelayCommand NavigateToAudio { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                if (_isVisible == value) return;
                _isVisible = value;

                RaisePropertyChanged("IsVisible");
            }
        }

        public MainMenuViewModel()
        {
            NavigateToGame = new RelayCommand(NavigateToGameExecute);
            NavigateToAudio = new RelayCommand(NavigateToAudioExecute);
        }

        private void NavigateToGameExecute()
        {
            Messenger.Default.Send(new NavigationMessage(typeof(GameViewModel)));
            Messenger.Default.Send(new StartGameMessage());
        }

        private void NavigateToAudioExecute()
        {
            Messenger.Default.Send(new NavigationMessage(typeof(AudioViewModel)));
        }
    }
}
