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
using System.Net.Http;
using System.Threading.Tasks;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace GoSend.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Registro : Page
    {
        public Registro()
        {
            this.InitializeComponent();
        }

        private async void buttonRegister_Click(object sender, RoutedEventArgs e)
        {
            LoadRing.IsActive = true;
            txtError.Text = "";
            if (ValidarCampos()) {
                var res = await RegistrarContacto();
                if (res)
                {
                    App.SaveValueTag("idphone",txtPhone.Text);
                    App.mgo = new IMM.Manager.ManagerGo(txtPhone.Text);
                    LoadRing.IsActive = false;
                    Frame.Navigate(typeof(Views.MainPage));
                }
                else
                    txtError.Text = "No se pudo establecer la conexion";
            }
            else
            {
                txtError.Text = "Algun Campo Invalido";
            }
            LoadRing.IsActive = false;
        }
        private bool ValidarCampos()
        {
            
            return true;
        }
        private async Task<bool> RegistrarContacto() {
            HttpClient cliente = new HttpClient() {
                BaseAddress = new Uri(constants.ServerUri)
            };
            try
            {
                HttpContent content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("f", "1"),
                new KeyValuePair<string, string>("nombre",  txtName.Text),
                new KeyValuePair<string,string>("apellido", txtApellido.Text),
                new KeyValuePair<string,string>("email",email.Text),
                new KeyValuePair<string,string>("clave",password.Password),
                new KeyValuePair<string, string>("phone",txtPhone.Text),
                new KeyValuePair<string, string>("sexo",(radioFem.IsChecked==true)?("0") :("1")),
                new KeyValuePair<string, string>("nacimiento",DateNac.Date.Date.ToString("yyyy-MM-dd"))
            });
                var result = await cliente.PostAsync("helper/ctrcont.php", content);
                if (result.IsSuccessStatusCode)
                {
                    string strHtml = result.Content.ReadAsStringAsync().Result;
                    if (strHtml.Trim().Equals("1")) return true;
                }
                return false;
            }
            catch
            {
            }
            return false;
        }

        private void Phone_Click(object sender, KeyRoutedEventArgs e)
        {
            if (!char.IsDigit(Convert.ToChar(e.Key)) && (e.Key != Windows.System.VirtualKey.Tab) )
                e.Handled = true;
        }
    }
}
