using GalaSoft.MvvmLight.Messaging;

namespace ShoutyBird.ViewModel
{
    public class PipeViewModel : BaseUnitViewModel
    {
        public PipeViewModel()
        {
            Messenger.Default.Register<UnitUpdateMessage>(this, UnitUpdateMessageRecieved);
        }

        private void UnitUpdateMessageRecieved(UnitUpdateMessage message)
        {
            if (message.Unit.GetType() != typeof(Bird)) return;
            
            if (IsCollision(Vertices, message.Unit.Vertices))
            {
                OnCollision(this, message.Unit);
            }
        }

        private bool IsCollision(Vertices a, Vertices b)
        {
            if (a.X1 <= b.X2 && 
                a.X2 >= b.X1 && 
                a.Y1 <= b.Y2 && 
                a.Y2 >= b.Y1)
                return true;

            return false;
        }

        public override void Update(double timeInterval)
        {
            base.Update(timeInterval);
        }
    }
}
