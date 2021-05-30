using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace ConsoleMongoDB
{
    public class DataLog
    {
        public ObjectId _Id { get; set; }
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string IPDevice{ get; set; }
        public string Text { get; set; }

        public DataLog()
        {

        }

    }
}
