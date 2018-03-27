using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    class Database
    {
        public MySqlConnection conn;

        public Database()
        {
            conn = new MySqlConnection("Server=den1.mysql5.gear.host; database=easymove; UID=easymove; password=Cz6iV4UR__0G");
            conn.Open();
        }
    }

}
