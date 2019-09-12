using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoSend.DB.SQLite;

namespace GoSend.DB.BL
{
    public class MensajeBL
    {
        /// <summary>
        /// Añade un mensaje
        /// </summary>
        /// <param name="message">New Message</param>
        public void addMessage(Mensaje message)
        {
            using (var dbContext = new GoSendEntities())
            {
                dbContext.AddObject(message);
            }
        }

        public void deleteMessage(int idMessage)
        {
            using (var dbContext = new GoSendEntities())
            {
                var objEntity = (from n in dbContext.mensaje
                                 where n.id == idMessage
                                 select n).FirstOrDefault(); 
                dbContext.RemoveObject(objEntity);
            }
        }

        /// <summary>
        /// Obtiene mensaje por su id
        /// </summary>
        /// <param name="idMessage">Id del mensaje</param>
        /// <returns></returns>
        public Mensaje getMessage(int idMessage)
        {
            using (var dbContext = new GoSendEntities())
            {
                var msg = (from n in dbContext.mensaje
                           where n.id == idMessage
                           select n).FirstOrDefault();
                return msg;
            }
        }
    }
}
