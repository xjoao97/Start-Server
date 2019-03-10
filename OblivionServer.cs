#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
//using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using log4net;
using MySql.Data.MySqlClient;
using Oblivion.Communication.Packets;
//using Oblivion.Communication.Encryption;
//using Oblivion.Communication.Encryption.Keys;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Core;
using Oblivion.Database;
using Oblivion.HabboHotel;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Net;
using Oblivion.Utilities;
using Timer = System.Timers.Timer;

#endregion

namespace Oblivion
{
    public static class OblivionServer
    {
        public const string PrettyVersion = "Start";
        public const string PrettyBuild = "2.2.2";
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.Server");

        private static ConfigurationData _configuration;
        private static Encoding _defaultEncoding;
        private static ConnectionHandling _connectionManager;
        private static Game _game;
        private static DatabaseManager _manager;
        public static ConfigData ConfigData;
        public static MusSocket MusSystem;
        public static CultureInfo CultureInfo;
        internal static LimitedConcurrencyLevelTaskScheduler Lcts;
        internal static TaskFactory QueueLowPackets;

        internal static LimitedConcurrencyLevelTaskScheduler Lcts2;
        internal static TaskFactory QueueHeavyPackets;

        internal static LimitedConcurrencyLevelTaskScheduler Lcts3;
        internal static TaskFactory QueuePriorityPackets;

        internal static LimitedConcurrencyLevelTaskScheduler Lcts4;
        internal static TaskFactory QueueRooms;

        public static Socket listener;
        public static Socket clientSocket;
        public static byte[] clientBuffer = new byte[1024];

        public static bool Event = false;
        public static DateTime LastEvent;
        public static DateTime ServerStarted;
        internal static Timer Timer;

        internal static object UnixTimeStampToDateTime(double timestamp)
        {
            throw new NotImplementedException();
        }

        private static readonly List<char> Allowedchars = new List<char>(new[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l',
            'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x',
            'y', 'z', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '-', '.'
        });

        private static readonly ConcurrentDictionary<int, Habbo> UsersCached = new ConcurrentDictionary<int, Habbo>();

        public static string SwfRevision = "PRODUCTION-201607262204-86871104";

        internal static void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            Console.Clear();
            Log.Info("Console and memory flushed. Next Time in: 2000 Seconds");
            GC.Collect(GC.MaxGeneration);
            GC.WaitForPendingFinalizers();
            Program.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle,
                (UIntPtr) 0xFFFFFFFF, (UIntPtr) 0xFFFFFFFF);
            GC.Collect();
            Timer.Start();
        }

        public static void Initialize()

        {
            ServerStarted = DateTime.Now;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine("    )                            ");
            Console.WriteLine(@" ( /(     ) (                     ");
            Console.WriteLine(@" )\()) ( /( )\(   )  (           ");
            Console.WriteLine(@"((_)\  )\())(_)\ /(( )\  (   (   ");
            Console.WriteLine(@"  ((_)((_)\ _((_)_))((_) )\  )\ )");
            Console.WriteLine(@" / _ \| |(_) |(_))((_)_)((_)_(_/(");
            Console.WriteLine(@"| (_) | '_ \ || \ V /| / _ \ ' \))");
            Console.WriteLine(@" \___/|_.__/_||_|\_/ |_\___/_||_|");

            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("                                " + PrettyVersion + " <Versão " + PrettyBuild + ">");
            Console.WriteLine("                                http://Hiddo.com.br");

            Console.WriteLine("");
            Console.Title = "Carregando, Oblivion Emulator.";
            _defaultEncoding = Encoding.Default;

            Console.WriteLine("");
            Console.WriteLine("");

            CultureInfo = CultureInfo.CreateSpecificCulture("en-GB");


            try
            {
                _configuration = new ConfigurationData(Path.Combine(Application.StartupPath, @"extra/config.ini"));

                var connectionString = new MySqlConnectionStringBuilder
                {
                    ConnectionTimeout = 10,
                    Database = GetConfig().data["db.name"],
                    DefaultCommandTimeout = 30,
                    Logging = false,
                    MaximumPoolSize = uint.Parse(GetConfig().data["db.pool.maxsize"]),
                    MinimumPoolSize = uint.Parse(GetConfig().data["db.pool.minsize"]),
                    Password = GetConfig().data["db.password"],
                    Pooling = true,
                    Port = uint.Parse(GetConfig().data["db.port"]),
                    Server = GetConfig().data["db.hostname"],
                    UserID = GetConfig().data["db.username"],
                    AllowZeroDateTime = true,
                    ConvertZeroDateTime = true
                };

                _manager = new DatabaseManager(connectionString.ToString());

                if (!_manager.IsConnected())
                {
                    Log.Error("Falha ao tentar se conectar ao MySQL.");
                    Console.ReadKey(true);
                    Environment.Exit(1);
                }

                Log.Info("Connected to Database!");
                Language.Init();
                Log.Info("Loaded lang system");
                //Reset our statistics first.
                using (var dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("TRUNCATE `catalog_marketplace_data`");
                    dbClient.RunFastQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0';");
                    dbClient.RunFastQuery("UPDATE `users` SET `online` = '0' WHERE `online` = '1'");
                    dbClient.RunFastQuery(
                        "UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0', `status` = '1'");
                }

                //Get the configuration & Game set.
                ConfigData = new ConfigData();
                _game = new Game();

                //Have our encryption ready.
//                HabboEncryptionV2.Initialize(new RSAKeys());

                //Make sure MUS is working.
                MusSystem = new MusSocket(GetConfig().data["mus.tcp.bindip"],
                    int.Parse(GetConfig().data["mus.tcp.port"]),
                    GetConfig().data["mus.tcp.allowedaddr"].Split(Convert.ToChar(";")), 0);

                //Accept connections.
                _connectionManager = new ConnectionHandling(int.Parse(GetConfig().data["game.tcp.port"]),
                    int.Parse(GetConfig().data["game.tcp.conlimit"]), int.Parse(GetConfig().data["game.tcp.conperip"]),
                    GetConfig().data["game.tcp.enablenagles"].ToLower() == "true");
                _connectionManager.Init();
//                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
//                listener.Bind(new IPEndPoint(IPAddress.Any, Convert.ToInt32(GetConfig().data["ws.tcp.port"])));
//                listener.BeginAccept(new AsyncCallback(OnClientConnect), null);

                _game.StartGameLoop();

                var timeUsed = DateTime.Now - ServerStarted;

                Console.WriteLine();

                Lcts = new LimitedConcurrencyLevelTaskScheduler(20);
                QueueLowPackets = new TaskFactory(Lcts);

                Lcts2 = new LimitedConcurrencyLevelTaskScheduler(20);
                QueueHeavyPackets = new TaskFactory(Lcts2);

                Lcts3 =
                    new LimitedConcurrencyLevelTaskScheduler(53);
                QueuePriorityPackets = new TaskFactory(Lcts3);

                Lcts4 = new LimitedConcurrencyLevelTaskScheduler(100);
                QueueRooms = new TaskFactory(Lcts4);
                ClientMessageFactory.Init();

                Log.Info("Oblivion Emulador -> Online! (" + timeUsed.Seconds + " s, " + timeUsed.Milliseconds + " ms)");
                Timer = new Timer {Interval = 200000};
                Timer.Elapsed += TimerElapsed;
                Timer.Start();
            }
            catch (KeyNotFoundException e)
            {
                Logging.WriteLine("Verifique seu arquivo de configuração - alguns valores parecem estar faltando.",
                    ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para sair...");
                Logging.WriteLine(e.ToString());
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            catch (InvalidOperationException e)
            {
                Logging.WriteLine("Falha ao iniciar o Oblivion Emulator: " + e.Message, ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para sair...");
                Console.ReadKey(true);
                Environment.Exit(1);
            }
            catch (Exception e)
            {
                Logging.WriteLine("Erro fatal durante a inicialização: " + e, ConsoleColor.Red);
                Logging.WriteLine("Pressione qualquer tecla para sair...");

                Console.ReadKey();
                Environment.Exit(1);
            }
        }

//        private static void OnClientConnect(IAsyncResult ar)
//        {
//            clientSocket = listener.EndAccept(ar);
//            clientSocket.BeginReceive(clientBuffer, 0, 1024, 0, DoShake);
//        }
        public static bool EnumToBool(string Enum) => Enum == "1";

        public static string BoolToEnum(bool Bool) => Bool ? "1" : "0";

        public static int GetRandomNumber(int min, int max) => RandomNumber.GenerateNewRandom(min, max);


        public static double GetUnixTimestamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            return ts.TotalSeconds;
        }

        internal static int GetIUnixTimestamp()
        {
            var ts = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            var unixTime = ts.TotalSeconds;
            return Convert.ToInt32(unixTime);
        }

        private static readonly DateTime Jan1st1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static long CurrentTimeMillis()
        {
            return (long)(DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
        }


        public static long Now()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0);
            var unixTime = ts.TotalMilliseconds;
            return (long) unixTime;
        }

        public static string FilterFigure(string figure)
            =>
                figure.Any(character => !IsValid(character))
                    ? "sh-3338-93.ea-1406-62.hr-831-49.ha-3331-92.hd-180-7.ch-3334-93-1408.lg-3337-92.ca-1813-62"
                    : figure;

        private static bool IsValid(char character) => Allowedchars.Contains(character);

        public static bool IsValidAlphaNumeric(string inputStr)
        {
            inputStr = inputStr.ToLower();
            return !string.IsNullOrEmpty(inputStr) && inputStr.All(IsValid);
        }

        public static string GetUsernameById(int userId)
        {
            string name;

            var client = GetGame().GetClientManager().GetClientByUserID(userId);
            if (client?.GetHabbo() != null)
                return client.GetHabbo().Username;

            var user = GetGame().GetCacheManager().GenerateUser(userId);
            if (user != null)
                return user.Username;

            using (var dbClient = GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `username` FROM `users` WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", userId);
                name = dbClient.getString();
            }

            if (string.IsNullOrEmpty(name))
                name = "Unknown User";

            return name;
        }

        public static Habbo GetHabboById(int userId)
        {
            try
            {
                var client = GetGame().GetClientManager().GetClientByUserID(userId);
                if (client != null)
                {
                    var user = client.GetHabbo();
                    if (user == null || user.Id <= 0) return null;
                    if (UsersCached.ContainsKey(userId))
                        UsersCached.TryRemove(userId, out user);
                    return user;
                }
                if (UsersCached.ContainsKey(userId))
                    return UsersCached[userId];
                var data = UserDataFactory.GetUserData(userId);
                var generated = data?.user;
                if (generated == null) return null;
                generated.InitInformation(data);
                UsersCached.TryAdd(userId, generated);
                return generated;
            }
            catch (Exception e)
            {
                Log.Error(e);
                return null;
            }
        }


        public static Habbo GetHabboByUsername(string userName)
        {
            try
            {
                using (var dbClient = GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("SELECT `id` FROM `users` WHERE `username` = @user LIMIT 1");
                    dbClient.AddParameter("user", userName);
                    var id = dbClient.getInteger();
                    if (id > 0)
                        return GetHabboById(Convert.ToInt32(id));
                }
                return null;
            }
            catch
            {
                return null;
            }
        }


        public static void PerformShutDown()
        {
            Console.Clear();
            Log.Info("O servidor está sendo desligado...");
            Console.Title = "Oblivion Emulator: Desligando!";

            GetGame()
                .GetClientManager()
                .SendMessage(
                    new BroadcastMessageAlertComposer(GetGame().GetLanguageLocale().TryGetValue("shutdown_alert")));
            GetGame().StopGameLoop();
            Thread.Sleep(2500);
            GetConnectionManager().Destroy(); //Stop listening.
            GetGame().GetPacketManager().UnregisterAll(); //Unregister the packets.
            //GetGame().GetPacketManager().WaitForAllToComplete();
            GetGame().GetClientManager().CloseAll(); //Close all connections
            GetGame().GetRoomManager().Dispose(); //Stop the game loop.
            GetGame().GetServerStatusUpdater().Dispose(); //Sets Server Status to 0

            using (var dbClient = _manager.GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `server_status` SET `users_online` = '0', `loaded_rooms` = '0'");
                dbClient.RunFastQuery("TRUNCATE `catalog_marketplace_data`");
//                dbClient.RunQuery("TRUNCATE `user_auth_ticket`");
                dbClient.RunFastQuery("TRUNCATE `user_auth_food`");
                dbClient.RunFastQuery("UPDATE `users` SET online = '0', auth_ticket = ''");
                dbClient.RunFastQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `users_now` > '0'");
            }

            Log.Info("Oblivion Emulator, desligado com sucesso.");

            Thread.Sleep(1000);
            Environment.Exit(0);
        }

        public static ConfigurationData GetConfig() => _configuration;

        public static ConfigData GetDbConfig() => ConfigData;

        public static Encoding GetDefaultEncoding() => _defaultEncoding;

        public static ConnectionHandling GetConnectionManager() => _connectionManager;

        public static Game GetGame() => _game;

        public static DatabaseManager GetDatabaseManager() => _manager;

        public static ICollection<Habbo> GetUsersCached() => UsersCached.Values;

        public static bool RemoveFromCache(int id, out Habbo data) => UsersCached.TryRemove(id, out data);
    }
}