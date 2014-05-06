using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Messages
{
    public class AudioVolumnMessage : MessageBase
    {
        public float VolumeSample { get; private set; }

        public AudioVolumnMessage(float volumeSample)
        {
            VolumeSample = volumeSample;
        }
    }
}
