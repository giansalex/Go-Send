using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace IMM.Manager.Converters
{
    class CvStateMsg : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var s = int.Parse(value.ToString());
            var sv = int.Parse(parameter.ToString());
            if (sv == 5)
            {
                if (s < 1)
                    return "Visible";
                else
                    return "Collapsed";
            }
            if (sv <= s)
                return "DodgerBlue";
            else
                return "Gray";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }
}
