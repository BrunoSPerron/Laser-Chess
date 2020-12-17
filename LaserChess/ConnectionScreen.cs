using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Game
{
    class ConnectionScreen
    {
        public ConnectionScreen()
        {
            ShowMenu();
        }

        void ShowMenu()
        {
            int choice = ExtendedConsole.ShowMenuAndGetChoice(new string[] { "Host", "Connect to IP" }, 5, 3);

            if (choice == 0)
            {
                Host();
            }
            else if (choice == 1)
            {
                ExtendedConsole.VirtualWrite("Enter host ip adress:", 5, 8);
                ExtendedConsole.Update();
                Console.SetCursorPosition(7, 9);
                Console.CursorVisible = true;
                string ip = Console.ReadLine();
                Console.CursorVisible = false;
                Client(ip);
            }
        }

        void Host()
        {
            TcpListener server = null;
            try
            {
                Int32 port = Settings.ConnectionPort;
                server = new TcpListener(IPAddress.Any, port);
                server.Start();

                ExtendedConsole.VirtualWrite("Waiting for connection... ", 8, 8);
                //ExtendedConsole.VirtualWrite("Your ip adress: " + localAddr, 8, 9);
                ExtendedConsole.Update();
                
                TcpClient client = server.AcceptTcpClient();
                ExtendedConsole.VirtualWrite("Connected!", 8, 10);
                ExtendedConsole.Update();

                new Game(client, true);
                client.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }
        }

        void Client(String serverIP)
        {
            try
            {
                Int32 port = Settings.ConnectionPort;
                TcpClient client = new TcpClient(serverIP, port);

                new Game(client, false);
                
                client.Close();
            }
            catch (ArgumentNullException e)
            {
                Console.WriteLine("ArgumentNullException: {0}", e);
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
        }
    }
}
