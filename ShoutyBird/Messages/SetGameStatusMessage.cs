using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Models;

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
