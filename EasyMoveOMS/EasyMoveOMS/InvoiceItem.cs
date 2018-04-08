using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    class InvoiceItem
    {
        public long id { get; set; }
        public long invoiceId { get; set; }
        public string name { get; set; }
        public double price { get; set; }

        public InvoiceItem() { }
        public InvoiceItem(long id, long invoiceId, string name, double price) { }
    }
}
