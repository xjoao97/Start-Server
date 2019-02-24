#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class RoomManager
    {
        private static readonly ILog Log = LogManager.GetLogger("Oblivion.HabboHotel.Rooms.RoomManager");
        private readonly HybridDictionary _loadedRoomData;
        //private DateTime _purgeLastExecution;

        private readonly HybridDictionary _roomModels;

        private readonly ConcurrentDictionary<int, Room> _rooms;
        public List<Room> LoadedBallRooms;


        private DateTime _cycleLastExecution;
        private DateTime _cycleBallLastExecution;


        public RoomManager()
        {
            _roomModels = new HybridDictionary();

            _rooms = new ConcurrentDictionary<int, Room>();
            _loadedRoomData = new HybridDictionary();
            LoadedBallRooms = new List<Room>();

            LoadModels();

            //    _purgeLastExecution = DateTime.Now.AddHours(3);

            Log.Info("Room Manager -> LOADED");
        }

        public int LoadedRoomDataCount => _loadedRoomData.Count;

        public int Count => _rooms.Count;

        public void OnCycle()
        {
            if (LoadedBallRooms.Count > 0)
            {
                var sinceBallLastTime = DateTime.Now - _cycleBallLastExecution;
                if (sinceBallLastTime.TotalMilliseconds >= 180)
                {
                    _cycleBallLastExecution = DateTime.Now;
                    foreach (var Room in LoadedBallRooms)
                    {
                        if (Room == null)
                            return;
                        try
                        {
                            if (Room.GotSoccer())
                            {
//                                lock (_lock)
                                    Room.GetSoccer().OnCycle();
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.LogCriticalException("INVALID MARIO BUG IN BALLMOVEMENT: <" + Room.Id +
                                                         "> :" +
                                                         e);
                        }
                    }
                }
            }
            try
            {
                var sinceLastTime = DateTime.Now - _cycleLastExecution;
                if (sinceLastTime.TotalMilliseconds >= 500)
                {
                    _cycleLastExecution = DateTime.Now;
                    foreach (var room in _rooms.Values.ToList())
                    {
                        if (room.isCrashed)
                            continue;

                        if (room.ProcessTask == null || room.ProcessTask.IsCompleted)
                        {
                            room.ProcessTask = new Task(room.ProcessRoom);
                            room.ProcessTask.Start();
                            room.IsLagging = 0;
                        }
                        else
                        {
                            room.IsLagging++;
                            if (room.IsLagging >= 30)
                            {
                                room.isCrashed = true;
                                UnloadRoom(room.Id);
                            }
                        }
                    }
                }
                OblivionServer.GetGame().RoomManagerCycleEnded = true;
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Issue with the RoomManager: " + e);
            }
        }

        public void LoadModels()
        {
            if (_roomModels.Count > 0)
                _roomModels.Clear();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '0'");
                var Data = dbClient.getTable();

                if (Data == null)
                    return;

                foreach (DataRow Row in Data.Rows)
                {
                    var Modelname = Convert.ToString(Row["id"]);
                    //var staticFurniture = Convert.ToString(Row["public_items"]);

                    _roomModels.Add(Modelname,
                        new RoomModel(Convert.ToInt32(Row["door_x"]), Convert.ToInt32(Row["door_y"]),
                            (double) Row["door_z"], Convert.ToInt32(Row["door_dir"]),
                            Convert.ToString(Row["heightmap"]), Convert.ToString(Row["public_items"]),
                            OblivionServer.EnumToBool(Row["club_only"].ToString()),
                            Convert.ToInt32(Row["wall_height"])));
                }
            }
        }

        public void LoadModel(string Id)
        {
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "SELECT id,door_x,door_y,door_z,door_dir,heightmap,public_items,club_only,poolmap,`wall_height` FROM `room_models` WHERE `custom` = '1' AND `id` = '" +
                    Id + "' LIMIT 1");
                var Row = dbClient.getRow();

                if (Row == null)
                    return;

                var Modelname = Convert.ToString(Row["id"]);
                if (!_roomModels.Contains(Id))
                    _roomModels.Add(Modelname,
                        new RoomModel(Convert.ToInt32(Row["door_x"]), Convert.ToInt32(Row["door_y"]),
                            Convert.ToDouble(Row["door_z"]), Convert.ToInt32(Row["door_dir"]),
                            Convert.ToString(Row["heightmap"]), Convert.ToString(Row["public_items"]),
                            OblivionServer.EnumToBool(Row["club_only"].ToString()),
                            Convert.ToInt32(Row["wall_height"])));
            }
        }

        public void ReloadModel(string Id)
        {
            if (!_roomModels.Contains(Id))
            {
                LoadModel(Id);
                return;
            }

            _roomModels.Remove(Id);
            LoadModel(Id);
        }

        public bool TryGetModel(string Id, out RoomModel Model)
        {
            if (_roomModels.Contains(Id))
            {
                Model = (RoomModel) _roomModels[Id];
                return true;
            }
            Model = null;
            return false;
        }

        public void UnloadRoom(int roomId)
        {
            if (_rooms.TryRemove(roomId, out Room room))
            {
                room.Dispose();
            }
        }

        public void UnloadRoom(Room Room, bool RemoveData = false)
        {
            if (Room == null)
                return;

            Room room;
            if (_rooms.TryRemove(Room.RoomId, out room))
            {
                Room.Dispose();

                if (RemoveData)
                {
                    _loadedRoomData.Remove(Room.Id);
                }
            }
            //Logging.WriteLine("[RoomMgr] Unloaded room: \"" + Room.Name + "\" (ID: " + Room.RoomId + ")");
        }

        public List<RoomData> SearchGroupRooms(string Query)
        {
            var InstanceMatches =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
                where RoomInstance.UsersNow >= 0 &&
                      RoomInstance.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Group != null &&
                      (RoomInstance.OwnerName.StartsWith(Query) ||
                       RoomInstance.Tags.Contains(Query) ||
                       RoomInstance.Name.Contains(Query))
                orderby RoomInstance.UsersNow descending
                select RoomInstance).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> SearchTaggedRooms(string Query)
        {
            var InstanceMatches =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
             where RoomInstance.UsersNow >= 0 &&
                      RoomInstance.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Tags.Contains(Query)
                orderby RoomInstance.UsersNow descending
                select RoomInstance).Take(50);
            return InstanceMatches.ToList();
        }

        public List<RoomData> GetPopularRooms(int category, int Amount = 50)
        {
            var rooms =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
             where RoomInstance.UsersNow > 0 &&
                      (category == -1 || RoomInstance.Category == category) &&
                      RoomInstance.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Score descending
                orderby RoomInstance.UsersNow descending
                select RoomInstance).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRecommendedRooms(int Amount = 50, int CurrentRoomId = 0)
        {
            var Rooms =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
             where RoomInstance.UsersNow >= 0 &&
                      RoomInstance.Score >= 0 &&
                      RoomInstance.Access != RoomAccess.INVISIBLE &&
                      RoomInstance.Id != CurrentRoomId
                orderby RoomInstance.Score descending
                orderby RoomInstance.UsersNow descending
                select RoomInstance).Take(Amount);
            return Rooms.ToList();
        }

        public List<RoomData> GetPopularRatedRooms(int Amount = 50)
        {
            var rooms =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
             where RoomInstance.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Score descending
                select RoomInstance).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetRoomsByCategory(int Category, int Amount = 50)
        {
            var rooms =
            (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
             where RoomInstance.Category == Category &&
                      RoomInstance.UsersNow > 0 &&
                      RoomInstance.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.UsersNow descending
                select RoomInstance).Take(Amount);
            return rooms.ToList();
        }

        public List<RoomData> GetOnGoingRoomPromotions(int Mode, int Amount = 50)
        {
            IEnumerable<RoomData> Rooms = null;

            if (Mode == 17)
                Rooms =
                (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
                 where RoomInstance.HasActivePromotion &&
                          RoomInstance.Access != RoomAccess.INVISIBLE
                    orderby RoomInstance.Promotion.TimestampStarted descending
                    select RoomInstance).Take(Amount);
            else
                Rooms =
                (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
                 where RoomInstance.HasActivePromotion &&
                          RoomInstance.Access != RoomAccess.INVISIBLE
                    orderby RoomInstance.UsersNow descending
                    select RoomInstance).Take(Amount);

            return Rooms.ToList();
        }


        public List<RoomData> GetPromotedRooms(int CategoryId, int Amount = 50)
        {
            var Rooms = (from RoomInstance in _loadedRoomData.Values.Cast<RoomData>()
                         where RoomInstance.HasActivePromotion &&
                      RoomInstance.Promotion.CategoryId == CategoryId &&
                      RoomInstance.Access != RoomAccess.INVISIBLE
                orderby RoomInstance.Promotion.TimestampStarted descending
                select RoomInstance).Take(Amount);

            return Rooms.ToList();
        }

        public Room TryGetRandomLoadedRoom()
        {
            var room =
            (from RoomInstance in _rooms
                where RoomInstance.Value.RoomData.UsersNow > 0 &&
                      RoomInstance.Value.RoomData.Access == RoomAccess.OPEN &&
                      RoomInstance.Value.RoomData.UsersNow < RoomInstance.Value.RoomData.UsersMax
             orderby RoomInstance.Value.RoomData.UsersNow descending
                select RoomInstance.Value).Take(1);

            return room.Any() ? room.First() : null;
        }

        public RoomModel GetModel(string Model) => _roomModels.Contains(Model) ? (RoomModel) _roomModels[Model] : null;

        public RoomData GenerateRoomData(int RoomId)
        {
            if (_loadedRoomData.Contains(RoomId))
                return (RoomData) _loadedRoomData[RoomId];

            var Data = new RoomData();

            Room Room;

            if (TryGetRoom(RoomId, out Room))
                return Room.RoomData;

            DataRow Row;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM rooms WHERE id = " + RoomId + " LIMIT 1");
                Row = dbClient.getRow();
            }

            if (Row == null)
                return null;

            Data.Fill(Row);

            if (!_loadedRoomData.Contains(RoomId))
                _loadedRoomData.Add(RoomId, Data);

            return Data;
        }

        public RoomData FetchRoomData(int RoomId, DataRow dRow)
        {
            if (_loadedRoomData.Contains(RoomId))
                return (RoomData) _loadedRoomData[RoomId];
            var data = new RoomData();

            data.Fill(dRow);

            if (!_loadedRoomData.Contains(RoomId))
                _loadedRoomData.Add(RoomId, data);
            return data;
        }

        public Room LoadRoom(int Id)
        {
            Room Room;

            if (TryGetRoom(Id, out Room))
                return Room;

            var Data = GenerateRoomData(Id);
            if (Data == null)
                return null;

            Room = new Room(Data);

            if (!_rooms.ContainsKey(Room.RoomId))
                _rooms.TryAdd(Room.RoomId, Room);

            return Room;
        }

        public bool TryGetRoom(int RoomId, out Room Room) => _rooms.TryGetValue(RoomId, out Room);

        public RoomData CreateRoom(GameClient Session, string Name, string Description, string Model, int Category,
            int MaxVisitors, int TradeSettings)
        {
            if (!_roomModels.Contains(Model))
            {
                Session.SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("room_model_missing"));
                return null;
            }

            if (Name.Length < 3)
            {
                Session.SendNotification(
                    OblivionServer.GetGame().GetLanguageLocale().TryGetValue("room_name_length_short"));
                return null;
            }

            int RoomId;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "INSERT INTO `rooms` (`roomtype`,`caption`,`description`,`owner`,`model_name`,`category`,`users_max`,`trade_settings`) VALUES ('private',@caption,@description,@UserId,@model,@category,@usersmax,@tradesettings)");
                dbClient.AddParameter("caption", Name);
                dbClient.AddParameter("description", Description);
                dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                dbClient.AddParameter("model", Model);
                dbClient.AddParameter("category", Category);
                dbClient.AddParameter("usersmax", MaxVisitors);
                dbClient.AddParameter("tradesettings", TradeSettings);

                RoomId = Convert.ToInt32(dbClient.InsertQuery());
            }

            var newRoomData = GenerateRoomData(RoomId);
            Session.GetHabbo().UsersRooms.Add(newRoomData);
            return newRoomData;
        }

        public ICollection<Room> GetRooms() => _rooms.Values;

        public void Dispose()
        {
            var length = _rooms.Count;
            var i = 0;
            foreach (var Room in _rooms.Values.ToList().Where(Room => Room != null))
            {
                OblivionServer.GetGame().GetRoomManager().UnloadRoom(Room);
                Console.Clear();
                Log.Info("<<- SERVER SHUTDOWN ->> ROOM ITEM SAVE: " + $"{(double) i / length * 100:0.##}" +
                         "%");
                i++;
            }
            Log.Info("Done disposing rooms!");
        }
    }
}