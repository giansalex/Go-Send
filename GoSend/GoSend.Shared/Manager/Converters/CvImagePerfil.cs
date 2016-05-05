using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace IMM.Manager.Converters
{
    public class CvImagePerfil : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var c = new IMM.DB.Helper.DatabaseHelperClass().ReadContacto((int)value);
            if (string.IsNullOrEmpty(c.imagen))
            {
                if (c.sexo == "1")
                    return "ms-appx:///res_share/IcoHombre.png";
                else
                    return "ms-appx:///res_share/IcoMujer.png";
            }
            else
            {
                return c.imagen;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }
}
