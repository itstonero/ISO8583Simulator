using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json.Serialization;

namespace Simulator
{
    public enum Transactions { MasterKey, SessionKey, PinKey, Parameter_Download, End_Of_Day, Call_Home, Fnx_Tnx, Download_Menu };
    class Program
    {
        static void Main(string[] args)
        {
            while(true)
            {
                Console.WriteLine("============================");
                Console.WriteLine("1. Make Request");
                Console.WriteLine("2. Parse Response");
                Console.WriteLine("3. Build Request");
                Console.WriteLine("4. Compute Hash");
                Console.WriteLine("============================");

                int.TryParse(Console.ReadLine(), out int nSelc);

                switch (nSelc)
                {
                    case 1:
                        MakeRequest();
                        break;
                    case 2:
                        ParseResponse();
                        break;
                    case 3:
                        BuildRequest();
                        break;
                    case 4:
                        ComputeHash();
                        break;
                    default:
                        return;
                }

                Console.Read();
            }

        }

        private static void ComputeHash()
        {
            Console.Clear();
            Console.WriteLine("Enter Data To Be Hashed :: ");
            var data = Console.ReadLine();
            Console.WriteLine("Enter Key To Decrypt :: 32 HEX");

            
            var shaContext = new SHA256CryptoServiceProvider();
            shaContext.ComputeHash(Encoding.UTF8.GetBytes(data));
            

           
            Console.WriteLine("HASH RESULT :: " + Encoding.UTF8.GetString(shaContext.Hash));
        }

        private static void BuildRequest()
        {
            var isoMsg = new Iso8583Extended()
            {
                MessageType = Iso8583Extended.MsgType._0800_NWRK_MNG_REQ
            };

            var dataValues = new Dictionary<Int32, String>
            {
                { 002, "5399237081652147"},
                { 003, "000000"},
                { 004, "000000000100"},
                { 007, "1125160355"},
                { 011, "145279"},
                { 012, "160355"},
                { 013, "1125"},
                { 014, "2309"},
                { 018, "5999"},
                { 022, "051"},
                { 023, "001"},
                { 025, "00"},
                { 026, "12"},
                { 028, "D00000000"},
                { 032, "539923" },
                { 035, "5399237081652147D2309221013721179"},
                { 037, "948927272738"},
                { 040, "221"},
                { 041, "201116FH"},
                { 042, "2011LA024839532"},
                { 043, "XPRESSPAYMENTSOLUTIO   LA           LANG"},
                { 049, "566"},
                { 055, "820239008407A0000000041010950500000080009A032011259C01005F2A0205665F3401009F02060000000001009F03060000000000009F0607A00000000410109F090200029F10120110A74003020400000000000000000000FF9F1A0205669F1E0830303030303030319F2608807F37AEE936F44C9F2701809F3303E0F8489F34034403029F3501229F3602047E9F3704C35F28659F4104000000019F5301528E14000000000000000042014403410342031E031F03"},
                { 123, "51011151134C101"},
                { 128, "0534799F87905513443E246E5CB3D42F3D052CBD508856ED1E85F939635CF67F" }
            };
            try
            {
                foreach (var item in dataValues)
                {
                    //isoMsg[item.Key] = item.Value;
                }

                while (true)
                {
                    Console.Clear();
                    Console.WriteLine("Enter Field Number : ");
                    string fieldNumberV = Console.ReadLine();
                    Console.WriteLine($"Enter Field {fieldNumberV} value : ");
                    string value = Console.ReadLine();

                    if (value.Length > 0 && int.TryParse(fieldNumberV, out int fieldNumber))
                    {
                        isoMsg[fieldNumber] = value;
                        continue;
                    }
                    break;
                }

                var built = isoMsg.ToMsg();
                Console.Clear();
                Console.WriteLine(Encoding.UTF8.GetString(built));
                Console.WriteLine(isoMsg.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }




        }

        static void ParseResponse()
        {
            Console.Clear();
            Console.WriteLine("Enter Payload :: ");
            var payload = Console.ReadLine();

            var iso = new Iso8583Extended();
            try
            {
                iso.Unpack(Encoding.UTF8.GetBytes(payload), 0);
                Console.WriteLine(iso.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }

        }

        static void MakeRequest()
        {
            Console.Clear();
            Console.WriteLine("Enter HOST IP :: ");
            var hostIp = Console.ReadLine();
            Console.WriteLine("Enter HOST PORT :: ");
            var hostPort = Console.ReadLine();
            Console.WriteLine("Enter Payload :: ");
            var payload = Console.ReadLine();

            try
            {
                using var tcpClient = new TcpClient();
                tcpClient.Connect(String.IsNullOrEmpty(hostIp) ? "196.46.20.30" : hostIp, int.TryParse(hostPort, out int port) ? port : 5334);
                //"196.46.20.30": 5334

                using var client = tcpClient.GetStream();
                var requestPayload = new List<byte> { (byte)(payload.Length / 256), (byte)(payload.Length % 256)};
                requestPayload.AddRange(Encoding.UTF8.GetBytes(payload));
                var responsePayload = new byte[1024];
                client.Write(requestPayload.ToArray());
                client.ReadTimeout = 30000;
                int totalRead = client.Read(responsePayload);

                if(totalRead > 30)
                {
                    Console.WriteLine(Encoding.UTF8.GetString(responsePayload));
                    var iso = new Iso8583Extended();
                    iso.Unpack(responsePayload, 2);
                    Console.WriteLine(iso.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException?.Message ?? ex.Message);
            }

        }

        private static bool certValidator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }
    }
}
