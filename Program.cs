using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConsoleMongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            MongoClient dbClient = new MongoClient("mongodb://127.0.0.1:27017");

            var dbList = dbClient.ListDatabases().ToList();

            var database = dbClient.GetDatabase("SysLog");
            var collection = database.GetCollection<BsonDocument>("Raw");

            //var document = new BsonDocument { { "student_id", 10000 }, {
            //        "scores",
            //        new BsonArray {
            //        new BsonDocument { { "type", "exam" }, { "score", 88.12334193287023 } },
            //        new BsonDocument { { "type", "quiz" }, { "score", 74.92381029342834 } },
            //        new BsonDocument { { "type", "homework" }, { "score", 89.97929384290324 } },
            //        new BsonDocument { { "type", "homework" }, { "score", 82.12931030513218 } }
            //        }
            //        }, { "class_id", 480 }
            //};

            //    collection.InsertOne(document);
            //await collection.InsertOneAsync(document);

            var filter = Builders<BsonDocument>.Filter.Eq("Id", 12);
            var Registro = collection.Find(filter).FirstOrDefault();

            //var Registro = collection.Find(new BsonDocument()).FirstOrDefault();
            if(Registro != null)
                Console.WriteLine(Registro.ToString());

            filter = Builders<BsonDocument>.Filter.Eq("IPDevice", "192.168.1.254  ");
            Registro = collection.Find(filter).FirstOrDefault();
            //if (Registro != null)
            //    Console.WriteLine(Registro.ToString());



            filter = Builders<BsonDocument>.Filter.Ne("IPDevice", "192.168.1.254  ");
            var Registro2 = collection.Find(filter).ToList();
            BsonValue IPDevice;
            BsonValue TextRaw;
            BsonValue Fecha;
            DateTime z;

            //LISTA TODOS LOS DATOS EN LA COLECCION
            foreach (BsonDocument doc in Registro2)
            {
                doc.TryGetValue("IPDevice",out IPDevice);
                doc.TryGetValue("Text", out TextRaw);
                doc.TryGetValue("DateTime", out Fecha);
                z = Fecha.ToUniversalTime();
                Console.WriteLine(IPDevice.ToString().Trim() + " - " + z.ToString("dd/MM/yyyy HH:mm:ss") + " - " + TextRaw );
            }


            //LISTA LAS 
            //Console.WriteLine("The list of databases on this server is: ");
            //foreach (var db in dbList)
            //{
            //    Console.WriteLine(db);
            //}
        }
    }
}
