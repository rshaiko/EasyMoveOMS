﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Client
    {
        public long id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string phoneH { get; set; }
        public string phoneW { get; set; }
        public String phones
        {
            get
            {
                String ps = "";
                if (phoneW.Length > 0) ps = ", " + phoneW;
                ps = phoneH + ps;
                return ps;
            }
        }

        public Client()
        {
            id = 0;
        }

        public Client(long id, string name, string email, string phoneH, string phoneW)
        {
            this.id = id;
            this.name = name;
            this.email = email;
            this.phoneH = phoneH;
            this.phoneW = phoneW;
        }

        public Client(string name, string email, string phoneH, string phoneW)
        {
            this.name = name;
            this.email = email;
            this.phoneH = phoneH;
            this.phoneW = phoneW;
        }

       
    }
}
