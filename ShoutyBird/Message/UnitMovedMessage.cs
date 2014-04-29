using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Models;

namespace ShoutyBird.Message
{
    public class UnitUpdateMessage : MessageBase
    {
        public BaseUnitModel Unit { get; private set; }

        public UnitUpdateMessage(BaseUnitModel unit)
        {
            Unit = unit;
        }
    }
}
