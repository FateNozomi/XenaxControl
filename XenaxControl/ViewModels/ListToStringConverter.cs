using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace XenaxControl.ViewModels
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class ListToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ObservableCollection<string> input = value[0] as ObservableCollection<string>;

            if (input == null)
            {
                return string.Empty;
            }

            if (input.Count <= 0)
            {
                return string.Empty;
            }

            return string.Join(string.Empty, input.ToArray());
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
