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

        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;

        private double _scaleFactor;
        private double _width;
        private double _height;

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

        public double Width
        {
            get { return _width; }
            set
            {
                if (_width == value) return;
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                if (_height == value) return;
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        public RelayCommand<KeyEventArgs> Move { get; private set; }

        public MainViewModel()
        {
            Position = new Vector { X = 0, Y = 0 };
            Width = 1;
            Height = 1;
            Timer worldTimer = new Timer(TimerTick) {AutoReset = true};
            worldTimer.Elapsed += Tick;
            worldTimer.Enabled = true;
            ScaleFactor = 10;
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);
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