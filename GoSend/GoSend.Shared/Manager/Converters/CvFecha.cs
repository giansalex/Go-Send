using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;

namespace IMM.Manager.Converters
{
    public class CvFecha : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTime fecha;
            if (DateTime.TryParse((string)value, out fecha))
            {
                var now = DateTime.Now;
                string result = "";

                if(fecha.Year != now.Year){
		            result = fecha.ToString("dd/MM/yyyy");
	            }
	            else if(now.Month != fecha.Month || fecha.Day <=(now.Day-7)){
                    result = fecha.ToString("dd/MM hh:mm tt");
	            }
	            else if (fecha.Day != now.Day) {
                    result = fecha.ToString("ddd hh:mm tt");
	            }
	            else{
                    var t = now.Subtract(fecha);
                    result = "Hace ";
                    if (t.Hours > 0)
                        result += t.Hours.ToString() + " h";
                    else if(t.Minutes > 0)
                        result += t.Minutes.ToString() + " min";
                        else
                        result += "un momento";
	            }
                return result;
            }
            else
                return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }
}
