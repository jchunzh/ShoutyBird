using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.ViewModel;

namespace ShoutyBird.Message
{
    public class UnitUpdateMessage : MessageBase
    {
        public BaseUnitViewModel Unit { get; private set; }

        public UnitUpdateMessage(BaseUnitViewModel unit)
        {
            Unit = unit;
        }
    }
}
