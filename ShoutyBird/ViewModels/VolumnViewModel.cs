using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.Messages;

namespace ShoutyBird.ViewModels
{
    public class VolumnViewModel : ViewModelBase
    {
        private float _volumn;

        public float Volumn
        {
            get { return _volumn; }
            set
            {
                if (_volumn == value) return;
                _volumn = value;

                RaisePropertyChanged("Volumn");
            }
        }

        public VolumnViewModel()
        {
            Messenger.Default.Register<AudioVolumnMessage>(this, AudioVolumnMessageRecieved);
        }

        private void AudioVolumnMessageRecieved(AudioVolumnMessage obj)
        {
            Volumn = obj.VolumeSample;
        }
    }
}
