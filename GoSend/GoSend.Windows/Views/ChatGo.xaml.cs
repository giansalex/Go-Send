using GoSend.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Core;
using IMM.DB.Model;
using IMM.Manager;
using IMM.DB.Helper;
using Windows.ApplicationModel.DataTransfer;

using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234237

namespace GoSend.Views
{
    /// <summary>
    /// A basic page that provides characteristics common to most applications.
    /// </summary>
    public sealed partial class ChatGo : Page, IShowMessage
    {
        #region Variables
        private Contacto CtActual;
        private NavigationHelper navigationHelper;
        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        #endregion

        public ChatGo()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session. The state will be null the first time a page is visited.</param>
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            CtActual = e.NavigationParameter as Contacto;

            pageTitle.Text = CtActual.alias;
            EstadoContacto.Text = CtActual.phone; // Muestra el estado del contacto
            DatabaseHelperClass db = new DatabaseHelperClass();
            var l = db.ReadMessageOfContact(CtActual.phone);
            foreach(var m in l){
                ListMsg.Items.Add(m);
            }
            //ListMsg.ItemsSource = this.l;
            if(l.Count > 0)
                ListMsg.Loaded += ListMsg_Loaded;
            ListMsg.Items.VectorChanged += Items_VectorChanged; // Se ubica en el ultimo sms
            App.mgo.setShowManager(this);
            App.mgo.SetState(CtActual.phone);// Envia estado de msg que me envio este user
        }

        async void ListMsg_Loaded(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() =>
            {
                ListMsg.ScrollIntoView(ListMsg.Items.Last());
            });
        }

        private async void Items_VectorChanged(Windows.Foundation.Collections.IObservableVector<object> sender, Windows.Foundation.Collections.IVectorChangedEventArgs @event)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ListMsg.ScrollIntoView(ListMsg.Items.Last());
            });
        }
        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void navigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
        }
        #region NavigationHelper registration

        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// 
        /// Page specific logic should be placed in event handlers for the  
        /// <see cref="GridCS.Common.NavigationHelper.LoadState"/>
        /// and <see cref="GridCS.Common.NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method 
        /// in addition to page state preserved during an earlier session.

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            navigationHelper.OnNavigatedFrom(e);
        }

        #endregion       
        public void ShowMessages(List<Message> mensajes, List<Contacto> newContactos)
        {
            foreach(var s in mensajes){
                if (s.phone != CtActual.phone)
                {
                    var name = new DatabaseHelperClass().ReadContacto(s.phone);
                    new IMM.Notify.ToastGenerator().ShowToast(name.alias, this.getContent(s));
                }
                else
                {
                    AddMsgView(s);
                }
            }
            if (mensajes.Any(i => i.phone == CtActual.phone))
            {
                var r = mensajes.Last(i => i.phone == CtActual.phone);
                App.mgo.SetState(CtActual.phone, r);
            }
        }
        private string getContent(Message msg)
        {
            var r = ManagerGo.getTipo(msg.tipo);
            if (r == TipoMessage.TEXTO) return msg.contenido;
            else return r.ToString();
        }

        private async void AddMsgView(Message m)
        {       
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ListMsg.Items.Add(m);
                ListMsg.ScrollIntoView(m);
            });
        }
        #region Events XAML
        private async void ShowMsgBox(string msg)
        {
            var w = new MessageDialog(msg, "GoSend");
            w.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            await w.ShowAsync();
        }
        private string validaMsg()
        {
            var msg = TxtMsg.Text.Trim();
            return msg;
        }
        private async void Send_Click(object sender, RoutedEventArgs e)
        {
            var sms = this.validaMsg();
            if (sms == "") return;
            var item = await App.mgo.EnviarMsg(sms, TipoMessage.TEXTO, CtActual.phone);

            this.AddMsgView(item);
            TxtMsg.Text = "";
        }

        private void MenuMessage_Select(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as Message;
            switch ((sender as MenuFlyoutItem).Tag.ToString())
            {
                case "1"://Copiar
                    var dataCopy = new DataPackage();
                    switch (ManagerGo.getTipo(item.tipo))
                    {
                        case TipoMessage.TEXTO:
                            dataCopy.SetText(item.contenido);
                            Clipboard.SetContent(dataCopy);
                            break;
                    }
                    
                    break;
                case "2"://Hablar
                    break;
                case "3"://Eliminar
                    new DatabaseHelperClass().DeleteMessage(item.id);
                    ListMsg.Items.Remove(item);
                    break;
                case "4"://Reenviar
                    break;
                case "5"://Update
                    StackUpdate.DataContext = item;
                    if(StackUpdate.Visibility == Windows.UI.Xaml.Visibility.Collapsed)
                        showUpd.Begin();
                    break;
            }
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void CloseUpdate_Click(object sender, RoutedEventArgs e)
        {
            HideUpd.Begin();
        }

        private async void UpdateMsg_Click(object sender, RoutedEventArgs e)
        {
            HideUpd.Begin();
            var msg = txtUpdMsg.Text.Trim();
            if (msg != "")
            {
                var m = (sender as AppBarButton).DataContext as Message;
                if (msg.Equals(m.contenido)) return;
                bool r = await App.mgo.UpdateMessageExist(m, msg);
                if (r)
                {
                    var idx = ListMsg.Items.IndexOf(m);
                    m.contenido = msg;
                    ListMsg.Items.RemoveAt(idx);
                    ListMsg.Items.Insert(idx, m);
                }
                else
                    this.ShowMsgBox("El mensaje no puede ser actualizado");
            }
        }
        #endregion

        #region Image, Audio and Files
        private async void SelectImage()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            //openPicker.CommitButtonText = "Abrir Imagen";
            openPicker.FileTypeFilter.Add(".jpg");
            openPicker.FileTypeFilter.Add(".jpeg");
            openPicker.FileTypeFilter.Add(".png");

            var storageFileW = await openPicker.PickSingleFileAsync();
            if (storageFileW == null) return;

            var msgr = await App.mgo.EnviarMsg(storageFileW.Path, TipoMessage.IMAGEN, CtActual.phone);
            this.AddMsgView(msgr);
        }

        private void SelectionAttach_Changed(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as GridView).SelectedIndex != -1)
            {
                //Flyout.GetAttachedFlyout((FrameworkElement)sender).Hide();
                switch ((sender as GridView).SelectedIndex)
                {
                    case 0: SelectImage();
                        break;
                    case 1: SelectFile();
                        break;
                    case 3: GrabarAudio();
                        break;
                }
            }
        }

        private void Attach_Listopc(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private async void PlayAudio_Click(object sender, RoutedEventArgs e)
        {
            var bt = sender as AppBarButton;
            //if (((SymbolIcon)(sender as AppBarButton).Icon).Symbol == Symbol.Pause) return;
            bt.Icon = new SymbolIcon(Symbol.Pause);
            MediaElement med = new MediaElement();
            var msg = (bt.DataContext as Message);
            StorageFile f = await StorageFile.GetFileFromPathAsync(msg.contenido);
            med.SetSource(await f.OpenAsync(FileAccessMode.Read), f.ContentType);
            med.MediaEnded += new RoutedEventHandler((send, mediaElemnt) =>
            {
                (sender as AppBarButton).Icon = new SymbolIcon(Symbol.Play);
            });  
            med.Play();
        
        }
        private void GrabarAudio()
        {
            if (PanelRecord.Visibility == Windows.UI.Xaml.Visibility.Visible) return;
            IMM.Controls.RecordControl cont = new IMM.Controls.RecordControl();
            cont.RecordFinish += cont_RecordFinish;
            PanelRecord.Children.Add(cont);
            PanelRecord.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        async void cont_RecordFinish(object sender, IMM.Controls.EventArgsRecord e)
        {
            PanelRecord.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            PanelRecord.Children.Remove((UIElement)sender);
            if (e.isSuccess)
            {
                StorageFile s = await StorageFile.GetFileFromPathAsync(e.Path);
                await s.OpenAsync(FileAccessMode.Read);
                var m = await App.mgo.EnviarMsg(e.Path, TipoMessage.AUDIO,CtActual.phone);
                this.AddMsgView(m);
            }
        }

        private async void SelectFile()
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.ViewMode = PickerViewMode.List;
            openPicker.SuggestedStartLocation = PickerLocationId.ComputerFolder;
            openPicker.CommitButtonText = "Send File";
            openPicker.FileTypeFilter.Add(".pdf");
            openPicker.FileTypeFilter.Add(".txt");
            openPicker.FileTypeFilter.Add(".doc");
            openPicker.FileTypeFilter.Add(".docx");
            var storageFileW = await openPicker.PickSingleFileAsync();
            if (storageFileW == null) return;
            var a = await storageFileW.GetBasicPropertiesAsync();
            if (a.Size > 1048576)
            {
                this.ShowMsgBox("El archivo " + storageFileW.Name + " supera 1Mb");
            }
            else
            {
                var msgr = await App.mgo.EnviarMsg(storageFileW.Path, TipoMessage.FILE, CtActual.phone);
                this.AddMsgView(msgr);
            }
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var file = await StorageFile.GetFileFromPathAsync(((sender as HyperlinkButton).DataContext as Message).contenido);
            Windows.System.LauncherOptions d = new Windows.System.LauncherOptions();
            //d.ContentType = file.ContentType;
            d.FallbackUri = new Uri("https://www.google.com/search?q=Abrir+archivo+" + file.FileType, UriKind.Absolute);
            d.TreatAsUntrusted = true;
            d.UI.PreferredPlacement = Placement.Above;
            await Windows.System.Launcher.LaunchFileAsync(file, d);
        }
         
        #endregion
    }
}
