using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data;
using System.IO;
using System.Collections;

namespace EasyMoveOMS
{
    public class Database
    {
        public MySqlConnection conn;

        public Database()
        {
            //conn = new MySqlConnection("Server=den1.mysql5.gear.host; database=easymove; UID=easymove; password=Cz6iV4UR__0G;SslMode=none");
            //string conS = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
             string conS = Properties.Settings.Default.connStr;
            conn = new MySqlConnection(conS);
            conn.Open();
        }


        public List<Client> GetAllClients()
        {
            List<Client> result = new List<Client>();
            using (MySqlCommand command = new MySqlCommand("SELECT * FROM clients", conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["id"];
                    string name = (string)reader["name"];
                    string email = (string)reader["email"];
                    string phone = (string)reader["phone"];
                    string fax = (string)reader["fax"];
                    Client cl = new Client(id, name, email, phone, fax);
                    result.Add(cl);
                }
            }
            return result;
        }

        internal void loadOrderData(ref Order o)
        {
            Client orderClient;
            Address address;
            List<Address> orderAddresses = new List<Address>();
            int row = 1;
            String sql = "SELECT o.*, c.id AS cId, c.name, c.email, c.phoneHome, c.phoneWork, a.id as aId, a.isBilling, a.addrLine, a.addrType, a.city, a.province, a.zip, a.notes, a.floor, a.stairs, a.elevator " +
                "FROM orders AS o JOIN clients AS c ON o.clientId=c.id JOIN addresses as a on o.id=a.orderId WHERE o.id=" + o.id + ";";
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (row == 1)
                    {
                        o.moveDate = (DateTime)reader["moveDate"];
                        o.moveTime = (TimeSpan)reader["moveTime"];
                        o.clientId = Convert.ToInt32(reader["clientId"]);
                        o.truckId = Convert.ToInt32(reader["truckId"]);
                        o.workers = Convert.ToInt32(reader["workers"]);
                        o.pricePerHour = (decimal)reader["pricePerHour"];
                        o.minTime = (TimeSpan)reader["minTime"];
                        o.maxTime = (TimeSpan)reader["maxTime"];
                        o.deposit = (decimal)reader["deposit"];
                        o.workTime = (TimeSpan)reader["workTime"];
                        o.travelTime = (TimeSpan)reader["travelTime"];
                        o.arriveTimeFrom = (TimeSpan)reader["arriveTimeFrom"];
                        o.arriveTimeTo = (TimeSpan)reader["arriveTimeTo"];
                        o.boxes = Convert.ToInt32(reader["boxes"]);
                        o.beds = Convert.ToInt32(reader["beds"]);
                        o.sofas = Convert.ToInt32(reader["sofas"]);
                        o.frigos = Convert.ToInt32(reader["frigos"]);
                        o.wds = Convert.ToInt32(reader["wds"]);
                        o.desks = Convert.ToInt32(reader["desks"]);
                        o.tables = Convert.ToInt32(reader["tables"]);
                        o.chairs = Convert.ToInt32(reader["chairs"]);
                        o.other = Convert.ToInt32(reader["other"]);
                        o.oversized = (bool)reader["oversized"];
                        o.overweight = (bool)reader["overweight"];
                        o.fragile = (bool)reader["fragile"];
                        o.expensive = (bool)reader["expensive"];
                        o.details = (string)reader["details"];
                        o.isPaid = (bool)reader["isPaid"];
                        o.orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["orderStatus"] + "");
                        o.contactOnDate = (DateTime)reader["contactOnDate"];
                        o.doneStartTime = (TimeSpan)reader["doneStartTime"];
                        o.doneEndTime = (TimeSpan)reader["doneEndTime"];
                        o.doneBreaksTime = (TimeSpan)reader["doneBreaksTime"];
                        o.doneTotalTime = (TimeSpan)reader["doneTotalTime"];
                        o.useIntAddress = (bool)reader["useIntAddress"];
                        o.timeTruckFrom = (TimeSpan)reader["timeTruckFrom"];
                        o.timeTruckTo = (TimeSpan)reader["timeTruckTo"];

                        //Client
                        orderClient = new Client();
                        orderClient.id = o.clientId;
                        orderClient.name = (string)reader["name"];
                        orderClient.email = (string)reader["email"];
                        orderClient.phoneH = (string)reader["phoneHome"];
                        orderClient.phoneW = (string)reader["phoneWork"];
                        o.orderClient = orderClient;
                    }
                    //Addresses
                    address = new Address();
                    address.id = Convert.ToInt32(reader["aId"]);
                    address.isBilling = (bool)reader["isBilling"];
                    address.addrLine = (string)reader["addrLine"];
                    address.city = (string)reader["city"];
                    address.zip = (string)reader["zip"];
                    address.province = (string)reader["province"];
                    address.notes = (string)reader["notes"];
                    //address.addrType = (Address.AddrType)Enum.Parse(typeof(Address.AddrType), reader["addrType"] + "");
                    switch (row)
                    {
                        case 1:
                            address.addrType = Address.AddrType.Actual;
                            break;
                        case 2:
                            address.addrType = Address.AddrType.Destination;
                            break;
                        case 3:
                            address.addrType = Address.AddrType.Intermediate;
                            break;
                        default:
                            break;
                    }
                    address.floor = Convert.ToInt32(reader["floor"]);
                    address.stairs = (bool)reader["stairs"];
                    address.elevator = (bool)reader["elevator"];
                    orderAddresses.Add(address);
                    row++;
                }
                o.orderAddresses = orderAddresses;
            }
        }

        internal void reloadOrderListScheduled(ref List<ListOrderItem> orderList)
        {
            ListOrderItem li;
            List<ListOrderItem> loi = new List<ListOrderItem>();
            String sql = "SELECT o.id, o.dateCreated, o.moveDate, o.moveTime, o.isPaid, o.orderStatus, c.name, c.phoneHome, c.phoneWork, a.addrLine, a.city  " +
                "FROM easymove.orders AS o LEFT JOIN addresses AS a ON o.id = a.orderId LEFT JOIN clients AS c ON o.clientId=c.id  " +
                "WHERE addrType='Actual' AND orderStatus='Scheduled' ORDER BY o.moveDate, o.moveTime;";
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    li = new ListOrderItem();
                    li.id = (int)reader["id"];
                    li.name = (string)reader["name"];
                    li.moveDate = (DateTime)reader["moveDate"];
                    li.dateCreated = (DateTime)reader["dateCreated"];
                    li.moveTime = (TimeSpan)reader["moveTime"];
                    li.isPaid = (bool)reader["isPaid"];
                    li.orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["orderStatus"] + "");
                    li.phoneHome = (string)reader["phoneHome"];
                    li.phoneWork = (string)reader["phoneWork"];
                    li.addrLine = (string)reader["addrLine"];
                    li.city = (string)reader["city"];
                    loi.Add(li);
                }
            }
            if (loi.Count > 0) orderList = loi;
        }

        internal void reloadOrderList(ref List<ListOrderItem> orderList)
        {
            ListOrderItem li;
            List<ListOrderItem> loi = new List<ListOrderItem>();
            String sql = "SELECT o.id, o.dateCreated, o.moveDate, o.moveTime, o.isPaid, o.orderStatus, c.name, c.phoneHome, c.phoneWork, a.addrLine, a.city  " +
                "FROM easymove.orders AS o LEFT JOIN addresses AS a ON o.id = a.orderId LEFT JOIN clients AS c ON o.clientId=c.id  " +
                "WHERE addrType='Actual' ORDER BY o.moveDate, o.moveTime;";
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    li = new ListOrderItem();
                    li.id = (int)reader["id"];
                    li.name = (string)reader["name"];
                    li.moveDate = (DateTime)reader["moveDate"];
                    li.dateCreated = (DateTime)reader["dateCreated"];
                    li.moveTime = (TimeSpan)reader["moveTime"];
                    li.isPaid = (bool)reader["isPaid"];
                    li.orderStatus = (OrderStatus)Enum.Parse(typeof(OrderStatus), reader["orderStatus"] + "");
                    li.phoneHome = (string)reader["phoneHome"];
                    li.phoneWork = (string)reader["phoneWork"];
                    li.addrLine = (string)reader["addrLine"];
                    li.city = (string)reader["city"];
                    loi.Add(li);
                }
            }
            if (loi.Count > 0) orderList = loi;
        }

        public List<Truck> GetWorkingTrucks()
        {
            List<Truck> result = new List<Truck>();
            using (MySqlCommand command = new MySqlCommand("SELECT id, name, make, model, year FROM trucks WHERE inUse=1", conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int id = (int)reader["id"];
                    string name = (string)reader["name"];
                    string make = (string)reader["make"];
                    string model = (string)reader["model"];
                    string year = (string)reader["year"];
                    Truck t = new Truck(id, name, make, model, year);
                    result.Add(t);
                }
            }
            return result;
        }

        internal void updatePayment(Payment p)
        {
            String sql = "UPDATE payments SET orderId=@orderId, method=@method, paymentDate=@paymentDate, amount=@amount, notes=@notes WHERE id=@id;";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", p.id);
                cmd.Parameters.AddWithValue("@orderId", p.orderId);
                cmd.Parameters.AddWithValue("@method", p.method+"");
                cmd.Parameters.AddWithValue("@paymentDate", p.paymentDate);
                cmd.Parameters.AddWithValue("@amount", p.amount);
                cmd.Parameters.AddWithValue("@notes", p.notes);
                cmd.CommandType = CommandType.Text;
                cmd.ExecuteNonQuery();
            }
        }

        
        internal long addPayment(Payment p)
        {
            String sql = "INSERT INTO payments (orderId, method, paymentDate, amount, notes) VALUES (@orderId, @method, @paymentDate, @amount, @notes); " +
                "SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", p.orderId);
                cmd.Parameters.AddWithValue("@method", p.method+"");
                cmd.Parameters.AddWithValue("@paymentDate", p.paymentDate);
                cmd.Parameters.AddWithValue("@amount", p.amount);
                cmd.Parameters.AddWithValue("@notes", p.notes);

                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }

        internal List<Payment> getAllPayments(long oId)
        {
            String sql = "SELECT * FROM payments WHERE orderId=" + oId + ";";
            List<Payment> payments = new List<Payment>();
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long id = Convert.ToInt32(reader["id"]);
                    long orderId = Convert.ToInt32(reader["orderId"]);
                    Payment.PayMethod method = (Payment.PayMethod)Enum.Parse(typeof(Payment.PayMethod), reader["method"]+"");
                    DateTime paymentDate = (DateTime)reader["paymentDate"];
                    decimal amount = Convert.ToDecimal(reader["amount"]);
                    String notes = reader["notes"]+"";
                    Payment p = new Payment(id, orderId, method, paymentDate, amount, notes);
                    payments.Add(p);
                }
            }
            return payments;
        }

        internal void deletePayment(long id)
        {
            string sql = "DELETE FROM payments WHERE id=@Id";
            MySqlCommand cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@Id", MySqlDbType.Int16).Value = id;
            cmd.CommandType = CommandType.Text;
            cmd.ExecuteNonQuery();
        }

        internal List<DayScheduleItem> getDayScheduleData(DateTime dt)
        {
            List<DayScheduleItem> dsi = new List<DayScheduleItem>();
            String dd = dt.ToString("yyyy-MM-dd");
            String sql = "SELECT o.id, o.timeTruckFrom, o.timeTruckTo, o.workers, t.id AS truckId, t.name FROM orders AS o JOIN trucks AS t ON o.truckId = t.id WHERE o.moveDate = '"+ dd + "' ORDER BY t.name, o.timeTruckFrom;";
            Console.WriteLine(sql);
            using (MySqlCommand command = new MySqlCommand(sql, conn))
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    long orderId = Convert.ToInt32(reader["id"]);
                    long truckId = Convert.ToInt32(reader["truckId"]);
                    TimeSpan timeTruckFrom = (TimeSpan)(reader["timeTruckFrom"]);
                    TimeSpan timeTruckTo = (TimeSpan)(reader["timeTruckTo"]);
                    int workers = Convert.ToInt16(reader["workers"]);
                    String name = reader["name"] + "";
                    DayScheduleItem item = new DayScheduleItem(orderId, truckId, name, timeTruckFrom, timeTruckTo, workers);
                    dsi.Add(item);
                }
            }
            return dsi;
        }

        internal long saveNewClient(Client c)
        {
            String sql = "INSERT INTO clients (name, email, phoneHome, phoneWork) VALUES (@name, @email, @phoneH, @phoneW); " +
                "SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@name", c.name);
                cmd.Parameters.AddWithValue("@email", c.email);
                cmd.Parameters.AddWithValue("@phoneH", c.phoneH);
                cmd.Parameters.AddWithValue("@phoneW", c.phoneW);
                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }

        internal long saveNewOrder(Order o)
        {
            String sql = "INSERT INTO orders (moveDate, moveTime, clientId, truckId, workers, pricePerHour, " +
                "minTime, maxTime, deposit, workTime, travelTime, timeTruckFrom, timeTruckTo, arriveTimeFrom, arriveTimeTo, " +
                "boxes, beds, sofas, frigos, wds, desks, tables, chairs, other, oversized, overweight, fragile, expensive, details, " +
                "isPaid, orderStatus, contactOnDate, doneStartTime, doneEndTime, doneBreaksTime, doneTotalTime, useIntAddress) " +
                "VALUES " +
                "(@moveDate, @moveTime, @clientId, @truckId, @workers, @pricePerHour, " +
                "@minTime, @maxTime, @deposit, @workTime, @travelTime, @timeTruckFrom, @timeTruckTo, @arriveTimeFrom, @arriveTimeTo, " +
                "@boxes, @beds, @sofas, @frigos, @wds, @desks, @tables, @chairs, @other, @oversized, @overweight, @fragile, @expensive, @details, " +
                "@isPaid, @orderStatus, @contactOnDate, @doneStartTime, @doneEndTime, @doneBreaksTime, @doneTotalTime, @useIntAddress); " +
                "SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@moveDate", o.moveDate);
                cmd.Parameters.AddWithValue("@moveTime", o.moveTime);
                cmd.Parameters.AddWithValue("@clientId", o.orderClient.id);
                cmd.Parameters.AddWithValue("@truckId", o.orderTruck.id);
                cmd.Parameters.AddWithValue("@workers", o.workers);
                cmd.Parameters.AddWithValue("@pricePerHour", o.pricePerHour);
                cmd.Parameters.AddWithValue("@minTime", o.minTime);
                cmd.Parameters.AddWithValue("@maxTime", o.maxTime);
                cmd.Parameters.AddWithValue("@deposit", o.deposit);
                cmd.Parameters.AddWithValue("@workTime", o.workTime);
                cmd.Parameters.AddWithValue("@travelTime", o.travelTime);
                cmd.Parameters.AddWithValue("@timeTruckFrom", o.timeTruckFrom);
                cmd.Parameters.AddWithValue("@timeTruckTo", o.timeTruckTo);
                cmd.Parameters.AddWithValue("@arriveTimeFrom", o.arriveTimeFrom);
                cmd.Parameters.AddWithValue("@arriveTimeTo", o.arriveTimeTo);
                cmd.Parameters.AddWithValue("@boxes", o.boxes);
                cmd.Parameters.AddWithValue("@beds", o.beds);
                cmd.Parameters.AddWithValue("@sofas", o.sofas);
                cmd.Parameters.AddWithValue("@frigos", o.frigos);
                cmd.Parameters.AddWithValue("@wds", o.wds);
                cmd.Parameters.AddWithValue("@desks", o.desks);
                cmd.Parameters.AddWithValue("@tables", o.tables);
                cmd.Parameters.AddWithValue("@chairs", o.chairs);
                cmd.Parameters.AddWithValue("@other", o.other);
                cmd.Parameters.AddWithValue("@oversized", o.oversized);
                cmd.Parameters.AddWithValue("@overweight", o.overweight);
                cmd.Parameters.AddWithValue("@fragile", o.fragile);
                cmd.Parameters.AddWithValue("@expensive", o.expensive);
                cmd.Parameters.AddWithValue("@details", o.details);
                cmd.Parameters.AddWithValue("@isPaid", o.isPaid);
                cmd.Parameters.AddWithValue("@orderStatus", o.orderStatus + "");
                cmd.Parameters.AddWithValue("@contactOnDate", o.contactOnDate);
                cmd.Parameters.AddWithValue("@doneStartTime", o.doneStartTime);
                cmd.Parameters.AddWithValue("@doneEndTime", o.doneEndTime);
                cmd.Parameters.AddWithValue("@doneBreaksTime", o.doneBreaksTime);
                cmd.Parameters.AddWithValue("@doneTotalTime", o.doneTotalTime);
                cmd.Parameters.AddWithValue("@useIntAddress", o.useIntAddress);

                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }

        internal void updateOrder(Order o)
        {
            String sql = "UPDATE orders SET  moveDate=@moveDate, moveTime=@moveTime, clientId=@clientId, truckId=@truckId, workers=@workers, " +
                "pricePerHour=@pricePerHour, minTime=@minTime, maxTime=@maxTime, deposit=@deposit, workTime=@workTime, travelTime=@travelTime, " +
                "timeTruckFrom=@timeTruckFrom, timeTruckTo=@timeTruckTo, " +
                "arriveTimeFrom=@arriveTimeFrom, arriveTimeTo=@arriveTimeTo, boxes=@boxes, beds=@beds, sofas=@sofas, " +
                "frigos=@frigos, wds=@wds, desks=@desks, tables=@tables, chairs=@chairs, other=@other, oversized=@oversized, " +
                "overweight=@overweight, fragile=@fragile, expensive=@expensive, details=@details, isPaid=@isPaid, " +
                "orderStatus=@orderStatus, contactOnDate=@contactOnDate, doneStartTime=@doneStartTime, doneEndTime=@doneEndTime, " +
                "doneBreaksTime=@doneBreaksTime, doneTotalTime=@doneTotalTime, useIntAddress=@useIntAddress " +
                    "WHERE id=@id;";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@id", o.id);
                cmd.Parameters.AddWithValue("@moveDate", o.moveDate);
                cmd.Parameters.AddWithValue("@moveTime", o.moveTime);
                cmd.Parameters.AddWithValue("@clientId", o.clientId);
                cmd.Parameters.AddWithValue("@truckId", o.orderTruck.id);
                cmd.Parameters.AddWithValue("@workers", o.workers);
                cmd.Parameters.AddWithValue("@pricePerHour", o.pricePerHour);
                cmd.Parameters.AddWithValue("@minTime", o.minTime);
                cmd.Parameters.AddWithValue("@maxTime", o.maxTime);
                cmd.Parameters.AddWithValue("@deposit", o.deposit);
                cmd.Parameters.AddWithValue("@workTime", o.workTime);
                cmd.Parameters.AddWithValue("@travelTime", o.travelTime);
                cmd.Parameters.AddWithValue("@timeTruckFrom", o.timeTruckFrom);
                cmd.Parameters.AddWithValue("@timeTruckTo", o.timeTruckTo);
                cmd.Parameters.AddWithValue("@arriveTimeFrom", o.arriveTimeFrom);
                cmd.Parameters.AddWithValue("@arriveTimeTo", o.arriveTimeTo);
                cmd.Parameters.AddWithValue("@boxes", o.boxes);
                cmd.Parameters.AddWithValue("@beds", o.beds);
                cmd.Parameters.AddWithValue("@sofas", o.sofas);
                cmd.Parameters.AddWithValue("@frigos", o.frigos);
                cmd.Parameters.AddWithValue("@wds", o.wds);
                cmd.Parameters.AddWithValue("@desks", o.desks);
                cmd.Parameters.AddWithValue("@tables", o.tables);
                cmd.Parameters.AddWithValue("@chairs", o.chairs);
                cmd.Parameters.AddWithValue("@other", o.other);
                cmd.Parameters.AddWithValue("@oversized", o.oversized);
                cmd.Parameters.AddWithValue("@overweight", o.overweight);
                cmd.Parameters.AddWithValue("@fragile", o.fragile);
                cmd.Parameters.AddWithValue("@expensive", o.expensive);
                cmd.Parameters.AddWithValue("@details", o.details);
                cmd.Parameters.AddWithValue("@isPaid", o.isPaid);
                cmd.Parameters.AddWithValue("@orderStatus", o.orderStatus + "");
                cmd.Parameters.AddWithValue("@contactOnDate", o.contactOnDate);
                cmd.Parameters.AddWithValue("@doneStartTime", o.doneStartTime);
                cmd.Parameters.AddWithValue("@doneEndTime", o.doneEndTime);
                cmd.Parameters.AddWithValue("@doneBreaksTime", o.doneBreaksTime);
                cmd.Parameters.AddWithValue("@doneTotalTime", o.doneTotalTime);
                cmd.Parameters.AddWithValue("@useIntAddress", o.useIntAddress);

                cmd.ExecuteNonQuery();
            }
        }

        internal void updateAddresses(List<Address> orderAddresses)
        {
            String sql = "UPDATE addresses SET orderId=@orderId, addrLine=@addrLine, city=@city, zip=@zip, province=@province, " +
                "floor=@floor, elevator=@elevator, stairs=@stairs, isBilling=@isBilling, addrType=@addrType, notes=@notes " +
                    "WHERE id=@id;";

            MySqlTransaction myTrans;
            myTrans = conn.BeginTransaction();
            try
            {
                foreach (Address a in orderAddresses)
                {
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", a.id);
                        cmd.Parameters.AddWithValue("@orderId", a.orderId);
                        cmd.Parameters.AddWithValue("@addrLine", a.addrLine);
                        cmd.Parameters.AddWithValue("@city", a.city);
                        cmd.Parameters.AddWithValue("@zip", a.zip);
                        cmd.Parameters.AddWithValue("@province", a.province);
                        cmd.Parameters.AddWithValue("@floor", a.floor);
                        cmd.Parameters.AddWithValue("@elevator", a.elevator);
                        cmd.Parameters.AddWithValue("@stairs", a.stairs);
                        cmd.Parameters.AddWithValue("@isBilling", a.isBilling);
                        cmd.Parameters.AddWithValue("@addrType", a.addrType + "");
                        cmd.Parameters.AddWithValue("@notes", a.notes);

                        cmd.ExecuteNonQuery();
                    }
                }
                myTrans.Commit();

            }
            catch (Exception e)
            {
                myTrans.Rollback();
                throw new InvalidDataException(e.ToString());
            }
        }

        internal long insertAddresses(List<Address> orderAddresses)
        {
            //Creating DataTable to insert all three addresses by one query

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("orderId", typeof(long)));
            dt.Columns.Add(new DataColumn("addrLine", typeof(string)));
            dt.Columns.Add(new DataColumn("city", typeof(string)));
            dt.Columns.Add(new DataColumn("zip", typeof(string)));
            dt.Columns.Add(new DataColumn("province", typeof(string)));
            dt.Columns.Add(new DataColumn("floor", typeof(int)));
            dt.Columns.Add(new DataColumn("elevator", typeof(int)));
            dt.Columns.Add(new DataColumn("stairs", typeof(int)));
            dt.Columns.Add(new DataColumn("isBilling", typeof(int)));
            dt.Columns.Add(new DataColumn("addrType", typeof(String)));
            dt.Columns.Add(new DataColumn("notes", typeof(String)));

            foreach (Address a in orderAddresses)
                dt.Rows.Add(new string[] { a.orderId+"", a.addrLine, a.city, a.zip, a.province, a.floor+"", a.elevator ? "1":"0",
                    a.stairs ? "1": "0", a.isBilling? "1":"0", a.addrType+"", a.notes });

            String sql = "INSERT INTO addresses (orderId, addrLine, city, zip, province, floor, elevator, stairs, " +
                "isBilling, addrType, notes) VALUES ";
            int x = 0;
            String comma;
            foreach (DataRow row in dt.Rows)
            {
                comma = x == 0 ? "" : ",";
                x++;
                sql += String.Format(@"{0}('{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')",
                                          comma,
                                          row["orderId"],
                                          row["addrLine"],
                                          row["city"],
                                          row["zip"],
                                          row["province"],
                                          row["floor"],
                                          row["elevator"],
                                          row["stairs"],
                                          row["isBilling"],
                                          row["addrType"],
                                          row["notes"]
                                          );
            }
            sql = sql + "; SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }

        internal long AddInvoice(Invoice inv)
        {
            string sql = "INSERT INTO invoices (orderId, invoiceDate, noTax, addrLine, city, zip, province) " +
                "VALUES (@orderId, @invoiceDate, @noTax,@addrLine,@city,@zip,@province);" +
                "SELECT LAST_INSERT_ID();";

            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", inv.orderId);
                cmd.Parameters.AddWithValue("@invoiceDate", inv.invoiceDate);
                //cmd.Parameters.AddWithValue("@clientAddrId", inv.clientAddrId);
                cmd.Parameters.AddWithValue("@noTax", inv.noTax ? 1 : 0);
                cmd.Parameters.AddWithValue("@addrLine", inv.address);
                cmd.Parameters.AddWithValue("@city", inv.city);
                cmd.Parameters.AddWithValue("@zip", inv.zip);
                cmd.Parameters.AddWithValue("@province", inv.province+"");

                long id = Convert.ToInt32(cmd.ExecuteScalar());
                //int id = (int)cmd.ExecuteScalar();
                return id;
            }
        }

        internal long AddNewBillingAddress(Address a)
        {
            string sql = "INSERT INTO addresses (orderId, addrLine, city, zip, province, floor, elevator, stairs, isBilling, addrType, notes)" +
                " VALUES (@orderId, @addrLine, @city, @zip, @province, @floor, @elevator, @stairs, @isBilling, @addrType, @notes);" +
                   "SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", a.orderId);
                cmd.Parameters.AddWithValue("@addrLine", a.addrLine);
                cmd.Parameters.AddWithValue("@city", a.city);
                cmd.Parameters.AddWithValue("@zip", a.zip);
                cmd.Parameters.AddWithValue("@province", a.province);
                cmd.Parameters.AddWithValue("@floor", a.floor);
                cmd.Parameters.AddWithValue("@elevator", a.elevator ? 1 : 0);
                cmd.Parameters.AddWithValue("@stairs", a.stairs ? 1 : 0);
                cmd.Parameters.AddWithValue("@isBilling", a.isBilling ? 1 : 0);
                cmd.Parameters.AddWithValue("@addrType", a.addrType);
                cmd.Parameters.AddWithValue("@notes", a.notes);

                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }

        }

        internal long AddInvoiceItems(InvoiceItem ii)
        {
            string sql = "INSERT INTO invoiceitems (invoiceId, name, price) VALUES (@invoiceId, @name, @price);" +
                "SELECT LAST_INSERT_ID();";
            MySqlTransaction myTrans;
            myTrans = conn.BeginTransaction();
            long id;
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                try
                {
                    cmd.Parameters.AddWithValue("@invoiceId", ii.invoiceId);
                    cmd.Parameters.AddWithValue("@name", ii.name);
                    cmd.Parameters.AddWithValue("@price", ii.price);

                    id = Convert.ToInt32(cmd.ExecuteScalar());

                    myTrans.Commit();
                    return id;
                }
                catch (Exception e)
                {
                    myTrans.Rollback();
                    throw new InvalidDataException(e.ToString());
                }

            }
        }

        
    }

}
