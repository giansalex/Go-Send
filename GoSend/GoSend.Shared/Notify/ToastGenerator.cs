using System;
using System.Collections.Generic;
using System.Text;

using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
namespace IMM.Notify
{
    public class ToastGenerator
    {
        public void ShowToast(string title, string content)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastText02;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(title));
            toastTextElements[1].AppendChild(toastXml.CreateTextNode(content));

            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "long");

            ToastNotification toast = new ToastNotification(toastXml);
            toast.ExpirationTime = DateTimeOffset.UtcNow.AddSeconds(3600);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
            //ToastNotificationManager.History.Remove(toast.Tag);
        }
    }
}
