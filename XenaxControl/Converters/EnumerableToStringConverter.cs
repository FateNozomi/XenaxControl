using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;

namespace XenaxControl.Converters
{
    [ValueConversion(typeof(List<string>), typeof(string))]
    public class EnumerableToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable enumerable = value[0] as IEnumerable;

            if (enumerable == null)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            foreach (var item in enumerable)
            {
                sb.AppendLine(item.ToString());
            }

            return sb.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
