using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Message
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
