using System;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.Messages;
using ShoutyBird.Models;

namespace ShoutyBird.ViewModels
{
    public class InGameMenuViewModel : ViewModelBase
    {
        private bool _isVisible;
        public RelayCommand ResumeGame { get; set; }
        public RelayCommand ExitGame { get; set; }

        public bool IsVisible
        {
            get { return _isVisible; }
            private set
            {
                if (_isVisible == value) return;
                _isVisible = value;

                RaisePropertyChanged("IsVisible");
            }
        }

        public InGameMenuViewModel()
        {
            ResumeGame = new RelayCommand(ResumeGameExecute);
            ExitGame = new RelayCommand(ExitGameExecute);
            IsVisible = false;
        }

        public void ShowMenu()
        {
            IsVisible = true;
        }

        private void ResumeGameExecute()
        {
            Messenger.Default.Send(new SetGameStatusMessage(GameStatus.Running));
            IsVisible = false;
        }

        private void ExitGameExecute()
        {
            Messenger.Default.Send(new SetGameStatusMessage(GameStatus.Stopped));
            Messenger.Default.Send(new NavigationMessage(typeof(MainMenuViewModel)));
        }
    }
}
