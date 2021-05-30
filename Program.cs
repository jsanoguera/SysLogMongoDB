using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ConsoleMongoDB
{
    class Program
    {
        static void Main(string[] args)
        {
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
            UdpClient udpListener = new UdpClient(514);
            byte[] bReceive; string sReceive; string sourceIP;


            /* Main Loop */
            /* Listen for incoming data on udp port 514 (default for SysLog events) */
            while (true)
            {
               try
                {
                    bReceive = udpListener.Receive(ref anyIP);
                    /* Convert incoming data from bytes to ASCII */
                    sReceive = Encoding.ASCII.GetString(bReceive);
                    /* Get the IP of the device sending the syslog */
                    sourceIP = anyIP.Address.ToString().Trim();

                    //ENVIA A LOG ARCHIVO CSV
                    //new Thread(new logHandler(sourceIP, sReceive).handleLog).Start();
                    //sReceive = "<189>date=2021-02-07 time=18:02:44 devname=FGT90D3Z13015268 devid=FGT90D3Z13015268 logid=0001000014 type=traffic subtype=local level=notice vd=root srcip=192.168.1.81 srcport=28874 srcintf=\"internal1\" dstip=192.168.1.2 dstport=443 dstintf=\"root\" sessionid=12934694 proto=6 action=close policyid=0 policytype=local-in-policy dstcountry=\"Reserved\" srccountry=\"Reserved\" trandisp=noop service=\"HTTPS\" app=\"Web Management(HTTPS)\" duration=3 sentbyte=2885 rcvdbyte=2341 sentpkt=9 rcvdpkt=8 appcat=\"unscanned\"";
                    //ENVIA A LOG EN BD 
                    new Thread(new OutputMongoDbRow(sourceIP, sReceive).AddRow).Start();
                    /* Start a new thread to handle received syslog event */

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Main - " + ex.ToString());
                }
            }

        }
        class OutputMongoDbRow
        {
            public OutputMongoDbRow(string sourceIP, string sReceive)
            {
                try
                {
                    MongoClient dbClient = new MongoClient("mongodb://192.168.0.25:27017");
                    var dbList = dbClient.ListDatabases().ToList();

                    var database = dbClient.GetDatabase("SysLog");
                    //var RawCol = database.GetCollection<DataLog>("Raw");
                    var RawCol = database.GetCollection<BsonDocument>("Log");
                    int pos = 0; int offset = 0;int p = 0; int p2 = 0; int p3 = 0; int t=0;int nMsgId = 0;
                    string k = "";
                    string v = "";
                    string v2 = "";
                    string subStr="";


                    //sReceive = "date=2021-02-11 time=10:08:11 devname=\"FG200D3913800929\" devid=\"FG200D3913800929\" logid=\"1059028704\" type=\"utm\" subtype=\"app-ctrl\" eventtype=\"app-ctrl-all\" level=\"information\" vd=\"root\" eventtime=1613048891 appid=41469 user=\"PBISCHOFF\" group=\"_SEC SALUD\" authserver=\"m78410\" srcip=192.168.104.67 dstip=23.64.109.30 srcport=1892 dstport=443 srcintf=\"lan\" srcintfrole=\"lan\" dstintf=\"wan1\" dstintfrole=\"wan\" proto=6 service=\"HTTPS\" direction=\"outgoing\" policyid=114 sessionid=29925422 applist=\"Default SALUD\" appcat=\"Collaboration\" app=\"Microsoft.Portal\" action=\"pass\" hostname=\"go.microsoft.com\" incidentserialno=1246766815 url=\"/\" msg=\"Collaboration: Microsoft.Portal,\" apprisk=\"elevated\" scertcname=\"go.microsoft.com\"";
                    //OBTIENE EL ID DE ACCION
                    t = sReceive.IndexOf('>', 0);
                    if (t>0)
                    {
                        nMsgId = int.Parse(sReceive.Substring(1, t-1));
                    }
                    sReceive = sReceive.Substring(t + 1, sReceive.Length-(t+1));
                    BsonDocument aa;
                    aa = new BsonDocument();
                    aa.Add("Fecha", DateTime.Now);
                    aa.Add("IPDevice", sourceIP);
                    aa.Add("MSGID", nMsgId);

                    if (nMsgId>0)
                    {
                        //VALIDA SI ES LOG DE FORTINET
                        //VALIDO PARA FORTINET    
                        t = sReceive.IndexOf('=', 1);
                        if (t != -1)
                        {
                            //PROCESAMIENTO DE LA CADENA v1.1
                            while (pos < sReceive.Length && pos!=-1)
                            {
                                offset = sReceive.IndexOf(' ', offset+1);
                                if (offset == -1)
                                {
                                    try
                                    {
                                        subStr = sReceive.Substring(pos, sReceive.Length - pos).Trim();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("1 - " + ex.Message + " - " + sReceive);
                                    }
                                }
                                else
                                {
                                    try
                                    {
                                        //EXTRAE LA DUPLA CLAVE-VALOR
                                        subStr = sReceive.Substring(pos, (offset - pos)).Trim();
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("2 - " + ex.Message + " - " + sReceive);
                                    }
                                }

                                //extrae cada elemento de la dupla
                                try
                                {
                                    p = subStr.IndexOf('=', 0);
                                    if (p > 0)
                                    {

                                        k = subStr.Substring(0, p);
                                        v = subStr.Substring(p + 1, subStr.Length - p - 1);
                                        p2 = v.IndexOf("\"", 0);

                                        if (p2 == -1)
                                        {
                                            //ES NUMERICO
                                            aa.Add(k, v);
                                        }
                                        else
                                        {
                                            try
                                            {
                                                //ES STRING
                                                p3 = sReceive.IndexOf("\"", pos + k.Length + 3);
                                                v2 = sReceive.Substring(pos + p + 3, (p3 - (pos + k.Length))-3);
                                                aa.Add(k, v2);
                                                offset = p3 + 1;
                                            }
                                            catch (Exception ex)
                                            {
                                                Console.WriteLine("4 - " + ex.Message + " - " + sReceive);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        aa.Add("Text", sReceive);
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("3 - " + ex.Message + " - " + sReceive);
                                }
                                pos += (offset-pos);
                            }
                        }
                        else
                        {
                            aa.Add("Text", sReceive);
                        }
                    }
                    else
                    {
                        aa.Add("Text", sReceive);
                    }
                    //GRABA EN BD
                    RawCol.InsertOne(aa);
                }
                catch (Exception ex) { Console.WriteLine("5 - " + ex.Message + " - " + sReceive); }
            }
            public void AddRow()
            {
                return;
            }
        }
    }
}
