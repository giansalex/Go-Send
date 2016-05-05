using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading.Tasks;
using IMM.Server;
using IMM.Server.Model;
using IMM.DB.Helper;
using IMM.DB.Model;
using IMM.Server;
namespace IMM.Manager
{
    public enum TipoMessage { TEXTO = 1, IMAGEN, AUDIO, FILE}

    public class ManagerGo : IDisposable
    {
        private ThreadMessage th;
        private string idphone;
        private IShowMessage ShowManager;
        public ManagerGo(string phone)
        {
            this.idphone = phone;
            Initialize();
        }
        private void Initialize()
        {
            th = new ThreadMessage(this.idphone);
            th.MessageReceived += th_MessageReceived;
            th.ExceptionOccured += th_ExceptionOccured;
            th.StartListen();
        }
        void th_ExceptionOccured(object sender, Exception ex)
        {
            th.StopListen();
            System.Diagnostics.Debug.WriteLine("MangerGo:" +ex.Message);
        }

        public async Task<Contacto> InsertContacto(string phone)
        {
            DatabaseHelperClass d = new DatabaseHelperClass();
            if (!d.ExistContact(phone))
            {
                ConexionServer c = new ConexionServer();
                var r = await c.GetDatoContacto(phone);
		        if(r != null){
		           new DatabaseHelperClass().Insert(r);
                   return r;
		        }
            }
            return null;
        }
        private async void th_MessageReceived(object sender, MessageEventArgs e)
        {
            //Agregamos contactos inexistentes, solo diferentes
            List<Contacto> cs = new List<Contacto>();
            List<Message> msgs = new List<Message>(e.Messages.Count);

            var list = e.Messages.Distinct(new ComparerPhone());// Diferentes contactos
            foreach (var el in list)
            {
                var s = await InsertContacto(el.phone);
                if (s != null) cs.Add(s);
            }
            cs.TrimExcess();
            // Guarda Mensajes
            DatabaseHelperClass d = new DatabaseHelperClass();
            foreach (MessageServer msg in e.Messages)
            {
                var m = new Message();
                m.tipo = int.Parse(msg.tipo);
                m.origen = "2";
                m.phone = msg.phone;
                m.idserver = msg.idServ;
                m.estado = 1;
                m.fecha = msg.fecha;
                TipoMessage t = ManagerGo.getTipo(m.tipo);
                switch (t)
                {
                    case TipoMessage.TEXTO:
                        m.contenido = msg.contenido;
                        break;
                    case TipoMessage.IMAGEN:
                    case TipoMessage.AUDIO:
                    case TipoMessage.FILE:
                        var df = new DownLoadFiles(msg.contenido);
                        var r = await df.DownLoad(getTipo(int.Parse(msg.tipo)));
                        if (!string.IsNullOrWhiteSpace(r)) m.contenido = r;
                        else continue;
                        break;

                }
                d.Insert(m);
                msgs.Add(m);
            }
            // Enviar todos los mensajes Aqui
            //d.InsertAll(msgs);
            
            if (this.ShowManager != null)
            {
                if (cs.Count > 0) ShowManager.ShowMessages(msgs, cs);
                else ShowManager.ShowMessages(msgs, null);
            }
            //Muestra mensajes
        }
        public async Task<Message> EnviarMsg(string msg, TipoMessage tipo, string destino)
        {
            Message mdb = new Message()
            {
                phone = destino,
                origen = "1",
                tipo = Convert.ToInt16(tipo),
                fecha = DateTime.Now.ToString(),
            };
            MessageServer msv = new MessageServer()
            {
                phone = destino,
                tipo = mdb.tipo.ToString(), 
            };

            switch (tipo)
            {
                case TipoMessage.TEXTO: // Texto
                    mdb.contenido = msv.contenido = msg;
                    break;
                case TipoMessage.IMAGEN: // Imagen
                case TipoMessage.FILE:
                    //linkServer= Subir archivo y obtener su link
                    //linkLocal = guarda archivo en localstorgae;
                    //mdb.contenido = linklocal;
                    //msv.contenido = linkServer;
                    var imageFile = new ProcessFile();
                    mdb.contenido = await imageFile.CopyToLocal(msg);
                    var r = await imageFile.SendFile(mdb.contenido);
                    if (!string.IsNullOrWhiteSpace(r))
                        msv.contenido = r;
                    else
                    {
                        mdb.estado = -1;
                        new DatabaseHelperClass().Insert(mdb); 
                        return mdb;
                    }
                    break;
                case TipoMessage.AUDIO:
                    var audioFile = new ProcessFile();
                    mdb.contenido = msg;
                    var rs = await audioFile.SendFile(msg);
                    if (!string.IsNullOrWhiteSpace(rs))
                        msv.contenido = rs;
                    else
                    {
                        mdb.estado = -1;
                        new DatabaseHelperClass().Insert(mdb); 
                        return mdb;
                    }
                    break;
            }
            
            var res = await new ConexionServer().SendMessage(msv);
            if (res != "") mdb.idserver = res;
            mdb.estado = ((res.Equals("")) ? -1 : 0);
            new DatabaseHelperClass().Insert(mdb); 
            return mdb;  
        }
        public async Task<bool> UpdateMessageExist(Message msg, string content)
        {
            //Comprbar que aun no sea leido
            bool Upd = true;
            if (msg.estado != -1)
            {
                Upd = await new ConexionServer().UpdateMessage(msg.idserver, content);
            }
            if(Upd)
            {
                var db = new DatabaseHelperClass();
                db.UpdateMessage(msg.id, "contenido", content);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Envia el estado de Visto al server
        /// </summary>
        /// <param name="phone">phone del contacto</param>
        public async void SetState(string phone, Message ms = null)
        {
            Message m = (ms == null) ? (new DatabaseHelperClass().LastMessageOfContact(phone, " AND origen=2")) : (ms);
            if (m != null)
                if (m.estado != 2)
                {
                    var r = await new ConexionServer().SetStateMsg(m.idserver, phone);
                    if (r != "")
                        new DatabaseHelperClass().UpdateMessage(m.id, "estado", "2");
                }
        }
        public void setShowManager(IShowMessage obj)
        {
            // Se sobreescribe los anteriores, luego en lista 
            this.ShowManager = obj;
        }

        public async Task<bool> ObtEstados(string t)
        {
            // * : Todos , Phone: Uno en particular
            return await this.th.GetStates(t);
        }

        public static TipoMessage getTipo(int tipo)
        {
            TipoMessage res;
            switch(tipo){
                case 1: res = TipoMessage.TEXTO;
                    break;
                case 2: res = TipoMessage.IMAGEN;
                    break;
                case 3: res = TipoMessage.AUDIO;
                    break;
                default: res = TipoMessage.FILE;
                    break;
            }
            return res;
        }

        public void Dispose()
        {
            this.ShowManager = null;
            this.th.StopListen();
        }
    }

    /// <summary>
    /// Utilizado para obtener Phones distintos
    /// </summary>
    class ComparerPhone : IEqualityComparer<MessageServer>
    {

        public bool Equals(MessageServer x, MessageServer y)
        {
            return x.phone == y.phone;
        }

        public int GetHashCode(MessageServer obj)
        {
            return 1;
        }
    }
}
