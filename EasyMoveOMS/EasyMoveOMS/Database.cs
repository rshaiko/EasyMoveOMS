using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;


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
                long id = Convert.ToInt32 (cmd.ExecuteScalar());
                return id;
            }
        }

        internal long saveNewOrder(Order o)
        {
            String sql = "INSERT INTO orders (moveDate, moveTime, clientId, truckId, workers, pricePerHour, " +
                "minTime, maxTime, deposit, travelTime, arriveTimeFrom, arriveTimeTo, " +
                "boxes, beds, sofas, frigos, wds, desks, tables, chairs, other, oversized, overweight, fragile, expensive, details, " +
                "isPaid, orderStatus, contactOnDate, doneStartTime, doneEndTime, doneBreaksTime, doneTotalTime) " +
                "VALUES " +
                "(@moveDate, @moveTime, @clientId, @truckId, @workers, @pricePerHour, " +
                "@minTime, @maxTime, @deposit, @travelTime, @arriveTimeFrom, @arriveTimeTo, " +
                "@boxes, @beds, @sofas, @frigos, @wds, @desks, @tables, @chairs, @other, @oversized, @overweight, @fragile, @expensive, @details, " +
                "@isPaid, @orderStatus, @contactOnDate, @doneStartTime, @doneEndTime, @doneBreaksTime, @doneTotalTime); " +
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
                cmd.Parameters.AddWithValue("@orderStatus", o.orderStatus+"");
                cmd.Parameters.AddWithValue("@contactOnDate", o.contactOnDate);
                cmd.Parameters.AddWithValue("@doneStartTime", o.doneStartTime);
                cmd.Parameters.AddWithValue("@doneEndTime", o.doneEndTime);
                cmd.Parameters.AddWithValue("@doneBreaksTime", o.doneBreaksTime);
                cmd.Parameters.AddWithValue("@doneTotalTime", o.doneTotalTime);



                //cmd.Parameters.AddWithValue("@name", c.Name);
                //cmd.Parameters.AddWithValue("@email", c.Email);
                //cmd.Parameters.AddWithValue("@phoneH", c.PhoneH);
                //cmd.Parameters.AddWithValue("@phoneW", c.PhoneW);
                long id = Convert.ToInt32(cmd.ExecuteScalar());
                return id;
            }
        }
    }

}
