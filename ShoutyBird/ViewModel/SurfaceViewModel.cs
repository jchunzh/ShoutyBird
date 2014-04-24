using ShoutyBird.Message;

namespace ShoutyBird.ViewModel
{
    public class SurfaceViewModel : BaseUnitViewModel
    {
        public SurfaceViewModel()
        {
            
        }

        protected override void UnitUpdateMessageRecieved(UnitUpdateMessage message)
        {
            if (message.Unit == this || message.Unit.GetType() != typeof(Bird)) return;
            
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
