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
using GoSend.Common;
using IMM.Manager;
using IMM.DB.Model;
using Windows.UI.Core;
using IMM.Model;
using IMM.DB.Helper;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GoSend.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListaDeContactos : Page , IShowMessage 
    {
        #region Variables
        private NavigationHelper navigationHelper;
        List<ContactoView> listC;
        /// <summary>
        /// NavigationHelper is used on each page to aid in navigation and 
        /// process lifetime management
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }
        #endregion
        public ListaDeContactos()
        {
            this.InitializeComponent();
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += navigationHelper_LoadState;
            this.navigationHelper.SaveState += navigationHelper_SaveState;
        }
        private void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {     
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
            listC = new List<ContactoView>(list.Count);
            foreach(var c in list){
                var last = db.LastMessageOfContact(c.phone);
                var ct = new ContactoView()
                {
                    Id = c.id,
                    Nombre = c.alias,
                    Phone = c.phone,
                    Perfil = c.perfil
                };
                listC.Add(ct);
                ListCont.Items.Add(ct);
            }
        }
        public void ShowMessages(List<Message> mensajes, List<Contacto> newContactos)
        {
            if (newContactos != null)
            {
                foreach (var c in newContactos)
                {
                    var ct = new ContactoView()
                    {
                        Id = c.id,
                        Nombre = c.alias,
                        Phone = c.phone,
                        Perfil = c.perfil
                    };

                    listC.Add(ct);
                    ListCont.Items.Add(ct);
                }
            }
            foreach (var s in mensajes)
            {
                var name = new DatabaseHelperClass().ReadContacto(s.phone);
                new IMM.Notify.ToastGenerator().ShowToast(name.alias, this.getContent(s));
            }
        }
        private string getContent(Message msg)
        {
            var r = ManagerGo.getTipo(msg.tipo);
            if (r == TipoMessage.TEXTO) return msg.contenido;
            else return r.ToString();
        }
        #region Events XAML
        private void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as MenuFlyoutItem).DataContext as ContactoView;
            switch((sender as MenuFlyoutItem).Tag.ToString()){
                case "1":
                    break;
                case "2"://Info
                    break;
                case "3"://Eliminar
                    //Comprobacion
                    new DatabaseHelperClass().DeleteContacto(item.Phone);
                    listC.Remove(item);
                    ListCont.Items.Remove(item);
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
            await App.mgo.ObtEstados(item.Phone);
            var c = new DatabaseHelperClass().ReadContacto(item.Id);
            this.Frame.Navigate(typeof(ChatGo), c);
        }
        private async void NewContacto_Click(object sender, RoutedEventArgs e)
        {
            if (NumberPhone.Text.Trim().Equals("")) return;
            var c = await App.mgo.InsertContacto(NumberPhone.Text);
            NumberPhone.Text = "";
            if (c == null) return;
            var ct = new ContactoView()
            {
                Id = c.id,
                Nombre = c.alias,
                Phone = c.phone,
                Perfil = c.perfil
            };

            listC.Add(ct);
            ListCont.Items.Add(ct);
        }
        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Flyout.ShowAttachedFlyout((FrameworkElement)sender);
        }
        private void NumberPhone_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (!char.IsDigit(Convert.ToChar(e.Key)))
                e.Handled = true;
        }
        #endregion
    }
}
