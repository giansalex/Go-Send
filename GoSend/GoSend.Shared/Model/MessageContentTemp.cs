using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;

namespace IMM.Model
{
    public class MessageContentTemp : TemplateSelector
    {
        public DataTemplate TextMessage
        {
            set;
            get;
        }
        public DataTemplate ImageMessage
        {
            get;
            set;
        }
        public DataTemplate AudioMessage
        {
            set;
            get;
        }
        public DataTemplate FileTemplate
        {
            set;
            get;
        }
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                var i = item as IMM.DB.Model.Message;
                switch (i.tipo)
                {
                    case 1: return this.TextMessage;
                    case 2: return this.ImageMessage;
                    case 3: return this.AudioMessage;
                    default: return this.FileTemplate;
                }
            }
            return null;
        }
    }
}
