using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using IMM.DB.Model;

namespace IMM.Model
{
    public class ContactoView : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre;
        private string _phone;

        private Message _lastmsg;
        private string _perfil;
        public int Id
        {
            get { return _id; }
            set { this.SetProperty(ref this._id, value); } 
        }
        public string Nombre
        { 
            get { return _nombre; } 
            set { this.SetProperty(ref this._nombre, value); } 
        }
        public Message LastMsg
        {
            get { return _lastmsg; }
            set { this.SetProperty(ref this._lastmsg, value); }
        }
        public string Phone
        {
            get { return _phone; }
            set { this.SetProperty(ref this._phone, value); }
        }
        public string Perfil
        {
            get { return _perfil; }
            set { this.SetProperty(ref this._perfil, value); }
        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false; 
            storage = value;
            this.OnPropertyChaned(propertyName);
            return true;
        }
        private void OnPropertyChaned(string propertyName)
        { 
            var eventHandler = this.PropertyChanged; 
            if (eventHandler != null) 
                eventHandler(this, new PropertyChangedEventArgs(propertyName)); 
        }
    }
}
