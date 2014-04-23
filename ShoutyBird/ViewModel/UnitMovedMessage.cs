using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.ViewModel
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
