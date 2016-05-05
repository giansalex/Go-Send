using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoSend.WebServer.Model
{
    public class Message
    {
        public string phone { set; get; }

        public string contenido { set; get; }

        public string tipo { set; get; } //tipo: 1=text,2=imagen,3=audio,4=file(<2mb)

        public string fecha { set; get; }

        public string idServ { set; get; }
    }
}
