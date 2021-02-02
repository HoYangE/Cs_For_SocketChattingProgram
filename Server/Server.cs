using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using MySql.Data.MySqlClient;

namespace Server
{
    class MySql 
    {
        public MySqlConnection ConnectSql()
        {
            var connectionString = "server=localhost;user=root;database=stydy_db;password=0407";
            var connection = new MySqlConnection(connectionString);

            try
            {
                connection.Open();
                Console.WriteLine("DB연결 완료");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return connection;
        }

        public void InsertData(MySqlConnection connection, string username, string password) 
        {
            string quary = string.Format("insert into mytable values('{0}','{1}')", username, password);
            var commend = new MySqlCommand(quary, connection);
            commend.ExecuteNonQuery();
        }

        public void AllDBReader(MySqlConnection connection) 
        {
            var commend = new MySqlCommand("select * from mytable", connection);
            MySqlDataReader dataReader = commend.ExecuteReader();

            string temp = string.Empty;
            if (dataReader.Read() == false) temp = "No return\n";
            else
            {
                do
                {
                    for (int i = 0; i < dataReader.FieldCount; i++)
                    {
                        if (i != dataReader.FieldCount - 1)
                            temp += dataReader[i] + " ; ";    // parser 넣어주기
                        else if (i == dataReader.FieldCount - 1)
                            temp += dataReader[i] + "\n";
                    }
                } while (dataReader.Read());
            }
            Console.Write(temp);
            dataReader.Close();
        }

        public bool FindUserName(MySqlConnection connection, string username) 
        {
            bool findUsername = false;

            var commend = new MySqlCommand("select * from mytable", connection);
            MySqlDataReader dataReader = commend.ExecuteReader();

            if (dataReader.Read() == false) findUsername = false;
            else
            {
                do
                {
                    for (int i = 0; i < dataReader.FieldCount; i+=2)
                    {
                        if (dataReader[i].ToString() == username)
                            findUsername = true;
                    }
                } while (dataReader.Read());
            }
            dataReader.Close();

            return findUsername;
        }

        public bool FindPassword(MySqlConnection connection, string username, string password)
        {
            bool findpassword = false;

            var commend = new MySqlCommand("select * from mytable", connection);
            MySqlDataReader dataReader = commend.ExecuteReader();

            if (dataReader.Read() == false) findpassword = false;
            else
            {
                do
                {
                    for (int i = 1; i < dataReader.FieldCount; i += 2)
                    {
                        if (dataReader[i-1].ToString() == username && dataReader[i].ToString() == password)
                            findpassword = true;
                    }
                } while (dataReader.Read());
            }
            dataReader.Close();

            return findpassword;
        }

        public void DropAllData(MySqlConnection connection) 
        {
            var commend = new MySqlCommand("truncate mytable", connection);
            commend.ExecuteNonQuery();
        }
    }

    class Server
    {
        static void Main(string[] args)
        {
            Socket server, client;

            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint serverIPAddr, clientIPAddr;
            serverIPAddr = new IPEndPoint(IPAddress.Any, 12345);

            server.Bind(serverIPAddr);
            server.Listen(20);

            Console.WriteLine("Watting Client");

            while (true)
            {
                client = server.Accept();
                clientIPAddr = (IPEndPoint)client.RemoteEndPoint;
                Console.WriteLine("Accept by : {0}", clientIPAddr.Address);               

                Thread thread = new Thread(() => ReceiveData(client));
                thread.Start();
            }
        }
        static void ReceiveData(Socket client) 
        {
            string userName = "";
            while (true)
            {
                userName = ReceiveUserData(client);
                if (userName == "Used")
                    continue;
                if (userName == "userOut")
                {
                    client.Close();
                    return;
                }
                break;
            }

            while (true)
            {
                byte[] data = new byte[512];
                try
                {
                    int receiveSize = client.Receive(data);
                    if (receiveSize < 0 || receiveSize > 512)
                        break;
                    Console.WriteLine(userName + " : " + Encoding.Default.GetString(data).Substring(0, receiveSize));
                }
                catch (Exception err)
                {
                    Console.WriteLine("Error Message : " + err.Message);
                    break;
                }
            }
            client.Close();
        }

        static string ReceiveUserData(Socket client) 
        {
            string username, password, userChoice, userDataString;
            char sp = ':';
            byte[] userData = new byte[512];
            string[] spstring = new string[3];
            try
            {
                int receiveSize = client.Receive(userData);
                userDataString = Encoding.Default.GetString(userData).Substring(0, receiveSize);
                Console.WriteLine(userDataString);
                spstring = userDataString.Split(sp);
            }
            catch (Exception err)
            {
                Console.WriteLine("Error Message : " + err.Message);
            }

            if (spstring[0] == "SingUp")
            {
                userChoice = spstring[0];
                username = spstring[1];                
                password = spstring[2];
                if (CheckUserNameSql(username) == true) 
                {
                    String HelloTextBuf = "Username is Already in Use";
                    client.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                    username = "Used";
                }
                else
                {
                    String HelloTextBuf = "Injoy Server";
                    client.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                    Console.WriteLine("Client In");

                    MySql mySql = new MySql();
                    MySqlConnection connection = mySql.ConnectSql();
                    mySql.InsertData(connection, username, password);
                    connection.Close();
                }
            }
            else if (spstring[0] == "SingIn")
            {
                userChoice = spstring[0];
                username = spstring[1];
                password = spstring[2];
                if (CheckPasswordSql(username, password) == false)
                {
                    String HelloTextBuf = "Username or Password Wrong Input";
                    client.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                    username = "Used";
                }
                else
                {
                    String HelloTextBuf = "Injoy Server";
                    client.Send(Encoding.Default.GetBytes(HelloTextBuf.Substring(0, HelloTextBuf.Length)));
                    Console.WriteLine("Client In");
                }

            }
            else
            {
                Console.WriteLine("WhatSing? I don't know... May be UserOut");
                username = "userOut";
            }
            return username;
        }

        static bool CheckUserNameSql(string username) 
        {
            bool haveUsername = false;
            MySql mySql = new MySql();
            MySqlConnection connection = mySql.ConnectSql();
            
            if(mySql.FindUserName(connection, username) == true) 
            {
                haveUsername = true;
            }

            connection.Close();
            return haveUsername;        
        }

        static bool CheckPasswordSql(string username,string password)
        {
            bool havePassword = false;
            MySql mySql = new MySql();
            MySqlConnection connection = mySql.ConnectSql();

            if (mySql.FindPassword(connection, username, password) == true)
            {
                havePassword = true;
            }

            connection.Close();
            return havePassword;
        }
    }
}