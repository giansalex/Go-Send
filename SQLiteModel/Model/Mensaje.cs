using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoSend.DB.SQLite
{
    public class Mensaje
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int id { set; get; }

        [SQLite.Indexed , SQLite.MaxLength(15)]
        public string phone { set; get; }

        //origen 1 = Me, 2 = You
        [SQLite.MaxLength(1)]
        public string origen { set; get; }

        [SQLite.MaxLength(30)]
        public string fecha { set; get; }

        [SQLite.MaxLength(2)]
        public int tipo { set; get; } 

        public string contenido { set; get; }

        /*
	    * estado : 
        * -1 = NO ENVIADO
	    * 0 = NO RECEPCIONADO
	    * 1 = RECEPCIONADO
	    * 2 = LEIDO - Leido
	    * 3 = CONCLUIDO (Eliminar)
        */
        [SQLite.MaxLength(2)]
        public int estado { set; get; }
        [SQLite.MaxLength(10)]
        public string idserver { set; get; }
        public Mensaje() { }
    }
}
