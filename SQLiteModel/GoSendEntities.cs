using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace GoSend.DB.SQLite
{
    /// <summary>
    /// Entidad de SQLite Database
    /// </summary>
    public class GoSendEntities : IDisposable
    {
        private SQLiteConnection con;
        public SQLiteConnection Conection
        {
            get
            {
                return con;
            }
        }

        public GoSendEntities()
        {
            var conectionString = new ResourceLoader("Resources").GetString("PathDB");
            this.con = new SQLiteConnection(conectionString);
        }
        public IQueryable<Contacto> contacto
        {
            get {
                    var contactos = con.Table<Contacto>();
                    return contactos.AsQueryable();
            }
        }

        public IQueryable<Mensaje> mensaje
        {
            get
            {
                    var mensajes = con.Table<Mensaje>();
                    return mensajes.AsQueryable();
            }
        }
        public int AddObject(object obj)
        {
            return con.Insert(obj);
        }

        public int RemoveObject(object obj)
        {
            return con.Delete(obj);
        }
        public void Dispose()
        {
            this.con.Close();
        }
    }
}
