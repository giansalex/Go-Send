using System.Collections.Generic;
using IMM.DB.Model;

namespace IMM.Manager
{
    /// <summary>
    /// Inteface utilizada para comunicar a ManagerGo con The Current Page
    /// </summary>
    public interface IShowMessage
    {
        /// <summary>
        /// Funcion que se ejecuta cuando se reciben mensajes, y se envian a la pantalla
        /// </summary>
        /// <param name="mensajes">Lista de mensajes Recibidos</param>
        /// <param name="newContactos">Lista de nuevos contactos, Null si no hay ninguno nuevo</param>
        void ShowMessages(List<Message> mensajes, List<Contacto> newContactos);
    }
}
