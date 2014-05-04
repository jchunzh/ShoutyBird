using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
                _waveIn.StopRecording();
                _waveIn.DeviceNumber = GetDeviceNumberFromCapabilities(SelectedDevice);
                _waveIn.StartRecording();
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

        private readonly WaveIn _waveIn = new WaveIn();

        public AudioViewModel()
        {
            DeviceCollection = new ObservableCollection<WaveInCapabilities>();
            DeviceCollection.CollectionChanged += (sender, args) => RaisePropertyChanged("DeviceCollection");
            NavigateBack = new RelayCommand(() => Messenger.Default.Send(new NavigateBackMessage()));
            GetAudioDevices();

            _waveIn.DataAvailable += SampleAudio;
            _waveIn.StartRecording();
        }

        private void SampleAudio(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            List<float> list = new List<float>();

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                //Converts 2 bytes to a short
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / (float)short.MaxValue;
                list.Add(sample32);
            }

            Messenger.Default.Send(new AudioVolumnMessage(list.Max()));
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

        private int GetDeviceNumberFromCapabilities(WaveInCapabilities capabilities)
        {
            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                if (AreCapabiliesEqual(WaveIn.GetCapabilities(i), capabilities))
                    return i;
            }

            throw new ArgumentException("Input capabilies not found");
        }

        private bool AreCapabiliesEqual(WaveInCapabilities a, WaveInCapabilities b)
        {
            return a.ManufacturerGuid == b.ManufacturerGuid && a.ProductGuid == b.ProductGuid;
        }
    }
}
