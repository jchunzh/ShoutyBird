using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.ViewModels;

namespace ShoutyBird.Message
{
    public class GameStatusMessage : MessageBase
    {
        public GameStatus Status { get; set; }

        public GameStatusMessage(GameStatus status)
        {
            Status = status;
        }
    }
}
