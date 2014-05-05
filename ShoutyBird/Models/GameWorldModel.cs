using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.ViewModels;

namespace ShoutyBird.Models
{
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

        private readonly System.Timers.Timer _gameWorldUpdateTimer;

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

        public GameWorldModel(double screenWidth, double screenHeight)
        {
            UnitCollection = new ObservableCollection<BaseUnitModel>();
            _screenWidth = screenWidth;
            _screenHeight = screenHeight;
            _random = new Random(0);
            Messenger.Default.Register<KeyDownMessage>(this, KeyDownMessageRecieved);
            Messenger.Default.Register<AudioVolumnMessage>(this, AudioVolumnMessageRecieved);
            SetupGame();
            _gameWorldUpdateTimer = new Timer(TimerTick);
            _gameWorldUpdateTimer.Elapsed += Update;
            _gameWorldUpdateTimer.AutoReset = true;
        }

        public void Start()
        {
            _gameWorldUpdateTimer.Start();
        }

        private void SetupGame()
        {
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
            //_unitsToAdd.Enqueue(topPipe);
            //_unitsToAdd.Enqueue(bottomPipe);
            //UnitViewModelCollection.Add(topPipeViewModel);
            //UnitViewModelCollection.Add(bottomPipeViewModel);

            //UnitIdViewModelDictionary.Add(topPipe.Id, topPipeViewModel);
            //UnitIdViewModelDictionary.Add(bottomPipe.Id, bottomPipeViewModel);
        }


        private void Update(object sender, ElapsedEventArgs e)
        {
            foreach (var unit in UnitCollection)
            {
                unit.Update(TimerTick);
            }
         
            //allow screen width * PipeSpaceFactor distance pass before making a new pipe
            if (_distancePassed >= PipeSpaceFactor * _screenWidth)
            {
                _distancePassed = 0;
                CreatePipe();
            }

            _distancePassed += Math.Abs(PipeSpeedFactor) * _screenWidth * TimerTick / 1000d;
        }

        private void AudioVolumnMessageRecieved(AudioVolumnMessage obj)
        {
            Bird.QueueJump(obj.VolumeSample);
        }

        private void KeyDownMessageRecieved(KeyDownMessage obj)
        {
            if (obj.Key == Key.Space)
                Bird.QueueJump(0.5);
        }

        private void BirdCollisionEvent(object sender, BaseUnitModel collidingModel)
        {
            Pause();
        }

        private void NonBirdCollisionEvent(object sender, BaseUnitModel collidingModel)
        {
            if (collidingModel.GetType() != typeof(BirdModel)) return;
            Pause();
        }

        private void Pause()
        {
            _gameWorldUpdateTimer.Stop();
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
    }
}
