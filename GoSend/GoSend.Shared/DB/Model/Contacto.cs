using System;
using System.Collections.Generic;
using System.Text;

namespace IMM.DB.Model
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

        [SQLite.MaxLength(2)]
        public string sexo { set; get; }
        public Contacto() { }
    }
}
