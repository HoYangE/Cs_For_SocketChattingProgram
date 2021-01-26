using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Server
{
    class Server
    {
        static void Main(string[] args)
        {
            IPEndPoint serverIPAddr = new IPEndPoint(IPAddress.Any, 12345);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Bind(serverIPAddr);
            server.Listen(20);

            Console.WriteLine("Watting Client");

            Socket client = server.Accept();
            IPEndPoint clientIPAddr = (IPEndPoint)client.RemoteEndPoint;
            Console.WriteLine("Accept by : {0}", clientIPAddr.Address);

            String HelloTextBuf = "Injoy Server";
            byte[] data = new byte[512];

            client.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));

            while (true)
            {
                int receiveSize = client.Receive(data);
                if (receiveSize < 0)
                    break;
                Console.WriteLine(Encoding.Default.GetString(data).Substring(0, receiveSize));
            }

            client.Close();
            server.Close();
        }
    }
}