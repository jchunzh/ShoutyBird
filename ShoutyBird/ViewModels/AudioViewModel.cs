using System.Collections.ObjectModel;
using System.Windows.Forms;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using ShoutyBird.Message;

namespace ShoutyBird.ViewModels
{
    public class AudioViewModel : ViewModelBase
    {
        public WaveInCapabilities SelectedDevice
        {
            get { return _selectedDevice; }
            set
            {
                if (Equals(_selectedDevice, value))return;
                _selectedDevice = value;
                RaisePropertyChanged("SelectedDevice");
            }
        }

        public ObservableCollection<WaveInCapabilities> DeviceCollection
        {
            get { return _deviceCollection; }
            set
            {
                if (Equals(_deviceCollection, value)) return;
                _deviceCollection = value;
                RaisePropertyChanged("DeviceCollection");
            }
        }

        public RelayCommand NavigateBack { get; set; }

        private WaveInCapabilities _selectedDevice;
        private ObservableCollection<WaveInCapabilities> _deviceCollection;

        public AudioViewModel()
        {
            DeviceCollection = new ObservableCollection<WaveInCapabilities>();
            DeviceCollection.CollectionChanged += (sender, args) => RaisePropertyChanged("DeviceCollection");
            NavigateBack = new RelayCommand(() => Messenger.Default.Send(new NavigateBackMessage()));
            GetAudioDevices();
        }

        private void GetAudioDevices()
        {
            int waveInDevices = WaveIn.DeviceCount;

            for (int i = 0; i < waveInDevices; i++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(i);
                DeviceCollection.Add(deviceInfo);
            }
        }
    }
}
