#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Core;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Rooms.Games.Teams;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class Gamemap
    {
        private Room _room;
        public bool DiagonalEnabled;

        private ConcurrentDictionary<Point, List<int>> mCoordinatedItems;
        private double[,] mItemHeightMap;
        private HybridDictionary userMap;

        public Gamemap(Room room)
        {
            _room = room;
            DiagonalEnabled = true;

            StaticModel = OblivionServer.GetGame().GetRoomManager().GetModel(room.ModelName);
            if (StaticModel == null)
            {
                OblivionServer.GetGame().GetRoomManager().LoadModel(room.ModelName);
                StaticModel = OblivionServer.GetGame().GetRoomManager().GetModel(room.ModelName);
            }

            if (StaticModel == null)
                return;

            Model = new DynamicRoomModel(StaticModel);

            mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();


//            gotPublicPool = room.RoomData.Model.gotPublicPool;
            GameMap = new byte[Model.MapSizeX, Model.MapSizeY];
            mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];

            userMap = new HybridDictionary();
        }

        public DynamicRoomModel Model { get; private set; }

        public RoomModel StaticModel { get; private set; }

        public byte[,] EffectMap { get; private set; }

        public byte[,] GameMap { get; private set; }

        public void AddUserToMap(RoomUser user, Point coord)
        {
            if (userMap.Contains(coord))
            {
                var usr = (List<RoomUser>) userMap[coord];
                usr.Add(user);
            }
            else
            {
                var users = new List<RoomUser> {user};
                userMap.Add(coord, users);
            }
        }

        public bool GetHighestItemForSquare(Point Square, out Item Item)
        {
            var Items = GetAllRoomItemForSquare(Square.X, Square.Y);
            Item = null;
            double HighestZ = -1;
            if (Items != null && Items.Any())
                foreach (var uItem in Items.ToList())
                {
                    if (uItem?.TotalHeight > HighestZ)
                    {
                        HighestZ = uItem.TotalHeight;
                        Item = uItem;
                    }
                }
            else
                return false;
            return true;
        }

        public void TeleportToItem(RoomUser user, Item item)
        {
            if (item == null || user == null)
                return;

            GameMap[user.X, user.Y] = user.SqState;
            UpdateUserMovement(new Point(user.Coordinate.X, user.Coordinate.Y),
                new Point(item.Coordinate.X, item.Coordinate.Y), user);
            user.X = item.GetX;
            user.Y = item.GetY;
            user.Z = item.GetZ;

            user.SqState = GameMap[item.GetX, item.GetY];
            GameMap[user.X, user.Y] = 1;
            user.RotBody = item.Rotation;
            user.RotHead = item.Rotation;

            user.GoalX = user.X;
            user.GoalY = user.Y;
            user.SetStep = false;
            user.IsWalking = false;
            user.UpdateNeeded = true;
        }

        public bool Walkingtofurni(RoomUser user, Item item)
        {
            if (user == null || item == null || !user.IsWalking)
                return false;

            if (user.X == item.GetX && user.Y == item.GetY)
                return true;

            if (user.Coordinate.X - 1 == item.GetX || user.Coordinate.Y - 1 == item.GetY ||
                user.Coordinate.X + 1 == item.GetX || user.Coordinate.Y + 1 == item.GetY)
                return true;

            if (user.GoalX == item.GetX && user.GoalY == item.GetY)
                return true;

            return Enumerable.Range(user.GoalX, user.GoalX + 10).Contains(item.GetX) ||
                   Enumerable.Range(user.GoalY, user.GoalY + 10).Contains(item.GetY) ||
                   Enumerable.Range(user.GoalX, user.GoalX - 10).Contains(item.GetX) ||
                   Enumerable.Range(user.GoalY, user.GoalY - 10).Contains(item.GetY);
        }

        public void UpdateUserMovement(Point oldCoord, Point newCoord, RoomUser user)
        {
            RemoveUserFromMap(user, oldCoord);
            AddUserToMap(user, newCoord);
        }

        public void RemoveUserFromMap(RoomUser user, Point coord)
        {
            if (userMap.Contains(coord))
            {
                var usr = (List<RoomUser>) userMap[coord];
                usr.Remove(user);
                userMap.Remove(coord);
            }
        }

        public bool MapGotUser(Point coord) => GetRoomUsers(coord).Count > 0;


        public List<RoomUser> GetRoomUsers(Point coord)
            => userMap.Contains(coord) ? (List<RoomUser>) userMap[coord] : new List<RoomUser>();


        public Point getRandomWalkableSquare()
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1); y++)
            for (var x = 0; x < GameMap.GetUpperBound(0); x++)
                if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                    walkableSquares.Add(new Point(x, y));

            var RandomNumber = OblivionServer.GetRandomNumber(0, walkableSquares.Count);
            var i = 0;

            foreach (var coord in walkableSquares.ToList())
            {
                if (i == RandomNumber)
                    return coord;
                i++;
            }

            return new Point(0, 0);
        }


        public bool isInMap(int X, int Y)
        {
            var walkableSquares = new List<Point>();
            for (var y = 0; y < GameMap.GetUpperBound(1); y++)
            for (var x = 0; x < GameMap.GetUpperBound(0); x++)
                if (StaticModel.DoorX != x && StaticModel.DoorY != y && GameMap[x, y] == 1)
                    walkableSquares.Add(new Point(x, y));

            return walkableSquares.Contains(new Point(X, Y));
        }

        public void AddToMap(Item item) => AddItemToMap(item);

        private void SetDefaultValue(int x, int y)
        {
            GameMap[x, y] = 0;
            EffectMap[x, y] = 0;
            mItemHeightMap[x, y] = 0.0;

            if (x == Model.DoorX && y == Model.DoorY)
                GameMap[x, y] = 3;
            else if (Model.SqState[x, y] == SquareState.OPEN)
                GameMap[x, y] = 1;
            else if (Model.SqState[x, y] == SquareState.SEAT)
                GameMap[x, y] = 2;
        }

        public void updateMapForItem(Item item)
        {
            RemoveFromMap(item);
            AddToMap(item);
        }

        public void GenerateMaps(bool checkLines = true)
        {
            while (true)
            {
                var MaxX = 0;
                var MaxY = 0;
                mCoordinatedItems = new ConcurrentDictionary<Point, List<int>>();

                if (checkLines)
                {
                    var items = _room.GetRoomItemHandler().GetFloor.ToList();
                    foreach (var item in items)
                    {
                        if (item.GetX > Model.MapSizeX && item.GetX > MaxX)
                            MaxX = item.GetX;
                        if (item.GetY > Model.MapSizeY && item.GetY > MaxY)
                            MaxY = item.GetY;
                    }
                    items.Clear();
                }

                #region Dynamic game map handling

                if (MaxY > Model.MapSizeY - 1 || MaxX > Model.MapSizeX - 1)
                {
                    if (MaxX < Model.MapSizeX)
                        MaxX = Model.MapSizeX;
                    if (MaxY < Model.MapSizeY)
                        MaxY = Model.MapSizeY;

                    Model.SetMapsize(MaxX + 7, MaxY + 7);
                    GenerateMaps(false);
                    continue;
                }

                if (MaxX != StaticModel.MapSizeX || MaxY != StaticModel.MapSizeY)
                {
                    EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                    mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                    //if (modelRemap)
                    //    Model.Generate(); //Clears model

                    for (var line = 0; line < Model.MapSizeY; line++)
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
//                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                            GameMap[chr, line] = 3;
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                            GameMap[chr, line] = 1;
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                            GameMap[chr, line] = 2;
//                        else if (Model.SqState[chr, line] == SquareState.POOL)
//                            EffectMap[chr, line] = 6;
                    }

                    /*if (gotPublicPool)
                        for (var y = 0; y < StaticModel.MapSizeY; y++)
                        for (var x = 0; x < StaticModel.MapSizeX; x++)
                            if (StaticModel.mRoomModelfx[x, y] != 0)
                                EffectMap[x, y] = StaticModel.mRoomModelfx[x, y];*/
                }
                #endregion

                #region Static game map handling

                else
                {
                    //mGameMap
                    //mUserItemEffect
                    EffectMap = new byte[Model.MapSizeX, Model.MapSizeY];
                    GameMap = new byte[Model.MapSizeX, Model.MapSizeY];


                    mItemHeightMap = new double[Model.MapSizeX, Model.MapSizeY];
                    //if (modelRemap)
                    //    Model.Generate(); //Clears model

                    for (var line = 0; line < Model.MapSizeY; line++)
                    for (var chr = 0; chr < Model.MapSizeX; chr++)
                    {
                        GameMap[chr, line] = 0;
//                        EffectMap[chr, line] = 0;

                        if (chr == Model.DoorX && line == Model.DoorY)
                            GameMap[chr, line] = 3;
                        else if (Model.SqState[chr, line] == SquareState.OPEN)
                            GameMap[chr, line] = 1;
                        else if (Model.SqState[chr, line] == SquareState.SEAT)
                            GameMap[chr, line] = 2;
//                        else if (Model.SqState[chr, line] == SquareState.POOL)
//                            EffectMap[chr, line] = 6;
                    }

//                    if (gotPublicPool)
//                        for (var y = 0; y < StaticModel.MapSizeY; y++)
//                        for (var x = 0; x < StaticModel.MapSizeX; x++)
//                            if (StaticModel.mRoomModelfx[x, y] != 0)
//                                EffectMap[x, y] = StaticModel.mRoomModelfx[x, y];
                }

                #endregion

                var tmpItems = _room.GetRoomItemHandler().GetFloor.ToList();
                foreach (var Item in tmpItems)
                    AddItemToMap(Item);
                // Array.Clear(tmpItems, 0, tmpItems.Length);
                tmpItems.Clear();
                if (_room.RoomBlockingEnabled == 0)
                    foreach (var user in _room.GetRoomUserManager().GetUserList().ToList().Where(user => user != null))
                    {
                        user.SqState = GameMap[user.X, user.Y];
                        GameMap[user.X, user.Y] = 0;
                    }

                try
                {
                    GameMap[Model.DoorX, Model.DoorY] = 3;
                }
                catch
                {
                }
                break;
            }
        }

        public DynamicRoomModel getDynamicRoomModel() => Model;

        private bool ConstructMapForItem(Item Item, Point Coord)
        {
            try
            {
                if (Coord.X > Model.MapSizeX - 1)
                {
                    Model.AddX();
                    GenerateMaps();
                    return false;
                }

                if (Coord.Y > Model.MapSizeY - 1)
                {
                    Model.AddY();
                    GenerateMaps();
                    return false;
                }

                if (Model.SqState[Coord.X, Coord.Y] == SquareState.BLOCKED)
                    Model.OpenSquare(Coord.X, Coord.Y, Item.GetZ);
                if (mItemHeightMap[Coord.X, Coord.Y] <= Item.TotalHeight)
                {
                    mItemHeightMap[Coord.X, Coord.Y] = Item.TotalHeight - Model.SqFloorHeight[Item.GetX, Item.GetY];
                    EffectMap[Coord.X, Coord.Y] = Convert.ToByte(Item.GetBaseItem().EffectM);

                    /* EffectMap[Coord.X, Coord.Y] = 0;
 
 
                     switch (Item.GetBaseItem().InteractionType)
                     {
                         case InteractionType.Pool:
                             EffectMap[Coord.X, Coord.Y] = 1;
                             break;
                         case InteractionType.NormalSkates:
                             EffectMap[Coord.X, Coord.Y] = 2;
                             break;
                         case InteractionType.IceSkates:
                             EffectMap[Coord.X, Coord.Y] = 3;
                             break;
                         case InteractionType.Lowpool:
                             EffectMap[Coord.X, Coord.Y] = 4;
                             break;
                         case InteractionType.Haloweenpool:
                             EffectMap[Coord.X, Coord.Y] = 5;
                             break;
                     }*/


                    //SwimHalloween
                    if (Item.GetBaseItem().Walkable)
                        // If this item is walkable and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                            GameMap[Coord.X, Coord.Y] = 1;
                    }
                    else if (Item.GetZ <= Model.SqFloorHeight[Item.GetX, Item.GetY] + 0.1 &&
                             Item.GetBaseItem().InteractionType == InteractionType.Gate && Item.ExtraData == "1")
                        // If this item is a gate, open, and on the floor, allow users to walk here.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                            GameMap[Coord.X, Coord.Y] = 1;
                    }
                    else if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.Bed ||
                             Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                    {
                        GameMap[Coord.X, Coord.Y] = 3;
                    }
                    else // Finally, if it's none of those, block the square.
                    {
                        if (GameMap[Coord.X, Coord.Y] != 3)
                            GameMap[Coord.X, Coord.Y] = 0;
                    }
                }

                // Set bad maps
                if (Item.GetBaseItem().InteractionType == InteractionType.Bed ||
                    Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                    GameMap[Coord.X, Coord.Y] = 3;
            }
            catch (Exception e)
            {
                Logging.LogException("Error during map generation for room " + _room.RoomId + ". Exception: " + e);
            }
            return true;
        }

        public void AddCoordinatedItem(Item item, Point coord)
        {
            var Items = new List<int>();

            if (!mCoordinatedItems.TryGetValue(coord, out Items))
            {
                Items = new List<int>();

                if (!Items.Contains(item.Id))
                    Items.Add(item.Id);

                if (!mCoordinatedItems.ContainsKey(coord))
                    mCoordinatedItems.TryAdd(coord, Items);
            }
            else
            {
                if (Items.Contains(item.Id)) return;
                Items.Add(item.Id);
                mCoordinatedItems[coord] = Items;
            }
        }

        public List<Item> GetCoordinatedItems(Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            var Items = new List<Item>();

            if (!mCoordinatedItems.ContainsKey(point)) return new List<Item>();
            var Ids = mCoordinatedItems[point];
            Items = GetItemsFromIds(Ids);
            return Items;
        }

        public bool RemoveCoordinatedItem(Item item, Point coord)
        {
            var point = new Point(coord.X, coord.Y);
            if (mCoordinatedItems == null || !mCoordinatedItems.ContainsKey(point)) return false;
            mCoordinatedItems[point].RemoveAll(x => x == item.Id);
            return true;
        }

        private void AddSpecialItems(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGate:
                    //IsTrans = true;
                    _room.GetSoccer().RegisterGate(item);


                    var splittedExtraData = item.ExtraData.Split(':');

                    if (string.IsNullOrEmpty(item.ExtraData) || splittedExtraData.Length <= 1)
                    {
                        item.Gender = "M";
                        switch (item.team)
                        {
                            case TEAM.YELLOW:
                                item.Figure = "lg-275-93.hr-115-61.hd-207-14.ch-265-93.sh-305-62";
                                break;
                            case TEAM.RED:
                                item.Figure = "lg-275-96.hr-115-61.hd-180-3.ch-265-96.sh-305-62";
                                break;
                            case TEAM.GREEN:
                                item.Figure = "lg-275-102.hr-115-61.hd-180-3.ch-265-102.sh-305-62";
                                break;
                            case TEAM.BLUE:
                                item.Figure = "lg-275-108.hr-115-61.hd-180-3.ch-265-108.sh-305-62";
                                break;
                        }
                    }
                    else
                    {
                        item.Gender = splittedExtraData[0];
                        item.Figure = splittedExtraData[1];
                    }
                    break;

                case InteractionType.Banzaifloor:
                {
                    _room.GetBanzai().AddTile(item, item.Id);
                    break;
                }

                case InteractionType.Banzaipyramid:
                {
                    _room.GetGameItemHandler().AddPyramid(item, item.Id);
                    break;
                }

                case InteractionType.Banzaitele:
                {
                    _room.GetGameItemHandler().AddTeleport(item, item.Id);
                    item.ExtraData = "";
                    break;
                }
                case InteractionType.Banzaipuck:
                {
                    _room.GetBanzai().AddPuck(item);
                    break;
                }

                case InteractionType.Football:
                {
                    _room.GetSoccer().AddBall(item);
                    break;
                }
                case InteractionType.FreezeTileBlock:
                {
                    _room.GetFreeze().AddFreezeBlock(item);
                    break;
                }
                case InteractionType.FreezeTile:
                {
                    _room.GetFreeze().AddFreezeTile(item);
                    break;
                }
                case InteractionType.Freezeexit:
                {
                    _room.GetFreeze().AddExitTile(item);
                    break;
                }
            }
            /* if (!item.GetBaseItem().ItemName.StartsWith("tile_stackmagic"))
                 return;
             if (!_room.GetGameMap().TileHeightApilable.ContainsKey(item.Coordinate))
                 _room.GetGameMap().TileHeightApilable.Add(item.Coordinate, this);
                 //todo
             _room.GetGameMap().mItemHeightMap[item.GetX, item.GetY] = item.TotalHeight;
        */
        }

        public void RemoveSpecialItem(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGate:
                    _room.GetSoccer().UnRegisterGate(item);
                    break;
                case InteractionType.Banzaifloor:
                    _room.GetBanzai().RemoveTile(item.Id);
                    break;
                case InteractionType.Banzaipuck:
                    _room.GetBanzai().RemovePuck(item.Id);
                    break;
                case InteractionType.Banzaipyramid:
                    _room.GetGameItemHandler().RemovePyramid(item.Id);
                    break;
                case InteractionType.Banzaitele:
                    _room.GetGameItemHandler().RemoveTeleport(item.Id);
                    break;
                case InteractionType.Football:
                    _room.GetSoccer().RemoveBall(item.Id);
                    break;
                case InteractionType.FreezeTile:
                    _room.GetFreeze().RemoveFreezeTile(item.Id);
                    break;
                case InteractionType.FreezeTileBlock:
                    _room.GetFreeze().RemoveFreezeBlock(item.Id);
                    break;
                case InteractionType.Freezeexit:
                    _room.GetFreeze().RemoveExitTile(item.Id);
                    break;
            }
        }

        public bool RemoveFromMap(Item item, bool handleGameItem)
        {
            if (item?.GetBaseItem() == null || _room == null)
                return false;

            if (handleGameItem)
                RemoveSpecialItem(item);

            if (_room.GotSoccer())
            {
                _room.GetSoccer().onGateRemove(item);
                _room.GetSoccer().RemoveBall(item.Id);
            }

            var isRemoved = item.GetCoords.Any(coord => RemoveCoordinatedItem(item, coord));


            var items = new HybridDictionary();
            foreach (var Tile in item.GetCoords.ToList())
            {
                var point = new Point(Tile.X, Tile.Y);
                if (mCoordinatedItems.ContainsKey(point))
                {
                    var Ids = mCoordinatedItems[point];
                    var __items = GetItemsFromIds(Ids);

                    if (!items.Contains(Tile))
                        items.Add(Tile, __items);
                }

                SetDefaultValue(Tile.X, Tile.Y);
            }

            foreach (Point Coord in items.Keys)
            {
                if (!items.Contains(Coord))
                    continue;

                var SubItems = (List<Item>) items[Coord];
                foreach (var Item in SubItems.ToList())
                    ConstructMapForItem(Item, Coord);
            }


            items.Clear();

            //  items = null;


            return isRemoved;
        }

        public bool RemoveFromMap(Item item) => RemoveFromMap(item, true);

        public bool AddItemToMap(Item Item, bool handleGameItem, bool NewItem = true)
        {
            if (handleGameItem)
            {
                AddSpecialItems(Item);

                switch (Item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FootballGoalRed:
                    case InteractionType.Footballcounterred:
                    case InteractionType.Banzaiscorered:
                    case InteractionType.Banzaigatered:
                    case InteractionType.Freezeredcounter:
                    case InteractionType.FreezeRedGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.RED);
                        break;
                    }
                    case InteractionType.FootballGoalGreen:
                    case InteractionType.Footballcountergreen:
                    case InteractionType.Banzaiscoregreen:
                    case InteractionType.Banzaigategreen:
                    case InteractionType.Freezegreencounter:
                    case InteractionType.FreezeGreenGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.GREEN);
                        break;
                    }
                    case InteractionType.FootballGoalBlue:
                    case InteractionType.Footballcounterblue:
                    case InteractionType.Banzaiscoreblue:
                    case InteractionType.Banzaigateblue:
                    case InteractionType.Freezebluecounter:
                    case InteractionType.FreezeBlueGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.BLUE);
                        break;
                    }
                    case InteractionType.FootballGoalYellow:
                    case InteractionType.Footballcounteryellow:
                    case InteractionType.Banzaiscoreyellow:
                    case InteractionType.Banzaigateyellow:
                    case InteractionType.Freezeyellowcounter:
                    case InteractionType.FreezeYellowGate:
                    {
                        if (!_room.GetRoomItemHandler().GetFloor.Contains(Item))
                            _room.GetGameManager().AddFurnitureToTeam(Item, TEAM.YELLOW);
                        break;
                    }
                    case InteractionType.Freezeexit:
                    {
                        _room.GetFreeze().AddExitTile(Item);
                        break;
                    }
                    case InteractionType.Roller:
                    {
                        if (!_room.GetRoomItemHandler().GetRollers().Contains(Item))
                            _room.GetRoomItemHandler().TryAddRoller(Item.Id, Item);
                        break;
                    }
                }
            }

            if (Item.GetBaseItem().Type != 's')
                return true;

            foreach (var coord in Item.GetCoords.ToList())
                AddCoordinatedItem(Item, new Point(coord.X, coord.Y));

            if (Item.GetX > Model.MapSizeX - 1)
            {
                Model.AddX();
                GenerateMaps();
                return false;
            }

            if (Item.GetY > Model.MapSizeY - 1)
            {
                Model.AddY();
                GenerateMaps();
                return false;
            }

            var Return = true;

            foreach (var coord in Item.GetCoords)
                Return = ConstructMapForItem(Item, coord);


            return Return;
        }


        public bool CanWalk(int X, int Y, bool Override)
        {
            if (Override)
                return true;

            return _room.GetRoomUserManager().GetUserForSquare(X, Y) == null || _room.RoomBlockingEnabled != 0;
        }

        public bool AddItemToMap(Item Item, bool NewItem = true) => AddItemToMap(Item, true, NewItem);

        public bool ItemCanMove(Item Item, Point MoveTo)
        {
            var Points =
                GetAffectedTiles(Item.GetBaseItem().Length, Item.GetBaseItem().Width, MoveTo.X, MoveTo.Y, Item.Rotation)
                    .Values.ToList();

            if (Points.Count == 0)
                return true;

            foreach (var Coord in Points)
            {
                if (Coord.X >= Model.MapSizeX || Coord.Y >= Model.MapSizeY)
                    return false;

                if (!SquareIsOpen(Coord.X, Coord.Y, false))
                    return false;
            }

            return true;
        }

        public byte GetFloorStatus(Point coord)
        {
            if (coord.X > GameMap.GetUpperBound(0) || coord.Y > GameMap.GetUpperBound(1))
                return 1;

            return GameMap[coord.X, coord.Y];
        }

        public void SetFloorStatus(int X, int Y, byte Status) => GameMap[X, Y] = Status;

        public double GetHeightForSquareFromData(Point coord)
        {
            if (coord.X > Model.SqFloorHeight.GetUpperBound(0) ||
                coord.Y > Model.SqFloorHeight.GetUpperBound(1))
                return 1;
            return Model.SqFloorHeight[coord.X, coord.Y];
        }

        public bool CanRollItemHere(int x, int y)
        {
            if (!ValidTile(x, y))
                return false;

            return Model.SqState[x, y] != SquareState.BLOCKED;
        }

        public bool SquareIsOpen(int x, int y, bool pOverride)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y)
                return false;

            var _Items = _room.GetGameMap().GetAllRoomItemForSquare(x, y);
            if (_Items.Count > 0)
            {
                var HasGroupGate =
                    _Items.ToList()
                        .Where(it => it.GetBaseItem().InteractionType == InteractionType.GuildGate)
                        .ToList().Any();
                if (HasGroupGate)
                    return true;
            }
            return CanWalk(GameMap[x, y], pOverride);
        }

        public Point GetChaseMovement(Item Item)
        {
            var Distance = 99;
            var Coord = new Point(0, 0);
            var iX = Item.GetX;
            var iY = Item.GetY;
            var X = false;

            foreach (var User in _room.GetRoomUserManager().GetRoomUsers())
                if (User.X == Item.GetX || Item.GetY == User.Y)
                    if (User.X == Item.GetX)
                    {
                        var Difference = Math.Abs(User.Y - Item.GetY);
                        if (Difference >= Distance)
                            continue;
                        Distance = Difference;
                        Coord = User.Coordinate;
                        X = false;
                    }
                    else if (User.Y == Item.GetY)
                    {
                        var Difference = Math.Abs(User.X - Item.GetX);
                        if (Difference >= Distance)
                            continue;
                        Distance = Difference;
                        Coord = User.Coordinate;
                        X = true;
                    }

            if (Distance > 5)
                return Item.GetSides().OrderBy(x => Guid.NewGuid()).FirstOrDefault();
            if (X && Distance < 99)
                if (iX > Coord.X)
                {
                    iX--;
                    return new Point(iX, iY);
                }
                else
                {
                    iX++;
                    return new Point(iX, iY);
                }
            if (!X && Distance < 99)
                if (iY > Coord.Y)
                {
                    iY--;
                    return new Point(iX, iY);
                }
                else
                {
                    iY++;
                    return new Point(iX, iY);
                }
            return Item.Coordinate;
        }

        public bool IsValidStep2(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override)
        {
            if (User == null)
                return false;

            if (!ValidTile(To.X, To.Y))
                return false;

            if (Override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            var Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                var HasGroupGate =
                    Items.ToList()
                        .Where(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate)
                        .ToList().Any();
                var HasLinkTile =
                    Items.ToList()
                        .Where(x => x.GetBaseItem().ItemName.ToLower() == "room_wl15_infolink")
                        .ToList().Any();
                if (HasLinkTile)
                {
                    var I = Items.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "room_wl15_infolink");
                    if (I != null)
                    {
                        var notif = new ServerPacket(ServerPacketHeader.NuxAlertMessageComposer);
                        var data = I.ExtraData.Replace("state	0	internalLink	", null);
                        notif.WriteString(data);
                        User.GetClient().SendMessage(notif);
                    }
                }


                if (HasGroupGate)
                {
                    var I = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == InteractionType.GuildGate);
                    if (I != null)
                    {
                        var Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(I.GroupId);
                        if (Group == null)
                            return false;

                        if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                            return false;

                        if (Group.IsMember(User.GetClient().GetHabbo().Id))
                        {
                            I.InteractingUser = User.GetClient().GetHabbo().Id;
                            I.ExtraData = "1";
                            I.UpdateState(false, true);

                            I.RequestUpdate(4, true);

                            return true;
                        }
                        if (User.Path.Count > 0)
                            User.Path.Clear();
                        User.PathRecalcNeeded = false;
                        return false;
                    }
                }
            }

            var Chair = false;
            double HighestZ = -1;
            foreach (var Item in Items.ToList())
            {
                if (Item == null)
                    continue;

                if (Item.GetZ < HighestZ)
                {
                    Chair = false;
                    continue;
                }

                HighestZ = Item.GetZ;
                if (Item.GetBaseItem().IsSeat)
                    Chair = true;
            }

            if (GameMap[To.X, To.Y] == 3 && !EndOfPath && !Chair || GameMap[To.X, To.Y] == 0 ||
                GameMap[To.X, To.Y] == 2 && !EndOfPath)
            {
                User.Path.Clear();
                User.PathRecalcNeeded = true;
            }

            var HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
            if (HeightDiff > 1.5 && !User.RidingHorse)
                return false;

            //Check this last, because ya.


            var Userx = _room.GetRoomUserManager().GetUserForSquare(To.X, To.Y);
            if (Userx != null)
                if (!Userx.IsWalking && EndOfPath)
                    return false;
            return true;
        }


        public bool IsValidStep(RoomUser User, Vector2D From, Vector2D To, bool EndOfPath, bool Override,
            bool Roller = false)
        {
            if (!ValidTile(To.X, To.Y))
                return false;

            if (Override)
                return true;

            /*
             * 0 = blocked
             * 1 = open
             * 2 = last step
             * 3 = door
             * */

            if (_room.RoomBlockingEnabled == 0 && SquareHasUsers(To.X, To.Y))
                return false;

            var Items = _room.GetGameMap().GetAllRoomItemForSquare(To.X, To.Y);
            if (Items.Count > 0)
            {
                var HasGroupGate =
                    Items
                        .Count(x => x != null && x.GetBaseItem().InteractionType == InteractionType.GuildGate) > 0;
                if (HasGroupGate)
                    return true;
            }

            if (GameMap[To.X, To.Y] == 3 && !EndOfPath || GameMap[To.X, To.Y] == 0 ||
                GameMap[To.X, To.Y] == 2 && !EndOfPath)
                return false;


            if (!Roller)
            {
                var HeightDiff = SqAbsoluteHeight(To.X, To.Y) - SqAbsoluteHeight(From.X, From.Y);
                if (HeightDiff > 1.5)
                    return false;
            }

            return true;
        }

        public static bool CanWalk(byte pState, bool pOverride)
        {
            if (pOverride) return true;
            if (pState == 3)
                return true;
            return pState == 1;
        }

        public bool itemCanBePlacedHere(int x, int y)
        {
            if (Model.MapSizeX - 1 < x || Model.MapSizeY - 1 < y ||
                x == Model.DoorX && y == Model.DoorY)
                return false;

            return GameMap[x, y] == 1;
        }


        public double SqAbsoluteHeight(int X, int Y)
        {
            var Points = new Point(X, Y);

            List<int> Ids;

            if (!mCoordinatedItems.TryGetValue(Points, out Ids)) return Model.SqFloorHeight[X, Y];
            var Items = GetItemsFromIds(Ids);

            return SqAbsoluteHeight(X, Y, Items);
        }

        public double SqAbsoluteHeight(int X, int Y, List<Item> ItemsOnSquare)
        {
            try
            {
                var deduct = false;
                double HighestStack = 0;
                var deductable = 0.0;

                if (ItemsOnSquare != null && ItemsOnSquare.Count > 0)
                    foreach (var Item in ItemsOnSquare.ToList().Where(Item => Item?.TotalHeight > HighestStack))
                    {
                        if (Item.GetBaseItem().IsSeat || Item.GetBaseItem().InteractionType == InteractionType.Bed ||
                            Item.GetBaseItem().InteractionType == InteractionType.TentSmall)
                        {
                            deduct = true;
                            deductable = Item.GetBaseItem().Height;
                        }
                        else
                        {
                            deduct = false;
                        }
                        HighestStack = Item.TotalHeight;
                    }

                double floorHeight = Model.SqFloorHeight[X, Y];
                var stackHeight = HighestStack - Model.SqFloorHeight[X, Y];

                if (deduct)
                    stackHeight -= deductable;

                if (stackHeight < 0)
                    stackHeight = 0;

                return floorHeight + stackHeight;
            }
            catch (Exception e)
            {
                Logging.HandleException(e, "Room.SqAbsoluteHeight");
                return 0;
            }
        }

        public bool ValidTile(int X, int Y) => X >= 0 && Y >= 0 && X < Model.MapSizeX && Y < Model.MapSizeY;

        public static Dictionary<int, ThreeDCoord> GetAffectedTiles(int Length, int Width, int PosX, int PosY,
            int Rotation)
        {
            var x = 0;

            var PointList = new Dictionary<int, ThreeDCoord>();

            if (Length > 1)
                if (Rotation == 0 || Rotation == 4)
                    for (var i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Width; j++)
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, i < j ? j : i));
                    }
                else if (Rotation == 2 || Rotation == 6)
                    for (var i = 1; i < Length; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Width; j++)
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, i < j ? j : i));
                    }

            if (Width > 1)
                if (Rotation == 0 || Rotation == 4)
                    for (var i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX + i, PosY, i));

                        for (var j = 1; j < Length; j++)
                            PointList.Add(x++, new ThreeDCoord(PosX + i, PosY + j, i < j ? j : i));
                    }
                else if (Rotation == 2 || Rotation == 6)
                    for (var i = 1; i < Width; i++)
                    {
                        PointList.Add(x++, new ThreeDCoord(PosX, PosY + i, i));

                        for (var j = 1; j < Length; j++)
                            PointList.Add(x++, new ThreeDCoord(PosX + j, PosY + i, i < j ? j : i));
                    }

            return PointList;
        }

        public List<Item> GetItemsFromIds(List<int> Input)
        {
            if (Input == null || Input.Count == 0)
                return new List<Item>();

            var Items = new List<Item>();

            lock (Input)
            {
                foreach (
                    var Itm in
                    Input.Select(Id => _room.GetRoomItemHandler().GetItem(Id))
                        .Where(Itm => Itm != null && !Items.Contains(Itm)))
                    Items.Add(Itm);
            }

            return Items.ToList();
        }

        public List<Item> GetRoomItemForSquare(int pX, int pY, double minZ)
        {
            var itemsToReturn = new List<Item>();


            var coord = new Point(pX, pY);
            if (mCoordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(mCoordinatedItems[coord]);

                itemsToReturn.AddRange(
                    itemsFromSquare.Where(item => item.GetZ > minZ)
                        .Where(item => item.GetX == pX && item.GetY == pY));
            }

            return itemsToReturn;
        }

        public List<Item> GetRoomItemForSquare(Vector2D vc) => GetRoomItemForSquare(vc.X, vc.Y);

        public List<Item> GetRoomItemForSquare(int pX, int pY)
        {
            var coord = new Point(pX, pY);
            var itemsToReturn = new List<Item>();

            if (mCoordinatedItems.ContainsKey(coord))
            {
                var itemsFromSquare = GetItemsFromIds(mCoordinatedItems[coord]);

                itemsToReturn.AddRange(
                    itemsFromSquare.Where(item => item.Coordinate.X == coord.X && item.Coordinate.Y == coord.Y));
            }

            return itemsToReturn;
        }

        public List<Item> GetAllRoomItemForSquare(int pX, int pY)
        {
            var Coord = new Point(pX, pY);

            var Items = new List<Item>();
            List<int> Ids;

            // CHANGED THIS ~  IF FAILED CHANGE BACK

            Items = mCoordinatedItems.TryGetValue(Coord, out Ids) ? GetItemsFromIds(Ids) : new List<Item>();

            return Items;
        }


        public bool SquareHasUsers(int X, int Y) => MapGotUser(new Point(X, Y));

        public bool SquareHasUsersThanMe(int x, int y, RoomUser me)
        {
            var coord = new Point(x, y);
            var users = GetRoomUsers(coord);
            return users.Any(user => users.Count > 0 && user != me);
        }

        public bool itemCanBePlacedAloneHere(int x, int y)
            => itemCanBePlacedHere(x, y) || GetRoomItemForSquare(x, y).Any();

        public bool getItemsFromSquare(int x, int y) => !itemCanBePlacedAloneHere(x + 1, y) ||
                                                        !itemCanBePlacedAloneHere(x - 1, y) ||
                                                        !itemCanBePlacedAloneHere(x, y - 1) ||
                                                        !itemCanBePlacedAloneHere(x, y + 1) ||
                                                        !itemCanBePlacedAloneHere(x - 1, y - 1) ||
                                                        !itemCanBePlacedAloneHere(x + 1, y + 1) ||
                                                        !itemCanBePlacedAloneHere(x - 1, y + 1) ||
                                                        !itemCanBePlacedAloneHere(x + 1, y - 1);

        public static bool TilesTouching(int X1, int Y1, int X2, int Y2)
        {
            if (!(Math.Abs(X1 - X2) > 1 || Math.Abs(Y1 - Y2) > 1)) return true;
            return X1 == X2 && Y1 == Y2;
        }

        public static int TileDistance(int X1, int Y1, int X2, int Y2) => Math.Abs(X1 - X2) + Math.Abs(Y1 - Y2);

        public void Dispose()
        {
            userMap.Clear();
            Model.Destroy();
            mCoordinatedItems.Clear();

            Array.Clear(GameMap, 0, GameMap.Length);
            Array.Clear(EffectMap, 0, EffectMap.Length);
            Array.Clear(mItemHeightMap, 0, mItemHeightMap.Length);

            userMap = null;
            GameMap = null;
            EffectMap = null;
            mItemHeightMap = null;
            mCoordinatedItems = null;

            Model = null;
            _room = null;
            StaticModel = null;
        }
    }
}