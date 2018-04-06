using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Client
    {
        private long _id;
        private string _name;
        private string _email;
        private string _phoneH;
        private string _phoneW;

        public Client()
        {

        }

        public Client(long id, string name, string email, string phoneH, string phoneW)
        {
            Id = id;
            Name = name;
            Email = email;
            PhoneH = phoneH;
            PhoneW = phoneW;
        }

        public Client(string name, string email, string phoneH, string phoneW)
        {
            Name = name;
            Email = email;
            PhoneH = phoneH;
            PhoneW = phoneW;
        }

        public long Id { get => _id; set => _id = value; }
        public string Name { get => _name; set => _name = value; }
        public string Email { get => _email; set => _email = value; }
        public string PhoneH { get => _phoneH; set => _phoneH = value; }
        public string PhoneW { get => _phoneW; set => _phoneW = value; }
    }
}
