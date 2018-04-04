using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Client
    {
        private int _id;
        private string _name;
        private string _email;
        private string _phone;
        private string _fax;

        public Client(int id, string name, string email, string phone, string fax)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            Fax = fax;
        }

        public int Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Email { get => _email; set => _email = value; }
        public string Phone { get => _phone; set => _phone = value; }
        public string Fax { get => _fax; set => _fax = value; }
    }
}
