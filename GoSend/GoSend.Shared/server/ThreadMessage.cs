using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IMM.Server.Model;
using System.Net.Http;

namespace IMM.Server
{
    public class ThreadMessage
    {
        #region Variables
        private Task work;
        private bool seguir;
        private KeyValuePair<string, string> CEnv;

        public delegate void AddOnMessageReceivedDelegate(object sender, MessageEventArgs e);
        public event AddOnMessageReceivedDelegate MessageReceived;
        private void OnMessageReceivedEvent(object sender, List<MessageServer> messages)
        {
            if (MessageReceived != null)
                MessageReceived(sender, new MessageEventArgs() { Messages = messages});
        }
        public delegate void AddOnExceptionOccuredDelegate(object sender, Exception ex);
        public event AddOnExceptionOccuredDelegate ExceptionOccured;
        private void OnExceptionOccuredEvent(object sender, Exception ex)
        {
            if (ExceptionOccured != null)
                ExceptionOccured(sender, ex);
        }
        #endregion
        public ThreadMessage(string phone)
        {
            CEnv = new KeyValuePair<string, string>("i1", phone);
        }
        public async void StartListen(){
            this.seguir = true;
            if (this.work != null) return;
            this.work = new Task(new Action(ListenForMessage));
            this.work.Start();
            await work.AsAsyncAction(); // corre de modo asincrono

            new Task(new Action(SendMsgNo)).Start();
        }
        private async void SendMsgNo()
        {
            var db = new IMM.DB.Helper.DatabaseHelperClass();
            var el = db.ExecuteMsg("SELECT * FROM Message WHERE estado=-1");
            var ps = el.Distinct(new CompPhone());
            foreach(var p in ps){
                await this.GetStates(p.phone);
            }
            var msText = el.FindAll(m => m.tipo == 1);
            foreach (var ms in msText)
            {
                var msg = new MessageServer()
                {
                    phone = ms.phone,
                    tipo = ms.tipo.ToString(),
                    contenido = ms.contenido,
                };
                var serv = await new ConexionServer().SendMessage(msg);
                if (serv != "")
                {
                    db.UpdateMessage(ms.id, "idserver", serv);
                    db.UpdateMessage(ms.id, "estado", "0");
                }
            }
        }
        public async Task<bool> GetStates(string phone)
        {
            var ms = new IMM.DB.Helper.DatabaseHelperClass().LastMessageOfContact(phone, " AND origen=1 AND estado in (0,1)");
            if (ms != null)
            {
                var h = new HttpClient() { BaseAddress = new Uri(GoSend.constants.ServerUri) };
                try
                {
                    var result = await h.GetAsync("helper/gSm.php?i=" + ms.idserver);
                    if (result.IsSuccessStatusCode)
                    {
                        String strHtml = result.Content.ReadAsStringAsync().Result;
                        if (strHtml.Trim() != "")
                        {
                            int st = int.Parse(strHtml);
                            using (SQLite.SQLiteConnection c = new SQLite.SQLiteConnection(GoSend.App.DB_PATH))
                            {
                                if (ms.estado != st)
                                {
                                   c.Execute("UPDATE Message SET estado=? WHERE phone=? AND origen=1 AND estado<? AND id<=?",st, phone,st, ms.id);
                                   if (st == 2) new ConexionServer().DeleteMsg(phone, ms.idserver);
                                 }
                            }
                        }
                    }
                }
                catch { }
            }
            return true;

        }
        private async void ListenForMessage()
        {
            HttpClient h = new HttpClient() { BaseAddress = new Uri(GoSend.constants.ServerUri)};
            while (this.seguir)
            {
                try
                {
                    var content = new FormUrlEncodedContent(new[]
                    {
                        CEnv
                     });
                    var result = await h.PostAsync("helper/TestMsj.php", content);
                    if (result.IsSuccessStatusCode)
                    {
                        String strHtml = result.Content.ReadAsStringAsync().Result;

                        if (!strHtml.Trim().Equals("0"))
                        {
                            ConexionServer s = new ConexionServer();
                            var ms = await s.GetMessages();
                            if (ms != null) this.OnMessageReceivedEvent(this, ms);
                        }
                        await Task.Delay(100);//Pause
                    }
                }
                catch (Exception er)
                {
                    this.StopListen();
                    OnExceptionOccuredEvent(this, er);
                }
            }
        }

        public void StopListen()
        {
            this.seguir = false;
            if (this.work == null) return;
            this.work = null;
        }

    }

    public class MessageEventArgs : EventArgs
    {
        public List<MessageServer> Messages { set; get; }
    }
    /// <summary>
    /// Se utiiza para obtener solo phones distintos en List<Message>
    /// </summary>
    class CompPhone : IEqualityComparer<DB.Model.Message>
    {

        public bool Equals(DB.Model.Message x, DB.Model.Message y)
        {
            return x.phone == y.phone;
        }

        public int GetHashCode(DB.Model.Message obj)
        {
            return 1;
        }
    }
}





