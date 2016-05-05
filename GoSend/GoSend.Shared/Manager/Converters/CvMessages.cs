using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using IMM.DB.Model;
namespace IMM.Manager.Converters
{
    public class CvMessages : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var msg = value as Message;
            Style st = new Style(typeof(TextBlock));
            st.Setters.Add(new Setter(TextBlock.TextProperty, "☞" + this.getContent(ref msg)));
            //Foreground
            var color = (msg.estado == 1 && msg.origen == "2") ? ("Orange") : ("White");
            st.Setters.Add(new Setter(TextBlock.ForegroundProperty, color));

            //FontWeight
            if(msg.tipo != 1)
                st.Setters.Add(new Setter(TextBlock.FontWeightProperty, "SemiBold"));

            return st;   
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
        private string getContent(ref Message msg)
        {
            var r = ManagerGo.getTipo(msg.tipo);
            if (r == TipoMessage.TEXTO) return msg.contenido;
            else return r.ToString();
        }
    }
}
