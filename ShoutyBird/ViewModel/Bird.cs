using System;
using System.Collections.Generic;
using System.Windows.Documents;
using ShoutyBird.Message;
using ShoutyCopter;

namespace ShoutyBird.ViewModel
{
    public class Bird : BaseUnitViewModel
    {
        private readonly Queue<Action> _actionQueue = new Queue<Action>();
        //Fraction max jump speed to jump
        private readonly Queue<double> _jumpQueue = new Queue<double>(); 

        private readonly double JumpSpeed;
        private readonly double MinJumpFactor;

        public Bird(double maxJumpSpeed, double minJumpFactor)
        {
            JumpSpeed = maxJumpSpeed;
            MinJumpFactor = minJumpFactor;
            Position = new Vector { X = 0, Y = 0 };
        }

        public override void Update(double timeInterval)
        {
            while (_jumpQueue.Count > 0)
            {
                //Action action = _actionQueue.Dequeue();
                double fraction = _jumpQueue.Dequeue();

                //if (action == Action.Jump)
                //{
                //    Velocity = new Vector
                //    {
                //        X = Velocity.X,
                //        Y = JumpSpeed
                //    };
                //}

                if (fraction < MinJumpFactor) continue;

                Velocity = new Vector
                           {
                               X = Velocity.X,
                               Y = -1 * Math.Pow(JumpSpeed, 1.8) *fraction
                           };
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

        public void QueueJump(double fraction)
        {
            _jumpQueue.Enqueue(fraction);
        }
    }
}
