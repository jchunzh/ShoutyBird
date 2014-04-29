using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Models;

namespace ShoutyBird.Message
{
    public class RemoveSurfaceMessage : MessageBase
    {
        public BaseUnitModel Surface { get; private set; }

        public RemoveSurfaceMessage(SurfaceModel surface)
        {
            Surface = surface;
        }
    }
}
