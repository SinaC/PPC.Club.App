using System;
using System.Windows;
using System.Windows.Data;

namespace PPC.Players.Converters
{
    //http://stackoverflow.com/questions/13571902/hide-cell-in-new-row-in-wpf-datagrid
    public class IsNamedObjectVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.GetType().Name == "NamedObject" // NamedObject is an internal microsoft type used for empty row
                ? Visibility.Hidden 
                : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
