using System;
using System.Collections.ObjectModel;
using System.Timers;
using System.Windows.Input;
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using ShoutyCopter;

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
        private const double TimerTick = 10d;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy = false;
        private Timer _worldTimer;
        private Bird _bird;
        private ObservableCollection<BaseUnitViewModel> _unitCollection;
        private bool _conditionOne;
        private bool _conditionTwo;
        private bool _conditionThree;
        private bool _conditionFour;

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

        private PipeViewModel Pipe { get; set; }

        public RelayCommand<KeyEventArgs> Move { get; private set; }
        public RelayCommand<MouseEventArgs> MouseCommand { get; private set; }

        public bool ConditionOne
        {
            get { return _conditionOne; }
            set
            {
                if (_conditionOne == value) return;
                _conditionOne = value;
                RaisePropertyChanged("ConditionOne");
            }
        }

        public bool ConditionTwo
        {
            get { return _conditionTwo; }
            set
            {
                if (_conditionTwo == value) return;
                _conditionTwo = value;
                RaisePropertyChanged("ConditionTwo");
            }
        }

        public bool ConditionThree
        {
            get { return _conditionThree; }
            set
            {
                if (_conditionThree == value) return;
                _conditionThree = value;
                RaisePropertyChanged("ConditionThree");
            }
        }

        public bool ConditionFour
        {
            get { return _conditionFour; }
            set
            {
                if (_conditionFour == value)
                    return;
                _conditionFour = value;
                RaisePropertyChanged("ConditionFour");
            }
        }


        public MainViewModel()
        {
            _worldTimer = new Timer(TimerTick) {AutoReset = true};
            _worldTimer.Elapsed += Tick;
            _worldTimer.Enabled = true;
            Bird = new Bird
                   {
                       BackgroundBrush = new SolidColorBrush(Colors.Red), 
                       Width = 1, 
                       Height = 1,
                       Acceleration = new Vector { X = 0, Y = Gravity}
                   };
            UnitCollection = new ObservableCollection<BaseUnitViewModel>();
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);

            UnitCollection.CollectionChanged += (sender, args) => 
                RaisePropertyChanged("UnitCollection");

            PipeViewModel pipe = new PipeViewModel
                                 {
                                     Position = new Vector {X = 20, Y = 0},
                                     Width = 10,
                                     Height = 10,
                                     ScaleFactor = 10,
                                     BackgroundBrush = new SolidColorBrush(Colors.Green),
                                     Velocity = new Vector {X = -10, Y = 0}
                                 };
            pipe.OnCollision += (s, e) =>
                                {
                                    if (e.GetType() == typeof (Bird))
                                    {
                                        Pause();
                                    }
                                };

            Pipe = pipe;
            UnitCollection.Add(pipe);
            UnitCollection.Add(Bird);
        }

        /// <summary>
        /// Update the unit's acceleration, velocity, and position
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Tick(object sender, ElapsedEventArgs e)
        {
            if (_isBusy) return;

            _isBusy = true;
            Timer timer = (Timer)sender;

            //Bird.Update(timer.Interval);
            foreach (var unit in UnitCollection)
            {
                unit.Update(timer.Interval);
            }

            ConditionOne = C1(Pipe.Vertices, Bird.Vertices);
            ConditionTwo = C2(Pipe.Vertices, Bird.Vertices);
            ConditionThree = C3(Pipe.Vertices, Bird.Vertices);
            ConditionFour = C4(Pipe.Vertices, Bird.Vertices);

            _time += ((Timer)sender).Interval;
            _isBusy = false;
        }

        public bool C1(Vertices a, Vertices b)
        {
            return a.X1 < b.X2;
        }

        public bool C2(Vertices a, Vertices b)
        {
            return a.X2 > b.X1;
        }

        public bool C3(Vertices a, Vertices b)
        {
            return a.Y1 < b.Y2;
        }

        public bool C4(Vertices a, Vertices b)
        {
            return a.Y2 > b.Y1;
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

        private void Start()
        {
            _worldTimer.Start();
        }
    }
}