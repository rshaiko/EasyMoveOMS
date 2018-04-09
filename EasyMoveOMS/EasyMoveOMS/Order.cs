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
        public TimeSpan timeTruckFrom { get; set; }
        public TimeSpan timeTruckTo { get; set; }
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
        public List<Payment> orderPayments { get; set; }

        public enum OrderStatus {Scheduled, Suspended, Done, Cancelled};

        public Order()
        {
            id = 0;
        }

    }

    public class DayScheduleItem
    {
        public DayScheduleItem(long orderId, string truckName, TimeSpan timeTruckFrom, TimeSpan timeTruckTo, int workers)
        {
            this.orderId = orderId;
            this.truckName = truckName;
            this.timeTruckFrom = timeTruckFrom;
            this.timeTruckTo = timeTruckTo;
            this.workers = workers;
        }

        public long orderId { get; set; }
        public String truckName { get; set; }
        public TimeSpan timeTruckFrom { get; set; }
        public TimeSpan timeTruckTo { get; set; }
        public int workers { get; set; }
        public String start
        {
            get
            {
                return string.Format("{0:H:mm}", timeTruckFrom);
            }
        }
        public String finish
        {
            get
            {
                return string.Format("{0:H:mm}", timeTruckTo);
            }
        }
    }

    public class Payment
    {
        public long id { get; set; }
        public long orderId { get; set; }
        public PayMethod method { get; set; }
        public DateTime paymentDate { get; set; }
        public decimal amount { get; set; }
        public String notes { get; set; }
        public String formattedMethod
        {
            get
            {
                switch (method)
                {
                    case PayMethod.Cash:
                        return "Cash";
                    case PayMethod.Check:
                        return "Check";
                    case PayMethod.Credit:
                        return "Credit card";
                    case PayMethod.Debit:
                        return "Debit card";
                    case PayMethod.Other:
                        return "Other";
                    default:
                        return "Unknown";
                }
                
            }
        }
        public String formattedDate
        {
            get
            {
                return paymentDate.ToString("yyyy-MM-dd");
            }
        }


        public enum PayMethod { Cash=0, Check=1, Credit=2, Debit=3, Other=4 }

        public Payment()
        {
            id = 0;
        }
        
        public Payment(long id, long orderId, PayMethod method, DateTime paymentDate, decimal amount, String notes)
        {
            this.id = id;
            this.orderId = orderId;
            this.method = method;
            this.paymentDate = paymentDate;
            this.amount = amount;
            this.notes = notes;
        }

        public override String ToString()
        {
            string liPayItem = String.Format("{0}: ${1} by {2})", paymentDate, amount, method);
            return liPayItem;
        }
    }


    public class Truck
    {
        public long id { get; set; }
        public string name { get; set; }
        public string make { get; set; }
        public string model { get; set; }
        public string year { get; set; }

        public Truck() { }

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
