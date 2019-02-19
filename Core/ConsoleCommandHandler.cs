#region

using System;
using log4net;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Moderation;

#endregion

namespace Oblivion.Core
{
    public class ConsoleCommandHandler
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.Core.ConsoleCommandHandler");

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (System.Net.Sockets.Socket) ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint);

                // Signal that the connection has been made.
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void InvokeCommand(string inputData)
        {
            if (string.IsNullOrEmpty(inputData))
                return;

            try
            {
                #region Command parsing

                var parameters = inputData.Split(' ');

                switch (parameters[0].ToLower())
                {
                        #region stop

                    case "stop":
                    case "shutdown":
                    case "close":
                    {
                        Logging.DisablePrimaryWriting(true);
                        Logging.WriteLine(
                            "Stoping the environment",
                            ConsoleColor.Yellow);
                        OblivionServer.PerformShutDown();
                        break;
                    }

                        #endregion

                        #region actualizarcatalogo

                    case "catalog":
                    {
                        OblivionServer.GetGame().GetCatalog().Init(OblivionServer.GetGame().GetItemManager());
                        OblivionServer.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                        break;
                    }

                    case "items":
                        OblivionServer.GetGame().GetItemManager().Init();
                        break;

                        #endregion

                        #region alert

                    case "alert":
                    {
                        var notice = inputData.Substring(6);

                        OblivionServer.GetGame()
                            .GetClientManager()
                            .SendMessage(
                                new BroadcastMessageAlertComposer(
                                    OblivionServer.GetGame().GetLanguageLocale().TryGetValue("console.noticefromadmin") +
                                    "\n\n" + notice));

                        Log.Info("Alerta enviado com sucesso.");
                        break;
                    }

                        #endregion

                        #region new commands

                    case "clear":
                        Console.Clear();
                        Program.minimizeMemory();
                        Log.Info("Console limpo e memória liberada.");
                        break;
                    case "help":
                        Log.Info(
                            "Comandos:\n shutdown - stop environment \n clear - clear console \n catalog - flush catalogue \n items - flush items \n alert - send alert \n lang - Reload lang system");
                        break;

                    #endregion
                    case "lang":
                        Language.Reload();
                        break;
                    case "lag":
                        new System.Threading.Tasks.Task(() =>
                        {
                            for (int i = 0; i < 5000; i++)
                            {
                                try
                                {
                                    OblivionServer.GetGame().GetRoomManager().LoadRoom(i);
                                }
                                catch (Exception exception)
                                {
                                    Console.WriteLine(exception.Message);
                                }
                            }
                        }).Start();
                        int countable = 0;
                        for (uint i = 0; i < 10000; i++)
                        {
                            countable++;
                            new System.Threading.Tasks.Task(() =>
                            {
                                System.Net.IPAddress ipAddress = System.Net.IPAddress.Parse("127.0.0.1");
                                System.Net.IPEndPoint remoteEP = new System.Net.IPEndPoint(ipAddress, 30002);
                                System.Net.Sockets.Socket client =
                                    new System.Net.Sockets.Socket(System.Net.Sockets.AddressFamily.InterNetwork,
                                        System.Net.Sockets.SocketType.Stream, System.Net.Sockets.ProtocolType.Tcp);

                                // Connect to the remote endpoint.
                                client.BeginConnect(remoteEP, ConnectCallback, client);
                            }).Start();
                            if (countable == 150)
                            {
                                System.Threading.Thread.Sleep(5000);
                                countable = 0;
                            }
                        }
                        Console.WriteLine("Lag Test started");
                        break;

                    default:
                    {
                        Log.Error(parameters[0].ToLower() +
                                  " comando desconhecido. Digite help para ver todos os comandos.");
                        break;
                    }
                }

                #endregion
            }
            catch (Exception e)
            {
                Log.Error("Error in command [" + inputData + "]: " + e);
            }
        }
    }
}