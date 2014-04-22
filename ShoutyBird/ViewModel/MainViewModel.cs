using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows.Controls;
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
        //Time between ticks in milliseconds
        private const double TimerTick = 10d;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy = false;
        private Bird _bird;

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
            Bird = new Bird();
            
            Move = new RelayCommand<KeyEventArgs>(MoveExecute);
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

            Bird.Update(timer.Interval);

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