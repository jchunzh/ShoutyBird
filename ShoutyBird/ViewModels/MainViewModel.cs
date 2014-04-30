using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using System;

namespace ShoutyBird.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private ViewModelBase _currentViewModel;

        public ViewModelBase CurrentViewModel
        {
            get { return _currentViewModel; }
            set
            {
                if (Equals(_currentViewModel, value))return;
                _currentViewModel = value;
                RaisePropertyChanged("CurrentViewModel");
            }
        }

        public MainViewModel()
        {
            Messenger.Default.Register<NavigationMessage>(this, NavigationMessageRecieved);
            CurrentViewModel = SimpleIoc.Default.GetInstance(typeof (MainMenuViewModel)) as ViewModelBase;
        }

        private void NavigationMessageRecieved(NavigationMessage obj)
        {
            CurrentViewModel = SimpleIoc.Default.GetInstance(obj.DataContextType) as ViewModelBase;
        }
    }
}
