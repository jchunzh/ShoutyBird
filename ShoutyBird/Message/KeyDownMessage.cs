using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.Message
{
    public class KeyDownMessage : MessageBase
    {
        public Key Key { get; private set; }

        public KeyDownMessage(Key key)
        {
            Key = key;
        }
    }
}
