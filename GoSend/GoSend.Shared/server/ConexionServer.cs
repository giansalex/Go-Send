using System;
using System.Collections.Generic;
using System.Text;

using System.Threading.Tasks;
//using System.Net.Http;
using Windows.Data.Json;
using IMM.Server.Model;
using Windows.Web.Http;

namespace IMM.Server
{
    /*Links Para Mensaje
     * Funciones: 
     * 1 = EnviarMensaje
     * 2 = Leer Mensajes
     * 
      Links para Contactos
     * 1 = Registrar
     * 2 = GetInfo
     * 3 = Actualiza Estado
     * 4 = Obtiene Estado
     *
     */
    enum ServerMsg { NEW_MSG=1, GET_MESSAGE=2}

    enum ServerContact { REGISTRAR = 1, GET_INFO = 2 }
    public class ConexionServer
    {
        private HttpClient clientIMM;
        private string BASE_URL;
        private Uri uriMsg;
        private Uri uriContacto;
        public String ID_Propio
        {
            set;
            get;
        }
        #region Event Error
        public delegate void AddOnExceptionOccuredDelegate(object sender, Exception ex);
        public event AddOnExceptionOccuredDelegate ExceptionOccured;
        private void OnExceptionOccuredEvent(object sender, Exception ex)
        {
            if (ExceptionOccured != null)
                ExceptionOccured(sender, ex);
        }
        #endregion
        /// s<summary>
        /// Inicia un Servicio HttpClient
        /// </summary>
        /// <param name="Server"> Url base del servidor.</param>
        public ConexionServer()
        {
            this.uriContacto = new Uri(GoSend.constants.ServerUri + "helper/ctrcont.php",UriKind.Absolute);
            this.uriMsg = new Uri(GoSend.constants.ServerUri + "helper/ctrmsg.php", UriKind.Absolute);
            ID_Propio = GoSend.App.GetValueTag("idphone");
        }

        #region Mensajes Server
        /// <summary>
        /// Envia un mensaje al Servidor web
        /// </summary>
        /// <param name="datos"> Elemento que contiene atributos del mensaje</param>
        public async Task<string> SendMessage(MessageServer datos)
        {
            try
            {
                IHttpContent content = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("f","1"),
                    new KeyValuePair<string, string>("i1",this.ID_Propio),
                    new KeyValuePair<string,string>("i2", datos.phone),
                    new KeyValuePair<string,string>("msg",datos.contenido),
                    new KeyValuePair<string,string>("tp",datos.tipo),
                });
                var result = await clientIMM.PostAsync(this.uriMsg, content);

                if (result.IsSuccessStatusCode)
                {
                    String strHtml = await result.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(strHtml)) return strHtml;
                }
            }
            catch (Exception) { }
            return "";
        }
        /// <summary>
        ///Extrae el mensaje nuevo
        /// </summary>
        public async Task<List<MessageServer>> GetMessages()
        {
            try
            {
                var content = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("f","2"),
                    new KeyValuePair<string, string>("i1", this.ID_Propio),  
                });
                var result = await clientIMM.PostAsync(this.uriMsg, content);
                if (result.IsSuccessStatusCode)
                {
                    String strHtml = await result.Content.ReadAsStringAsync();
                    if (!strHtml.Equals(""))
                    {                   
                        return ProcesarDataJson(strHtml);
                    }
                }
            }
            catch (Exception err) { OnExceptionOccuredEvent(this, err); }
            return null;
        }

        public async Task<bool> UpdateMessage(string id, string msg)
        {
            try
            {
                var cont = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("i",id),
                    new KeyValuePair<string, string>("m",msg),
                });
                var result = await clientIMM.PostAsync(new Uri(GoSend.constants.ServerUri + "helper/uSm.php", UriKind.Absolute), cont);

                if (result.IsSuccessStatusCode)
                {
                    String strHtml = await result.Content.ReadAsStringAsync();
                    if (strHtml.Trim().Equals("1")) return true;
                }
            }catch{}
            return false;
        }
        /// <summary>
        /// Procesa datos Codificados en Json
        /// </summary>
        /// <param name="JsonStr"> Strin que contiene el Json</param>
        private List<MessageServer> ProcesarDataJson(string JsonStr)
        {
            JsonArray res = JsonArray.Parse(JsonStr);
            List<MessageServer> ListRes = new List<MessageServer> { };
            
            foreach (var rows in res)
            {
                JsonObject row = rows.GetObject();

                ListRes.Add(new MessageServer()
                {
                    idServ = row["id"].GetString(),
                    phone = row["Origen"].GetString(),
                    contenido = row["Mensaje"].GetString(),
                    fecha = this.getTime(row["Fecha"].GetString()),
                    tipo = row["Tipo"].GetString(),
                }
                );

            }
            return ListRes;
        }
        private string getTime(string utc)
        {
            var d = DateTime.SpecifyKind(DateTime.Parse(utc), DateTimeKind.Utc);
            return d.ToLocalTime().ToString();
        }
        #endregion

        #region Contacto Server
        /// <summary>
        /// Extrae Datos de un contacto
        /// </summary>
        /// <param name="phone">telefono del contacto</param>
        /// <returns></returns>
        public async Task<IMM.DB.Model.Contacto> GetDatoContacto(string phone)
        {
             var content = new HttpFormUrlEncodedContent(new[]
             {    
                new KeyValuePair<string, string>("f", "2"),
                new KeyValuePair<string, string>("i1", phone),
             });
                var result = await clientIMM.PostAsync(this.uriContacto, content);
                if (result.IsSuccessStatusCode)
                {
                    String strHtml = await result.Content.ReadAsStringAsync();
                    strHtml = strHtml.Trim();
		            if(strHtml.Equals("0")) return null;
                    JsonArray res = JsonArray.Parse(strHtml);
                    DB.Model.Contacto contact = new DB.Model.Contacto();
                    
                    foreach (var rows in res)
                    {
                        JsonObject row = rows.GetObject();
                        contact.alias = row["alias"].GetString();
                        contact.sexo = row["sexo"].GetString();
                    }
                    contact.phone = phone;
                    return contact;
                }
            return null;
        }
        /// <summary>
        /// Envia el estado del Usuario
        /// </summary>
        /// <param name="estado">Nuevo estado</param>
        public async void SendStateUser(string estado)
        {
            try
            {
                    var content = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("f", "3"),
                    new KeyValuePair<string, string>("i1", this.ID_Propio),
                    new KeyValuePair<string, string>("state", estado),
                });
                    await clientIMM.PostAsync(this.uriContacto, content);
            }
            catch (Exception err)
            {
                OnExceptionOccuredEvent(this, err);
            }
        }

        /// <summary>
        /// Obtiene el estado de un Contacto
        /// </summary>
        /// <param name="_phone">numero celular del Contacto</param>
        /// <returns></returns>
        public async Task<string> GetEstado(string _phone)
        {
            var content = new HttpFormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("f", "4"),
                new KeyValuePair<string, string>("i1", _phone),
            });
            var result = await clientIMM.PostAsync(this.uriContacto, content);
            if (result.IsSuccessStatusCode)
            {
                String strHtml = await result.Content.ReadAsStringAsync();
                return strHtml;
            }
            return "";
        }
        #endregion

        #region States
        public async Task<string> SetStateMsg(string id, string phone)
        {
            try
            {
                var content = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("i",id),
                    new KeyValuePair<string, string>("o", phone),
                    new KeyValuePair<string, string>("d", this.ID_Propio)
                });
                var result = await clientIMM.PostAsync(new Uri(GoSend.constants.ServerUri + "helper/sSm.php"), content);

                if (result.IsSuccessStatusCode)
                {
                    String strHtml = await result.Content.ReadAsStringAsync();
                    if (strHtml.Trim().Equals("1")) return strHtml;
                }
            }
            catch (Exception) { }
            return "";
        }
        /// <summary>
        /// Elimina los mensajes que se corroboran que se vieron
        /// </summary>
        /// <param name="phone">phone del contact</param>
        /// <param name="ids">id en el server del message</param>
        public async void DeleteMsg(string phone, string ids)
        {
            try
            {
                var content = new HttpFormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("i",ids),
                    new KeyValuePair<string, string>("o", this.ID_Propio),
                    new KeyValuePair<string, string>("d", phone)
                });
                await clientIMM.PostAsync(new Uri(GoSend.constants.ServerUri + "helper/dSm.php", UriKind.Absolute), content);
            }
            catch (Exception er)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(er.Message);
#endif
            }
        }
        #endregion
    }
}
