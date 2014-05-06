using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.ViewModels;

namespace ShoutyBird.Message
{
    public class SetGameStatusMessage : MessageBase
    {
        public GameStatus FutureStatus { get; set; }

        public SetGameStatusMessage(GameStatus futureStatus)
        {
            FutureStatus = futureStatus;
        }
    }
}
