using System;
using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace ShoutyCopter.ViewModel
{
    public class Bird : ViewModelBase
    {
        private const float Gravity = 9.8f;

        private Vector _position;
        private double _scaleFactor;
        private double _width;
        private double _height;
        private Vector _velocity;
        private Vector _deltaVelocity;
        private Vector _deltaPosition;

        private readonly Queue<Action> _actionQueue = new Queue<Action>();

        public Vector DisplayPosition
        {
            get { return ToDisplayUnits(Position); }
        }

        public double DisplayHeight
        {
            get { return ToDisplayUnits(Height); }
        }

        public double DisplayWidth
        {
            get { return ToDisplayUnits(Width); }
        }

        public double ScaleFactor
        {
            get { return _scaleFactor; }
            set
            {
                if (_scaleFactor == value) return;
                _scaleFactor = value;
                RaisePropertyChanged("ScaleFactor");
            }
        }

        public Vector Position
        {
            get { return _position; }
            set
            {
                if (Equals(_position, value)) return;
                _position = value;
                RaisePropertyChanged("DisplayPosition");
            }
        }

        public Vector Velocity
        {
            get { return _velocity; }
            set
            {
                if (Equals(_velocity, value)) return;
                _velocity = value;
                RaisePropertyChanged("Velocity");
            }
        }

        public Vector DeltaVelocity
        {
            get { return _deltaVelocity; }
            set
            {
                if (Equals(_deltaVelocity, value)) return;
                _deltaVelocity = value;
                RaisePropertyChanged("DeltaVelocity");
            }
        }

        public Vector DeltaPosition
        {
            get { return _deltaPosition; }
            set
            {
                if (Equals(_deltaPosition, value)) return;
                _deltaPosition = value;
                RaisePropertyChanged("DeltaPosition");
            }
        }

        protected Vector Acceleration { get; set; }

        protected double Width
        {
            get { return _width; }
            set
            {
                if (_width == value) return;
                _width = value;
                RaisePropertyChanged("DisplayWidth");
            }
        }

        protected double Height
        {
            get { return _height; }
            set
            {
                if (_height == value) return;
                _height = value;
                RaisePropertyChanged("DisplayHeight");
            }
        }

        protected Vector ToDisplayUnits(Vector gameUnit)
        {
            return new Vector { X = gameUnit.X * ScaleFactor, Y = gameUnit.Y * ScaleFactor };
        }

        protected double ToDisplayUnits(double gameUnit)
        {
            return gameUnit * ScaleFactor;
        }

        public Bird()
        {
            Acceleration = new Vector() { X = 0, Y = Gravity };
            ScaleFactor = 10;
            Position = new Vector { X = 0, Y = 0 };
            Width = 1;
            Height = 1;
        }

        public void Update(double timeInterval)
        {
            Vector prevVelocity = Velocity;
            Velocity = new Vector
            {
                X = CalculateVelocityChange(Acceleration.X, Velocity.X, timeInterval),
                Y = CalculateVelocityChange(Acceleration.Y, Velocity.Y, timeInterval)
            };

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

            DeltaVelocity = new Vector { X = Velocity.X - prevVelocity.X, Y = Velocity.Y - prevVelocity.Y };
            Vector prevPos = Position;
            Position = new Vector { X = Position.X, Y = CacluatePositionChange(Acceleration.Y, Velocity.Y, Position.Y, timeInterval) };
            DeltaPosition = new Vector { X = Position.X - prevPos.X, Y = Position.Y - prevPos.Y };
        }

        public void QueueJump()
        {
            _actionQueue.Enqueue(Action.Jump);
        }

        /// <summary>
        /// Calculates the position change
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="velocity"></param>
        /// <param name="position"></param>
        /// <param name="timeInterval">In milliseconds</param>
        /// <returns></returns>
        private double CacluatePositionChange(double acceleration, double velocity, double position, double timeInterval)
        {
            return acceleration * Math.Pow(timeInterval / 1000, 2) + velocity * timeInterval / 1000 + position;
        }

        private double CalculateVelocityChange(double acceleration, double velocity, double timeInterval)
        {
            return acceleration * timeInterval / 1000 + velocity;
        }
    }
}
