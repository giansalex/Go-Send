using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoSend.DB.SQLite;

namespace GoSend.DB.BL
{
    public class MensajeContactoBL
    {
        /// <summary>
        /// Obtiene lista de mensajes de un contacto
        /// </summary>
        /// <param name="phone">Phone del contacto</param>
        /// <returns></returns>
        public List<Mensaje> getMessages(string phone)
        {
            using (var dbContext = new GoSendEntities())
            {
                var messages = (from m in dbContext.mensaje
                                where m.phone.Equals(phone)
                                select m);
                return messages.ToList();
            }
        }

        /// <summary>
        /// Elimina todos los mensajes de un contacto
        /// </summary>
        /// <param name="phone">phone del Contacto</param>
        public void DeleteMessages(string phone)
        {
            using (var dbContext = new GoSendEntities())
            {
                var messages = (from m in dbContext.mensaje
                                where m.phone == phone
                                select m);
                foreach (var msg in messages)
                {
                    dbContext.RemoveObject(msg);
                }
            }
        }

        /// <summary>
        /// Eliminar un contacto y sus mensajes
        /// </summary>
        /// <param name="phone">Phone del Contacto</param>
        public void DeleteContacto(string phone)
        {
            using (var dbContext = new GoSendEntities())
            {
                var contacto = (from n in dbContext.contacto
                                where n.phone == phone
                                select n).FirstOrDefault();
                dbContext.RemoveObject(contacto);
                this.DeleteMessages(phone);
            }
        }
    }
}
