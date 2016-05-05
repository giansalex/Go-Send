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
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using GoSend.Common;
using IMM.Manager;
using IMM.DB.Model;
using Windows.UI.Core;
using IMM.Model;
using IMM.DB.Helper;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GoSend.Views
{
    class CollectionContactos<T> : ObservableCollection<T>, INotifyCollectionChanged
    {
        event NotifyCollectionChangedEventHandler CollectionChanged;

        public void Add(T item)
        {
            base.Add(item);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            }
        }
        public void Insert(int indice, T value)
        {
            base.Insert(indice,value);
            if (CollectionChanged != null)
            {
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add));
            }
        }
        public CollectionContactos(IEnumerable<T> collection)
            : base(collection)
        {
        }
        public CollectionContactos()
        {
        }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListContactos : Page , IShowMessage 
    {
        #region Variables
        private NavigationHelper navigationHelper;
        CollectionContactos<ContactoView> listc;
        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        #endregion
        public ListContactos()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            //App.mgo.ObtEstados("*"); // Obtiene Estados
            LoadContactos();
            App.mgo.setShowManager(this);
        }
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

        private void LoadContactos()
        {
            var db = new DatabaseHelperClass();
           
            var list = db.ReadContactos();
            listc = new CollectionContactos<ContactoView>();
            foreach (var c in list)
            {
                var last = db.LastMessageOfContact(c.phone);
                if (last != null)
                {
                    var ct = new ContactoView()
                    {
                        Id = c.id,
                        Nombre = c.alias,
                        Phone = c.phone,
                        LastMsg = last,
                    };
                    listc.Add(ct);
                }
            }
            listc = new CollectionContactos<ContactoView>(listc.OrderByDescending(i => DateTime.Parse(i.LastMsg.fecha)));
            ListCont.ItemsSource = listc;
        }
        public async void ShowMessages(List<Message> mensajes, List<Contacto> newContactos)
        {
            foreach(var s in mensajes){
                if (listc.Any(i => i.Phone == s.phone))
                {
                    var item = listc.Single(x => x.Phone == s.phone);
                     await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                     {
                        item.LastMsg = s;
                        listc.Move(listc.IndexOf(item), 0);
                     });
                     
                }
                else
                {
                    var c = new DatabaseHelperClass().ReadContacto(s.phone);
                    var ct = new ContactoView()
                    {
                        Id = c.id,
                        Nombre = c.alias,
                        Phone = c.phone,
                        LastMsg = s,
                    };
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            listc.Insert(0,ct);      
                        });
                }
             }
        }

        #region Events XAML
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as ContactoView;
            switch((sender as MenuFlyoutItem).Tag.ToString()){
                case "1":
                   //Relatar
                    break;
                case "2"://Info
                    break;
                case "3"://Eliminar
                    //Comprobacion
                    new DatabaseHelperClass().DeleteContacto(item.Phone);
                    break;
            }
        }

        private void RightClick_Item(object sender, RightTappedRoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private async void Click_Item(object sender, TappedRoutedEventArgs e)
        {
            var item = (sender as Grid).DataContext as ContactoView;
            await App.mgo.ObtEstados(item.Phone); // Obtiene el estado de los sms que se envio a este usuario
            var c = new DatabaseHelperClass().ReadContacto(item.Id);
            this.Frame.Navigate(typeof(ChatGo), c);
        }

        #endregion
    }
}
