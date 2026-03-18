using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Babel.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        // Converts enum to bool
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            return value.ToString()?.Equals(parameter.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        // Converts bool back to enum
        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (!(value is bool boolValue) || parameter == null)
                return AvaloniaProperty.UnsetValue;

            // Only update the enum when the RadioButton is checked
            return boolValue ? Enum.Parse(targetType, parameter.ToString()!) : AvaloniaProperty.UnsetValue;
        }
    }
}