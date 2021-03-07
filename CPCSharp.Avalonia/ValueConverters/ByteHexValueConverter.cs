using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace CPCSharp.App.ValueConverters {
    public class ByteHexValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var byteValue = (byte)value;
            return $"0x{byteValue:x2}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}