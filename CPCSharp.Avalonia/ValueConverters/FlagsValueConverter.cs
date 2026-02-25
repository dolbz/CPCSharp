using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Z80;

namespace CPCSharp.App.ValueConverters {
    public class FlagsValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var flags = (Z80Flags)value;
            return string.Concat(
                flags.HasFlag(Z80Flags.Sign_S)              ? "S" : "·",
                flags.HasFlag(Z80Flags.Zero_Z)              ? "Z" : "·",
                flags.HasFlag(Z80Flags.HalfCarry_H)         ? "H" : "·",
                flags.HasFlag(Z80Flags.ParityOverflow_PV)   ? "P" : "·",
                flags.HasFlag(Z80Flags.AddSubtract_N)       ? "N" : "·",
                flags.HasFlag(Z80Flags.Carry_C)             ? "C" : "·"
            );
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
