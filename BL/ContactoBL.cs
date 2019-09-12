using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoSend.DB.SQLite;
namespace GoSend.DB.BL
{
    public class ContactoBL
    {
        /// <summary>
        /// Añade un contacto a la lista
        /// </summary>
        /// <param name="contacto">Nuevo contacto</param>
        public void addContact(Contacto contacto)
        {
            using (var dbContext = new GoSendEntities())
            {
                dbContext.AddObject(contacto);                
            }
        }

        /// <summary>
        /// Obtiene una lista de todos los Contactos
        /// </summary>
        /// <returns></returns>
        public List<Contacto> getAllContacts()
        {
            using (var dbContext = new GoSendEntities())
            {
                return dbContext.contacto.ToList();
            }
        }

        /// <summary>
        /// Obtiene un contacto por su ID
        /// </summary>
        /// <param name="idContacto">Id del contacto</param>
        /// <returns></returns>
        public Contacto getContact(int idContacto)
        {
            using (var dbContext = new GoSendEntities())
            {
                var contact = (from c in dbContext.contacto
                               where c.id == idContacto
                                select c).FirstOrDefault();
                return contact;
            }
        }
    }
}
