using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
namespace TestSpeedTcpClient
{


    class Program
    {
        static int repeatCount = 10000;

        static byte[] GetTestArray()
        {

            var res = new byte[40];

            for (byte i = 0; i < 40; i++)
                res[i] = i;

            return res;
        }

        // Запрос с подключением и разрывом соединения
        static byte[] SendMessage(byte[] ms, IPEndPoint IpEndpoint)
        {

            using (var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
            {
                client.Connect(IpEndpoint);
                //      client.NoDelay = true;

                using (var ns = new NetworkStream(client))
                {

                    ns.Write(BitConverter.GetBytes(ms.Length), 0, 4);
                    ns.Write(ms, 0, ms.Length);

                    using (var br = new BinaryReader(ns))
                    {
                        var streamSize = br.ReadInt32();

                        var res = br.ReadBytes(streamSize);

                        return res;
                    }

                }
            }
        }


        // Запрос с постоянным соединением
        static byte[] SendMessage2(byte[] ms, NetworkStream ns)
        {

            ns.Write(BitConverter.GetBytes(ms.Length), 0, 4);
            ns.Write(ms, 0, ms.Length);


            var buffer = new byte[4];

            ns.Read(buffer, 0, 4);
            var streamSize = BitConverter.ToInt32(buffer, 0);
            var res = new byte[streamSize];
            ns.Read(res, 0, streamSize);

            return res;
        }


        // Тест скорости с постоянным соединением
        static int TestPermanentConnection()
        {
            int count = 0;
            var IpEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6892);
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(IpEndpoint);
            var ns = new NetworkStream(client);
            var res = GetTestArray();
            for (int i = 0; i < repeatCount; i++)
            {
                res = SendMessage2(res, ns);
                count += res[0];

            }

            return count;
        }

         // Тест скорости с подключением и разрывом соединения
        static int TestOneConnection()
        {

            int count = 0;
            var IpEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6892);

            var res = GetTestArray();
            for (int i = 0; i < repeatCount; i++)
            {
                res = SendMessage(res, IpEndpoint);
                count += res[0];

            }

            return count;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello Client!");

            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();

            var res = TestOneConnection();
            // var res = TestPermanentConnection();
            stopWatch.Stop();
            var ts = stopWatch.Elapsed;
            var elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds,
       ts.Milliseconds / 10, 0);


            Console.WriteLine(res);
            Console.WriteLine(elapsedTime);
            Console.ReadKey();
        }

    }
}