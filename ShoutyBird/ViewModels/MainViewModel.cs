using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;

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

        private ViewModelBase _prevViewModel { get; set; }

        public RelayCommand<KeyEventArgs> KeyDown { get; set; }

        public MainViewModel()
        {
            Messenger.Default.Register<NavigationMessage>(this, NavigationMessageRecieved);
            Messenger.Default.Register<NavigateBackMessage>(this, NavigateBackMessageRecieved);
            CurrentViewModel = SimpleIoc.Default.GetInstance(typeof (MainMenuViewModel)) as ViewModelBase;

            KeyDown = new RelayCommand<KeyEventArgs>(KeyDownExecute);
        }

        private void KeyDownExecute(KeyEventArgs obj)
        {
            Messenger.Default.Send(new KeyDownMessage(obj.Key));
        }

        private void NavigateBackMessageRecieved(NavigateBackMessage obj)
        {
            ViewModelBase temp = CurrentViewModel;
            CurrentViewModel = _prevViewModel;
            _prevViewModel = temp;
        }

        private void NavigationMessageRecieved(NavigationMessage obj)
        {
            _prevViewModel = CurrentViewModel;
            CurrentViewModel = SimpleIoc.Default.GetInstance(obj.DataContextType) as ViewModelBase;
        }
    }
}
