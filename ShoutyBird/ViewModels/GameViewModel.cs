using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using NAudio.Wave;
using ShoutyBird.Message;
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

        private ObservableCollection<UnitViewModel> _unitViewModelCollection;
       
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

        public GameStatus Status { get; set; }

        private readonly Dictionary<int, UnitViewModel> UnitIdViewModelDictionary = new Dictionary<int, UnitViewModel>();
        private readonly GameWorldModel _world;

        public GameViewModel()
        {
            //SetupAudio();
            double screenWidth = 640;
            double screenHeight = 800;
            _scale = 10;
            _screenWidth = ToGameUnits(screenWidth, _scale);
            _screenHeight = ToGameUnits(screenHeight, _scale);
            _world = new GameWorldModel(_screenWidth, _screenHeight);
            _world.UnitCollection.CollectionChanged += (sender, args) =>
                                                       {
                                                           if (args.Action == NotifyCollectionChangedAction.Add)
                                                           {
                                                               foreach (BaseUnitModel unit in args.NewItems)
                                                               {
                                                                   _unitsToAdd.Enqueue(unit);
                                                               }
                                                           }
                                                           else if (args.Action == NotifyCollectionChangedAction.Remove)
                                                           {
                                                               foreach (BaseUnitModel unit in args.NewItems)
                                                               {
                                                                   _unitsToRemove.Enqueue(unit);
                                                               }
                                                           }
                                                       };
          
            //When user hits jump key
            Messenger.Default.Register<RemoveSurfaceMessage>(this, RemovePipeMessageRecieved);
            Messenger.Default.Register<AudioVolumnMessage>(this, AudioVolumnMessageRecieved);

            UnitViewModelCollection = new ObservableCollection<UnitViewModel>();

            foreach (var unit in _world.UnitCollection)
            {
                UnitViewModel viewModel = new UnitViewModel(unit.Id)
                                          {
                                              DisplayPosition = unit.DisplayPosition,
                                              Height = unit.DisplayHeight,
                                              Width = unit.DisplayWidth,
                                              Type = unit.Type
                                          };
                 
                UnitViewModelCollection.Add(viewModel);
                UnitIdViewModelDictionary.Add(unit.Id, viewModel);
            }

            UnitViewModelCollection.CollectionChanged += (sender, args) =>
                RaisePropertyChanged("UnitViewModelCollection");

            //Needs to be Windows.Forms.Timer as the other timers are asynchronous
            _displayUpdateTimer = new Timer
            {
                Interval = TimerTick,
            };
            _displayUpdateTimer.Tick += Tick;
            _displayUpdateTimer.Start();

            _world.Start();
        }

        private void AudioVolumnMessageRecieved(AudioVolumnMessage obj)
        {
            Volumn = obj.VolumeSample;
        }

        private void RemovePipeMessageRecieved(RemoveSurfaceMessage message)
        {
            //Event is fired on not the main thread (maybe?). Just in case, add unit to list of units to remove
            lock (_unitCollectionLock)
            {
                _unitsToRemove.Enqueue(message.Surface);
            }
        }

        private int _score;
        private float _volumn;

        private void Tick(object state, EventArgs e)
        {
            Score = _world.Score;
           
            while (_unitsToAdd.Count > 0)
            {
                BaseUnitModel unit = _unitsToAdd.Dequeue();
                UnitViewModel newUnitViewModel = new UnitViewModel(unit.Id);
                newUnitViewModel.Width = unit.Width;
                newUnitViewModel.Height = unit.Height;
                newUnitViewModel.DisplayPosition = unit.DisplayPosition;
                newUnitViewModel.Type = unit.Type;
                UnitViewModelCollection.Add(newUnitViewModel);
                UnitIdViewModelDictionary.Add(newUnitViewModel.Id, newUnitViewModel);
            }

            //TODO not thread safe
            foreach (var unit in _world.UnitCollection)
            {
                UnitViewModel unitViewModel;
                if (!UnitIdViewModelDictionary.TryGetValue(unit.Id, out unitViewModel)) continue;

                unitViewModel.Height = unit.DisplayHeight;
                unitViewModel.Width = unit.DisplayWidth;
                unitViewModel.DisplayPosition = unit.DisplayPosition;
            }

            lock (_unitCollectionLock)
            {
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
            }

            _time += TimerTick;
            _isBusy = false;
        }

        private double ToGameUnits(double displayUnit, double scale)
        {
            return displayUnit / scale;
        }

    }
}