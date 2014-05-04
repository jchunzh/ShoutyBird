using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using ShoutyBird.Message;
using ShoutyBird.Models;

namespace ShoutyBird.ViewModels
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class GameViewModel : ViewModelBase
    {
        /// <summary>
        /// In ingame units/s^2
        /// </summary>
        protected const float Gravity = 500f;
        //Time between ticks in milliseconds
        private const int TimerTick = 10;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy;
        private readonly Timer _worldTimer;
        private BirdModel _bird;

        private readonly object _unitCollectionLock = new object();
        private readonly Queue<BaseUnitModel> _unitsToRemove = new Queue<BaseUnitModel>();

        private ObservableCollection<UnitViewModel> _unitViewModelCollection;
        private readonly Random _random;
        private readonly WaveIn _waveIn = new WaveIn();

        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenWidth;
        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenHeight;
        private readonly double _scale;
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

        public List<BaseUnitModel> UnitCollection;
        
        public ObservableCollection<UnitViewModel> UnitViewModelCollection
        {
            get { return _unitViewModelCollection; }
            set
            {
                if (Equals(_unitViewModelCollection, value)) return;
                _unitViewModelCollection = value;
                RaisePropertyChanged("UnitCollection");
            }
        }

        public BirdModel Bird
        {
            get { return _bird; }
            set
            {
                if (Equals(_bird, value)) return;
                _bird = value;
                RaisePropertyChanged("Bird");
            }
        }

        public int Score
        {
            get { return _score; }
            set
            {
                if (_score == value) return;
                _score = value;
                RaisePropertyChanged("Score");
            }
        }

        public float Volumn
        {
            get { return _volumn; }
            set
            {
                if (_volumn == value) return;
                _volumn = value;

                RaisePropertyChanged("Volumn");
            }
        }

        public GameStatus Status { get; set; }

        public GameViewModel()
        {
            //SetupAudio();
            double screenWidth = 640;
            double screenHeight = 800;
            _random = new Random(0);
            _scale = 10;
            _screenWidth = ToGameUnits(screenWidth, _scale);
            _screenHeight = ToGameUnits(screenHeight, _scale);
          
            //When user hits jump key
            Messenger.Default.Register<KeyDownMessage>(this, KeyDownMessageRecieved);
            Messenger.Default.Register<RemoveSurfaceMessage>(this, RemovePipeMessageRecieved);
            Messenger.Default.Register<AudioVolumnMessage>(this, AudioVolumnMessageRecieved);

            UnitCollection = new List<BaseUnitModel>();
            UnitViewModelCollection = new ObservableCollection<UnitViewModel>();
            UnitViewModelCollection.CollectionChanged += (sender, args) =>
                RaisePropertyChanged("UnitViewModelCollection");

            //Needs to be Windows.Forms.Timer as the other timers are asynchronous
            _worldTimer = new Timer
            {
                Interval = TimerTick,
            };
            _worldTimer.Tick += Tick;

            SetupGame();
        }

        private void AudioVolumnMessageRecieved(AudioVolumnMessage obj)
        {
            Volumn = obj.VolumeSample;
            Bird.QueueJump(Volumn);
        }

        private void KeyDownMessageRecieved(KeyDownMessage obj)
        {
            if (obj.Key == Key.Space)
                Bird.QueueJump(0.5);
        }

        private void RemovePipeMessageRecieved(RemoveSurfaceMessage message)
        {
            //Event is fired on not the main thread (maybe?). Just in case, add unit to list of units to remove
            lock (_unitCollectionLock)
            {
                _unitsToRemove.Enqueue(message.Surface);
            }
        }

        private void SetupGame()
        {
            Status = GameStatus.Restarting;
            _worldTimer.Stop();
            _unitsToRemove.Clear();
            Score = 0;
            UnitViewModel birdViewModel = new UnitViewModel();
            birdViewModel.Type = UnitType.Bird;
            Bird = new BirdModel(birdViewModel, BirdJumpSpeed, MinBirdJumpFactor)
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
            Bird.PositionChanged += (s, e) =>
            {
                BirdModel model = (BirdModel)s;
                model.ViewModel.DisplayPosition = model.DisplayPosition;
            };
            birdViewModel.Width = Bird.DisplayWidth;
            birdViewModel.Height = Bird.DisplayHeight;
            birdViewModel.DisplayPosition = Bird.DisplayPosition;

            UnitViewModel floorViewModel = new UnitViewModel();
            UnitViewModel ceilingViewModel = new UnitViewModel();
            SurfaceModel floor = new SurfaceModel(floorViewModel)
            {
                Velocity = Vector.Zero,
                Width = _screenWidth,
                Height = _screenHeight * FloorHeightFactor,
                Position = new Vector { X = 0, Y = _screenHeight * (1 - FloorHeightFactor) },
                ScaleFactor = _scale,
            };
            floor.Collision += NonBirdCollisionEvent;

            SurfaceModel ceiling = new SurfaceModel(ceilingViewModel)
            {
                Velocity = Vector.Zero,
                Width = _screenWidth,
                Height = 10,
                Position = new Vector { X = 0, Y = -9.6 },
                ScaleFactor = _scale,
            };
            ceiling.Collision += NonBirdCollisionEvent;

            floorViewModel.DisplayPosition = floor.DisplayPosition;
            floorViewModel.Width = floor.DisplayWidth;
            floorViewModel.Height = floor.DisplayHeight;
            ceilingViewModel.DisplayPosition = ceiling.DisplayPosition;
            ceilingViewModel.Width = ceiling.DisplayWidth;
            ceilingViewModel.Height = ceiling.DisplayHeight;

            floorViewModel.Type = UnitType.Floor;
            ceilingViewModel.Type = UnitType.Ceiling;

            //Reset collections
            lock (_unitCollectionLock)
            {
                UnitCollection.Clear();
                UnitViewModelCollection.Clear();

                UnitCollection.Add(Bird);
                UnitCollection.Add(floor);
                UnitCollection.Add(ceiling);

                UnitViewModelCollection.Add(birdViewModel);
                UnitViewModelCollection.Add(floorViewModel);
                UnitViewModelCollection.Add(ceilingViewModel);
                CreatePipe();
            }

            Status = GameStatus.Running;
            _worldTimer.Start();
        }

        /// <summary>
        /// Amount of distance in game units the pipes have covered
        /// </summary>
        private double _distancePassed = 0;
        private int _score;
        private float _volumn;

        private void Tick(object state, EventArgs e)
        {
            if (Status == GameStatus.Restarting)
                return;

            if (_isBusy) return;

            _isBusy = true;

            lock (_unitCollectionLock)
            {
                foreach (var unit in UnitCollection)
                {
                    unit.Update(TimerTick);
                }
            }

            //allow screen width * PipeSpaceFactor distance pass before making a new pipe
            if (_distancePassed >= PipeSpaceFactor*_screenWidth)
            {
                _distancePassed = 0;
                lock (_unitCollectionLock)
                {
                    CreatePipe();
                }
            }

            lock (_unitCollectionLock)
            {
                while (_unitsToRemove.Count > 0)
                {
                    var unit = _unitsToRemove.Dequeue();
                    UnitCollection.Remove(unit);
                    UnitViewModelCollection.Remove(unit.ViewModel);
                }
            }

            _distancePassed += Math.Abs(PipeSpeedFactor) * _screenWidth * TimerTick / 1000d;
            _time += TimerTick;
            _isBusy = false;
        }

        private void CreatePipe()
        {
            double pipeHeightFactor = GetRandomPipeHeightFactor();
            //Screen height that pipes can be at
            double pipeScreenHeight = _screenHeight*(1 - FloorHeightFactor);
            double topPipeHeight = pipeScreenHeight*pipeHeightFactor;
            double bottomPipeHeight = pipeScreenHeight*(1 - pipeHeightFactor - PipeGapFactor);
            double bottomPipeYPos = (PipeGapFactor + pipeHeightFactor)*pipeScreenHeight;

            UnitViewModel topPipeViewModel = new UnitViewModel();
            UnitViewModel bottomPipeViewModel = new UnitViewModel();

            PipeModel topPipe = new PipeModel(topPipeViewModel)
            {
                Position = new Vector { X = _screenWidth + 1, Y = 0 },
                Width = _screenWidth * PipeWidthFactor,
                Height = topPipeHeight,
                ScaleFactor = _scale,
                Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
            };
            topPipe.Collision += NonBirdCollisionEvent;
            topPipeViewModel.Height = topPipe.DisplayHeight;
            topPipeViewModel.Width = topPipe.DisplayWidth;
            topPipeViewModel.DisplayPosition = topPipe.DisplayPosition;

            //Score
            topPipe.PositionChanged += (sender, args) =>
                                       {
                                           var pipe = (SurfaceModel) sender;
                                           pipe.ViewModel.DisplayPosition = pipe.DisplayPosition;

                                           if (args.PreviousPosition.X + (pipe.Width / 2) > Bird.Position.X + (Bird.Width/2) &&
                                               args.NewPosition.X + (pipe.Width / 2) <= Bird.Position.X + (Bird.Width/2))
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

            PipeModel bottomPipe = new PipeModel(bottomPipeViewModel)
                                          {
                                              Position = new Vector { X = _screenWidth + 1, Y = bottomPipeYPos },
                                              Width = _screenWidth * PipeWidthFactor,
                                              Height = bottomPipeHeight,
                                              ScaleFactor = _scale,
                                              Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
                                          };
            bottomPipeViewModel.Width = bottomPipe.DisplayWidth;
            bottomPipeViewModel.Height = bottomPipe.DisplayHeight;
            bottomPipeViewModel.DisplayPosition = bottomPipe.DisplayPosition;
            bottomPipe.PositionChanged += (s, e) =>
                                          {
                                              PipeModel pipe = (PipeModel) s;
                                              pipe.ViewModel.DisplayPosition = pipe.DisplayPosition;
                                          };
            bottomPipe.PositionChanged += (sender, args) =>
            {
                SurfaceModel p = (SurfaceModel) sender;
                if (p.Vertices.X2 < -10)
                {
                    //Delete the pipe if it goes off screen
                    Messenger.Default.Send(new RemoveSurfaceMessage(p));
                }
            };
            bottomPipe.Collision += NonBirdCollisionEvent;

            topPipeViewModel.Type = UnitType.Pipe;
            bottomPipeViewModel.Type = UnitType.Pipe;

            UnitCollection.Add(topPipe);
            UnitCollection.Add(bottomPipe);
            UnitViewModelCollection.Add(topPipeViewModel);
            UnitViewModelCollection.Add(bottomPipeViewModel);
        }

        private void Pause()
        {
            SetupGame();
            //_worldTimer.Stop();
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

        private double ToGameUnits(double displayUnit, double scale)
        {
            return displayUnit / scale;
        }

        /// <summary>
        /// Double is between 0.1 and 1.0 - PipeGapFactor - 0.1
        /// </summary>
        /// <returns></returns>
        private double GetRandomPipeHeightFactor()
        {
            int randNum = _random.Next(50, 1000 - (int)(PipeGapFactor * 1000) - 50);
            return randNum/1000d;
        }
    }
}