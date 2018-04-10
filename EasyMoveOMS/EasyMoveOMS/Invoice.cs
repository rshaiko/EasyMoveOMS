using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
   public class Invoice
    {
        public long id { get; set; }
        public long orderId { get; set; }
        public DateTime invoiceDate { get; set; }
        //public long clientAddrId { get; set; }
        public bool noTax { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string zip { get; set; }
        public enum Province  {QC=0, ON=1,AB=2,BC=3,MB=4,NB=5,NS=6,PE=7,SK=8,NL=9,NT=10,NU=11,YT=12};
        public Province province { get; set; }


        public Invoice() { }
        public Invoice(long id, long orderId, DateTime invoiceDate, bool noTax,
            string addr, string city, string zip, Province pr) { }
    }
}
