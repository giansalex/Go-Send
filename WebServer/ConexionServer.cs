using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace GoSend.WebServer
{
    public class ConexionServer
    {
        public ConexionServer()
        {
            ResourceLoader d = new ResourceLoader("resources");
            d.GetString("BaseUri");
        }
    }
}
