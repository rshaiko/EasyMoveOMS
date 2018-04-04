using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public class Address
    {
        long id;
        long orderId;
        String addrLine;
        String city;
        String zip;
        String province;
        int floor;
        bool elevator;
        bool stairs;
        bool isBilling;
        AddrType adType;
        String notes;

        public enum AddrType { Actual, Destination, Intermediate };


        public Address()
        {

        }

        //full constructor
        public Address(long id, long orderId, String addrLine, String city, String zip, String province, int floor, bool elevator,
            bool stairs, bool isBilling, AddrType addrType, String notes)
        {
            Id = id;
            OrderId = orderId;
            AddrLine = addrLine;
            City = city;
            Zip = zip;
            Province = province;
            Floor = floor;
            Elevator = elevator;
            Stairs = stairs;
            IsBilling = isBilling;
            AdType = adType;
            Notes = notes;
        }

        //constructor without id's
        public Address(String addrLine, String city, String zip, String province, int floor, bool elevator,
            bool stairs, bool isBilling, AddrType adType, String notes)
        {
            AddrLine = addrLine;
            City = city;
            Zip = zip;
            Province = province;
            Floor = floor;
            Elevator = elevator;
            Stairs = stairs;
            IsBilling = isBilling;
            AdType = adType;
            Notes = notes;
        }
        public long Id { get => id; set => id = value; }
        public long OrderId { get => orderId; set => orderId = value; }
        public string AddrLine { get => addrLine; set => addrLine = value; }
        public string City { get => city; set => city = value; }
        public string Zip { get => zip; set => zip = value; }
        public string Province { get => province; set => province = value; }
        public int Floor { get => floor; set => floor = value; }
        public bool Elevator { get => elevator; set => elevator = value; }
        public bool Stairs { get => stairs; set => stairs = value; }
        public bool IsBilling { get => isBilling; set => isBilling = value; }
        public AddrType AdType { get => adType; set => adType = value; }
        public string Notes { get => notes; set => notes = value; }
    }
}
