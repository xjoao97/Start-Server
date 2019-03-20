#region

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.Core;
using Oblivion.HabboHotel.Achievements;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Chat.Commands;
using Oblivion.HabboHotel.Users.Badges;
using Oblivion.HabboHotel.Users.Clothing;
using Oblivion.HabboHotel.Users.Effects;
using Oblivion.HabboHotel.Users.Inventory;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Messenger.FriendBar;
using Oblivion.HabboHotel.Users.Navigator.SavedSearches;
using Oblivion.HabboHotel.Users.Permissions;
using Oblivion.HabboHotel.Users.Process;
using Oblivion.HabboHotel.Users.Relationships;
using Oblivion.HabboHotel.Users.UserDataManagement;

#endregion

namespace Oblivion.HabboHotel.Users
{
    public class Habbo
    {
        private readonly HabboStats _habboStats;
        private readonly PermissionComponent _permissions;
        private readonly DateTime _timeCached;


        private GameClient _client;
        private ClothingComponent _clothing;
        private bool _disconnected;

        private EffectsComponent _fx;

        private bool _habboSaved;


        private SearchesComponent _navigatorSearches;
        public bool _NUX;
        public bool AnswredPoll = false;
        private ProcessComponent _process;
        public ConcurrentDictionary<string, UserAchievement> Achievements;
        private BadgeComponent BadgeComponent;
        internal List<string> BlockedCommands;
        public string chatColour = null;
        internal bool DisableEventAlert;
        public ArrayList FavoriteRooms;
        private InventoryComponent InventoryComponent;
        public double LastCustomCommand;
        private HabboMessenger Messenger;
        public List<int> MutedUsers;

        public bool PassedNuxNavigator = false,
            PassedNuxDuckets = false,
            PassedNuxItems = false,
            PassedNuxChat = false,
            PassedNuxCatalog = false,
            AlreadyWelcome;

        public Dictionary<int, int> quests;


        public List<int> RatedRooms;
        private int _lastRoom = 0;
        public Dictionary<int, Relationship> Relationships;

        public List<RoomData> UsersRooms;

        public Habbo(int Id, string Username, int Rank, string Motto, string Look, string Gender, int Credits,
            int ActivityPoints, int HomeRoom,
            bool HasFriendRequestsDisabled, int LastOnline, bool AppearOffline, bool HideInRoom, double CreateDate,
            int Diamonds, int EventPoints,
            string machineID, bool nux, string clientVolume, bool ChatPreference, bool FocusPreference, bool PetsMuted,
            bool BotsMuted, bool AdvertisingReportBlocked, double LastNameChange,
            int GOTWPoints, bool IgnoreInvites, double TimeMuted, double TradingLock, bool AllowGifts,
            int FriendBarState, bool DisableForcedEffects, bool AllowMimic, string PrefixName, string PrefixColor, string NameColor)
        {
            this.PrefixName = PrefixName;
            this.PrefixColor = PrefixColor;
            this.NameColor = NameColor;
            this.Id = Id;
            this.Username = Username;
            this.Rank = Rank;
            this.Motto = Motto;
            this.Look = OblivionServer.GetGame().GetAntiMutant().RunLook(Look);
            this.Gender = Gender.ToLower();
            FootballLook = OblivionServer.FilterFigure(Look.ToLower());
            FootballGender = Gender.ToLower();
            this.Credits = Credits;
            Duckets = ActivityPoints;
            this.Diamonds = Diamonds;
            EventPoint = EventPoints;
            this.GOTWPoints = GOTWPoints;
            this.HomeRoom = HomeRoom;
            this.LastOnline = LastOnline;
            AccountCreated = CreateDate;
            ClientVolume = new List<int>();
            foreach (var Str in clientVolume.Split(','))
            {
                ClientVolume.Add(int.TryParse(Str, out int Val) ? int.Parse(Str) : 100);
            }

            this.LastNameChange = LastNameChange;
            MachineId = machineID;
            _NUX = nux;
            this.ChatPreference = ChatPreference;
            this.FocusPreference = FocusPreference;
            IsExpert = IsExpert;
            this.AppearOffline = AppearOffline;
            AllowTradingRequests = true; //TODO
            AllowUserFollowing = true; //TODO
            AllowFriendRequests = HasFriendRequestsDisabled; //TODO
            AllowMessengerInvites = IgnoreInvites;
            AllowPetSpeech = PetsMuted;
            AllowBotSpeech = BotsMuted;
            AllowPublicRoomStatus = HideInRoom;
            AllowConsoleMessages = true;
            this.AllowGifts = AllowGifts;
            this.AllowMimic = AllowMimic;
            ReceiveWhispers = true;
            IgnorePublicWhispers = false;
            PlayingFastFood = false;
            FriendbarState = FriendBarStateUtility.GetEnum(FriendBarState);
            ChristmasDay = ChristmasDay;
            WantsToRideHorse = 0;
            TimeAFK = 0;
            this.DisableForcedEffects = DisableForcedEffects;


            _disconnected = false;
            _habboSaved = false;
            ChangingName = false;

            FloodTime = 0;
            FriendCount = 0;
            this.TimeMuted = TimeMuted;
            _timeCached = DateTime.Now;

            TradingLockExpiry = TradingLock;
            if (TradingLockExpiry > 0 && OblivionServer.GetUnixTimestamp() > TradingLockExpiry)
            {
                TradingLockExpiry = 0;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `user_id` = '" + Id +
                                      "' LIMIT 1");
                }
            }

            BannedPhraseCount = 0;
            SessionStart = OblivionServer.GetUnixTimestamp();
            LastCustomCommand = 0;
            MessengerSpamCount = 0;
            MessengerSpamTime = 0;
            CreditsUpdateTick = OblivionStaticGameSettings.UserCreditsUpdateTimer;

            TentId = 0;
            HopperId = 0;
            IsHopping = false;
            TeleporterId = 0;
            IsTeleporting = false;
            TeleportingRoomID = 0;
            RoomAuthOk = false;
            CurrentRoomId = 0;
            AlreadyWelcome = false;
            HasSpoken = false;
            LastAdvertiseReport = 0;
            AdvertisingReported = false;
            AdvertisingReportedBlocked = AdvertisingReportBlocked;

            WiredInteraction = false;
            QuestLastCompleted = 0;
            InventoryAlert = false;
            IgnoreBobbaFilter = false;
            WiredTeleporting = false;
            CustomBubbleId = 0;
            OnHelperDuty = false;
            FastfoodScore = 0;
            PetId = 0;
            TempInt = 0;

            LastGiftPurchaseTime = DateTime.Now;
            LastMottoUpdateTime = DateTime.Now;
            LastClothingUpdateTime = DateTime.Now;
            LastForumMessageUpdateTime = DateTime.Now;

            GiftPurchasingWarnings = 0;
            MottoUpdateWarnings = 0;
            ClothingUpdateWarnings = 0;

            SessionGiftBlocked = false;
            SessionMottoBlocked = false;
            SessionClothingBlocked = false;

            FavoriteRooms = new ArrayList();
            MutedUsers = new List<int>();
            Achievements = new ConcurrentDictionary<string, UserAchievement>();
            Relationships = new Dictionary<int, Relationship>();
            RatedRooms = new List<int>();
            UsersRooms = new List<RoomData>();
            BlockedCommands = new List<string>();

            _permissions = new PermissionComponent();
            _permissions.Init(this);

            #region Stats

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts`, `calendar_gifts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                dbClient.AddParameter("user_id", Id);
                var StatRow = dbClient.GetRow();

                if (StatRow == null) //No row, add it yo
                {
                    dbClient.RunFastQuery("INSERT INTO `user_stats` (`id`) VALUES ('" + Id + "')");
                    dbClient.SetQuery(
                        "SELECT `id`,`roomvisits`,`onlinetime`,`respect`,`respectgiven`,`giftsgiven`,`giftsreceived`,`dailyrespectpoints`,`dailypetrespectpoints`,`achievementscore`,`quest_id`,`quest_progress`,`groupid`,`tickets_answered`,`respectstimestamp`,`forum_posts`, `calendar_gifts` FROM `user_stats` WHERE `id` = @user_id LIMIT 1");
                    dbClient.AddParameter("user_id", Id);
                    StatRow = dbClient.GetRow();
                }

                try
                {
                    _habboStats = new HabboStats(Convert.ToInt32(StatRow["roomvisits"]),
                        Convert.ToDouble(StatRow["onlineTime"]), Convert.ToInt32(StatRow["respect"]),
                        Convert.ToInt32(StatRow["respectGiven"]), Convert.ToInt32(StatRow["giftsGiven"]),
                        Convert.ToInt32(StatRow["giftsReceived"]), Convert.ToInt32(StatRow["dailyRespectPoints"]),
                        Convert.ToInt32(StatRow["dailyPetRespectPoints"]), Convert.ToInt32(StatRow["AchievementScore"]),
                        Convert.ToInt32(StatRow["quest_id"]), Convert.ToInt32(StatRow["quest_progress"]),
                        Convert.ToInt32(StatRow["groupid"]), Convert.ToString(StatRow["respectsTimestamp"]),
                        Convert.ToInt32(StatRow["forum_posts"]), Convert.ToString(StatRow["calendar_gifts"]));

                    if (Convert.ToString(StatRow["respectsTimestamp"]) != DateTime.Today.ToString("MM/dd"))
                    {
                        _habboStats.RespectsTimestamp = DateTime.Today.ToString("MM/dd");
//                        SubscriptionData SubData = null;

                        var DailyRespects = 3;

                        if (_permissions.HasRight("mod_tool"))
                            DailyRespects = 3;

                        _habboStats.DailyRespectPoints = DailyRespects;
                        _habboStats.DailyPetRespectPoints = DailyRespects;

                        dbClient.RunFastQuery("UPDATE `user_stats` SET `dailyRespectPoints` = '" + DailyRespects +
                                          "', `dailyPetRespectPoints` = '" + DailyRespects +
                                          "', `respectsTimestamp` = '" + DateTime.Today.ToString("MM/dd") +
                                          "' WHERE `id` = '" + Id + "' LIMIT 1");
                    }
                }
                catch (Exception e)
                {
                    Logging.LogException(e.ToString());
                }
            }

            Group G = OblivionServer.GetGame().GetGroupManager().TryGetGroup(_habboStats.FavouriteGroupId);
            if (G == null)
                _habboStats.FavouriteGroupId = 0;

            #endregion
        }

        public string sexWith { get; set; }
        public string fuckWith { get; set; }

        public string PrefixName { get; set; }
        public string PrefixColor { get; set; }
        public string NameColor { get; set; }

        public int Id { get; set; }

        public string Username { get; set; }

        public int Rank { get; set; }

        public string Motto { get; set; }

        public string Look { get; set; }

        public string Gender { get; set; }

        public string FootballLook { get; set; }

        public string FootballGender { get; set; }

        public int Credits { get; set; }

        public int Duckets { get; set; }

        public int Diamonds { get; set; }
        public int EventPoint { get; set; }

        public int GOTWPoints { get; set; }

        public int HomeRoom { get; set; }

        public double LastOnline { get; set; }

        public double AccountCreated { get; set; }

        public List<int> ClientVolume { get; set; }

        public double LastNameChange { get; set; }

        public string MachineId { get; set; }

        public bool ChatPreference { get; set; }

        public bool FocusPreference { get; set; }

        public bool IsExpert { get; set; }

        public bool AppearOffline { get; set; }


        public int TempInt { get; set; }

        public bool AllowTradingRequests { get; set; }

        public bool AllowUserFollowing { get; set; }

        public bool AllowFriendRequests { get; set; }

        public bool AllowMessengerInvites { get; set; }

        public bool AllowPetSpeech { get; set; }

        public bool AllowBotSpeech { get; set; }

        public bool AllowPublicRoomStatus { get; set; }

        public bool AllowConsoleMessages { get; set; }

        public bool AllowGifts { get; set; }

        public bool AllowMimic { get; set; }

        public bool ReceiveWhispers { get; set; }

        public bool IgnorePublicWhispers { get; set; }

        public bool PlayingFastFood { get; set; }

        public FriendBarState FriendbarState { get; set; }

        public int ChristmasDay { get; set; }

        public int WantsToRideHorse { get; set; }

        public int TimeAFK { get; set; }

        public bool DisableForcedEffects { get; set; }

        public bool ChangingName { get; set; }

        public int FriendCount { get; set; }

        public double FloodTime { get; set; }

        public int BannedPhraseCount { get; set; }

        public bool RoomAuthOk { get; set; }

        public int CurrentRoomId { get; set; }

        public int QuestLastCompleted { get; set; }

        public int MessengerSpamCount { get; set; }

        public double MessengerSpamTime { get; set; }

        public double TimeMuted { get; set; }

        public double TradingLockExpiry { get; set; }

        public double SessionStart { get; set; }

        public int TentId { get; set; }

        public int HopperId { get; set; }

        public bool IsHopping { get; set; }

        public int TeleporterId { get; set; }

        public bool IsTeleporting { get; set; }

        public int TeleportingRoomID { get; set; }

        public bool HasSpoken { get; set; }

        public double LastAdvertiseReport { get; set; }

        public bool AdvertisingReported { get; set; }

        public bool AdvertisingReportedBlocked { get; set; }

        public bool WiredInteraction { get; set; }

        public bool InventoryAlert { get; set; }

        public bool IgnoreBobbaFilter { get; set; }

        public bool WiredTeleporting { get; set; }

        public int CustomBubbleId { get; set; }

        public bool OnHelperDuty { get; set; }

        public int FastfoodScore { get; set; }

        public int PetId { get; set; }

        public int CreditsUpdateTick { get; set; }

        public IChatCommand IChatCommand { get; set; }

        public DateTime LastGiftPurchaseTime { get; set; }

        public DateTime LastMottoUpdateTime { get; set; }

        public DateTime LastClothingUpdateTime { get; set; }

        public DateTime LastForumMessageUpdateTime { get; set; }

        public int GiftPurchasingWarnings { get; set; }

        public int MottoUpdateWarnings { get; set; }

        public int ClothingUpdateWarnings { get; set; }

        public bool SessionGiftBlocked { get; set; }

        public bool SessionMottoBlocked { get; set; }

        public bool SessionClothingBlocked { get; set; }

        public bool InRoom => CurrentRoomId >= 1 && CurrentRoom != null;

        public Room CurrentRoom
        {
            get
            {
                if (CurrentRoomId <= 0)
                    return null;

                return OblivionServer.GetGame().GetRoomManager().TryGetRoom(CurrentRoomId, out Room _room) ? _room : null;
            }
        }

        public string GetQueryString
        {
            get
            {
                _habboSaved = true;
                return "UPDATE `users` SET `online` = '0', `last_online` = '" + OblivionServer.GetUnixTimestamp() +
                       "', `activity_points` = '" + Duckets + "', `credits` = '" + Credits + "', `vip_points` = '" +
                       Diamonds + "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GOTWPoints +
                       "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" +
                       FriendBarStateUtility.GetInt(FriendbarState) + "', `epoints` = '" + EventPoint + "' WHERE id = '" +
                       Id +
                       "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits +
                       "', `onlineTime` = '" +
                       (OblivionServer.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime) +
                       "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" + _habboStats.RespectGiven +
                       "', `giftsGiven` = '" + _habboStats.GiftsGiven + "', `giftsReceived` = '" +
                       _habboStats.GiftsReceived + "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints +
                       "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints +
                       "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" +
                       _habboStats.QuestID + "', `quest_progress` = '" + _habboStats.QuestProgress + "', `groupid` = '" +
                       _habboStats.FavouriteGroupId + "',`forum_posts` = '" + _habboStats.ForumPosts +
                       "' WHERE `id` = '" + Id + "' LIMIT 1;";
            }
        }

        public HabboStats GetStats() => _habboStats;

        public bool CacheExpired()
        {
            var Span = DateTime.Now - _timeCached;
            return Span.TotalMinutes >= 30;
        }

        public bool InitProcess()
        {
            _process = new ProcessComponent();
            return _process.Init(this);
        }

        public bool InitSearches()
        {
            _navigatorSearches = new SearchesComponent();
            return _navigatorSearches.Init(this);
        }

        public void UpdateCreditsBalance(GameClient Session)
        {
            if (Session == null)
                return;
            var Amount = Session.GetHabbo().Credits;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `users` SET `credits` = `credits` + '" + Amount + "' WHERE `id` = '" +
                                  Session.GetHabbo().Id + "' LIMIT 1");
            }
            Session.SendMessage(new CreditBalanceComposer(Amount));
        }

        public bool InitFX()
        {
            _fx = new EffectsComponent();
            return _fx.Init(this);
        }

        public bool InitClothing()
        {
            _clothing = new ClothingComponent();
            return _clothing.Init(this);
        }


        public void InitInformation(UserData data)
        {
            BadgeComponent = new BadgeComponent(this, data);
            Relationships = data.Relations;
        }

        public void Init(GameClient client, UserData data)
        {
            Achievements = data.achievements;

            FavoriteRooms = new ArrayList();
            foreach (var id in data.favouritedRooms)
                FavoriteRooms.Add(id);

            MutedUsers = data.ignores;

            _client = client;
            BadgeComponent = new BadgeComponent(this, data);
            InventoryComponent = new InventoryComponent(Id, client);

            quests = data.quests;

            Messenger = new HabboMessenger(Id);
            Messenger.Init(data.friends, data.requests);
            FriendCount = Convert.ToInt32(data.friends.Count);
            _disconnected = false;
            UsersRooms = data.rooms;
            Relationships = data.Relations;
            BlockedCommands = data.BlockedCommands;
            DisableEventAlert = data.DisabledEventAlert;

            InitSearches();
            InitFX();
            InitClothing();
        }


        public PermissionComponent GetPermissions() => _permissions;

        public void OnDisconnect()
        {
            if (_disconnected)
                return;

            try
            {
                _process?.Dispose();
            }
            catch
            {
            }

            _disconnected = true;

            OblivionServer.GetGame().GetClientManager().UnregisterClient(Id, Username);
            if (!_habboSaved)
            {
                _habboSaved = true;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("UPDATE `users` SET `online` = '0', `last_online` = '" +
                                      OblivionServer.GetUnixTimestamp() + "', `activity_points` = '" + Duckets +
                                      "', `credits` = '" + Credits + "', `vip_points` = '" + Diamonds +
                                      "', `home_room` = '" + HomeRoom + "', `gotw_points` = '" + GOTWPoints +
                                      "', `time_muted` = '" + TimeMuted + "',`friend_bar_state` = '" +
                                      FriendBarStateUtility.GetInt(FriendbarState) + "', `epoints` = '" + EventPoint +
                                      "' WHERE id = '" + Id +
                                      "' LIMIT 1;UPDATE `user_stats` SET `roomvisits` = '" + _habboStats.RoomVisits +
                                      "', `onlineTime` = '" +
                                      (OblivionServer.GetUnixTimestamp() - SessionStart + _habboStats.OnlineTime) +
                                      "', `respect` = '" + _habboStats.Respect + "', `respectGiven` = '" +
                                      _habboStats.RespectGiven + "', `giftsGiven` = '" + _habboStats.GiftsGiven +
                                      "', `giftsReceived` = '" + _habboStats.GiftsReceived +
                                      "', `dailyRespectPoints` = '" + _habboStats.DailyRespectPoints +
                                      "', `dailyPetRespectPoints` = '" + _habboStats.DailyPetRespectPoints +
                                      "', `AchievementScore` = '" + _habboStats.AchievementPoints + "', `quest_id` = '" +
                                      _habboStats.QuestID + "', `quest_progress` = '" + _habboStats.QuestProgress +
                                      "', `groupid` = '" + _habboStats.FavouriteGroupId + "',`forum_posts` = '" +
                                      _habboStats.ForumPosts + "' WHERE `id` = '" + Id + "' LIMIT 1;");

                    if (GetPermissions().HasRight("mod_tickets"))
                        dbClient.RunFastQuery(
                            "UPDATE `moderation_tickets` SET `status` = 'open', `moderator_id` = '0' WHERE `status` ='picked' AND `moderator_id` = '" +
                            Id + "'");
                }
            }

            Dispose();

            _client = null;
        }

        public void Dispose()
        {
            if (InRoom)
                CurrentRoom?.GetRoomUserManager().RemoveUserFromRoom(_client, false);

            InventoryComponent?.SetIdleState();

            UsersRooms?.Clear();

            if (Messenger != null)
            {
                Messenger.AppearOffline = true;
                Messenger.Destroy();
            }

            _fx?.Dispose();

            _clothing?.Dispose();

            _permissions?.Dispose();
        }

        public void CheckCreditsTimer()
        {
            try
            {
                CreditsUpdateTick--;

                if (CreditsUpdateTick <= 0)
                {
                    var CreditUpdate = OblivionStaticGameSettings.UserCreditsUpdateAmount;
                    var DucketUpdate = OblivionStaticGameSettings.UserPixelsUpdateAmount;


                    Credits += CreditUpdate;
                    Duckets += DucketUpdate;

                    _client.SendMessage(new CreditBalanceComposer(Credits));
                    _client.SendMessage(new HabboActivityPointNotificationComposer(Duckets, DucketUpdate));

                    CreditsUpdateTick = OblivionStaticGameSettings.UserCreditsUpdateTimer;
                }
            }
            catch
            {
            }
        }

        public GameClient GetClient() => _client ?? OblivionServer.GetGame().GetClientManager().GetClientByUserID(Id);

        public HabboMessenger GetMessenger() => Messenger;

        public BadgeComponent GetBadgeComponent() => BadgeComponent;

        public InventoryComponent GetInventoryComponent() => InventoryComponent;

        public SearchesComponent GetNavigatorSearches() => _navigatorSearches;

        public EffectsComponent Effects() => _fx;

        public ClothingComponent GetClothing() => _clothing;

        public int GetQuestProgress(int p)
        {
            quests.TryGetValue(p, out int progress);
            return progress;
        }

        public UserAchievement GetAchievementData(string p)
        {
            Achievements.TryGetValue(p, out UserAchievement achievement);
            return achievement;
        }

        public void ChangeName(string Username)
        {
            LastNameChange = OblivionServer.GetUnixTimestamp();
            this.Username = Username;

            SaveKey("username", Username);
            SaveKey("last_change", LastNameChange.ToString());
        }

        public void SaveKey(string Key, string Value)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET " + Key + " = @value WHERE `id` = '" + Id + "' LIMIT 1;");
                dbClient.AddParameter("value", Value);
                dbClient.RunQuery();
            }
        }

        public void PrepareRoom(int Id, string Password)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null)
                return;

            if (GetClient().GetHabbo().InRoom)
            {
                if (
                    !OblivionServer.GetGame()
                        .GetRoomManager()
                        .TryGetRoom(GetClient().GetHabbo().CurrentRoomId, out Room OldRoom))
                    return;

                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(GetClient(), false);
            }

            if (GetClient().GetHabbo().IsTeleporting && GetClient().GetHabbo().TeleportingRoomID != Id)
            {
                GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            var Room = OblivionServer.GetGame().GetRoomManager().LoadRoom(Id);
            if (Room == null)
            {
                GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            if (Room.isCrashed)
            {
                GetClient().SendNotification("This room has crashed! :(");
                GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }

            GetClient().GetHabbo().CurrentRoomId = Room.RoomId;

            if (Room.GetRoomUserManager().UserCount >= Room.UsersMax &&
                !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_full") &&
                GetClient().GetHabbo().Id != Room.OwnerId && Room.Type == "private")
            {
                GetClient().SendMessage(new CantConnectComposer(1));
                GetClient().SendMessage(new CloseConnectionComposer());
                return;
            }


            if (!GetClient().GetHabbo().GetPermissions().HasRight("room_ban_override") &&
                Room.UserIsBanned(GetClient().GetHabbo().Id))
                if (Room.HasBanExpired(GetClient().GetHabbo().Id))
                {
                    Room.RemoveBan(GetClient().GetHabbo().Id);
                }
                else
                {
                    GetClient().GetHabbo().RoomAuthOk = false;
                    GetClient().SendMessage(new CantConnectComposer(4));
                    GetClient().SendMessage(new CloseConnectionComposer());
                    return;
                }

            GetClient().SendMessage(new OpenConnectionComposer());
            if (!Room.CheckRights(GetClient(), true, true) && !GetClient().GetHabbo().IsTeleporting &&
                !GetClient().GetHabbo().IsHopping)
                if (Room.Access == RoomAccess.DOORBELL &&
                    !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                    if (Room.UserCount > 0)
                    {
                        GetClient().SendMessage(new DoorbellComposer(""));
                        Room.SendMessage(new DoorbellComposer(GetClient().GetHabbo().Username), true);
                        return;
                    }
                    else
                    {
                        GetClient().SendMessage(new FlatAccessDeniedComposer(""));
                        GetClient().SendMessage(new CloseConnectionComposer());
                        return;
                    }
                else if (Room.Access == RoomAccess.PASSWORD &&
                         !GetClient().GetHabbo().GetPermissions().HasRight("room_enter_locked"))
                    if (Password.ToLower() != Room.Password.ToLower() || string.IsNullOrWhiteSpace(Password))
                    {
                        GetClient().SendMessage(new GenericErrorComposer(-100002));
                        GetClient().SendMessage(new CloseConnectionComposer());
                        return;
                    }

            if (!EnterRoom(Room))
                GetClient().SendMessage(new CloseConnectionComposer());
        }

        public bool EnterRoom(Room Room)
        {
            if (Room == null)
            {
                GetClient().SendMessage(new CloseConnectionComposer());
                return false;
            }
           
            if (Room.OwnerId == GetClient().GetHabbo().Id)
                Room.GetTraxManager().LoadList(GetClient());
            GetClient().SendMessage(new RoomReadyComposer(Room.RoomId, Room.ModelName));
            if (Room.Wallpaper != "0.0")
                GetClient().SendMessage(new RoomPropertyComposer("wallpaper", Room.Wallpaper));
            if (Room.Floor != "0.0")
                GetClient().SendMessage(new RoomPropertyComposer("floor", Room.Floor));

            GetClient().SendMessage(new RoomPropertyComposer("landscape", Room.Landscape));
            GetClient()
                .SendMessage(new RoomRatingComposer(Room.Score,
                    !(GetClient().GetHabbo().RatedRooms.Contains(Room.RoomId) ||
                      Room.OwnerId == GetClient().GetHabbo().Id)));

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery(
                    "INSERT INTO user_roomvisits (user_id,room_id,entry_timestamp,exit_timestamp,hour,minute) VALUES ('" +
                    GetClient().GetHabbo().Id + "','" + GetClient().GetHabbo().CurrentRoomId + "','" +
                    OblivionServer.GetUnixTimestamp() + "','0','" + DateTime.Now.Hour + "','" + DateTime.Now.Minute +
                    "');"); // +
            }

            if (Room.OwnerId != Id && _lastRoom != Room.Id)
            {
                GetClient().GetHabbo().GetStats().RoomVisits += 1;
                OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(GetClient(), "ACH_RoomEntry", 1);
            }
            _lastRoom = Room.Id;
            return true;
        }
    }
}