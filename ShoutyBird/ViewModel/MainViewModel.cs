using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using ShoutyBird.Message;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using Timer = System.Windows.Forms.Timer;

namespace ShoutyBird.ViewModel
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
    public class MainViewModel : ViewModelBase
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
        private Bird _bird;

        private readonly object removeQueueLock = new object();
        private readonly Queue<BaseUnitViewModel> _unitsToRemove = new Queue<BaseUnitViewModel>();

        private ObservableCollection<BaseUnitViewModel> _unitCollection;
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

        public ObservableCollection<BaseUnitViewModel> UnitCollection
        {
            get { return _unitCollection; }
            set
            {
                if (Equals(_unitCollection, value)) return;
                _unitCollection = value;
                RaisePropertyChanged("UnitCollection");
            }
        }

        public Bird Bird
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

        public RelayCommand<KeyEventArgs> Move { get; private set; }

        public MainViewModel(double screenWidth, double screenHeight)
        {
            SetupAudio();

            _random = new Random(0);
            _scale = 10;
            _screenWidth = ToGameUnits(screenWidth, _scale);
            _screenHeight = ToGameUnits(screenHeight, _scale);
            //Needs to be Windows.Forms.Timer as the other timers are asynchronous
            _worldTimer = new Timer
                          {
                              Interval = TimerTick,
                          };
            _worldTimer.Tick += Tick;
            _worldTimer.Start();
            //CreateBird
            Bird = new Bird(BirdJumpSpeed, MinBirdJumpFactor)
                   {
                       Width = _screenWidth * BirdWidthFactor, 
                       Height = _screenHeight * BirdHeightFactor,
                       Acceleration = new Vector { X = 0, Y = Gravity},
                       ScaleFactor = _scale,
                   };
            Bird.Position = new Vector
                            {
                                X = (_screenWidth + Bird.Width)/2,
                                Y = (_screenHeight + Bird.Height)/8
                            };
            Bird.Collision += (sender, unit) => Pause();

            SurfaceViewModel floor = new SurfaceViewModel
                                     {
                                         Velocity = Vector.Zero,
                                         Width = _screenWidth,
                                         Height = _screenHeight* FloorHeightFactor,
                                         Position = new Vector {X = 0, Y = _screenHeight*(1 - FloorHeightFactor)},
                                         ScaleFactor = _scale,
                                     };
            floor.Collision += (sender, unit) =>
                               {
                                   //Only collide with the bird
                                   if (unit.GetType() != typeof (Bird)) return;

                                   Pause();
                               };

            //When user hits jump key
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);

            Messenger.Default.Register<RemoveSurfaceMessage>(this, RemovePipeMessageRecieved);

            UnitCollection = new ObservableCollection<BaseUnitViewModel>();
            UnitCollection.CollectionChanged += (sender, args) =>
                RaisePropertyChanged("UnitCollection");

            CreatePipe();
            UnitCollection.Add(Bird);
            UnitCollection.Add(floor);
        }

        private void SetupAudio()
        {
            int waveInDevices = WaveIn.DeviceCount;

            int selectedDevice = -1;
            for (int i = 0; i < waveInDevices; i++)
            {
                WaveInCapabilities deviceInfo = WaveIn.GetCapabilities(i);
            }

            selectedDevice = 0;

            _waveIn.DeviceNumber = selectedDevice;
            _waveIn.DataAvailable += WaveInOnDataAvailable;
            _waveIn.WaveFormat = new WaveFormat(8000, 1);

            _waveIn.StartRecording();
        }

        private void WaveInOnDataAvailable(object sender, WaveInEventArgs waveInEventArgs)
        {
            //int sum = 0;
            byte[] buffer = waveInEventArgs.Buffer;
            List<float> list = new List<float>();

            for (int index = 0; index < waveInEventArgs.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                list.Add(sample32);
            }

            Volumn = list.Max(s => s);
            Bird.QueueJump(Volumn);
        }

        private void RemovePipeMessageRecieved(RemoveSurfaceMessage message)
        {
            //Event is fired on not the main thread (maybe?). Just in case, add unit to list of units to remove
            lock (removeQueueLock)
            {
                _unitsToRemove.Enqueue(message.Surface);
            }
        }

        /// <summary>
        /// Amount of distance in game units the pipes have covered
        /// </summary>
        private double _distancePassed = 0;
        private int _score;
        private float _volumn;

        private void Tick(object state, EventArgs e)
        {
            if (_isBusy) return;

            _isBusy = true;

            foreach (var unit in UnitCollection)
            {
                unit.Update(TimerTick);
            }

            //allow screen width * PipeSpaceFactor distance pass before making a new pipe
            if (_distancePassed >= PipeSpaceFactor*_screenWidth)
            {
                _distancePassed = 0;
                CreatePipe();
            }

            lock (removeQueueLock)
            {
                if (_unitsToRemove.Count > 0)
                {
                    var unit = _unitsToRemove.Dequeue();
                    UnitCollection.Remove(unit);
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

            PipeViewModel topPipe = new PipeViewModel
            {
                Position = new Vector { X = _screenWidth + 1, Y = 0 },
                Width = _screenWidth * PipeWidthFactor,
                Height = topPipeHeight,
                ScaleFactor = _scale,
                Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
            };
            topPipe.Collision += (s, e) =>
            {
                //only collide with bird
                if (e.GetType() != typeof(Bird)) return;
                Pause();
            };

            //Score
            topPipe.PositionChanged += (sender, args) =>
                                       {
                                           var pipe = (SurfaceViewModel) sender;

                                           if (args.PreviousPosition.X + (pipe.Width / 2) > Bird.Position.X + (Bird.Width/2) &&
                                               args.NewPosition.X + (pipe.Width / 2) <= Bird.Position.X + (Bird.Width/2))
                                           {
                                               Score++;
                                           }
                                       };

            topPipe.PositionChanged += (sender, args) =>
                                      {
                                          SurfaceViewModel p = (SurfaceViewModel)sender;
                                          if (p.Vertices.X2 < -10)
                                          {
                                             //Delete the pipe if it goes off screen
                                             Messenger.Default.Send(new RemoveSurfaceMessage(p));
                                          }
                                      };

            PipeViewModel bottomPipe = new PipeViewModel
                                          {
                                              Position = new Vector { X = _screenWidth + 1, Y = bottomPipeYPos },
                                              Width = _screenWidth * PipeWidthFactor,
                                              Height = bottomPipeHeight,
                                              ScaleFactor = _scale,
                                              Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
                                          };
            bottomPipe.Collision += (s, e) =>
                                    {
                                        if (e.GetType() != typeof (Bird)) return;
                                        Pause();
                                    };

            UnitCollection.Add(topPipe);
            UnitCollection.Add(bottomPipe);
        }

        private void MoveExecute(KeyEventArgs keyEvent)
        {
            if (keyEvent.Key == Key.Space)
            {
                Bird.QueueJump(1d);
            }
        }

        private void Pause()
        {
            _worldTimer.Stop();
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