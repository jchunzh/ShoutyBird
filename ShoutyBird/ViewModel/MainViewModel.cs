using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyCopter;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
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

        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenWidth;
        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenHeight;
        private readonly double _scale;
        //Pipe width / scren width
        private const double PipeWidthFactor = 0.1835;
        //Bird width / screen width
        private const double BirdWidthFactor = 0.1089;
        //Bird height / screen height
        private const double BirdHeightFactor = 0.0695;
        private const double BirdJumpSpeed = -100;

        //Pipe speed is about 37.9% of the screen width per second
        private const double PipeSpeedFactor = -0.379;

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

        public RelayCommand<KeyEventArgs> Move { get; private set; }

        public MainViewModel(double screenWidth, double screenHeight)
        {
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
            Bird = new Bird(BirdJumpSpeed)
                   {
                       BackgroundBrush = new SolidColorBrush(Colors.Red), 
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
            UnitCollection = new ObservableCollection<BaseUnitViewModel>();
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);

            Messenger.Default.Register<RemoveSurfaceMessage>(this, RemovePipeMessageRecieved);

            UnitCollection.CollectionChanged += (sender, args) => 
                RaisePropertyChanged("UnitCollection");

            CreatePipe();
            UnitCollection.Add(Bird);
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
        /// Update the unit's acceleration, velocity, and position
        /// </summary>
        private void Tick(object state, EventArgs e)
        {
            if (_isBusy) return;

            _isBusy = true;

            foreach (var unit in UnitCollection)
            {
                unit.Update(TimerTick);
            }

            lock (removeQueueLock)
            {
                if (_unitsToRemove.Count > 0)
                {
                    var unit = _unitsToRemove.Dequeue();
                    UnitCollection.Remove(unit);
                }
            }

            _time += TimerTick;
            _isBusy = false;
        }

        private void CreatePipe()
        {
            SurfaceViewModel surface = new SurfaceViewModel
            {
                Position = new Vector { X = _screenWidth + 1, Y = 0 },
                Width = _screenWidth * PipeWidthFactor,
                Height = 10,
                ScaleFactor = _scale,
                BackgroundBrush = new SolidColorBrush(Colors.Green),
                Velocity = new Vector { X = PipeSpeedFactor * _screenWidth, Y = 0 }
            };
            surface.Collision += (s, e) =>
            {
                if (e.GetType() == typeof(Bird))
                {
                    Pause();
                }
            };

            surface.PositionChanged += (sender, args) =>
                                      {
                                          SurfaceViewModel p = (SurfaceViewModel)sender;
                                          if (p.Vertices.X2 < -10)
                                          {
                                             //Delete the pipe if it goes off screen
                                             Messenger.Default.Send(new RemoveSurfaceMessage(p));
                                          }
                                      };

            UnitCollection.Add(surface);
        }

        private void MoveExecute(KeyEventArgs keyEvent)
        {
            if (keyEvent.Key == Key.Space)
            {
                Bird.QueueJump();
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
    }
}