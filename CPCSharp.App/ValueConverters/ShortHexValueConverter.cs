using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CPCSharp.App.ValueConverters {
    public class ShortHexValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var byteValue = (ushort)value;
            return $"0x{byteValue:x4}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}