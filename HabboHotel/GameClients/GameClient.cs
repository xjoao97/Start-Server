#region

using System;
using System.Diagnostics;
using ConnectionManager;
using Oblivion.Communication.Interfaces;
using Oblivion.Communication.Packets.Incoming;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.BuildersClub;
using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.Communication.Packets.Outgoing.Inventory.Achievements;
using Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.Communication.Packets.Outgoing.Nux;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.Core;
using Oblivion.HabboHotel.Moderation;
using Oblivion.HabboHotel.Permissions;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Messenger.FriendBar;
using Oblivion.HabboHotel.Users.UserDataManagement;
using Oblivion.Net;

#endregion

namespace Oblivion.HabboHotel.GameClients
{
    public class GameClient
    {
        private ConnectionInformation _connection;
        private bool _disconnected;
        private Habbo _habbo;
        private GamePacketParser _packetParser;
        public string MachineId;

        public GameClient(int ClientId, ConnectionInformation pConnection)
        {
            ConnectionID = ClientId;
            _connection = pConnection;
            _packetParser = new GamePacketParser(this);

            PingCount = 0;
        }

        public int PingCount { get; set; }

        public int ConnectionID { get; }

        private void SwitchParserRequest()
        {
            _packetParser.SetConnection(_connection);
            _packetParser.onNewPacket += parser_onNewPacket;
            var data = (_connection.parser as InitialPacketParser).currentData;
            _connection.parser.Dispose();
            _connection.parser = _packetParser;
            _connection.parser.handlePacketData(data);
        }

        private void parser_onNewPacket(ClientPacket Message)
        {
            try
            {
                OblivionServer.GetGame().GetPacketManager().TryExecutePacket(this, Message);
            }
            catch (Exception e)
            {
                Logging.LogPacketException(Message.ToString(), e.ToString());
            }
        }

        private void PolicyRequest()
            => _connection.SendData(OblivionServer.GetDefaultEncoding().GetBytes("<?xml version=\"1.0\"?>\r\n" +
                                                                                 "<!DOCTYPE cross-domain-policy SYSTEM \"/xml/dtds/cross-domain-policy.dtd\">\r\n" +
                                                                                 "<cross-domain-policy>\r\n" +
                                                                                 "<allow-access-from domain=\"*\" to-ports=\"1-31111\" />\r\n" +
                                                                                 "</cross-domain-policy>\x0"));


        public void StartConnection()
        {
            if (_connection == null)
                return;

            PingCount = 0;

            (_connection.parser as InitialPacketParser).PolicyRequest += PolicyRequest;
            (_connection.parser as InitialPacketParser).SwitchParserRequest += SwitchParserRequest;
            _connection.startPacketProcessing();
        }

        public bool TryAuthenticate(string AuthTicket)
        {
            try
            {
                byte errorCode;
                var userData = UserDataFactory.GetUserData(AuthTicket, out errorCode);
                if (userData?.user == null)
                    return false;

                if (errorCode == 1 || errorCode == 2)
                {
                    Console.WriteLine("Can't login " + errorCode);
                    Disconnect();
                    return false;
                }

                #region Ban Checking

                //Let's have a quick search for a ban before we successfully authenticate..
                ModerationBan BanRecord;
                if (!string.IsNullOrEmpty(MachineId))
                    if (OblivionServer.GetGame().GetModerationManager().IsBanned(MachineId, out BanRecord))
                        if (OblivionServer.GetGame().GetModerationManager().MachineBanCheck(MachineId))
                        {
                            Disconnect();
                            return false;
                        }
                
                    if (OblivionServer.GetGame().GetModerationManager().IsBanned(userData.user.Username, out BanRecord))
                        if (OblivionServer.GetGame().GetModerationManager().UsernameBanCheck(userData.user.Username))
                        {
                            Disconnect();
                            return false;
                        }

                #endregion

                OblivionServer.GetGame()
                    .GetClientManager()
                    .RegisterClient(this, userData.userID, userData.user.Username);
                _habbo = userData.user;
                if (_habbo != null)
                {
                    userData.user.Init(this, userData);

                    SendMessage(new AuthenticationOKComposer());
                    SendMessage(new AvatarEffectsComposer(_habbo.Effects().GetAllEffects));
                    SendMessage(new NavigatorSettingsComposer(_habbo.HomeRoom));
                    SendMessage(new FavouritesComposer(userData.user.FavoriteRooms));
                    SendMessage(new FigureSetIdsComposer(_habbo.GetClothing().GetClothingAllParts));
                    //1984
                    //2102
                    SendMessage(new UserRightsComposer(_habbo.Rank));
                    SendMessage(new AvailabilityStatusComposer());
                    //1044
                    SendMessage(new AchievementScoreComposer(_habbo.GetStats().AchievementPoints));
                    //3674
                    //3437
                    //                    SendMessage(new BuildersClubMembershipComposer());
                    SendMessage(new BuildersClubMembershipComposer()); //to enable floor and others things

                    SendMessage(new CfhTopicsInitComposer());

                    SendMessage(
                        new BadgeDefinitionsComposer(OblivionServer.GetGame().GetAchievementManager()._achievements));
                    SendMessage(new SoundSettingsComposer(_habbo.ClientVolume, _habbo.ChatPreference,
                        _habbo.AllowMessengerInvites, _habbo.FocusPreference,
                        FriendBarStateUtility.GetInt(_habbo.FriendbarState)));
                    //SendMessage(new TalentTrackLevelComposer());


                    if (GetHabbo().GetMessenger() != null)
                        GetHabbo().GetMessenger().OnStatusChanged(true);


                    if (!string.IsNullOrEmpty(MachineId))
                    {
                        if (_habbo.MachineId != MachineId)
                            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery(
                                    "UPDATE `users` SET `machine_id` = @MachineId WHERE `id` = @id LIMIT 1");
                                dbClient.AddParameter("MachineId", MachineId);
                                dbClient.AddParameter("id", _habbo.Id);
                                dbClient.RunQuery();
                            }

                        _habbo.MachineId = MachineId;
                    }

                    PermissionGroup PermissionGroup;
                    if (OblivionServer.GetGame().GetPermissionManager().TryGetGroup(_habbo.Rank, out PermissionGroup))
                        if (!string.IsNullOrEmpty(PermissionGroup.Badge))
                            if (!_habbo.GetBadgeComponent().HasBadge(PermissionGroup.Badge))
                                _habbo.GetBadgeComponent().GiveBadge(PermissionGroup.Badge, true, this);

                    if (!OblivionServer.GetGame().GetCacheManager().ContainsUser(_habbo.Id))
                        OblivionServer.GetGame().GetCacheManager().GenerateUser(_habbo.Id);

                    _habbo.InitProcess();

                    //SendMessage(new CampaignCalendarDataComposer(_habbo.GetStats().openedGifts));

                    if (userData.user.Rank > 4)
                        SendMessage(new ModeratorInitComposer(
                            OblivionServer.GetGame().GetModerationManager().UserMessagePresets,
                            OblivionServer.GetGame().GetModerationManager().RoomMessagePresets,
                            OblivionServer.GetGame().GetModerationManager().UserActionPresets,
                            OblivionServer.GetGame().GetModerationManager().GetModerationTool().GetTickets));
                    if (!string.IsNullOrWhiteSpace(OblivionServer.GetDbConfig().DBData["welcome_message"]))
                        SendMessage(new MotdNotificationComposer(OblivionServer.GetDbConfig().DBData["welcome_message"]));


                    var notif = new ServerPacket(ServerPacketHeader.NuxAlertMessageComposer);
                    notif.WriteString("habbopages/bemvindo.txt");
                    SendMessage(notif);

                    SendMessage(new NuxAlertComposer("nux/lobbyoffer/hide"));
                    var nuxshow = new ServerPacket(ServerPacketHeader.NuxAlertMessageComposer);
                    nuxshow.WriteString("nux/lobbyoffer/show");
                    SendMessage(nuxshow);

                    OblivionServer.GetGame().GetRewardManager().CheckRewards(this);
                    OblivionServer.GetGame().GetAchievementManager().ProgressRegistrationAchievements(this);
                    OblivionServer.GetGame().GetAchievementManager().ProgressLoginAchievements(this);
                    return true;
                }
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Bug during user login: " + e);
            }
            return false;
        }

        public void SendWhisper(string Message, int Colour = 34)
        {
            if (GetHabbo() == null || GetHabbo().CurrentRoom == null)
                return;

            var User = GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(GetHabbo().Username);
            if (User == null)
                return;

            SendMessage(new WhisperComposer(User.VirtualId, Message, 0, Colour == 0 ? User.LastBubble : Colour));
        }

        public void SendNotification(string Message) => SendMessage(new BroadcastMessageAlertComposer(Message));
//        private readonly object _lock = new object();

        public void SendMessage(IServerPacket Message)
        {
            var bytes = Message.GetBytes();

            if (GetConnection() == null)
                return;
            if (Debugger.IsAttached)
                Logging.WriteLine("Serverside Packet: " + Message);
            GetConnection().SendData(bytes);
        }

        public ConnectionInformation GetConnection() => _connection;

        public Habbo GetHabbo() => _habbo;

        public void Disconnect()
        {
            try
            {
                if (GetHabbo() != null)
                {
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery(GetHabbo().GetQueryString);
                    }
                    //RoomPollManager.Dispose(GetHabbo());
                    GetHabbo().OnDisconnect();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }


            if (_disconnected) return;
            _connection?.Dispose();
            _disconnected = true;
        }

        public void Dispose()
        {
            if (GetHabbo() != null)
                GetHabbo().OnDisconnect();

            MachineId = string.Empty;
            _disconnected = true;
            _habbo = null;
            _connection = null;
            //RC4Client = null;
            _packetParser = null;
        }
    }
}