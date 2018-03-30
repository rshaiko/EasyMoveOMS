using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace EasyMoveOMS
{
    public static class Api
    {
        public static string getJson(string url)
        {
            WebClient client = new WebClient();
            Stream data = client.OpenRead(url);

            //WebRequest request = WebRequest.Create(url);
            //WebResponse response = request.GetResponse();
            //Stream data = response.GetResponseStream();
            StreamReader reader = new StreamReader(data);
            string responseFromServer = reader.ReadToEnd();
            //response.Close();
            return responseFromServer;
        }
    }


    // --- START classes GoogleMatrix
    public class GoogleMatrixData
    {
        public string[] destination_addresses { get; set; }
        public string[] origin_addresses { get; set; }
        public Row[] rows { get; set; }
        public string status { get; set; }
    }
    public class Row
    {
        public Element[] elements { get; set; }
    }

    public class Element
    {
        public Distance distance { get; set; }
        public Duration duration { get; set; }
        public string status { get; set; }
    }

    public class Distance
    {
        public string text { get; set; }
        public int value { get; set; }
    }

    public class Duration
    {
        public string text { get; set; }
        public int value { get; set; }
    }
    // --- END classes GoogleMatrix

}
