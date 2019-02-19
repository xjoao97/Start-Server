#region

using System;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using log4net;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Badges;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.Core;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Messages.Net
{
    public class MusConnection
    {
        private static readonly ILog log = LogManager.GetLogger("Oblivion.Messages.Net.MusConnection");
        private byte[] _buffer = new byte[1024];
        private Socket _socket;

        public MusConnection(Socket Socket)
        {
            _socket = Socket;

            try
            {
                _socket.BeginReceive(_buffer, 0, _buffer.Length, SocketFlags.None, OnEvent_RecieveData, _socket);
            }
            catch
            {
                tryClose();
            }
        }

        public void tryClose()
        {
            try
            {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
            }
            catch
            {
            }

            _socket = null;
            _buffer = null;
        }

        public void OnEvent_RecieveData(IAsyncResult iAr)
        {
            try
            {
                int bytes;

                try
                {
                    bytes = _socket.EndReceive(iAr);
                }
                catch
                {
                    tryClose();
                    return;
                }

                var data = Encoding.Default.GetString(_buffer, 0, bytes);

                if (data.Length > 0)
                    processCommand(data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            tryClose();
        }

        public void processCommand(string data)
        {
            GameClient Client = null;

            var header = data.Split(Convert.ToChar(1))[0];
            var param = data.Split(Convert.ToChar(1))[1];

            var Params = param.Split(':');

            switch (header.ToLower())
            {
                    #region User Related

                    #region :reload_credits <UserID>

                case "reload_credits":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    var Credits = 0;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `credits` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", UserId);
                        Credits = dbClient.getInteger();
                    }

                    Client.GetHabbo().Credits = Credits;
                    Client.SendMessage(new CreditBalanceComposer(Client.GetHabbo().Credits));
                    break;
                }

                    #endregion

                    #region :reload_pixels <UserID>

                case "reload_pixels":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    var Pixels = 0;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `activity_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", UserId);
                        Pixels = dbClient.getInteger();
                    }

                    Client.GetHabbo().Duckets = Pixels;
                    Client.SendMessage(new HabboActivityPointNotificationComposer(Client.GetHabbo().Duckets, Pixels));
                    break;
                }

                    #endregion

                    #region :reload_diamonds <UserID>

                case "reload_diamonds":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    var Diamonds = 0;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `vip_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", UserId);
                        Diamonds = dbClient.getInteger();
                    }

                    Client.GetHabbo().Diamonds = Diamonds;
                    Client.SendMessage(new HabboActivityPointNotificationComposer(Diamonds, 0, 5));
                    break;
                }

                    #endregion

                    #region :reload_gotw <UserID>

                case "reload_gotw":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    var GOTWPoints = 0;
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `gotw_points` FROM `users` WHERE `id` = @id LIMIT 1");
                        dbClient.AddParameter("id", UserId);
                        GOTWPoints = dbClient.getInteger();
                    }

                    Client.GetHabbo().GOTWPoints = GOTWPoints;
                    Client.SendMessage(new HabboActivityPointNotificationComposer(GOTWPoints, 0, 103));
                    break;
                }

                    #endregion

                    #region :reload_user_rank userID

                case "reload_user_rank":
                {
                    var UserId = Convert.ToInt32(Params[0]);

                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `rank` FROM `users` WHERE `id` = @userID LIMIT 1");
                        dbClient.AddParameter("userID", UserId);
                        Client.GetHabbo().Rank = dbClient.getInteger();
                    }
                    break;
                }

                    #endregion

                    #region :reload_motto userID

                case "reload_motto":
                {
                    var UserId = Convert.ToInt32(Params[0]);

                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `motto` FROM `users` WHERE `id` = @userID LIMIT 1");
                        dbClient.AddParameter("userID", UserId);
                        Client.GetHabbo().Motto = dbClient.getString();
                    }

                    if (Client.GetHabbo().InRoom)
                    {
                        var Room = Client.GetHabbo().CurrentRoom;
                        if (Room == null)
                            return;

                        var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Client.GetHabbo().Id);
                        if (User == null || User.GetClient() == null)
                            return;

                        Room.SendMessage(new UserChangeComposer(User, false));
                    }
                    break;
                }

                    #endregion

                    #region :alert_user <userid> <message>

                case "alert":
                case "alert_user":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    var alertMessage = Convert.ToString(Params[1]);

                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    Client.SendMessage(new BroadcastMessageAlertComposer(alertMessage));
                    break;
                }

                    #endregion

                    #region :reload_badges <UserID>

                case "update_badges":
                case "reload_badges":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);

                    if (Client != null)
                        if (Client.GetHabbo() != null)
                            Client.SendMessage(new BadgesComposer(Client));
                    break;
                }

                    #endregion

                    #region :givebadge <UserID> <badge>

                case "givebadge":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    var badgeCode = Convert.ToString(Params[1]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);

                    if (Client != null)
                        if (Client.GetHabbo() != null)
                            Client.GetHabbo().GetBadgeComponent().GiveBadge(badgeCode, true, Client);
                    break;
                }

                    #endregion

                    #region :disconnect <username>

                case "disconnect":
                {
                    try
                    {
                        var TargetClient =
                            OblivionServer.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(Params[0]));
                        if (TargetClient != null && TargetClient.GetConnection() != null)
                            TargetClient.GetConnection().Dispose();
                    }
                    catch
                    {
                        log.Error("Error disconnecting user using MUS");
                    }
                    return;
                }

                    #endregion

                    #region :reload_last_change userID

                case "reload_last_change":
                {
                    var UserId = Convert.ToInt32(Params[0]);

                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("SELECT `last_change` FROM `users` WHERE `id` = @userID LIMIT 1");
                        dbClient.AddParameter("userID", UserId);
                        Client.GetHabbo().LastNameChange = dbClient.getInteger();
                    }
                    break;
                }

                    #endregion

                    #region :goto <UserID> <RoomID>

                case "goto":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    var RoomId = Convert.ToInt32(Params[1]);

                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client?.GetHabbo() == null)
                        break;

                    if (!int.TryParse(Params[1], out RoomId))
                        break;

                    if (RoomId == Client.GetHabbo().CurrentRoomId)
                        break;

                    var _room = OblivionServer.GetGame().GetRoomManager().LoadRoom(RoomId);
                    if (_room == null)
                    {
                        Client.SendNotification("Failed to find the requested room!");
                    }
                    else
                    {
                        if (!Client.GetHabbo().InRoom)
                            Client.SendMessage(new RoomForwardComposer(_room.Id));
                        else
                            Client.GetHabbo().PrepareRoom(_room.Id, "");
                    }
                }
                    break;

                    #endregion

                    #endregion

                    #region :add_preview <photo_id> <user_id> <created_at>

                case "add_preview":
                {
                    int PhotoId = Convert.ToInt32(Params[0]);
                    int UserId = Convert.ToInt32(Params[1]);
                    long CreatedAt = Convert.ToInt64(Params[2]);

                        Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);

                        if (Client?.GetHabbo() == null || Client.GetHabbo().CurrentRoomId < 1)
                        break;

                        OblivionServer.GetGame()
                        .GetCameraManager()
                        .AddPreview(new CameraPhotoPreview(PhotoId, UserId, CreatedAt));
                    break;
                }

                    #endregion

                    #region Fastfood

                    #region :progress_achievement

                case "progress_achievement":
                {
                    var UserId = Convert.ToInt32(Params[0]);
                    Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(UserId);
                    if (Client == null || Client.GetHabbo() == null)
                        break;

                    var Achievement = Convert.ToString(Params[1]);
                    var Progress = Convert.ToInt32(Params[2]);

                    OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Client, Achievement, Progress);
                    break;
                }

                    #endregion

                    #endregion

                    #region Settings related

                    #region :reload_filter/:update_filter

                case "update_filter":
                case "reload_filter":
                case "recache_filter":
                case "refresh_filter":
                {
                    OblivionServer.GetGame().GetChatManager().GetFilter().Init();
                    break;
                }

                    #endregion

                    #region :reload_catalog/:reload_catalog

                case "update_catalog":
                case "reload_catalog":
                case "recache_catalog":
                case "refresh_catalog":
                case "update_catalogue":
                case "reload_catalogue":
                case "recache_catalogue":
                case "refresh_catalogue":
                {
                    OblivionServer.GetGame().GetCatalog().Init(OblivionServer.GetGame().GetItemManager());
                    OblivionServer.GetGame().GetClientManager().SendMessage(new CatalogUpdatedComposer());
                    break;
                }

                    #endregion

                    #region :reload_items/:update_items

                case "update_items":
                case "reload_items":
                case "recache_items":
                case "refresh_items":
                {
                    OblivionServer.GetGame().GetItemManager().Init();
                    break;
                }

                    #endregion

                    #region :reload_navigator/:update_navigator

                case "update_navigator":
                case "reload_navigator":
                case "recache_navigator":
                case "refresh_navigator":
                {
                    OblivionServer.GetGame().GetNavigator().Init();
                    break;
                }

                    #endregion

                    #region :reload_ranks/:update_ranks

                case "update_ranks":
                case "reload_ranks":
                case "recache_ranks":
                case "refresh_ranks":
                {
                    OblivionServer.GetGame().GetPermissionManager().Init();

                    foreach (
                        var C in
                        OblivionServer.GetGame()
                            .GetClientManager()
                            .GetClients.ToList()
                            .Where(C => C?.GetHabbo() != null && C.GetHabbo().GetPermissions() != null))
                        C.GetHabbo().GetPermissions().Init(Client.GetHabbo());
                    break;
                }

                    #endregion

                    #region :reload_settings/:update_settings

                case "update_settings":
                case "reload_settings":
                case "recache_settings":
                case "refresh_settings":
                {
                    OblivionServer.ConfigData = new ConfigData();
                    break;
                }

                    #endregion

                    #region :reload_quests/:update_quests

                case "reload_quests":
                case "update_quests":
                {
                    OblivionServer.GetGame().GetQuestManager().Init();
                    break;
                }

                    #endregion

                    #region :reload_vouchers/:update_vouchers

                case "reload_vouchers":
                case "update_vouchers":
                {
                    OblivionServer.GetGame().GetCatalog().GetVoucherManager().Init();
                    break;
                }

                    #endregion

                    #region :reload_bans/:update_bans

                case "update_bans":
                case "reload_bans":
                {
                    OblivionServer.GetGame().GetModerationManager().ReCacheBans();
                    break;
                }

                    #endregion

                    #endregion

                default:
                {
                    log.Error("Unrecognized MUS packet: '" + header + "'");
                    return;
                }
            }

            log.Info("Successfully Parsed MUS command: '" + header + "'");
        }
    }
}