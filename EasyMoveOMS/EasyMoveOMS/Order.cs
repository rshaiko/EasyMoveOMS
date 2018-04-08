using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Order
    {
        public long id { get; set; }
        public DateTime moveDate { get; set; }
        public TimeSpan moveTime { get; set; }
        public long truckId { get; set; }
        public int workers { get; set; }
        
        public decimal pricePerHour { get; set; }
        public TimeSpan minTime { get; set; }
        public TimeSpan maxTime { get; set; }
        
        public decimal deposit { get; set; }
        public TimeSpan workTime { get; set; }
        public TimeSpan travelTime { get; set; }
        public TimeSpan arriveTimeFrom { get; set; }
        public TimeSpan arriveTimeTo { get; set; }
        public int boxes { get; set; }
        public int beds { get; set; }
        public int sofas { get; set; }
        public int frigos { get; set; }
        public int wds { get; set; }
        public int desks { get; set; }
        public int tables { get; set; }
        public int chairs { get; set; }
        public int other { get; set; }
        public bool oversized { get; set; }
        public bool overweight { get; set; }
        public bool fragile { get; set; }
        public bool expensive { get; set; }
        public String details { get; set; }
        public bool isPaid { get; set; }
        public OrderStatus orderStatus { get; set; }
        public DateTime contactOnDate { get; set; }
        public TimeSpan doneStartTime { get; set; }
        public TimeSpan doneEndTime { get; set; }
        public TimeSpan doneBreaksTime { get; set; }
        public TimeSpan doneTotalTime { get; set; }
        public bool useIntAddress { get; set; }

        public List<Address> orderAddresses { get; set; } //= new List<Address>();
        public Client orderClient { get; set; }// = new Client();
        public Truck orderTruck { get; set; }

        public enum OrderStatus {Scheduled, Suspended, Done, Cancelled};




        public Order()
        {
            id = 0;
        }

        //public Order ( long id, DateTime moveDate, TimeSpan moveTime, long truckId, int workers,
        //    decimal pricePerHour, TimeSpan minTime, TimeSpan maxTime,  decimal deposit, TimeSpan travelTime, TimeSpan arriveTimeFrom, TimeSpan arriveTimeTo, int boxes, int beds, int sofas, int frigos, int wds, int desks, int tables,
        //    int chairs, int other, bool oversized, bool overweight, bool fragile, bool expensive, String details, bool isPaid, OrderStatus orderStatus,
        //    DateTime contactOnDate, TimeSpan doneStartTime, TimeSpan doneEndTime, TimeSpan doneBreaksTime, TimeSpan doneTotalTime)
        //{

        //}

        
    }
}
