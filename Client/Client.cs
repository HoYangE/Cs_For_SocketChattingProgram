using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Client
{
    class Client
    {
        static bool SingUpApproved = false;
        static object thisLock = new object();

        static string SingUp(Socket server)
        {
            string userName, password;
            string HelloTextBuf;
            while (true)
            {
                Console.Write("Input UserName Please : ");
                userName = Console.ReadLine();
                Console.Write("Input Password Please : ");
                password = Console.ReadLine();

                if (CheckEnglishNumber(userName) == false || CheckEnglishNumber(password) == false)
                    continue;

                HelloTextBuf = "SingUp" + ":" + userName + ":" + password;
                if (HelloTextBuf.Length > 512)
                    continue;
                server.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                break;
            }
            return userName;
        }

        static string SingIn(Socket server)
        {
            string userName, password;
            string HelloTextBuf;
            while (true)
            {
                Console.Write("Input UserName Please : ");
                userName = Console.ReadLine();
                Console.Write("Input Password Please : ");
                password = Console.ReadLine();

                if (CheckEnglishNumber(userName) == false || CheckEnglishNumber(password) == false)
                    continue;

                HelloTextBuf = "SingIn" + ":" + userName + ":" + password;
                if (HelloTextBuf.Length > 512)
                    continue;
                server.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                break;
            }
            return userName;
        }

        static bool CheckEnglishNumber(string letter)
        {
            bool isCheck = true;

            for (int i = 0; i < letter.Length; i++)
            {
                if (!(letter[i] >= 'A' && letter[i] <= 'Z') && !(letter[i] >= 'a' && letter[i] <= 'z') && !(letter[i] >= '0' && letter[i] <= '9'))
                {
                    isCheck = false;
                }
            }
            return isCheck;
        }

        static void ReceiveData(Socket server)
        {
            SingChoice(server);

            while (true)
            {
                byte[] data = new byte[512];
                try
                {
                    int receiveSize = server.Receive(data);
                    if (receiveSize < 0 || receiveSize > 512)
                        break;
                    string receiveDataString = Encoding.Default.GetString(data).Substring(0, receiveSize);

                    if (receiveDataString == "Username is Already in Use")
                    {
                        Console.WriteLine("Username is Already in Use!");
                        Console.WriteLine("Please SingUp Again");
                        SingChoice(server);
                    }
                    else if (receiveDataString == "Username or Password Wrong Input")
                    {
                        Console.WriteLine("Username or Password Wrong Input!");
                        Console.WriteLine("Please SingIn Again");
                        SingChoice(server);
                    }
                    else
                    {
                        lock (thisLock)
                            SingUpApproved = true;
                        Console.WriteLine(Encoding.Default.GetString(data).Substring(0, receiveSize));
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error Message : " + err.Message);
                    break;
                }
            }
            server.Close();
        }

        static string SingChoice(Socket server) 
        {
            string userName = "";

            Console.WriteLine("SingUp or SingIn");
            while (true)
            {
                string singChoice = Console.ReadLine();
                if (singChoice == "SingUp")
                {
                    userName = SingUp(server);
                    break;
                }
                else if (singChoice == "SingIn")
                {
                    userName = SingIn(server);
                    break;
                }
                else
                    continue;
            }
            return userName;
        }

        static void Main(string[] args)
        {
            IPEndPoint serverIPAddr = new IPEndPoint(IPAddress.Loopback, 12345);
            Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            server.Connect(serverIPAddr); 

            string HelloTextBuf;
            byte[] data = new byte[512];

            Thread thread = new Thread(() => ReceiveData(server));
            thread.Start();

            //int receiveSize = server.Receive(data);
            //Console.WriteLine(Encoding.Default.GetString(data).Substring(0, receiveSize));

            while (true) 
            {
                lock (thisLock)
                {
                    if (SingUpApproved == true)
                        break;
                }
            }

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
        }
    }
}