using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Babel.Converters;

public class EnumToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Colors.Transparent);

        bool isSelected = value.ToString() == parameter.ToString();
        
        if (isSelected)
            return new SolidColorBrush(Colors.White);
        else
            return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public class EnumToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return new SolidColorBrush(Colors.White);

        bool isSelected = value.ToString() == parameter.ToString();
        
        if (isSelected)
            return new SolidColorBrush(Colors.Black);
        else
            return new SolidColorBrush(Colors.White);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}