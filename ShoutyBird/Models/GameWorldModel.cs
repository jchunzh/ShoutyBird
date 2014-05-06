using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.ViewModels;
using Timer = System.Timers.Timer;

namespace ShoutyBird.Models
{
    public delegate void UnitCollectionUpdateHandler(IEnumerable<BaseUnitModel> updatedUnits);

    public delegate void UnitAddedHandler(BaseUnitModel addedUnit);

    public delegate void UnitRemovedHandler(BaseUnitModel removedUnit);

    public class GameWorldModel
    {
        //Time between ticks in milliseconds
        private const int TimerTick = 10;
        /// <summary>
        /// In ingame units/s^2
        /// </summary>
        protected const float Gravity = 500f;
        //Pipe width / screen width
        private const double PipeWidthFactor = 0.1835;
        //Bird width / screen width
        private const double BirdWidthFactor = 0.1089;
        //Bird height / screen height
        private const double BirdHeightFactor = 0.0695;
        private const double BirdJumpSpeed = 20;
        private const double MinBirdJumpFactor = 0.05;
        //Fraction of pipe height the pipe gap is
        private const double PipeGapFactor = 0.352;
        //Fraction of screen width the space between each set of pipes
        private const double PipeSpaceFactor = 0.393;
        /// <summary>
        /// Pipe speed is about 37.9% of the screen width per second
        /// </summary>
        private const double PipeSpeedFactor = -0.379;
        //Fraction of screen height the floor is
        private const double FloorHeightFactor = 0.239;

        private readonly Random _random;

        private System.Timers.Timer _gameWorldUpdateTimer;

        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenWidth;
        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenHeight;
        private readonly double _scale = 10;

        /// <summary>
        /// Amount of distance in game units the pipes have covered
        /// </summary>
        private double _distancePassed = 0;

        public readonly ObservableCollection<BaseUnitModel> UnitCollection;

        public int Score { get; private set; }
        public BirdModel Bird { get; private set; }
        public GameStatus Status { get; private set; }

        public event UnitCollectionUpdateHandler UnitCollectionUpdated;
        public event UnitAddedHandler UnitAdded;
        public event UnitRemovedHandler UnitRemoved;

        public GameWorldModel(double screenWidth, double screenHeight)
        {
            Status = GameStatus.Stopped;
            UnitCollection = new ObservableCollection<BaseUnitModel>();
            UnitCollection.CollectionChanged += UnitCollection_CollectionChanged;

            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _random = new Random(0);
            Messenger.Default.Register<KeyDownMessage>(this, KeyDownMessageRecieved);
            Messenger.Default.Register<AudioVolumnMessage>(this, AudioVolumnMessageRecieved);   
        }

        void UnitCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (BaseUnitModel unit in e.NewItems)
                {
                    OnUnitAdded(unit);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (BaseUnitModel unit in e.NewItems)
                {
                    OnUnitRemoved(unit);
                }
            }
        }

        public void Simulate()
        {
            Status = GameStatus.Running;
            SetupGame();
            _gameWorldUpdateTimer = new Timer(TimerTick);
            _gameWorldUpdateTimer.Elapsed += Update;
            _gameWorldUpdateTimer.AutoReset = true;
            _gameWorldUpdateTimer.Start();
        }

        public void StopSimulation()
        {
            Status = GameStatus.Stopped;
            _gameWorldUpdateTimer.Stop();
        }

        public void PauseSimulation()
        {
            Status = GameStatus.Paused;
            _gameWorldUpdateTimer.Stop();
        }

        public void ResumeSimulation()
        {
            Status = GameStatus.Running;
            _gameWorldUpdateTimer.Start();
        }

        private void SetupGame()
        {
            _distancePassed = 0;
            Score = 0;
            Bird = new BirdModel(UnitType.Bird, BirdJumpSpeed, MinBirdJumpFactor)
            {
                Width = _screenWidth * BirdWidthFactor,
                Height = _screenHeight * BirdHeightFactor,
                Acceleration = new Vector { X = 0, Y = Gravity },
                ScaleFactor = _scale,
            };
            Bird.Position = new Vector
            {
                X = (_screenWidth + Bird.Width) / 2,
                Y = (_screenHeight + Bird.Height) / 8
            };
            Bird.Collision += BirdCollisionEvent;

            SurfaceModel floor = new SurfaceModel(UnitType.Floor)
            {
                Velocity = Vector.Zero,
                Width = _screenWidth,
                Height = _screenHeight * FloorHeightFactor,
                Position = new Vector { X = 0, Y = _screenHeight * (1 - FloorHeightFactor) },
                ScaleFactor = _scale,
            };
            floor.Collision += NonBirdCollisionEvent;

            SurfaceModel ceiling = new SurfaceModel(UnitType.Ceiling)
            {
                Velocity = Vector.Zero,
                Width = _screenWidth,
                Height = 10,
                Position = new Vector { X = 0, Y = -9.6 },
                ScaleFactor = _scale,
            };
            ceiling.Collision += NonBirdCollisionEvent;

            UnitCollection.Clear();

            UnitCollection.Add(Bird);
            UnitCollection.Add(floor);
            UnitCollection.Add(ceiling);
            CreatePipe();
        }

        private void CreatePipe()
        {
            double pipeHeightFactor = GetRandomPipeHeightFactor();
            //Screen height that pipes can be at
            double pipeScreenHeight = _screenHeight * (1 - FloorHeightFactor);
            double topPipeHeight = pipeScreenHeight * pipeHeightFactor;
            double bottomPipeHeight = pipeScreenHeight * (1 - pipeHeightFactor - PipeGapFactor);
            double bottomPipeYPos = (PipeGapFactor + pipeHeightFactor) * pipeScreenHeight;

            PipeModel topPipe = new PipeModel(UnitType.Pipe)
            {
                Position = new Vector { X = _screenWidth + 1, Y = 0 },
                Width = _screenWidth * PipeWidthFactor,
                Height = topPipeHeight,
                ScaleFactor = _scale,
                Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
            };
            topPipe.Collision += NonBirdCollisionEvent;

            //Score
            topPipe.PositionChanged += (sender, args) =>
            {
                var pipe = (SurfaceModel)sender;

                if (args.PreviousPosition.X + (pipe.Width / 2) > Bird.Position.X + (Bird.Width / 2) &&
                    args.NewPosition.X + (pipe.Width / 2) <= Bird.Position.X + (Bird.Width / 2))
                {
                    Score++;
                }
            };

            topPipe.PositionChanged += (sender, args) =>
            {
                SurfaceModel p = (SurfaceModel)sender;
                if (p.Vertices.X2 < -10)
                {
                    //Delete the pipe if it goes off screen
                    Messenger.Default.Send(new RemoveSurfaceMessage(p));
                }
            };

            PipeModel bottomPipe = new PipeModel(UnitType.Pipe)
            {
                Position = new Vector { X = _screenWidth + 1, Y = bottomPipeYPos },
                Width = _screenWidth * PipeWidthFactor,
                Height = bottomPipeHeight,
                ScaleFactor = _scale,
                Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
            };

            bottomPipe.PositionChanged += (sender, args) =>
            {
                SurfaceModel p = (SurfaceModel)sender;
                if (p.Vertices.X2 < -10)
                {
                    //Delete the pipe if it goes off screen
                    Messenger.Default.Send(new RemoveSurfaceMessage(p));
                }
            };
            bottomPipe.Collision += NonBirdCollisionEvent;

            UnitCollection.Add(topPipe);
            UnitCollection.Add(bottomPipe);
        }

        private bool isBusy = false;

        private void Update(object sender, ElapsedEventArgs e)
        {
            if (isBusy)
                return;

            isBusy = true;

            foreach (var unit in UnitCollection)
            {
                unit.Update(TimerTick);
            }

            OnUnitCollectionUpdated(UnitCollection);

            //Check for collisions
            foreach (var i in UnitCollection)
            {
                foreach (var j in UnitCollection)
                {
                    if (i == j) continue;

                    if (IsCollision(i.Vertices, j.Vertices))
                    {
                        i.OnCollision(i, j);
                        j.OnCollision(j, i);
                    }
                }
            }
         
            //allow screen width * PipeSpaceFactor distance pass before making a new pipe
            if (_distancePassed >= PipeSpaceFactor * _screenWidth)
            {
                _distancePassed = 0;
                CreatePipe();
            }

            _distancePassed += Math.Abs(PipeSpeedFactor) * _screenWidth * TimerTick / 1000d;

            isBusy = false;
        }

        private void AudioVolumnMessageRecieved(AudioVolumnMessage obj)
        {
            if (Status == GameStatus.Running)
                Bird.QueueJump(obj.VolumeSample);
        }

        private void KeyDownMessageRecieved(KeyDownMessage obj)
        {
            if (Status == GameStatus.Running && obj.Key == Key.Space)
                Bird.QueueJump(0.5);
        }

        private void BirdCollisionEvent(object sender, BaseUnitModel collidingModel)
        {
            Stop();
        }

        private void NonBirdCollisionEvent(object sender, BaseUnitModel collidingModel)
        {
            if (collidingModel.GetType() != typeof(BirdModel)) return;
            Stop();
        }

        private void Stop()
        {
            _gameWorldUpdateTimer.Stop();
            Status = GameStatus.Stopped;
        }

        /// <summary>
        /// Double is between 0.1 and 1.0 - PipeGapFactor - 0.1
        /// </summary>
        /// <returns></returns>
        private double GetRandomPipeHeightFactor()
        {
            int randNum = _random.Next(50, 1000 - (int)(PipeGapFactor * 1000) - 50);
            return randNum / 1000d;
        }

        private void OnUnitCollectionUpdated(IEnumerable<BaseUnitModel> updatedUnits)
        {
            if (UnitCollectionUpdated != null)
                UnitCollectionUpdated(updatedUnits);
        }

        private void OnUnitAdded(BaseUnitModel addedUnit)
        {
            if (UnitAdded != null)
                UnitAdded(addedUnit);
        }

        private void OnUnitRemoved(BaseUnitModel removedUnit)
        {
            if (UnitRemoved != null)
                UnitRemoved(removedUnit);
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
    }
}
