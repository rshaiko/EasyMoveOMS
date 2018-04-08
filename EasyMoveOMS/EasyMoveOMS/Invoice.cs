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
        public long clientAddrId { get; set; }
        public bool noTax { get; set; }

        public Invoice() { }
        public Invoice(long id, long orderId, DateTime invoiceDate, long clientAddrId, bool noTax) { }
    }
}
