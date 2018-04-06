using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Order
    {
        public long id;
        public DateTime moveDate;
        public TimeSpan moveTime;
        public long truckId;
        public int workers;
        
        public decimal pricePerHour;
        public TimeSpan minTime;
        public TimeSpan maxTime;
        
        public decimal deposit;
        public TimeSpan travelTime;
        public TimeSpan arriveTimeFrom;
        public TimeSpan arriveTimeTo;
        public int boxes;
        public int beds;
        public int sofas;
        public int frigos;
        public int wds;
        public int desks;
        public int tables;
        public int chairs;
        public int other;
        public bool oversized;
        public bool overweight;
        public bool fragile;
        public bool expensive;
        public String details;
        public bool isPaid;
        public OrderStatus orderStatus;
        public DateTime contactOnDate;
        public TimeSpan doneStartTime;
        public TimeSpan doneEndTime;
        public TimeSpan doneBreaksTime;
        public TimeSpan doneTotalTime;

        public List<Address> orderAddresses; //= new List<Address>();
        public Client orderClient;// = new Client();
        public Truck orderTruck;
        public enum OrderStatus {Scheduled, Suspended, Done, Cancelled};

        public Order()
        {

        }

        public Order ( long id, DateTime moveDate, TimeSpan moveTime, long truckId, int workers,
            decimal pricePerHour, TimeSpan minTime, TimeSpan maxTime,  decimal deposit, TimeSpan travelTime, TimeSpan arriveTimeFrom, TimeSpan arriveTimeTo, int boxes, int beds, int sofas, int frigos, int wds, int desks, int tables,
            int chairs, int other, bool oversized, bool overweight, bool fragile, bool expensive, String details, bool isPaid, OrderStatus orderStatus,
            DateTime contactOnDate, TimeSpan doneStartTime, TimeSpan doneEndTime, TimeSpan doneBreaksTime, TimeSpan doneTotalTime)
        {

        }

        
    }
}
