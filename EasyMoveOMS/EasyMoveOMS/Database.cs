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

namespace EasyMoveOMS
{
    public class Database
    {
        public MySqlConnection conn;

        public Database()
        {
            conn = new MySqlConnection("Server=den1.mysql5.gear.host; database=easymove; UID=easymove; password=Cz6iV4UR__0G;SslMode=none");
            //string conS = ConfigurationManager.ConnectionStrings["connectionString"].ConnectionString;
            // string conS = Properties.Settings.Default.connStr;
            //conn = new MySqlConnection(conS);
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
                "minTime, maxTime, deposit, workTime, travelTime, arriveTimeFrom, arriveTimeTo, " +
                "boxes, beds, sofas, frigos, wds, desks, tables, chairs, other, oversized, overweight, fragile, expensive, details, " +
                "isPaid, orderStatus, contactOnDate, doneStartTime, doneEndTime, doneBreaksTime, doneTotalTime, useIntAddress) " +
                "VALUES " +
                "(@moveDate, @moveTime, @clientId, @truckId, @workers, @pricePerHour, " +
                "@minTime, @maxTime, @deposit, @workTime, @travelTime, @arriveTimeFrom, @arriveTimeTo, " +
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
                cmd.Parameters.AddWithValue("@clientId", o.orderClient.id);
                cmd.Parameters.AddWithValue("@truckId", o.orderTruck.id);
                cmd.Parameters.AddWithValue("@workers", o.workers);
                cmd.Parameters.AddWithValue("@pricePerHour", o.pricePerHour);
                cmd.Parameters.AddWithValue("@minTime", o.minTime);
                cmd.Parameters.AddWithValue("@maxTime", o.maxTime);
                cmd.Parameters.AddWithValue("@deposit", o.deposit);
                cmd.Parameters.AddWithValue("@workTime", o.workTime);
                cmd.Parameters.AddWithValue("@travelTime", o.travelTime);
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
            string sql = "INSERT INTO invoices (orderId, invoiceDate, clientAddrId, noTax) VALUES (@orderId, @invoiceDate, @clientAddrId, @noTax);" +
                "SELECT LAST_INSERT_ID();";
            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@orderId", inv.orderId);
                cmd.Parameters.AddWithValue("@invoiveDate", inv.invoiceDate);
                cmd.Parameters.AddWithValue("@clientAddrId", inv.clientAddrId);
                cmd.Parameters.AddWithValue("@noTax", inv.noTax ? 1 : 0);
                long id = Convert.ToInt32(cmd.ExecuteScalar());
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
