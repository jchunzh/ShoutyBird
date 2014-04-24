using System.Collections.Generic;
using System.Windows.Documents;
using ShoutyCopter;

namespace ShoutyBird.ViewModel
{
    public class Bird : BaseUnitViewModel
    {
        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        private readonly double JumpSpeed;

        public Bird(double jumpSpeed)
        {
            JumpSpeed = jumpSpeed;
            Position = new Vector { X = 0, Y = 0 };
        }

        public override void Update(double timeInterval)
        {
            while (_actionQueue.Count > 0)
            {
                Action action = _actionQueue.Dequeue();

                if (action == Action.Jump)
                {
                    Velocity = new Vector
                    {
                        X = Velocity.X,
                        Y = JumpSpeed
                    };
                }
            }

            base.Update(timeInterval);
        }

        protected override void UnitUpdateMessageRecieved(UnitUpdateMessage message)
        {
            if (message.Unit == this) return;

            if (IsCollision(message.Unit.Vertices, Vertices))
            {
                OnCollision(this, message.Unit);
            }
        }

        public void QueueJump()
        {
            _actionQueue.Enqueue(Action.Jump);
        }
    }
}
