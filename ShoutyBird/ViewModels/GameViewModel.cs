using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using ShoutyBird.Message;
using ShoutyBird.Messages;
using ShoutyBird.Models;
using Timer = System.Windows.Forms.Timer;

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
        //Time between ticks in milliseconds
        private const int TimerTick = 10;
        /// <summary>
        /// in milliseconds
        /// </summary>
        private double _time;
        private bool _isBusy;
        /// <summary>
        /// Timer for when the viewmodels are updated
        /// </summary>
        private readonly Timer _displayUpdateTimer;

        private readonly object _unitCollectionLock = new object();
        private readonly Queue<BaseUnitModel> _unitsToRemove = new Queue<BaseUnitModel>();
        private readonly Queue<BaseUnitModel> _unitsToAdd = new Queue<BaseUnitModel>(); 
        private readonly Queue<BaseUnitModel> _unitsToUpdate = new Queue<BaseUnitModel>(); 

        private ObservableCollection<UnitViewModel> _unitViewModelCollection;

        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenWidth;
        /// <summary>
        /// In ingame units
        /// </summary>
        private readonly double _screenHeight;
        private readonly double _scale;

        private int _score;
        private float _volumn;
        private InGameMenuViewModel _menu;

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

        public InGameMenuViewModel Menu

        {
            get { return _menu; }
            set
            {
                if (Equals(_menu, value)) return;
                _menu = value;

                RaisePropertyChanged("Menu");
            }
        }

        private readonly Dictionary<int, UnitViewModel> UnitIdViewModelDictionary = new Dictionary<int, UnitViewModel>();
        private readonly GameWorldModel _world;

        private readonly BackgroundWorker _backgroundWorker = new BackgroundWorker();

        public GameViewModel()
        {
            double screenWidth = 640;
            double screenHeight = 800;
            _scale = 10;
            _screenWidth = ToGameUnits(screenWidth, _scale);
            _screenHeight = ToGameUnits(screenHeight, _scale);

            UnitViewModelCollection = new ObservableCollection<UnitViewModel>();
            UnitViewModelCollection.CollectionChanged += (sender, args) =>
                RaisePropertyChanged("UnitViewModelCollection");

            _world = new GameWorldModel(_screenWidth, _screenHeight);
            _world.UnitAdded += unit => _unitsToAdd.Enqueue(unit);
            _world.UnitRemoved += unit => _unitsToRemove.Enqueue(unit);
            _world.UnitCollectionUpdated += unitCollection =>
                                            {
                                                foreach (BaseUnitModel unit in unitCollection)
                                                {
                                                    _unitsToUpdate.Enqueue(unit);
                                                }
                                            };

            foreach (BaseUnitModel unit in _world.UnitCollection)
            {
                UnitAdded(unit);
            }
          
            //When user hits jump key
            Messenger.Default.Register<RemoveSurfaceMessage>(this, RemovePipeMessageRecieved);

            //Needs to be Windows.Forms.Timer as the other timers are asynchronous
            _displayUpdateTimer = new Timer
            {
                Interval = TimerTick,
            };
            _displayUpdateTimer.Tick += Tick;
            _displayUpdateTimer.Start();

            _backgroundWorker.DoWork += BackgroundWorkerOnDoWork;
            _backgroundWorker.RunWorkerCompleted += StopGame;
            _backgroundWorker.WorkerSupportsCancellation = true;

            Messenger.Default.Register<StartGameMessage>(this, StartGame);
            Messenger.Default.Register<SetGameStatusMessage>(this, SetGameStatus);
            Messenger.Default.Register<KeyDownMessage>(this, KeyDownMessageRecieved);
            Menu = new InGameMenuViewModel();
            _world.SetupGame();
        }

        private void KeyDownMessageRecieved(KeyDownMessage obj)
        {
            if (obj.Key == Key.Escape)
            {
                if (_world.Status == GameStatus.Running)
                    Pause();
                else if (_world.Status == GameStatus.Paused)
                    Resume();
            }
        }

        private void SetGameStatus(SetGameStatusMessage obj)
        {
            switch (obj.FutureStatus)
            {
                case GameStatus.Paused:
                    Pause();
                    break;
                case GameStatus.Running:
                    Resume();
                    break;
                case GameStatus.Stopped:
                    Exit();
                    break;
            }
        }

        private void Exit()
        {
            _world.StopSimulation();
            UnitViewModelCollection.Clear();
            UnitIdViewModelDictionary.Clear();
        }

        private void StopGame(object sender, RunWorkerCompletedEventArgs e)
        {
            StartGame(null);
        }

        private void StartGame(StartGameMessage obj)
        {
            UnitViewModelCollection.Clear();
            UnitIdViewModelDictionary.Clear();

            if (_backgroundWorker.IsBusy)
                throw new NotImplementedException("About to start background worker while it is still running");

            _backgroundWorker.RunWorkerAsync();
        }

        private void BackgroundWorkerOnDoWork(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            _world.Simulate();

            while (_world.Status == GameStatus.Running || _world.Status == GameStatus.Paused)
            {
                Thread.Sleep(10);
            }
        }

        private void RemovePipeMessageRecieved(RemoveSurfaceMessage message)
        {
            //Event is fired on not the main thread (maybe?). Just in case, add unit to list of units to remove
            lock (_unitCollectionLock)
            {
                _unitsToRemove.Enqueue(message.Surface);
            }
        }

        private void Tick(object state, EventArgs e)
        {
            Score = _world.Score;
           
            while (_unitsToAdd.Count > 0)
            {
                BaseUnitModel unit = _unitsToAdd.Dequeue();
                UnitAdded(unit);
            }

            while (_unitsToUpdate.Count > 0)
            {
                BaseUnitModel unit = _unitsToUpdate.Dequeue();
                UnitViewModel unitViewModel;
                if (unit == null || !UnitIdViewModelDictionary.TryGetValue(unit.Id, out unitViewModel)) continue;

                unitViewModel.Height = unit.DisplayHeight;
                unitViewModel.Width = unit.DisplayWidth;
                unitViewModel.DisplayPosition = unit.DisplayPosition;
            }
            
            while (_unitsToRemove.Count > 0)
            {
                var unit = _unitsToRemove.Dequeue();
                UnitViewModel unitViewModel;
                if (UnitIdViewModelDictionary.TryGetValue(unit.Id, out unitViewModel))
                {
                    UnitViewModelCollection.Remove(unitViewModel);
                    UnitIdViewModelDictionary.Remove(unit.Id);
                }
            }

            _time += TimerTick;
            _isBusy = false;
        }

        private void UnitAdded(BaseUnitModel unit)
        {
            UnitViewModel newUnitViewModel = new UnitViewModel(unit.Id);
            newUnitViewModel.Width = unit.DisplayWidth;
            newUnitViewModel.Height = unit.DisplayHeight;
            newUnitViewModel.DisplayPosition = unit.DisplayPosition;
            newUnitViewModel.Type = unit.Type;
            UnitViewModelCollection.Add(newUnitViewModel);
            UnitIdViewModelDictionary.Add(newUnitViewModel.Id, newUnitViewModel);
            
        }

        private double ToGameUnits(double displayUnit, double scale)
        {
            return displayUnit / scale;
        }

        private void Pause()
        {
            _world.PauseSimulation();
            Menu.ShowMenu();
        }

        private void Resume()
        {
            _world.ResumeSimulation();
        }
    }
}