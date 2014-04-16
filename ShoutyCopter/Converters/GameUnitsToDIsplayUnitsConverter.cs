using System;
using System.Globalization;
using System.Windows.Data;

namespace ShoutyCopter.Converters
{
    public class GameUnitsToDIsplayUnitsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is double)) return null;
            double gameValue = (double) value;
            double scaleFactor = parameter == null ? 1 : (double) parameter;

            return gameValue*scaleFactor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
