using System;
using ShoutyCopter;

namespace ShoutyBird.ViewModel
{
    public class PositionChangedEventArgs : EventArgs
    {
        public Vector PreviousPosition { get; private set; }
        public Vector NewPosition { get; private set; }
        public Vector Delta { get; private set; }

        public PositionChangedEventArgs(Vector previousPosition, Vector newPosition)
        {
            PreviousPosition = previousPosition;
            NewPosition = newPosition;
            Delta = new Vector
                    {
                        X = newPosition.X - previousPosition.X,
                        Y = newPosition.Y - previousPosition.Y
                    };
        }
    }
}
