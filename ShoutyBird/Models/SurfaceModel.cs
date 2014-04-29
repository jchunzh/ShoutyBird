using ShoutyBird.Message;
using ShoutyBird.ViewModels;

namespace ShoutyBird.Models
{
    public class SurfaceModel : BaseUnitModel
    {
        public SurfaceModel(UnitViewModel viewModel) : base(viewModel)
        {
            
        }

        protected override void UnitUpdateMessageRecieved(UnitUpdateMessage message)
        {
            if (message.Unit == this || message.Unit.GetType() != typeof(BirdModel)) return;
            
            if (IsCollision(Vertices, message.Unit.Vertices))
            {
                OnCollision(this, message.Unit);
            }
        }

        public override void Update(double timeInterval)
        {
            base.Update(timeInterval);
        }
    }
}
