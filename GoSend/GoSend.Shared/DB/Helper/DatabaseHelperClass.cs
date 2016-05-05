using SQLite;
using IMM.DB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Storage; // ApplicationData
using System.IO;       // Path

namespace IMM.DB.Helper
{
    public class DatabaseHelperClass
    {
        //SQLiteConnection dbConn;
        private string BD_PATH  = GoSend.App.DB_PATH;
        //Create Tabble 
        /* */
        #region Mensajes
        // Retrieve the specific contact from the database. 
        public List<Message> ExecuteMsg(string Query)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                var res = dbConn.Query<Message>(Query);
                return res;
            }
        }
        public Message ReadMessage(int idmsg)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                var existingMsg = dbConn.Query<Message>("SELECT * FROM Message where id=?",idmsg).FirstOrDefault();
                return existingMsg;
            }
        }
        public List<Message> ReadMessageOfContact(string phoneContact)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                
                //var rows = dbConn.ExecuteScalar<int>("SELECT COUNT(id) FROM Message WHERE phone='" + phoneContact + "'");
                //string cmdSQL;
                //if (rows > 10)
                //    cmdSQL = cmdSQL = "select * from Message where id_msg > " + ids[ids.Count - 10] + " AND (origen =\"" + phoneContact + "\" or destino =\"" + phoneContact + "\")";
                //else
                //    cmdSQL = "select * from Message where origen =\"" + phoneContact + "\" or destino =\"" + phoneContact + "\"";
                //var Msgs = dbConn.Query<Message>(cmdSQL);
                var Msgs = dbConn.Query<Message>("SELECT * FROM Message WHERE phone='" + phoneContact + "'");
                return Msgs;
            }
        }
        public Message LastMessageOfContact(string phone, string extra="")
        {
            try
            {
                int id;
                using (var dbConn = new SQLiteConnection(BD_PATH))
                {
                    id = dbConn.ExecuteScalar<int>("SELECT MAX(id) FROM Message WHERE phone='" + phone + "'" + extra);
                }
                return this.ReadMessage(id);
            }catch(Exception){}
            return null;
        }
        //Update existing conatct 
        public void UpdateMessage(int id,string field, string valor)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.Execute("UPDATE Message SET " + field +"=? WHERE id=?", valor, id);
            }
        }
        /// <summary>
        /// Guarda un nuevo mensaje
        /// </summary>
        /// <param name="newMsg">object of class Message</param>
        /// <returns>Retorna Indice en Sqlite</returns>
        public void Insert(Message newMsg)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                int res = 0;
                dbConn.RunInTransaction(() =>
                {
                    res = dbConn.Insert(newMsg);//Filas afectadas

                });
            }
        }

        //Delete specific contact 
        public void DeleteMessage(int Id)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.Execute("DELETE FROM Message WHERE id=?",Id);
            }
        }
        public void DeleteMsgOfContact(string phoneContact)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.Execute("DELETE FROM Message WHERE phone=?",phoneContact);
            }
        }
        #endregion

        #region Contactos
        public List<Contacto> ExecuteCont(String Query)
        {
            using (var dbConn = new SQLiteConnection(this.BD_PATH))
            {
                var existingcontact = dbConn.Query<Contacto>(Query);
                return existingcontact;
            }
        }
        public Contacto ReadContacto(int id)
        {
            using (var dbConn = new SQLiteConnection(this.BD_PATH))
            {
                var existingcontact = dbConn.Query<Contacto>("select * from Contacto where id=?",id).FirstOrDefault();

                return existingcontact;
            }
        }

        public Contacto ReadContacto(string phone)
        {
            using (var dbConn = new SQLiteConnection(this.BD_PATH))
            {
                var existingcontact = dbConn.Query<Contacto>("select * from Contacto where phone=?", phone).FirstOrDefault();

                return existingcontact;
            }
        }
        /// <summary>
        /// Inserta un nuevo contacto
        /// </summary>
        /// <param name="newContact">object of Contacto</param>
        public void Insert(Contacto newContact)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.RunInTransaction(() =>
                {
                    dbConn.Insert(newContact);
                });
            }
        }
        /// <summary>
        /// Comprueba si existe un contacto
        /// </summary>
        /// <param name="_phone">phone a corroborar</param>
        /// <returns>True si existe, false si no.</returns>
        public bool ExistContact(string _phone)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                var c = dbConn.ExecuteScalar<int>("SELECT COUNT(*) FROM Contacto WHERE phone=?",_phone);
                if (c != 0) return true;    
            }
            return false;
        }
        public ObservableCollection<Contacto> ReadContactos()
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                List<Contacto> myCollection = dbConn.Table<Contacto>().ToList<Contacto>();
                ObservableCollection<Contacto> ContactsList = new ObservableCollection<Contacto>(myCollection);
                return ContactsList;
            }
        }
        //public Contacto ReadContacto(string phone)
        //{
        //    using (var dbConn = new SQLiteConnection(BD_PATH))
        //    {
        //        var existingcontact = dbConn.Query<Contacto>("select * from Contacto where phone =\"" + phone + "\"").FirstOrDefault();               
        //        return existingcontact;
        //    }
        //}
        /// <summary>
        /// Actualiza Registro de un contacto
        /// </summary>
        /// <param name="id">id del contacto</param>
        /// <param name="campo">Campo a modificar</param>
        /// <param name="valor">Nuevo valor del campo</param>
        public void UpdateContact(int id,string campo, object valor)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.Execute("UPDATE Contacto SET " + campo + "='" + valor + "' WHERE id=?", id);
            }
        }

        /// <summary>
        /// Elimina un contacto y sus mensajes
        /// </summary>
        /// <param name="phone">phone del contacto a elmiminar</param>
        public void DeleteContacto(string phone)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                dbConn.Execute("DELETE FROM Contacto WHERE phone=?", phone);
                dbConn.Execute("DELETE FROM Message WHERE phone=?", phone);
            }
        }
        #endregion
        /*
        public string ExecuteScalar(string Query)
        {
            using (var dbConn = new SQLiteConnection(BD_PATH))
            {
                var existingMsg = dbConn.ExecuteScalar<string>(Query);

                return existingMsg;
            }
        }*/
    }
}
