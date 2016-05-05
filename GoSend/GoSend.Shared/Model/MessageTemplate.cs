using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using IMM.DB.Model;
namespace IMM.Model
{
    public abstract class TemplateSelector : Windows.UI.Xaml.Controls.ContentControl
    {
        public abstract DataTemplate SelectTemplate(object item, DependencyObject container);

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            ContentTemplate = SelectTemplate(newContent, this);
        }
    }

    public class MessageTemplate : TemplateSelector
    {
        public DataTemplate MeMessage
        {
            set;
            get;
        }
        public DataTemplate YouMessage
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item != null)
            {
                var m = item as Message;
                if (m.origen == "1")
                    return this.MeMessage;
                else
                    return this.YouMessage;
            }

            return null;
        }
    }
}
