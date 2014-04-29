using System;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.ViewModels;

namespace ShoutyBird.Models
{
    public delegate void CollisionEventHandler(object sender, BaseUnitModel collidingUnit);
    public delegate void PositionChangedEventHandler(object sender, PositionChangedEventArgs e);

    public abstract class BaseUnitModel : IDisposable
    {
        private Vector _position;
        public event CollisionEventHandler Collision;
        public event PositionChangedEventHandler PositionChanged;
        public event EventHandler Updated;

        public double ScaleFactor { get; set; }

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

        public Vertices Vertices { get; private set; }

        public Vector Position
        {
            get { return _position; }
            set
            {
                _position = value;
                UpdateVertices();
            }
        }

        /// <summary>
        /// In gameunits / second
        /// </summary>
        public Vector Velocity { get; set; }

        public Vector Acceleration { get; set; }

        public double Width { get; set; }

        public double Height { get; set; }

        public Vector ToDisplayUnits(Vector gameUnit)
        {
            return new Vector { X = gameUnit.X * ScaleFactor, Y = gameUnit.Y * ScaleFactor };
        }

        protected double ToDisplayUnits(double gameUnit)
        {
            return gameUnit * ScaleFactor;
        }

        public UnitViewModel ViewModel { get; private set; }

        protected BaseUnitModel(UnitViewModel viewModel)
        {
            ViewModel = viewModel;
            Updated += (sender, args) => Messenger.Default.Send(new UnitUpdateMessage(this));
            Messenger.Default.Register<UnitUpdateMessage>(this, UnitUpdateMessageRecieved);
        }

        protected virtual void UnitUpdateMessageRecieved(UnitUpdateMessage message)
        {
        }

        protected bool IsCollision(Vertices a, Vertices b)
        {
            if (a.X1 <= b.X2 &&
                a.X2 >= b.X1 &&
                a.Y1 <= b.Y2 &&
                a.Y2 >= b.Y1)
                return true;

            return false;
        }

        public virtual void Update(double timeInterval)
        {
            Velocity = new Vector
            {
                X = CalculateVelocityChange(Acceleration.X, Velocity.X, timeInterval),
                Y = CalculateVelocityChange(Acceleration.Y, Velocity.Y, timeInterval)
            };

            Vector prevPosition = Position;
            Position = new Vector
            {
                X = CacluatePositionChange(Acceleration.X, Velocity.X, Position.X, timeInterval),
                Y = CacluatePositionChange(Acceleration.Y, Velocity.Y, Position.Y, timeInterval)
            };

            if (Acceleration.X != 0 || Acceleration.Y != 0 || Velocity.X != 0 || Velocity.Y != 0)
                OnPositionChanged(prevPosition);

            OnUpdated();
        }

        /// <summary>
        /// Calculates the position change
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="velocity"></param>
        /// <param name="position"></param>
        /// <param name="timeInterval">In milliseconds</param>
        /// <returns></returns>
        protected double CacluatePositionChange(double acceleration, double velocity, double position, double timeInterval)
        {
            return acceleration * Math.Pow(timeInterval / 1000, 2) + velocity * timeInterval / 1000 + position;
        }

        protected double CalculateVelocityChange(double acceleration, double velocity, double timeInterval)
        {
            return acceleration * timeInterval / 1000 + velocity;
        }

        protected void OnCollision(object sender, BaseUnitModel collidingUnit)
        {
            if (Collision != null)
                Collision(sender, collidingUnit);
        }

        protected void OnUpdated()
        {
            if (Updated != null)
                Updated(this, null);
        }

        protected void OnPositionChanged(Vector prevPosition)
        {
            if (PositionChanged != null)
                PositionChanged(this, new PositionChangedEventArgs(prevPosition, Position));
        }

        private void UpdateVertices()
        {
            Vertices = new Vertices
                       {
                           X1 = Position.X,
                           X2 = Position.X + Width,
                           Y1 = Position.Y,
                           Y2 = Position.Y + Height
                       };
        }

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
