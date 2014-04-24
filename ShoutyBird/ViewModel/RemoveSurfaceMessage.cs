using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.ViewModel
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
