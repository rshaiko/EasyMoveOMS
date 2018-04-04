using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    class SmallClasses
    {
        //Email address validation
        public static bool emailIsValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
    public class Truck
    {
        public long id { get; set; }
        public string name { get; set; }
        string make { get; set; }
        string model { get; set; }
        string year { get; set; }

        public Truck(long id, string name, string make, string model, string year)
        {
            this.id = id;
            this.name = name;
            this.make = make;
            this.model = model;
            this.year = year;
        }

        public override String ToString()
        {
            string cbbTruckItem = String.Format("{0} ({1} {2}, {3})", name, make, model, year);
            return cbbTruckItem;
        }

    }
}
