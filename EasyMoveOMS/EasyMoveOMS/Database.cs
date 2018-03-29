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
    class Database
    {
        public MySqlConnection conn;

        public Database()
        {
            //conn = new MySqlConnection("Server=den1.mysql5.gear.host; database=easymove; UID=easymove; password=Cz6iV4UR__0G");
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

    }

}
