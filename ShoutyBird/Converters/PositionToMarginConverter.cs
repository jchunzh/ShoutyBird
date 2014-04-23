using System;
using System.Globalization;
using System.Windows.Data;
using ShoutyCopter;

namespace ShoutyBird.Converters
{
    public class PositionToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector? position = value as Vector?;

            if (position == null)
                return null;

            return string.Format("{0}, {1}, 0,0", position.Value.X, position.Value.Y);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
