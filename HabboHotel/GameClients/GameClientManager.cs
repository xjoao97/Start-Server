#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ConnectionManager;
using log4net;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.Core;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.HabboHotel.GameClients
{
    public delegate void ClientDisconnected(GameClient client);

    public class GameClientManager
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.HabboHotel.GameClients.GameClientManager");

        private readonly ConcurrentDictionary<int, GameClient> _clients;
        private readonly HybridDictionary _userIDRegister;
        private readonly HybridDictionary _usernameRegister;

        private readonly Stopwatch clientPingStopwatch;

        private readonly Queue timedOutConnections;


        public GameClientManager()
        {
            _clients = new ConcurrentDictionary<int, GameClient>();
            _userIDRegister = new HybridDictionary();
            _usernameRegister = new HybridDictionary();

            timedOutConnections = new Queue();

            clientPingStopwatch = new Stopwatch();
            clientPingStopwatch.Start();
        }

        public int Count => _clients.Count;

        public ICollection<GameClient> GetClients => _clients.Values;


        public void OnCycle()
        {
            TestClientConnections();
            HandleTimeouts();
        }

        public GameClient GetClientByUserID(int userID) => _userIDRegister.Contains(userID) ? (GameClient) _userIDRegister[userID] : null;

        public GameClient GetClientByUsername(string username)
            => _usernameRegister.Contains(username.ToLower()) ? (GameClient) _usernameRegister[username.ToLower()] : null;

        public bool TryGetClient(int ClientId, out GameClient Client) => _clients.TryGetValue(ClientId, out Client);

        public bool UpdateClientUsername(GameClient Client, string OldUsername, string NewUsername)
        {
            if (Client == null || !_usernameRegister.Contains(OldUsername.ToLower()))
                return false;

            _usernameRegister.Remove(OldUsername.ToLower());
            _usernameRegister.Add(NewUsername.ToLower(), Client);
            return true;
        }

        public string GetNameById(int Id)
        {
            var client = GetClientByUserID(Id);

            if (client != null)
                return client.GetHabbo().Username;

            string username;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT username FROM users WHERE id = @id LIMIT 1");
                dbClient.AddParameter("id", Id);
                username = dbClient.getString();
            }

            return username;
        }

        public IEnumerable<GameClient> GetClientsById(Dictionary<int, MessengerBuddy>.KeyCollection users)
            => users.Select(GetClientByUserID).Where(client => client != null);

        public IEnumerable<GameClient> GetClientsById(List<int> users)
            => users.Select(GetClientByUserID).Where(client => client != null);

        public void StaffAlert(ServerPacket Message, int Exclude = 0)
        {
            foreach (
                var client in
                GetClients.ToList()
                    .Where(client => client?.GetHabbo() != null)
                    .Where(client => client.GetHabbo().Rank >= 2 && client.GetHabbo().Id != Exclude))
                client.SendMessage(Message);
        }

        public void EventAlert(ServerPacket Message, int Exclude = 0)
        {
            foreach (
                var client in
                GetClients.ToList()
                    .Where(client => client?.GetHabbo() != null)
                    .Where(client => !client.GetHabbo().DisableEventAlert && client.GetHabbo().Id != Exclude))
                client.SendMessage(Message);
        }

        public void ModAlert(string Message)
        {
            foreach (
                var client in
                GetClients.ToList()
                    .Where(client => client?.GetHabbo() != null)
                    .Where(client => client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                                     !client.GetHabbo().GetPermissions().HasRight("staff_ignore_mod_alert")))
                try
                {
                    client.SendWhisper(Message, 5);
                }
                catch
                {
                }
        }

        public void DoAdvertisingReport(GameClient Reporter, GameClient Target)
        {
            if (Reporter == null || Target == null || Reporter.GetHabbo() == null || Target.GetHabbo() == null)
                return;

            var Builder = new StringBuilder();
            Builder.Append("New report submitted!\r\r");
            Builder.Append("Reporter: " + Reporter.GetHabbo().Username + "\r");
            Builder.Append("Reported User: " + Target.GetHabbo().Username + "\r\r");
            Builder.Append(Target.GetHabbo().Username + "s last 10 messages:\r\r");

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `message` FROM `chatlogs` WHERE `user_id` = '" + Target.GetHabbo().Id +
                                  "' ORDER BY `id` DESC LIMIT 10");
                var GetLogs = dbClient.getTable();

                if (GetLogs != null)
                {
                    var Number = 11;
                    foreach (DataRow Log in GetLogs.Rows)
                    {
                        Number -= 1;
                        Builder.Append(Number + ": " + Convert.ToString(Log["message"]) + "\r");
                    }
                }
            }

            foreach (
                var Client in
                GetClients.ToList()
                    .Where(Client => Client?.GetHabbo() != null)
                    .Where(Client => Client.GetHabbo().GetPermissions().HasRight("mod_tool") &&
                                     !Client.GetHabbo().GetPermissions().HasRight("staff_ignore_advertisement_reports"))
            )
                Client.SendMessage(new MotdNotificationComposer(Builder.ToString()));
        }


        public void SendMessage(ServerPacket Packet, string fuse = "")
        {
            foreach (var Client in _clients.Values.ToList().Where(Client => Client?.GetHabbo() != null))
            {
                if (!string.IsNullOrEmpty(fuse))
                    if (!Client.GetHabbo().GetPermissions().HasRight(fuse))
                        continue;

                Client.SendMessage(Packet);
            }
        }

        public void CreateAndStartClient(int clientID, ConnectionInformation connection)
        {
            var Client = new GameClient(clientID, connection);
            if (_clients.TryAdd(Client.ConnectionID, Client))
                Client.StartConnection();
            else
                connection.Dispose();
        }

        public void DisposeConnection(int clientID)
        {
            GameClient Client;
            if (!TryGetClient(clientID, out Client))
                return;

            Client?.Dispose();

            _clients.TryRemove(clientID, out Client);
        }

        public void LogClonesOut(int UserID)
        {
            var client = GetClientByUserID(UserID);
            client?.Disconnect();
        }

        public void RegisterClient(GameClient client, int userID, string username)
        {
            if (_usernameRegister.Contains(username.ToLower()))
                _usernameRegister[username.ToLower()] = client;
            else
                _usernameRegister.Add(username.ToLower(), client);

            if (_userIDRegister.Contains(userID))
                _userIDRegister[userID] = client;
            else
                _userIDRegister.Add(userID, client);
        }

        public void UnregisterClient(int userid, string username)
        {
            _userIDRegister.Remove(userid);
            _usernameRegister.Remove(username.ToLower());
        }

        public void CloseAll()
        {
            foreach (var client in GetClients.ToList().Where(client => client?.GetHabbo() != null))
                try
                {
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(client.GetHabbo().GetQueryString);
                    }
                    Console.Clear();
                    log.Info("<<- SERVIDOR SENDO DESLIGADO ->> INVENTÁRIOS SALVOS");
                }
                catch
                {
                }

            log.Info("Salvando os inventários dos usuários!");
            log.Info("Fechando as conexões...");
            try
            {
                foreach (var client in GetClients.ToList().Where(client => client?.GetConnection() != null))
                {
                    try
                    {
                        client.GetConnection().Dispose();
                    }
                    catch
                    {
                    }

                    Console.Clear();
                    log.Info("<<- SERVIDOR DESLIGADO ->> FECHANDO CONEXÕES");
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException(e.ToString());
            }

            if (_clients.Count > 0)
                _clients.Clear();

            log.Info("Conexões fechadas!");
        }

        private void TestClientConnections()
        {
            if (clientPingStopwatch.ElapsedMilliseconds >= 30000)
            {
                clientPingStopwatch.Restart();

                try
                {
                    var ToPing = new List<GameClient>();

                    foreach (var client in _clients.Values.ToList())
                        if (client.PingCount < 6)
                        {
                            client.PingCount++;

                            ToPing.Add(client);
                        }
                        else
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(client);
                            }
                        }

                    foreach (var Client in ToPing.ToList())
                        try
                        {
                            Client.SendMessage(new PongComposer());
                        }
                        catch
                        {
                            lock (timedOutConnections.SyncRoot)
                            {
                                timedOutConnections.Enqueue(Client);
                            }
                        }
                }
                catch
                {
                }
            }
        }

        private void HandleTimeouts()
        {
            if (timedOutConnections.Count > 0)
                lock (timedOutConnections.SyncRoot)
                {
                    while (timedOutConnections.Count > 0)
                    {
                        GameClient client = null;

                        if (timedOutConnections.Count > 0)
                            client = (GameClient) timedOutConnections.Dequeue();

                        client?.Disconnect();
                    }
                }
        }
    }
}