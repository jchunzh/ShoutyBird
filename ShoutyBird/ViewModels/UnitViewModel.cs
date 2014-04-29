using GalaSoft.MvvmLight;

namespace ShoutyBird.ViewModels
{
    public class UnitViewModel : ViewModelBase
    {
        private Vector _displayPosition;
        private double _width;
        private double _height;

        public Vector DisplayPosition
        {
            get { return _displayPosition; }
            set
            {
                if (Equals(_displayPosition, value)) return;
                _displayPosition = value;
                RaisePropertyChanged("DisplayPosition");
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
    }
}
