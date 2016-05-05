using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoSend.DB.SQLite
{
    public class Contacto
    {
        [SQLite.PrimaryKey, SQLite.AutoIncrement]
        public int id { get; set; }

        [SQLite.Indexed, SQLite.MaxLength(20)]
        public string phone { set; get; }

        public string alias { get; set; }

        public string imagen { get; set; }

        [SQLite.MaxLength(100)]
        public string perfil { get; set; }

        public byte sexo { set; get; }
    }
}
