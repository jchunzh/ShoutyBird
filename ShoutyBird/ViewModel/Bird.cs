using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;
using ShoutyCopter;

namespace ShoutyBird.ViewModel
{
    public class Bird : BaseUnitViewModel
    {
        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        public Bird()
        {
            Acceleration = new Vector { X = 0, Y = Gravity };
            ScaleFactor = 10;
            Position = new Vector { X = 0, Y = 0 };
            Width = 1;
            Height = 1;
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
                        Y = -10
                    };
                }
            }

            base.Update(timeInterval);
        }

        public void QueueJump()
        {
            _actionQueue.Enqueue(Action.Jump);
        }

    }
}
