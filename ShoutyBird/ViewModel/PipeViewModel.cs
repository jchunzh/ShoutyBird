using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.ViewModel
{
    public class PipeViewModel : BaseUnitViewModel
    {
        public PipeViewModel()
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
