using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
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
        protected const float Gravity = 9.8f;
        //Time between ticks in milliseconds
        private const int TimerTick = 10;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy = false;
        private readonly Timer _worldTimer;
        private Bird _bird;

        private object removeQueueLock = new object();
        private readonly Queue<BaseUnitViewModel> _unitsToRemove = new Queue<BaseUnitViewModel>();

        private ObservableCollection<BaseUnitViewModel> _unitCollection;

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
        public RelayCommand<MouseEventArgs> MouseCommand { get; private set; }


        public MainViewModel()
        {
            //_worldTimer = new Timer(Tick, null, 0, TimerTick);
            _worldTimer = new Timer
                          {
                              Interval = TimerTick,
                          };
            _worldTimer.Tick += Tick;
            _worldTimer.Start();
            Bird = new Bird
                   {
                       BackgroundBrush = new SolidColorBrush(Colors.Red), 
                       Width = 1, 
                       Height = 1,
                       Acceleration = new Vector { X = 0, Y = Gravity}
                   };
            UnitCollection = new ObservableCollection<BaseUnitViewModel>();
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);

            Messenger.Default.Register<RemovePipeMessage>(this, RemovePipeMessageRecieved);

            UnitCollection.CollectionChanged += (sender, args) => 
                RaisePropertyChanged("UnitCollection");

            CreatePipe();
            UnitCollection.Add(Bird);
        }

        private void RemovePipeMessageRecieved(RemovePipeMessage message)
        {
            lock (removeQueueLock)
            {
                _unitsToRemove.Enqueue(message.Pipe);
            }
        }

        /// <summary>
        /// Update the unit's acceleration, velocity, and position
        /// </summary>
        private void Tick(object state, EventArgs e)
        {
            if (_isBusy) return;

            _isBusy = true;

            //Bird.Update(timer.Interval);
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
            PipeViewModel pipe = new PipeViewModel
            {
                Position = new Vector { X = 20, Y = 0 },
                Width = 10,
                Height = 10,
                ScaleFactor = 10,
                BackgroundBrush = new SolidColorBrush(Colors.Green),
                Velocity = new Vector { X = -10, Y = 0 }
            };
            pipe.OnCollision += (s, e) =>
            {
                if (e.GetType() == typeof(Bird))
                {
                    Pause();
                }
            };

            pipe.OnPositionChanged += (sender, args) =>
                                      {
                                          PipeViewModel p = (PipeViewModel)sender;
                                          if (p.Vertices.X2 < 0)
                                          {
                                            Messenger.Default.Send(new RemovePipeMessage(p));
                                          }
                                      };

            UnitCollection.Add(pipe);
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
    }
}