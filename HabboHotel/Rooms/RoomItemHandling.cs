#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Data.Moodlight;
using Oblivion.HabboHotel.Items.Data.Toner;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class RoomItemHandling
    {
        private readonly List<int> _rollerItemsMoved, _rollerUsersMoved;
        private readonly List<ServerPacket> _rollerMessages;

        private readonly ConcurrentDictionary<int, Item> _rollers;
        private ConcurrentDictionary<int, Item> _floorItems;

        private ConcurrentDictionary<int, Item> _movedItems;
        private int _mRollerCycle;
        private double _mRollerSpeed;
        private Room _room;

        private ConcurrentQueue<Item> _roomItemUpdateQueue;
        private ConcurrentDictionary<int, Item> _wallItems;

        public int HopperCount;
        public bool Usedwiredscorebord;

        public RoomItemHandling(Room room)
        {
            _room = room;

            HopperCount = 0;
            GotRollers = false;
            _mRollerSpeed = 1.35;
            _mRollerCycle = 0;

            _movedItems = new ConcurrentDictionary<int, Item>();

            _rollers = new ConcurrentDictionary<int, Item>();
            _wallItems = new ConcurrentDictionary<int, Item>();
            _floorItems = new ConcurrentDictionary<int, Item>();

            _rollerItemsMoved = new List<int>();
            _rollerUsersMoved = new List<int>();
            _rollerMessages = new List<ServerPacket>();

            _roomItemUpdateQueue = new ConcurrentQueue<Item>();
            Usedwiredscorebord = false;
        }

        public void UpdateWiredScoreBord()
        {
            var messages = (from scoreitem in _floorItems.Values
                where scoreitem.GetBaseItem().InteractionType == InteractionType.WiredScoreBoard
                select new ObjectUpdateComposer(scoreitem, _room.OwnerId)).Cast<ServerPacket>().ToList();
            _room.SendMessage(messages);
        }

        internal void ScorebordChangeCheck()
        {
            if (_room.WiredScoreFirstBordInformation.Count == 3)
            {
                var now = DateTime.Now;
                var getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                var getmonthtoday = Convert.ToInt32(DateTime.Now.ToString("MM"));
                var getweektoday = CultureInfo.GetCultureInfo("Nl-nl")
                    .Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);

                var SuperCheck = new List<bool>
                {
                    getdaytoday != _room.WiredScoreFirstBordInformation[0],
                    getmonthtoday != _room.WiredScoreFirstBordInformation[1],
                    getweektoday != _room.WiredScoreFirstBordInformation[2]
                };

                _room.WiredScoreFirstBordInformation[0] = getdaytoday;
                _room.WiredScoreFirstBordInformation[1] = getmonthtoday;
                _room.WiredScoreFirstBordInformation[2] = getweektoday;

                if (SuperCheck[0])
                    _room.WiredScoreBordDay.Clear();

                if (SuperCheck[1])
                    _room.WiredScoreBordMonth.Clear();

                if (SuperCheck[2])
                    _room.WiredScoreBordWeek.Clear();
            }
        }

        public bool GotRollers { get; set; }

        public ICollection<Item> GetFloor => _floorItems.Values;

        public ICollection<Item> GetWall => _wallItems.Values;

        public IEnumerable<Item> GetWallAndFloor => _floorItems.Values.Concat(_wallItems.Values);

        public void TryAddRoller(int itemId, Item roller) => _rollers.TryAdd(itemId, roller);

        public void QueueRoomItemUpdate(Item item) => _roomItemUpdateQueue.Enqueue(item);

        public void SetSpeed(double p) => _mRollerSpeed = p;

        public string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                    return null;
                if (wallPosition.Contains(Convert.ToChar(9)))
                    return null;

                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;

                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);
                if (widthX < -1000 || widthY < -1 || widthX > 700 || widthY > 700)
                    return null;

                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);
                if (lengthX < -1 || lengthY < -1000 || lengthX > 700 || lengthY > 700)
                    return null;


                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }

        public void LoadFurniture()
        {
            _floorItems.Clear();
            _wallItems.Clear();

            var items = ItemLoader.GetItemsForRoom(_room.Id, _room);
            foreach (var item in items.ToList().Where(item => item != null))
            {
                if (item.UserID == 0)
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery("UPDATE `items` SET `user_id` = @UserId WHERE `id` = @ItemId LIMIT 1");
                        dbClient.AddParameter("ItemId", item.UserID);
                        dbClient.AddParameter("UserId", _room.OwnerId);
                        dbClient.RunQuery();
                    }

                if (item.IsFloorItem)
                {
                    if (!_room.GetGameMap().ValidTile(item.GetX, item.GetY))
                    {
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE `items` SET `room_id` = '0' WHERE `id` = '" + item.Id +
                                              "' LIMIT 1");
                        }

                        var client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(item.UserID);
                        client?.GetHabbo()
                            .GetInventoryComponent()
                            .AddNewItem(item.Id, item.BaseItem, item.ExtraData, item.GroupId, true, true,
                                item.LimitedNo, item.LimitedTot);
                        client?.GetHabbo().GetInventoryComponent().UpdateItems(false);
                        continue;
                    }

                    if (!_floorItems.ContainsKey(item.Id))
                        _floorItems.TryAdd(item.Id, item);
                }
                else if (item.IsWallItem)
                {
                    if (string.IsNullOrWhiteSpace(item.wallCoord))
                    {
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" +
                                              item.Id +
                                              "' LIMIT 1");
                            dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                            dbClient.RunQuery();
                        }

                        item.wallCoord = ":w=0,2 l=11,53 l";
                    }

                    try
                    {
                        item.wallCoord = WallPositionCheck(":" + item.wallCoord.Split(':')[1]);
                    }
                    catch
                    {
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @WallPosition WHERE `id` = '" +
                                              item.Id +
                                              "' LIMIT 1");
                            dbClient.AddParameter("WallPosition", ":w=0,2 l=11,53 l");
                            dbClient.RunQuery();
                        }

                        item.wallCoord = ":w=0,2 l=11,53 l";
                    }

                    if (!_wallItems.ContainsKey(item.Id))
                        _wallItems.TryAdd(item.Id, item);
                }
            }

            foreach (var item in _floorItems.Values.ToList())
                if (item.IsRoller)
                {
                    GotRollers = true;
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                {
                    if (_room.MoodlightData == null)
                        _room.MoodlightData = new MoodlightData(item.Id);
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Toner)
                {
                    if (_room.TonerData == null)
                        _room.TonerData = new TonerData(item.Id);
                }
                else if (item.IsWired)
                {
                    if (_room?.GetWired() == null)
                        continue;

                    _room.GetWired().LoadWiredBox(item);
                }
                else if (item.GetBaseItem().InteractionType == InteractionType.Hopper)
                {
                    HopperCount++;
                }
        }

        public Item GetItem(int pId)
        {
            if (_floorItems?.ContainsKey(pId) == true)
            {
                Item item;
                if (_floorItems.TryGetValue(pId, out item))
                    return item;
            }
            else if (_wallItems?.ContainsKey(pId) == true)
            {
                Item item;
                if (_wallItems.TryGetValue(pId, out item))
                    return item;
            }

            return null;
        }

        public void RemoveFurniture(GameClient session, int pId, bool wasPicked = true)
        {
            var item = GetItem(pId);
            if (item == null)
                return;

            if (item.GetBaseItem().InteractionType == InteractionType.FootballGate)
                _room.GetSoccer().UnRegisterGate(item);

            if (item.GetBaseItem().InteractionType != InteractionType.Gift)
                item.Interactor.OnRemove(session, item);

            if (item.GetBaseItem().InteractionType == InteractionType.GuildGate)
            {
                item.UpdateCounter = 0;
                item.UpdateNeeded = false;
            }

            RemoveRoomItem(item);
        }

        private void RemoveRoomItem(Item item)
        {
            if (item.IsFloorItem)
                _room.SendMessage(new ObjectRemoveComposer(item, item.UserID));
            else if (item.IsWallItem)
                _room.SendMessage(new ItemRemoveComposer(item, item.UserID));

            //TODO: Recode this specific part
            if (item.IsWallItem)
            {
                _wallItems.TryRemove(item.Id, out item);
            }
            else
            {
                _floorItems.TryRemove(item.Id, out item);
                //mFloorItems.OnCycle();
                _room.GetGameMap().RemoveFromMap(item);
            }

            RemoveItem(item);
            _room.GetGameMap().GenerateMaps();
            _room.GetRoomUserManager().UpdateUserStatusses();
        }

        private List<ServerPacket> CycleRollers()
        {
            if (!GotRollers)
                return new List<ServerPacket>();

            if (_mRollerCycle >= _mRollerSpeed || _mRollerSpeed == 0)
            {
                _rollerItemsMoved.Clear();
                _rollerUsersMoved.Clear();
                _rollerMessages.Clear();

                foreach (var roller in _rollers.Values.ToList())
                {
                    if (roller == null)
                        continue;

                    var nextSquare = roller.SquareInFront;

                    var itemsOnRoller = _room.GetGameMap().GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ);
                    var itemsOnNext = _room.GetGameMap().GetAllRoomItemForSquare(nextSquare.X, nextSquare.Y).ToList();

                    if (itemsOnRoller.Count > 10)
                        itemsOnRoller =
                            _room.GetGameMap()
                                .GetRoomItemForSquare(roller.GetX, roller.GetY, roller.GetZ)
                                .Take(10)
                                .ToList();

                    var nextSquareIsRoller =
                        itemsOnNext.Count(x => x.GetBaseItem().InteractionType == InteractionType.Roller) > 0;
                    var nextRollerClear = true;

                    var nextZ = 0.0;
                    var nextRoller = false;

                    foreach (var item in itemsOnNext.ToList().Where(item => item.IsRoller))
                    {
                        if (item.TotalHeight > nextZ)
                            nextZ = item.TotalHeight;

                        nextRoller = true;
                    }

                    if (nextRoller)
                        foreach (var item in itemsOnNext.ToList().Where(item => item.TotalHeight > nextZ))
                            nextRollerClear = false;

                    if (itemsOnRoller.Count > 0)
                        foreach (
                            var rItem in
                            itemsOnRoller.ToList()
                                .Where(rItem => rItem != null)
                                .Where(rItem => !_rollerItemsMoved.Contains(rItem.Id) &&
                                                _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) &&
                                                nextRollerClear &&
                                                roller.GetZ < rItem.GetZ &&
                                                _room.GetRoomUserManager().GetUserForSquare(nextSquare.X, nextSquare.Y) ==
                                                null))
                        {
                            if (!nextSquareIsRoller)
                                nextZ = rItem.GetZ - roller.GetBaseItem().Height;
                            else
                                nextZ = rItem.GetZ;

                            _rollerMessages.Add(UpdateItemOnRoller(rItem, nextSquare, roller.Id, nextZ));
                            _rollerItemsMoved.Add(rItem.Id);
                        }

                    var rollerUser = _room.GetGameMap().GetRoomUsers(roller.Coordinate).FirstOrDefault();

                    if (!rollerUser?.IsWalking == true && nextRollerClear && _room.GetGameMap()
                            .IsValidStep(rollerUser, new Vector2D(roller.GetX, roller.GetY),
                                new Vector2D(nextSquare.X, nextSquare.Y), true, false, true) &&
                        _room.GetGameMap().CanRollItemHere(nextSquare.X, nextSquare.Y) &&
                        _room.GetGameMap().GetFloorStatus(nextSquare) != 0)
                        if (rollerUser != null && !_rollerUsersMoved.Contains(rollerUser.HabboId))
                        {
                            if (!nextSquareIsRoller)
                                nextZ = rollerUser.Z - roller.GetBaseItem().Height;
                            else
                                nextZ = rollerUser.Z;

                            rollerUser.isRolling = true;
                            rollerUser.rollerDelay = 1;

                            _rollerMessages.Add(UpdateUserOnRoller(rollerUser, nextSquare, roller.Id, nextZ));
                            _rollerUsersMoved.Add(rollerUser.HabboId);
                        }
                }

                _mRollerCycle = 0;
                return _rollerMessages;
            }
            _mRollerCycle++;

            return new List<ServerPacket>();
        }

        public ServerPacket UpdateItemOnRoller(Item pItem, Point nextCoord, int pRolledId, double nextZ)
        {
            var mMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
            mMessage.WriteInteger(pItem.GetX);
            mMessage.WriteInteger(pItem.GetY);

            mMessage.WriteInteger(nextCoord.X);
            mMessage.WriteInteger(nextCoord.Y);

            mMessage.WriteInteger(1);

            mMessage.WriteInteger(pItem.Id);

            mMessage.WriteString(TextHandling.GetString(pItem.GetZ));
            mMessage.WriteString(TextHandling.GetString(nextZ));

            mMessage.WriteInteger(pRolledId);

            SetFloorItem(pItem, nextCoord.X, nextCoord.Y, nextZ);

            return mMessage;
        }

        public ServerPacket UpdateUserOnRoller(RoomUser pUser, Point pNextCoord, int pRollerId, double nextZ)
        {
            var mMessage = new ServerPacket(ServerPacketHeader.SlideObjectBundleMessageComposer);
            mMessage.WriteInteger(pUser.X);
            mMessage.WriteInteger(pUser.Y);

            mMessage.WriteInteger(pNextCoord.X);
            mMessage.WriteInteger(pNextCoord.Y);

            mMessage.WriteInteger(0);
            mMessage.WriteInteger(pRollerId);
            mMessage.WriteInteger(2);
            mMessage.WriteInteger(pUser.VirtualId);
            mMessage.WriteString(TextHandling.GetString(pUser.Z));
            mMessage.WriteString(TextHandling.GetString(nextZ));

            _room.GetGameMap()
                .UpdateUserMovement(new Point(pUser.X, pUser.Y), new Point(pNextCoord.X, pNextCoord.Y), pUser);
            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 1;
            pUser.X = pNextCoord.X;
            pUser.Y = pNextCoord.Y;
            pUser.Z = nextZ;

            _room.GetGameMap().GameMap[pUser.X, pUser.Y] = 0;

            if (pUser.GetClient() != null && pUser.GetClient().GetHabbo() != null)
            {
                var items = _room.GetGameMap().GetRoomItemForSquare(pNextCoord.X, pNextCoord.Y);
                foreach (var IItem in items.ToList().Where(IItem => IItem != null))
                    _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, pUser.GetClient().GetHabbo(), IItem);

                var item = _room.GetRoomItemHandler().GetItem(pRollerId);
                if (item != null)
                    _room.GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, pUser.GetClient().GetHabbo(), item);
            }

            return mMessage;
        }

        public void SaveFurniture()
        {
            try
            {
                if (_movedItems.Count > 0)
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        foreach (var item in _movedItems.Values.ToList())
                        {
                            if (!string.IsNullOrEmpty(item.ExtraData))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `extra_data` = @edata" + item.Id +
                                                  " WHERE `id` = '" + item.Id + "' LIMIT 1");
                                dbClient.AddParameter("edata" + item.Id, item.ExtraData);
                                dbClient.RunQuery();
                            }

                            if (item.IsWallItem &&
                                (!item.GetBaseItem().ItemName.Contains("wallpaper_single") ||
                                 !item.GetBaseItem().ItemName.Contains("floor_single") ||
                                 !item.GetBaseItem().ItemName.Contains("landscape_single")))
                            {
                                dbClient.SetQuery("UPDATE `items` SET `wall_pos` = @wallPos WHERE `id` = '" + item.Id +
                                                  "' LIMIT 1");
                                dbClient.AddParameter("wallPos", item.wallCoord);
                                dbClient.RunQuery();
                            }

                            dbClient.RunQuery("UPDATE `items` SET `x` = '" + item.GetX + "', `y` = '" + item.GetY +
                                              "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation +
                                              "' WHERE `id` = '" + item.Id + "' LIMIT 1");
                        }
                        var now = DateTime.Now;
                        var getdaytoday = Convert.ToInt32(now.ToString("MMddyyyy"));
                        var getmonthtoday = Convert.ToInt32(now.ToString("MM"));
                        var getweektoday = CultureInfo.GetCultureInfo("Nl-nl")
                            .Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);


                        if (Usedwiredscorebord)
                        {
                            dbClient.RunQuery("DELETE FROM `wired_scorebord` WHERE roomid = " + _room.RoomId);

                            KeyValuePair<int, string> data;
                            foreach (
                                var mdayuserids in
                                _room.WiredScoreBordDay.Keys.Where(
                                    mdayuserids => _room.WiredScoreBordDay.ContainsKey(mdayuserids)))
                            {
                                data = _room.WiredScoreBordDay[mdayuserids];

                                dbClient.SetQuery(
                                    "INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`) VALUES ('" +
                                    _room.RoomId + "', '" + mdayuserids + "', @dusername" + mdayuserids + ", '" +
                                    data.Key + "', 'day', '" + getdaytoday + "')");
                                dbClient.AddParameter(string.Concat("dusername", mdayuserids), data.Value);
                                dbClient.RunQuery();
                            }
                            foreach (
                                var mmonthuserids in
                                _room.WiredScoreBordMonth.Keys.Where(
                                    mmonthuserids => _room.WiredScoreBordMonth.ContainsKey(mmonthuserids)))
                            {
                                data = _room.WiredScoreBordMonth[mmonthuserids];

                                dbClient.SetQuery(
                                    "INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`) VALUES ('" +
                                    _room.RoomId + "', '" + mmonthuserids + "', @musername" + mmonthuserids + ", '" +
                                    data.Key + "', 'month', '" + getmonthtoday + "')");
                                dbClient.AddParameter(string.Concat("musername", mmonthuserids), data.Value);
                                dbClient.RunQuery();
                            }
                            foreach (
                                var weekuserids in
                                _room.WiredScoreBordWeek.Keys.Where(
                                    weekuserids => _room.WiredScoreBordDay.ContainsKey(weekuserids)))
                            {
                                data = _room.WiredScoreBordDay[weekuserids];

                                dbClient.SetQuery(
                                    "INSERT INTO `wired_scorebord` (`roomid`, `userid`, `username`, `punten`, `soort`, `timestamp`) VALUES ('" +
                                    _room.RoomId + "', '" + weekuserids + "', @wusername" + weekuserids + ", '" +
                                    data.Key + "', 'week', '" + getweektoday + "')");
                                dbClient.AddParameter(string.Concat("wusername", weekuserids), data.Value);
                                dbClient.RunQuery();
                            }
                        }
                    }
                Usedwiredscorebord = false;
            }
            catch (Exception e)
            {
                Logging.LogCriticalException("Error during saving furniture for room " + _room.RoomId + ". Stack: " + e);
            }
        }

        public bool SetFloorItem(GameClient session, Item item, int newX, int newY, int newRot, bool newItem,
            bool onRoller, bool sendMessage, bool updateRoomUserStatuses = false)
        {
            var needsReAdd = false;

            if (newItem)
                if (item.IsWired)
                    if (item.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps &&
                        _room.GetRoomItemHandler()
                            .GetFloor.Count(x => x.GetBaseItem().WiredType == WiredBoxType.EffectRegenerateMaps) > 0)
                        return false;

            var itemsOnTile = GetFurniObjects(newX, newY);
            if (item.GetBaseItem().InteractionType == InteractionType.Roller &&
                itemsOnTile.Count(
                    x => x.GetBaseItem().InteractionType == InteractionType.Roller && x.Id != item.Id && item.GetZ > 0.2) >
                0)
                return false;

            if (!newItem)
                needsReAdd = _room.GetGameMap().RemoveFromMap(item);

            var affectedTiles = Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY,
                newRot);

            if (!_room.GetGameMap().ValidTile(newX, newY) ||
                _room.GetGameMap().SquareHasUsers(newX, newY) && !item.GetBaseItem().IsSeat)
            {
                if (needsReAdd)
                    _room.GetGameMap().AddToMap(item);
                return false;
            }

            if (affectedTiles.Values.Any(tile => !_room.GetGameMap().ValidTile(tile.X, tile.Y) ||
                                                 _room.GetGameMap().SquareHasUsers(tile.X, tile.Y) &&
                                                 !item.GetBaseItem().IsSeat))
            {
                if (needsReAdd)
                    _room.GetGameMap().AddToMap(item);
                return false;
            }

            // Start calculating new Z coordinate
            double newZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];

            if (!onRoller)
            {
                // Make sure this tile is open and there are no users here
                if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN && !item.GetBaseItem().IsSeat)
                    return false;

                if (
                    affectedTiles.Values.Any(
                        tile => _room.GetGameMap().Model.SqState[tile.X, tile.Y] != SquareState.OPEN &&
                                !item.GetBaseItem().IsSeat))
                {
                    if (needsReAdd)
                        _room.GetGameMap().AddToMap(item);
                    return false;
                }

                // And that we have no users
                if (!item.GetBaseItem().IsSeat && !item.IsRoller)
                    if (
                        affectedTiles.Values.Any(
                            tile => _room.GetGameMap().GetRoomUsers(new Point(tile.X, tile.Y)).Count > 0))
                    {
                        if (needsReAdd)
                            _room.GetGameMap().AddToMap(item);
                        return false;
                    }
            }

            // Find affected objects
            var itemsAffected = new List<Item>();
            var itemsComplete = new List<Item>();

            foreach (
                var temp in
                affectedTiles.Values.ToList()
                    .Select(tile => GetFurniObjects(tile.X, tile.Y))
                    .Where(temp => temp != null))
                itemsAffected.AddRange(temp);


            itemsComplete.AddRange(itemsOnTile);
            itemsComplete.AddRange(itemsAffected);

            if (!onRoller && item.GetBaseItem().InteractionType != InteractionType.Stacktool)
                if (
                    itemsComplete.Any(
                        current5 =>
                            current5 != null && current5.Id != item.Id && current5.GetBaseItem() != null &&
                            !current5.GetBaseItem().Stackable))
                {
                    if (needsReAdd)
                        _room.GetGameMap().AddToMap(item);
                    return false;
                }


            // If this is a rotating action, maintain item at current height
            if (item.Rotation != newRot && item.GetX == newX && item.GetY == newY)
                newZ = item.GetZ;

            foreach (var I in itemsComplete.ToList().Where(I => I?.Id != item.Id))
            {
                if (I?.GetBaseItem() == null)
                    continue;

                if (!I.GetBaseItem().Stackable)
                {
                    if (needsReAdd)
                    {
                        _room.GetGameMap().AddToMap(item);
                    }
                    return false;
                }
                if (I.GetBaseItem().InteractionType == InteractionType.Stacktool)
                {
                    newZ = I.GetZ;
                    break;
                }

                if (I.GetZ < 0)
                    newZ = 0;

                if (I.TotalHeight > newZ)
                    newZ = I.TotalHeight;
            }


            // Verify the rotation is correct
            if (newRot != 0 && newRot != 2 && newRot != 4 && newRot != 6 && newRot != 8 &&
                !item.GetBaseItem().ExtraRot)
                newRot = 0;

            item.Rotation = newRot;
            //var oldX = Item.GetX;
            //var oldY = Item.GetY;
            item.SetState(newX, newY, newZ, affectedTiles);

            if (!onRoller && session != null)
                item.Interactor.OnPlace(session, item);


            if (newItem)
            {
                if (_floorItems.ContainsKey(item.Id))
                {
                    session?.SendNotification(OblivionServer.GetGame()
                        .GetLanguageLocale()
                        .TryGetValue("room_item_placed"));
                    _room.GetGameMap().RemoveFromMap(item);
                    return true;
                }

                if (item.IsFloorItem && !_floorItems.ContainsKey(item.Id))
                    _floorItems.TryAdd(item.Id, item);
                else if (item.IsWallItem && !_wallItems.ContainsKey(item.Id))
                    _wallItems.TryAdd(item.Id, item);

                if (sendMessage)
                    _room.SendMessage(new ObjectAddComposer(item));
            }
            else
            {
                UpdateItem(item);
                if (!onRoller && sendMessage)
                    _room.SendMessage(new ObjectUpdateComposer(item, _room.OwnerId));
            }
            _room.GetGameMap().AddToMap(item);

            if (item.GetBaseItem().IsSeat)
                updateRoomUserStatuses = true;

            if (updateRoomUserStatuses)
                _room.GetRoomUserManager().UpdateUserStatusses();

            if (item.GetBaseItem().InteractionType == InteractionType.Tent ||
                item.GetBaseItem().InteractionType == InteractionType.TentSmall)
            {
                _room.RemoveTent(item.Id, item);
                _room.AddTent(item.Id);
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX +
                                  "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation +
                                  "' WHERE `id` = '" + item.Id + "' LIMIT 1");
            }
            return true;
        }

        public List<Item> GetFurniObjects(int x, int y) => _room.GetGameMap().GetCoordinatedItems(new Point(x, y));

        public bool SetFloorItem(Item item, int newX, int newY, double newZ)
        {
            if (_room == null)
                return false;

            _room.GetGameMap().RemoveFromMap(item);

            item.SetState(newX, newY, newZ,
                Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY, item.Rotation));
            if (item.GetBaseItem().InteractionType == InteractionType.Toner)
                if (_room.TonerData == null)
                    _room.TonerData = new TonerData(item.Id);
            UpdateItem(item);
            _room.GetGameMap().AddItemToMap(item);
            return true;
        }

        public bool SetWallItem(GameClient session, Item item)
        {
            if (!item.IsWallItem || _wallItems.ContainsKey(item.Id))
                return false;

            if (_floorItems.ContainsKey(item.Id))
            {
                session.SendNotification(OblivionServer.GetGame().GetLanguageLocale().TryGetValue("room_item_placed"));
                return true;
            }


            item.Interactor.OnPlace(session, item);
            if (item.GetBaseItem().InteractionType == InteractionType.Moodlight)
                if (_room.MoodlightData != null)
                {
                    _room.MoodlightData = new MoodlightData(item.Id);
                    item.ExtraData = _room.MoodlightData.GenerateExtraData();
                }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `items` SET `room_id` = '" + _room.RoomId + "', `x` = '" + item.GetX +
                                  "', `y` = '" + item.GetY + "', `z` = '" + item.GetZ + "', `rot` = '" + item.Rotation +
                                  "', `wall_pos` = @WallPos WHERE `id` = '" + item.Id + "' LIMIT 1");
                dbClient.AddParameter("WallPos", item.wallCoord);
                dbClient.RunQuery();
            }

            _wallItems.TryAdd(item.Id, item);

            _room.SendMessage(new ItemAddComposer(item));

            return true;
        }

        public void UpdateItem(Item item)
        {
            if (item == null)
                return;
            if (!_movedItems.ContainsKey(item.Id))
                _movedItems.TryAdd(item.Id, item);
        }


        public void RemoveItem(Item item)
        {
            if (item == null)
                return;

            if (_movedItems.ContainsKey(item.Id))
                _movedItems.TryRemove(item.Id, out item);
            if (_rollers.ContainsKey(item.Id))
                _rollers.TryRemove(item.Id, out item);
        }

        public void OnCycle()
        {
            if (GotRollers)
                try
                {
                    _room.SendMessage(CycleRollers());
                }
                catch //(Exception e)
                {
                    // Logging.LogThreadException(e.ToString(), "rollers for room with ID " + room.RoomId);
                    GotRollers = false;
                }

            if (_roomItemUpdateQueue.Count > 0)
            {
                var addItems = new List<Item>();
                while (_roomItemUpdateQueue.Count > 0)
                {
                    Item item;
                    if (_roomItemUpdateQueue.TryDequeue(out item))
                    {
                        item.ProcessUpdates();

                        if (item.UpdateCounter > 0)
                            addItems.Add(item);
                    }
                }

                foreach (var item in addItems.ToList().Where(item => item != null))
                    _roomItemUpdateQueue.Enqueue(item);
            }
        }

        internal List<Item> RemoveAllFurniture(GameClient session)
        {
            var items = new List<Item>();
            var roomGamemap = _room.GetGameMap();

            foreach (var item in _floorItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);

                roomGamemap.RemoveSpecialItem(item);
                _room.SendMessage(new ObjectRemoveComposer(item, item.UserID));

                items.Add(item);
            }

            foreach (var item in _wallItems.Values.ToArray())
            {
                item.Interactor.OnRemove(session, item);

                _room.SendMessage(new ItemRemoveComposer(item, item.UserID));


                items.Add(item);
            }

            ClearItems();

            using (var queryReactor = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                queryReactor.RunQuery($"UPDATE items SET room_id='0' WHERE room_id='{_room.RoomId}'");
            }
            _room.GetGameMap().GenerateMaps();
            _room.GetRoomUserManager().SerializeStatusUpdates();
            return items;
        }

        /// <summary>
        ///     Remove Items On Room GroupBy Username
        /// </summary>
        /// <param name="roomItemList"></param>
        /// <param name="session"></param>
        internal void RemoveItemsByOwner(ref List<Item> roomItemList, ref GameClient session)
        {
            var toUpdate = new List<GameClient>();

            foreach (var item in roomItemList)
            {
                if (item == null)
                    return;

                if (item.UserID == 0)
                    item.UserID = session.GetHabbo().Id;

                var client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(item.UserID);

                if (item.GetBaseItem().InteractionType != InteractionType.Postit)
                {
                    if (!toUpdate.Contains(client))
                        toUpdate.Add(client);

                    if (client == null)
                        using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                        {
                            dbClient.RunQuery("UPDATE items SET room_id = '0' WHERE id = " + item.Id);
                        }
                    else
                        client.GetHabbo().GetInventoryComponent().AddItem(item);
                }
            }

            foreach (var client in toUpdate)
                client?.GetHabbo().GetInventoryComponent().UpdateItems(true);
            toUpdate.Clear();
        }

        public bool CheckPosItem(GameClient session, Item Item, int newX, int newY, int newRot, bool newItem,
            bool sendNotify = true)
        {
            try
            {
                var dictionary = Gamemap.GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, newX,
                    newY, newRot);
                if (!_room.GetGameMap().ValidTile(newX, newY))
                    return false;


                if (
                    dictionary.Values.ToList()
                        .Any(
                            coord =>
                                _room.GetGameMap().Model.DoorX == coord.X &&
                                _room.GetGameMap().Model.DoorY == coord.Y))
                    return false;

                if (_room.GetGameMap().Model.DoorX == newX && _room.GetGameMap().Model.DoorY == newY)
                    return false;


                if (dictionary.Values.ToList().Any(coord => !_room.GetGameMap().ValidTile(coord.X, coord.Y)))
                    return false;

                double num = _room.GetGameMap().Model.SqFloorHeight[newX, newY];
                if (Item.Rotation == newRot && Item.GetX == newX && Item.GetY == newY && Item.GetZ != num)
                    return false;

                if (_room.GetGameMap().Model.SqState[newX, newY] != SquareState.OPEN)
                    return false;

                if (
                    dictionary.Values.ToList()
                        .Any(coord => _room.GetGameMap().Model.SqState[coord.X, coord.Y] != SquareState.OPEN))
                    return false;
                if (!Item.GetBaseItem().IsSeat)
                {
                    if (_room.GetGameMap().SquareHasUsers(newX, newY))
                        return false;

                    if (dictionary.Values.ToList().Any(coord => _room.GetGameMap().SquareHasUsers(coord.X, coord.Y)))
                        return false;
                }

                var furniObjects = GetFurniObjects(newX, newY);
                var collection = new List<Item>();
                var list3 = new List<Item>();
                foreach (
                    var list4 in
                    dictionary.Values.ToList()
                        .Select(coord => GetFurniObjects(coord.X, coord.Y))
                        .Where(list4 => list4 != null))
                    collection.AddRange(list4);

                if (furniObjects == null)
                    furniObjects = new List<Item>();

                list3.AddRange(furniObjects);
                list3.AddRange(collection);
                return list3.ToList().All(item => item.Id == Item.Id || item.GetBaseItem().Stackable);
            }
            catch
            {
                return false;
            }
        }


        public ICollection<Item> GetRollers() => _rollers.Values;

        public void Dispose()
        {
            foreach (var item in GetWallAndFloor.ToList())
                item?.Destroy();

            ClearItems();
            _room = null;
            _roomItemUpdateQueue = null;
            _floorItems = null;
            _movedItems = null;
            _wallItems = null;
        }

        private void ClearItems()
        {
            _floorItems.Clear();
            _wallItems.Clear();
            _movedItems.Clear();
            _rollers.Clear();

//            _rollers = null;
        }
    }
}