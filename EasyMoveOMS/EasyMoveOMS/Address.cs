using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Address
    {
        public long id { get; set; }
        public long orderId { get; set; }
        public String addrLine { get; set; }
        public String city { get; set; }
        public String zip { get; set; }
        public String province { get; set; }
        public int floor { get; set; }
        public bool elevator { get; set; }
        public bool stairs { get; set; }
        public bool isBilling { get; set; }
        public AddrType addrType { get; set; }
        public String notes { get; set; }

        public enum AddrType { Actual, Destination, Intermediate };


        public Address()
        {
            id = 0;
            orderId = 0;
        }

        //full constructor
        public Address(long id, long orderId, String addrLine, String city, String zip, String province, int floor, bool elevator,
            bool stairs, bool isBilling, AddrType addrType, String notes)
        {
            this.id = id;
            this.orderId = orderId;
            this.addrLine = addrLine;
            this.city = city;
            this.zip = zip;
            this.province = province;
            this.floor = floor;
            this.elevator = elevator;
            this.stairs = stairs;
            this.isBilling = isBilling;
            this.addrType = addrType;
            this.notes = notes;
        }

        //constructor without id's
        public Address(String addrLine, String city, String zip, String province, int floor, bool elevator,
            bool stairs, bool isBilling, AddrType addrType, String notes)
        {
            id = 0;
            orderId = 0;
            this.addrLine = addrLine;
            this.city = city;
            this.zip = zip;
            this.province = province;
            this.floor = floor;
            this.elevator = elevator;
            this.stairs = stairs;
            this.isBilling = isBilling;
            this.addrType = addrType;
            this.notes = notes;
        }
        
    }
}
