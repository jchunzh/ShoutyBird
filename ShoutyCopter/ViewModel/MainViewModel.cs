using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Timers;
using System.Windows.Input;
using System;

namespace ShoutyCopter.ViewModel
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
        private Vector _position;

        private const float Gravity = 1000f;
        //Time between ticks in milliseconds
        private const double TimerTick = 10d;
        private readonly Timer _worldTimer;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;

        public Vector Position
        {
            get { return _position; }
            set
            {
                if (Equals(_position, value)) return;
                _position = value;
                RaisePropertyChanged("Position");
            }
        }
        public RelayCommand<KeyEventArgs> Move { get; private set; }

        public MainViewModel()
        {
            Position = new Vector { X = 0, Y = 0 };
            _worldTimer = new Timer(TimerTick);
            _worldTimer.AutoReset = true;
            _worldTimer.Elapsed += Tick;
            _worldTimer.Enabled = true;

            Move = new RelayCommand<KeyEventArgs>(MoveExecute);
        }

        private string _text;
        public string Text
        {
            get { return _text; }
            set
            {
                if (_text == value) return;
                _text = value;
                RaisePropertyChanged("Text");
            }
        }

        private void Tick(object sender, ElapsedEventArgs e)
        {
            Timer timer = (Timer)sender;

            Position = new Vector { X = Position.X, Y = Acceleration(Gravity, 0, Position.Y, timer.Interval) };
            _time += ((Timer)sender).Interval;
        }

        /// <summary>
        /// Calculates the position change
        /// </summary>
        /// <param name="acceleration"></param>
        /// <param name="velocity"></param>
        /// <param name="position"></param>
        /// <param name="timeInterval">In milliseconds</param>
        /// <returns></returns>
        private double Acceleration(double acceleration, double velocity, double position, double timeInterval)
        {
            return acceleration * Math.Pow(timeInterval / 1000, 2) + velocity * timeInterval / 1000 + position;
        }

        private void MoveExecute(KeyEventArgs keyEvent)
        {
         
        }


    }
}