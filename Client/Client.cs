using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            IPEndPoint serverIPAddr = new IPEndPoint(IPAddress.Loopback, 12345);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Connect(serverIPAddr);

            string HelloTextBuf;
            byte[] data = new byte[512];

            int receiveSize = server.Receive(data);
            Console.WriteLine(Encoding.Default.GetString(data).Substring(0, receiveSize));

            while (true)
            {
                HelloTextBuf = Console.ReadLine();
                if (HelloTextBuf == "end")
                    break;
                if (HelloTextBuf.Length > 512)
                    continue;
                server.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
            }

            server.Close();
            server.Close();
        }
    }
}