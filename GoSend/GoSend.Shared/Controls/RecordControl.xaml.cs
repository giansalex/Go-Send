using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using IMM.Manager.Media;
// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace IMM.Controls
{
    public class EventArgsRecord : EventArgs
    {
        public String Path { set; get; }

        public bool isSuccess { set; get; }
    }
    public sealed partial class RecordControl : UserControl
    {
        private RecordProcess record;

        public delegate void EventFinishRecord(object sender, EventArgsRecord e);
        public event EventFinishRecord RecordFinish;
        private void onRecordFinish(object sender, EventArgsRecord e)
        {
            if (RecordFinish != null)
                RecordFinish(sender, e);
        }
        public RecordControl()
        {
            this.InitializeComponent();
            record = new RecordProcess();
            record.ExceptionOccured += new RecordProcess.AddOnExceptionOccuredDelegate((s, t) => {
                onRecordFinish(this, new EventArgsRecord() { isSuccess = false});
            });
        }

        private async void Play_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as AppBarButton;
            var play =((SymbolIcon)btn.Icon).Symbol == Symbol.Microphone;
            if (play)
            {
                record.StartRecord();
                btn.Icon = new SymbolIcon(Symbol.Stop);
            }
            else
            {
                record.StopRecord();
                string r = await record.SaveFile();
                record = null;
                onRecordFinish(this, new EventArgsRecord() { isSuccess = true, Path = r });
            }
        }

        private void Close_Click(object sender, TappedRoutedEventArgs e)
        {
            record.StopRecord();
            onRecordFinish(this, new EventArgsRecord() { isSuccess = false });
        }
    }
}
