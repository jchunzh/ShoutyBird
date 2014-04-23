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
        //Time between ticks in milliseconds
        private const double TimerTick = 10d;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy = false;
        private Bird _bird;
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
            Timer worldTimer = new Timer(TimerTick) {AutoReset = true};
            worldTimer.Elapsed += Tick;
            worldTimer.Enabled = true;
            Bird = new Bird { BackgroundBrush = new SolidColorBrush(Colors.Red)};
            UnitCollection = new ObservableCollection<BaseUnitViewModel>();
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);

            UnitCollection.CollectionChanged += (sender, args) => 
                RaisePropertyChanged("UnitCollection");
            UnitCollection.Add(new PipeViewModel
                               {
                                   Position = new Vector { X = 20, Y = 0 }, 
                                   Width = 10, 
                                   Height = 10, 
                                   ScaleFactor = 10, 
                                   BackgroundBrush = new SolidColorBrush(Colors.Green), 
                                   Velocity = new Vector { X = -10, Y = 0}
                               });
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

            _time += ((Timer)sender).Interval;
            _isBusy = false;
        }

        private void MoveExecute(KeyEventArgs keyEvent)
        {
            if (keyEvent.Key == Key.Space)
            {
                Bird.QueueJump();
            }
        }

    }
}