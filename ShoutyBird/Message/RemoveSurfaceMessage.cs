using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.ViewModel;

namespace ShoutyBird.Message
{
    public class RemoveSurfaceMessage : MessageBase
    {
        public BaseUnitViewModel Surface { get; private set; }

        public RemoveSurfaceMessage(SurfaceViewModel surface)
        {
            Surface = surface;
        }
    }
}
