using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
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
