#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Oblivion.Communication.Interfaces;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Data.Moodlight;
using Oblivion.HabboHotel.Items.Data.Toner;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Rooms.AI.Speech;
using Oblivion.HabboHotel.Rooms.Games;
using Oblivion.HabboHotel.Rooms.Games.Banzai;
using Oblivion.HabboHotel.Rooms.Games.Football;
using Oblivion.HabboHotel.Rooms.Games.Freeze;
using Oblivion.HabboHotel.Rooms.Games.Teams;
using Oblivion.HabboHotel.Rooms.Instance;
using Oblivion.HabboHotel.Rooms.Polls.Quick;
using Oblivion.HabboHotel.Rooms.Trading;
using Oblivion.HabboHotel.Rooms.TraxMachine;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class Room : RoomData
    {
        private readonly FilterComponent _filterComponent;

        private readonly RoomUserManager _roomUserManager;
        private readonly RoomTraxManager _traxManager;
        private readonly WiredComponent _wiredComponent;


        private readonly Dictionary<int, List<RoomUser>> Tents;

        private BattleBanzai _banzai;
        private Freeze _freeze;
        private GameItemHandler _gameItemHandler;
        private GameManager _gameManager;

        private Gamemap _gamemap;

        private RoomItemHandling _roomItemHandling;
        private Soccer _soccer;

        public ArrayList ActiveTrades;

        public Dictionary<int, double> Bans;
        public bool DiscoMode;
        public bool isCrashed;
        internal bool OngoingProcess;

        public DateTime lastRegeneration;
        public DateTime lastTimerReset;
        public bool mDisposed;
        public MoodlightData MoodlightData;
        public Dictionary<int, double> MutedUsers;


//        public Task ProcessTask;
        public bool RoomMuted;
        public TeamManager teambanzai;
        public TeamManager teamfreeze;

        public TonerData TonerData;

        public List<int> UsersWithRights;

        public Room(RoomData Data)
        {
            IsLagging = 0;
            IdleTime = 0;

            RoomData = Data;
            RoomMuted = false;
            mDisposed = false;

            Id = Data.Id;
            Name = Data.Name;
            Description = Data.Description;
            OwnerName = Data.OwnerName;
            OwnerId = Data.OwnerId;
            WiredScoreBordDay = Data.WiredScoreBordDay;
            WiredScoreBordWeek = Data.WiredScoreBordWeek;
            WiredScoreBordMonth = Data.WiredScoreBordMonth;
            WiredScoreFirstBordInformation = Data.WiredScoreFirstBordInformation;

            Category = Data.Category;
            Type = Data.Type;
            Access = Data.Access;
            UsersNow = 0;
            UsersMax = Data.UsersMax;
            ModelName = Data.ModelName;
            Score = Data.Score;
            Tags = new List<string>();
            foreach (var tag in Data.Tags)
                Tags.Add(tag);

            AllowPets = Data.AllowPets;
            AllowPetsEating = Data.AllowPetsEating;
            RoomBlockingEnabled = Data.RoomBlockingEnabled;
            Hidewall = Data.Hidewall;
            Group = Data.Group;

            Password = Data.Password;
            Wallpaper = Data.Wallpaper;
            Floor = Data.Floor;
            Landscape = Data.Landscape;

            WallThickness = Data.WallThickness;
            FloorThickness = Data.FloorThickness;

            ChatMode = Data.ChatMode;
            ChatSize = Data.ChatSize;
            ChatSpeed = Data.ChatSpeed;
            ChatDistance = Data.ChatDistance;
            ExtraFlood = Data.ExtraFlood;

            TradeSettings = Data.TradeSettings;

            WhoCanBan = Data.WhoCanBan;
            WhoCanKick = Data.WhoCanKick;
            WhoCanBan = Data.WhoCanBan;
            ActiveTrades = new ArrayList();
            Bans = new Dictionary<int, double>();
            MutedUsers = new Dictionary<int, double>();
            Tents = new Dictionary<int, List<RoomUser>>();

            _gamemap = new Gamemap(this);
            if (_roomItemHandling == null)
                _roomItemHandling = new RoomItemHandling(this);
            _roomUserManager = new RoomUserManager(this);

            _filterComponent = new FilterComponent(this);
            _wiredComponent = new WiredComponent(this);
            _traxManager = new RoomTraxManager();
            _traxManager.Init(this);
            GetRoomItemHandler().LoadFurniture();
            GetGameMap().GenerateMaps();

            LoadPromotions();
            LoadRights();
            LoadBans();
            LoadFilter();
            InitBots();
            InitPets();
            //InitTrax();

            Data.UsersNow = 1;
        }

        public int IsLagging { get; set; }
        public int IdleTime { get; set; }
        public QuickPoll QuickPoll { get; private set; }

        public List<string> WordFilterList { get; set; }


        public int UserCount => _roomUserManager.GetRoomUsers().Count;

        public int RoomId => Id;

        public bool CanTradeInRoom => true;

        public RoomData RoomData { get; }
        internal void FixGameMap() => _gamemap = new Gamemap(this);

        public Gamemap GetGameMap() => _gamemap;

        public RoomItemHandling GetRoomItemHandler()
            => _roomItemHandling ?? (_roomItemHandling = new RoomItemHandling(this));

        public RoomUserManager GetRoomUserManager() => _roomUserManager;

        public Soccer GetSoccer() => _soccer ?? (_soccer = new Soccer(this));

        public TeamManager GetTeamManagerForBanzai()
            => teambanzai ?? (teambanzai = TeamManager.createTeamforGame("banzai"));

        public TeamManager GetTeamManagerForFreeze()
            => teamfreeze ?? (teamfreeze = TeamManager.createTeamforGame("freeze"));

        public BattleBanzai GetBanzai() => _banzai ?? (_banzai = new BattleBanzai(this));

        public Freeze GetFreeze() => _freeze ?? (_freeze = new Freeze(this));

        public GameManager GetGameManager() => _gameManager ?? (_gameManager = new GameManager(this));

        public GameItemHandler GetGameItemHandler()
            => _gameItemHandler ?? (_gameItemHandler = new GameItemHandler(this));

        public bool GotSoccer() => _soccer != null;

        public bool GotBanzai() => _banzai != null;

        public bool GotFreeze() => _freeze != null;

        public void ClearTags() => Tags.Clear();

        public void AddTagRange(List<string> tags) => Tags.AddRange(tags);
        public RoomTraxManager GetTraxManager() => _traxManager;

        public void InitBots()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT `id`,`room_id`,`name`,`motto`,`look`,`x`,`y`,`z`,`rotation`,`gender`,`user_id`,`ai_type`,`walk_mode`,`automatic_chat`,`speaking_interval`,`mix_sentences`,`chat_bubble` FROM `bots` WHERE `room_id` = '" +
                    RoomId + "' AND `ai_type` != 'pet'");
                var Data = dbClient.getTable();
                if (Data == null)
                    return;

                foreach (DataRow Bot in Data.Rows)
                {
                    dbClient.SetQuery("SELECT `text` FROM `bots_speech` WHERE `bot_id` = '" + Convert.ToInt32(Bot["id"]) +
                                      "'");
                    var BotSpeech = dbClient.getTable();

                    var Speeches = (from DataRow Speech in BotSpeech.Rows
                        select new RandomSpeech(Convert.ToString(Speech["text"]), Convert.ToInt32(Bot["id"]))).ToList();

                    _roomUserManager.DeployBot(
                        new RoomBot(Convert.ToInt32(Bot["id"]), Convert.ToInt32(Bot["room_id"]),
                            Convert.ToString(Bot["ai_type"]), Convert.ToString(Bot["walk_mode"]),
                            Convert.ToString(Bot["name"]), Convert.ToString(Bot["motto"]), Convert.ToString(Bot["look"]),
                            int.Parse(Bot["x"].ToString()), int.Parse(Bot["y"].ToString()),
                            int.Parse(Bot["z"].ToString()), int.Parse(Bot["rotation"].ToString()), 0, 0, 0, 0,
                            ref Speeches, "M", 0, Convert.ToInt32(Bot["user_id"].ToString()),
                            Convert.ToBoolean(Bot["automatic_chat"]), Convert.ToInt32(Bot["speaking_interval"]),
                            OblivionServer.EnumToBool(Bot["mix_sentences"].ToString()),
                            Convert.ToInt32(Bot["chat_bubble"])),
                        null);
                }
            }
        }

        public void InitPets()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT `id`,`user_id`,`room_id`,`name`,`x`,`y`,`z` FROM `bots` WHERE `room_id` = '" +
                                  RoomId + "' AND `ai_type` = 'pet'");
                var Data = dbClient.getTable();

                if (Data == null)
                    return;

                foreach (DataRow Row in Data.Rows)
                {
                    dbClient.SetQuery(
                        "SELECT `type`,`race`,`color`,`experience`,`energy`,`nutrition`,`respect`,`createstamp`,`have_saddle`,`anyone_ride`,`hairdye`,`pethair`,`gnome_clothing` FROM `bots_petdata` WHERE `id` = '" +
                        Row[0] + "' LIMIT 1");
                    var mRow = dbClient.getRow();
                    if (mRow == null)
                        continue;

                    var Pet = new Pet(Convert.ToInt32(Row["id"]), Convert.ToInt32(Row["user_id"]),
                        Convert.ToInt32(Row["room_id"]), Convert.ToString(Row["name"]), Convert.ToInt32(mRow["type"]),
                        Convert.ToString(mRow["race"]),
                        Convert.ToString(mRow["color"]), Convert.ToInt32(mRow["experience"]),
                        Convert.ToInt32(mRow["energy"]), Convert.ToInt32(mRow["nutrition"]),
                        Convert.ToInt32(mRow["respect"]), Convert.ToDouble(mRow["createstamp"]),
                        Convert.ToInt32(Row["x"]), Convert.ToInt32(Row["y"]),
                        Convert.ToDouble(Row["z"]), Convert.ToInt32(mRow["have_saddle"]),
                        Convert.ToInt32(mRow["anyone_ride"]), Convert.ToInt32(mRow["hairdye"]),
                        Convert.ToInt32(mRow["pethair"]), Convert.ToString(mRow["gnome_clothing"]));

                    var RndSpeechList = new List<RandomSpeech>();

                    _roomUserManager.DeployBot(
                        new RoomBot(Pet.PetId, RoomId, "pet", "freeroam", Pet.Name, "", Pet.Look, Pet.X, Pet.Y,
                            Convert.ToInt32(Pet.Z), 0, 0, 0, 0, 0, ref RndSpeechList, "", 0, Pet.OwnerId, false, 0,
                            false, 0), Pet);
                }
            }
        }

        //public void InitTrax() => _traxManager.LoadList();
        public FilterComponent GetFilter() => _filterComponent;

        public WiredComponent GetWired() => _wiredComponent;

        public void LoadPromotions()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_promotions` WHERE `room_id` = " + Id + " LIMIT 1;");
                var GetPromotion = dbClient.getRow();

                if (GetPromotion == null) return;
                if (Convert.ToDouble(GetPromotion["timestamp_expire"]) > OblivionServer.GetUnixTimestamp())
                    RoomData.Promotion = new RoomPromotion(Convert.ToString(GetPromotion["title"]),
                        Convert.ToString(GetPromotion["description"]),
                        Convert.ToDouble(GetPromotion["timestamp_start"]),
                        Convert.ToDouble(GetPromotion["timestamp_expire"]),
                        Convert.ToInt32(GetPromotion["category_id"]));
            }
        }

        public void LoadRights()
        {
            UsersWithRights = new List<int>();
            if (Group != null)
                return;

            DataTable Data;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT room_rights.user_id FROM room_rights WHERE room_id = @roomid");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.getTable();
            }

            if (Data == null) return;
            foreach (DataRow Row in Data.Rows)
                UsersWithRights.Add(Convert.ToInt32(Row["user_id"]));
        }

        private void LoadFilter()
        {
            WordFilterList = new List<string>();

            DataTable Data;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `room_filter` WHERE `room_id` = @roomid;");
                dbClient.AddParameter("roomid", Id);
                Data = dbClient.getTable();
            }

            if (Data == null)
                return;

            foreach (DataRow Row in Data.Rows)
                WordFilterList.Add(Convert.ToString(Row["word"]));
        }

        public void LoadBans()
        {
            this.Bans = new Dictionary<int, double>();

            DataTable Bans;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id, expire FROM room_bans WHERE room_id = " + Id);
                Bans = dbClient.getTable();
            }

            if (Bans == null)
                return;

            foreach (DataRow ban in Bans.Rows)
                this.Bans.Add(Convert.ToInt32(ban[0]), Convert.ToDouble(ban[1]));
        }

        public bool CheckRights(GameClient Session) => CheckRights(Session, false);

        public bool CheckRights(GameClient Session, bool RequireOwnership, bool CheckForGroups = false)
        {
            try
            {
                if (Session?.GetHabbo() == null)
                    return false;

                if (Session.GetHabbo().Username == OwnerName && Type == "private")
                    return true;

                if (Session.GetHabbo().GetPermissions().HasRight("room_any_owner"))
                    return true;

                if (!RequireOwnership && Type == "private")
                {
                    if (Session.GetHabbo().GetPermissions().HasRight("room_any_rights"))
                        return true;

                    if (UsersWithRights.Contains(Session.GetHabbo().Id))
                        return true;
                }

                if (CheckForGroups && Type == "private")
                {
                    if (Group == null)
                        return false;

                    if (Group.IsAdmin(Session.GetHabbo().Id))
                        return true;

                    if (Group.AdminOnlyDeco == 0)
                        if (Group.IsAdmin(Session.GetHabbo().Id))
                            return true;
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.CheckRights");
            }
            return false;
        }

        public void OnUserShoot(RoomUser User, Item Ball)
        {
            string Key = null;
            foreach (
                var item in
                GetRoomItemHandler()
                    .GetFurniObjects(Ball.GetX, Ball.GetY)
                    .ToList()
                    .Where(item => item.GetBaseItem().ItemName.StartsWith("fball_goal_")))
            {
                Key = item.GetBaseItem().ItemName.Split('_')[2];
                User.UnIdle();
                User.DanceId = 0;


                OblivionServer.GetGame()
                    .GetAchievementManager()
                    .ProgressAchievement(User.GetClient(), "ACH_FootballGoalScored", 1);

                SendMessage(new ActionComposer(User.VirtualId, 1));
            }

            if (Key != null)
            {
                Func<Item, bool> predicate = p => p.GetBaseItem().ItemName == "fball_score_" + Key;

                foreach (
                    var item2 in
                    GetRoomItemHandler()
                        .GetFloor.Where(predicate)
                        .ToList()
                        .Where(item2 => item2.GetBaseItem().ItemName == "fball_score_" + Key))
                {
                    item2.ExtraData = !string.IsNullOrEmpty(item2.ExtraData)
                        ? (Convert.ToInt32(item2.ExtraData) + 1).ToString()
                        : "1";
                    item2.UpdateState();
                }
            }
        }

        public void ProcessRoom()
        {
            OngoingProcess = true;

            if (isCrashed || mDisposed)
                return;
            try
            {
                if (GetRoomUserManager().GetRoomUsers().Count == 0)
                    IdleTime++;
                else if (IdleTime > 0)
                    IdleTime = 0;

                if (RoomData.HasActivePromotion && RoomData.Promotion.HasExpired)
                    RoomData.EndPromotion();

                if (IdleTime >= 60 && !RoomData.HasActivePromotion)
                {
                    OblivionServer.GetGame().GetRoomManager().UnloadRoom(this);
                    return;
                }

                try
                {
                    GetRoomItemHandler().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId + "] is currently having issues cycling the room items." +
                                         e);
                }

                try
                {
                    GetRoomUserManager().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId + "] is currently having issues cycling the room users." +
                                         e);
                }

                #region Status Updates

                try
                {
                    GetRoomUserManager().SerializeStatusUpdates();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId +
                                         "] is currently having issues cycling the room user statuses." + e);
                }

                #endregion

                #region Game Item Cycle

                try
                {
                    _gameItemHandler?.OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId + "] is currently having issues cycling the game items." +
                                         e);
                }

                #endregion

                try
                {
                    _traxManager.OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId + "] is currently having issues cycling the trax." + e);
                }

                try
                {
                    GetWired().OnCycle();
                }
                catch (Exception e)
                {
                    Logging.LogException("Room ID [" + RoomId + "] is currently having issues cycling wired." + e);
                }
            }
            catch (Exception e)
            {
                Logging.WriteLine("Room ID [" + RoomId + "] has crashed.");
                Logging.LogException("Room ID [" + RoomId + "] has crashed." + e);
                OnRoomCrash(e);
            }
            finally
            {
                OngoingProcess = false;
            }
        }

        private void OnRoomCrash(Exception e)
        {
            Logging.LogThreadException(e.ToString(), "Room cycle task for room " + RoomId);

            try
            {
                foreach (var user in _roomUserManager.GetRoomUsers().ToList().Where(user => user?.GetClient() != null))
                {
                    user.GetClient().SendNotification("Sorry, it appears that room has crashed!");
                    //Unhandled exception in room: " + e);

                    try
                    {
                        GetRoomUserManager().RemoveUserFromRoom(user.GetClient(), true);
                    }
                    catch (Exception e2)
                    {
                        Logging.LogException(e2.ToString());
                    }
                }
            }
            catch (Exception e3)
            {
                Logging.LogException(e3.ToString());
            }

            isCrashed = true;
            OblivionServer.GetGame().GetRoomManager().UnloadRoom(this, true);
        }


        public bool CheckMute(GameClient Session)
        {
            if (MutedUsers.ContainsKey(Session.GetHabbo().Id))
                if (MutedUsers[Session.GetHabbo().Id] < OblivionServer.GetUnixTimestamp())
                    MutedUsers.Remove(Session.GetHabbo().Id);
                else
                    return true;

            return Session.GetHabbo().TimeMuted > 0 || RoomMuted && Session.GetHabbo().Username != OwnerName;
        }

        /*public void AddChatlog(int Id, string Message) //disabled atm
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `chatlogs` (user_id, room_id, message, timestamp) VALUES (@user, @room, @message, @time)");
                dbClient.AddParameter("user", Id);
                dbClient.AddParameter("room", RoomId);
                dbClient.AddParameter("message", Message);
                dbClient.AddParameter("time", OblivionServer.GetUnixTimestamp());
                dbClient.RunQuery();
            }
        }*/

        public void SendObjects(GameClient Session)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            Session.SendMessage(new HeightMapComposer(Room.GetGameMap().Model.Heightmap));
            Session.SendMessage(new FloorHeightMapComposer(Room.GetGameMap().Model.GetRelativeHeightmap(),
                Room.GetGameMap().StaticModel.WallHeight));
            foreach (var RoomUser in _roomUserManager.GetUserList().ToList())
            {
                if (RoomUser == null)
                    continue;

                Session.SendMessage(new UsersComposer(RoomUser));


                if (RoomUser.IsBot && RoomUser.BotData.DanceId > 0)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.BotData.DanceId));
                else if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.IsDancing)
                    Session.SendMessage(new DanceComposer(RoomUser, RoomUser.DanceId));

                if (RoomUser.IsAsleep)
                    Session.SendMessage(new SleepComposer(RoomUser, true));

                if (RoomUser.CarryItemID > 0 && RoomUser.CarryTimer > 0)
                    Session.SendMessage(new CarryObjectComposer(RoomUser.VirtualId, RoomUser.CarryItemID));

                if (!RoomUser.IsBot && !RoomUser.IsPet && RoomUser.CurrentEffect > 0)
                    Room.SendMessage(new AvatarEffectComposer(RoomUser.VirtualId, RoomUser.CurrentEffect));
            }
            Session.SendMessage(new UserUpdateComposer(_roomUserManager.GetUserList().ToList()));
            Session.SendMessage(new ObjectsComposer(Room.GetRoomItemHandler().GetFloor.ToArray(), this));
            Session.SendMessage(new ItemsComposer(Room.GetRoomItemHandler().GetWall.ToArray(), this));
        }

        private void SaveAI()
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                foreach (
                    var User in
                    GetRoomUserManager()
                        .GetRoomUsers()
                        .ToList()
                        .Where(User => User != null && User.IsBot))
                {
                    dbClient.SetQuery(
                        "UPDATE bots SET x=@x, y=@y, z=@z, name=@name, look=@look, rotation=@rotation WHERE id=@id LIMIT 1;");
                    dbClient.AddParameter("name", User.BotData.Name);
                    dbClient.AddParameter("look", User.BotData.Look);
                    dbClient.AddParameter("rotation", User.BotData.Rot);
                    dbClient.AddParameter("x", User.X);
                    dbClient.AddParameter("y", User.Y);
                    dbClient.AddParameter("z", User.Z);
                    dbClient.AddParameter("id", User.BotData.BotId);
                    dbClient.RunQuery();
                }
            }
        }

        public void Dispose()
        {
            SendMessage(new CloseConnectionComposer());

            if (!mDisposed)
            {
                isCrashed = false;
                mDisposed = true;


                GetRoomItemHandler().SaveFurniture();

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `users_now` = '0' WHERE `id` = '" + Id + "' LIMIT 1");
                }

                if (_roomUserManager.PetCount > 0)
                    _roomUserManager.UpdatePets();

                SaveAI();

                UsersNow = 0;
                RoomData.UsersNow = 0;

                UsersWithRights.Clear();
                Bans.Clear();
                MutedUsers.Clear();
                Tents.Clear();

                TonerData = null;
                MoodlightData = null;

                _filterComponent.Cleanup();
                _wiredComponent.Cleanup();
                _gameItemHandler?.Dispose();

                _gameManager?.Dispose();

                _freeze?.Dispose();

                _banzai?.Dispose();

                _soccer?.Dispose();

                _gamemap?.Dispose();
                _roomUserManager?.Dispose();

                _roomItemHandling?.Dispose();

                ActiveTrades.Clear();
                teambanzai?.Dispose();
                teamfreeze?.Dispose();
            }
        }

        #region Room Bans

        public bool UserIsBanned(int pId) => Bans.ContainsKey(pId);

        public void RemoveBan(int pId) => Bans.Remove(pId);

        public void AddBan(int pId, long Time)
        {
            if (!Bans.ContainsKey(Convert.ToInt32(pId)))
                Bans.Add(pId, OblivionServer.GetUnixTimestamp() + Time);

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("REPLACE INTO `room_bans` VALUES (" + pId + ", " + Id + ", " +
                                  (OblivionServer.GetUnixTimestamp() + Time) + ")");
            }
        }

        public QuickPoll GenerateQPoll(int duration, string question)
            => QuickPoll = new QuickPoll(-2, this, question, duration);

        public List<int> BannedUsers()
        {
            var Bans = new List<int>();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT user_id FROM room_bans WHERE expire > UNIX_TIMESTAMP() AND room_id=" + Id);
                var Table = dbClient.getTable();

                Bans.AddRange(from DataRow Row in Table.Rows select Convert.ToInt32(Row[0]));
            }

            return Bans;
        }

        public bool HasBanExpired(int pId)
        {
            if (!UserIsBanned(pId))
                return true;

            return Bans[pId] < OblivionServer.GetUnixTimestamp();
        }

        public void Unban(int UserId)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("DELETE FROM `room_bans` WHERE `user_id` = '" + UserId + "' AND `room_id` = '" + Id +
                                  "' LIMIT 1");
            }

            if (Bans.ContainsKey(UserId))
                Bans.Remove(UserId);
        }

        #endregion

        #region Trading

        public bool HasActiveTrade(RoomUser User) => !User.IsBot && HasActiveTrade(User.GetClient().GetHabbo().Id);

        public bool HasActiveTrade(int UserId)
            =>
                ActiveTrades.Count != 0 && ActiveTrades.ToArray().Cast<Trade>().Any(Trade => Trade.ContainsUser(UserId));

        public Trade GetUserTrade(int UserId)
            => ActiveTrades.ToArray().Cast<Trade>().FirstOrDefault(Trade => Trade.ContainsUser(UserId));

        public void TryStartTrade(RoomUser UserOne, RoomUser UserTwo)
        {
            if (UserOne == null || UserTwo == null || UserOne.IsBot || UserTwo.IsBot || UserOne.IsTrading ||
                UserTwo.IsTrading || HasActiveTrade(UserOne) || HasActiveTrade(UserTwo))
                return;

            ActiveTrades.Add(new Trade(UserOne.GetClient().GetHabbo().Id, UserTwo.GetClient().GetHabbo().Id, RoomId));
        }

        public void TryStopTrade(int UserId)
        {
            var Trade = GetUserTrade(UserId);

            if (Trade == null)
                return;

            Trade.CloseTrade(UserId);
            ActiveTrades.Remove(Trade);
        }

        #endregion

        #region Tents

        public void AddTent(int TentId)
        {
            if (Tents.ContainsKey(TentId))
                Tents.Remove(TentId);

            Tents.Add(TentId, new List<RoomUser>());
        }

        public void RemoveTent(int TentId, Item Item)
        {
            if (!Tents.ContainsKey(TentId))
                return;

            var Users = Tents[TentId];
            foreach (
                var User in
                Users.ToList().Where(User => User?.GetClient() != null && User.GetClient().GetHabbo() != null))
                User.GetClient().GetHabbo().TentId = 0;

            if (Tents.ContainsKey(TentId))
                Tents.Remove(TentId);
        }

        public void AddUserToTent(int TentId, RoomUser User, Item Item)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                    Tents.Add(TentId, new List<RoomUser>());

                if (!Tents[TentId].Contains(User))
                    Tents[TentId].Add(User);
                User.GetClient().GetHabbo().TentId = TentId;
            }
        }

        public void RemoveUserFromTent(int TentId, RoomUser User, Item Item)
        {
            if (User != null && User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                if (!Tents.ContainsKey(TentId))
                    Tents.Add(TentId, new List<RoomUser>());

                if (Tents[TentId].Contains(User))
                    Tents[TentId].Remove(User);

                User.GetClient().GetHabbo().TentId = 0;
            }
        }

        public void SendToTent(int Id, int TentId, IServerPacket Packet)
        {
            if (!Tents.ContainsKey(TentId))
                return;

            foreach (
                var User in
                Tents[TentId].ToList()
                    .Where(
                        User =>
                            User?.GetClient() != null && User.GetClient().GetHabbo() != null &&
                            !User.GetClient().GetHabbo().MutedUsers.Contains(Id) &&
                            User.GetClient().GetHabbo().TentId == TentId))
                User.GetClient().SendMessage(Packet);
        }

        #endregion

        #region Communication (Packets)

        internal void SendFastMessage(IServerPacket message)
        {
            try
            {
                lock (GetRoomUserManager().GetUsers())
                {
                    foreach (
                        var client in
                        GetRoomUserManager()
                            .GetUsers().Values.Where(user => user != null && !user.IsBot)
                            .Select(user => user.GetClient())
                            .Where(client => client != null))
                        client.SendMessage(message);
                }
            }
            catch
            {
            }
        }


        public void SendMessage(IServerPacket Message, bool UsersWithRightsOnly = false)
        {
            if (Message == null || _roomUserManager?.GetUserList() == null)
                return;

            try
            {
                var Users = _roomUserManager.GetUserList().ToList();


                foreach (RoomUser User in
                    from User in Users
                    where User != null && !User.IsBot
                    where User.GetClient() != null && User.GetClient().GetConnection() != null
                    where !UsersWithRightsOnly || CheckRights(User.GetClient())
                    select User)
                {
                    User.GetClient().SendMessage(Message);
                }
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SendMessage");
            }
        }

        public void BroadcastPacket(byte[] Packet)
        {
            foreach (
                var User in
                _roomUserManager.GetUserList()
                    .ToList()
                    .Where(User => User != null && !User.IsBot)
                    .Where(User => User.GetClient() != null && User.GetClient().GetConnection() != null))
                User.GetClient().GetConnection().SendData(Packet);
        }

        public void SendMessage(List<ServerPacket> Messages)
        {
            if (Messages.Count == 0)
                return;

            try
            {
                var TotalBytes = new byte[0];
                var Current = 0;

                foreach (var Packet in Messages.ToList())
                {
                    var ToAdd = Packet.GetBytes();
                    var NewLen = TotalBytes.Length + ToAdd.Length;

                    Array.Resize(ref TotalBytes, NewLen);

                    foreach (var t in ToAdd)
                    {
                        TotalBytes[Current] = t;
                        Current++;
                    }
                }

                BroadcastPacket(TotalBytes);
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SendMessage List<ServerPacket>");
            }
        }

        #endregion
    }
}