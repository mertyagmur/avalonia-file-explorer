using Avalonia.Data.Converters;
using System;
using System.Globalization;

namespace FileExplorer.Converters;

public class IconConverter : IValueConverter
{
	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return (bool)value ? "📁" : "📄";
	}

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	{
		throw new NotImplementedException();
	}
}
