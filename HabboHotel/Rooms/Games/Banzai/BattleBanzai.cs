#region

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Rooms.Games.Teams;
using Oblivion.Utilities.Enclosure;

#endregion

namespace Oblivion.HabboHotel.Rooms.Games.Banzai
{
    public class BattleBanzai
    {
        private ConcurrentDictionary<int, Item> _banzaiTiles;
        private ConcurrentDictionary<int, Item> _pucks;
        private Room _room;
        private GameField field;
        private byte[,] floorMap;
        private double timestarted;

        public BattleBanzai(Room room)
        {
            _room = room;
            isBanzaiActive = false;
            timestarted = 0;
            _pucks = new ConcurrentDictionary<int, Item>();
            _banzaiTiles = new ConcurrentDictionary<int, Item>();
        }

        public bool isBanzaiActive { get; private set; }

        public void AddTile(Item item, int itemID)
        {
            if (!_banzaiTiles.ContainsKey(itemID))
                _banzaiTiles.TryAdd(itemID, item);
        }

        public void RemoveTile(int itemID)
        {
            Item Item;
            _banzaiTiles.TryRemove(itemID, out Item);
        }

        public void AddPuck(Item item)
        {
            if (!_pucks.ContainsKey(item.Id))
                _pucks.TryAdd(item.Id, item);
        }

        public void RemovePuck(int itemID)
        {
            Item Item;
            _pucks.TryRemove(itemID, out Item);
        }

        public void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;

            foreach (var item in _pucks.Values.ToList())
            {
                var NewX = 0;
                var NewY = 0;
                var differenceX = User.X - item.GetX;
                var differenceY = User.Y - item.GetY;

                if (differenceX == 0 && differenceY == 0)
                {
                    switch (User.RotBody)
                    {
                        case 4:
                            NewX = User.X;
                            NewY = User.Y + 2;
                            break;
                        case 6:
                            NewX = User.X - 2;
                            NewY = User.Y;
                            break;
                        case 0:
                            NewX = User.X;
                            NewY = User.Y - 2;
                            break;
                        case 2:
                            NewX = User.X + 2;
                            NewY = User.Y;
                            break;
                        case 1:
                            NewX = User.X + 2;
                            NewY = User.Y - 2;
                            break;
                        case 7:
                            NewX = User.X - 2;
                            NewY = User.Y - 2;
                            break;
                        case 3:
                            NewX = User.X + 2;
                            NewY = User.Y + 2;
                            break;
                        case 5:
                            NewX = User.X - 2;
                            NewY = User.Y + 2;
                            break;
                    }

                    if (
                        !_room.GetRoomItemHandler()
                            .CheckPosItem(User.GetClient(), item, NewX, NewY, item.Rotation, false, false))
                        switch (User.RotBody)
                        {
                            case 0:
                                NewX = User.X;
                                NewY = User.Y + 1;
                                break;
                            case 2:
                                NewX = User.X - 1;
                                NewY = User.Y;
                                break;
                            case 4:
                                NewX = User.X;
                                NewY = User.Y - 1;
                                break;
                            case 6:
                                NewX = User.X + 1;
                                NewY = User.Y;
                                break;
                            case 5:
                                NewX = User.X + 1;
                                NewY = User.Y - 1;
                                break;
                            case 3:
                                NewX = User.X - 1;
                                NewY = User.Y - 1;
                                break;
                            case 7:
                                NewX = User.X + 1;
                                NewY = User.Y + 1;
                                break;
                            case 1:
                                NewX = User.X - 1;
                                NewY = User.Y + 1;
                                break;
                        }
                }
                else if (differenceX <= 1 && differenceX >= -1 && differenceY <= 1 && differenceY >= -1 &&
                         VerifyPuck(User, item.Coordinate.X, item.Coordinate.Y))
                    //VERYFIC BALL CHECAR SI ESTA EN DIRECCION ASIA LA PELOTA
                {
                    NewX = differenceX * -1;
                    NewY = differenceY * -1;

                    NewX = NewX + item.GetX;
                    NewY = NewY + item.GetY;
                }

                if (item.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                    MovePuck(item, User.GetClient(), NewX, NewY, User.Team);
            }

            if (isBanzaiActive)
                HandleBanzaiTiles(User.Coordinate, User.Team, User);
        }

        private static bool VerifyPuck(RoomUser user, int actualx, int actualy)
            => Rotation.Calculate(user.X, user.Y, actualx, actualy) == user.RotBody;

        public void BanzaiStart()
        {
            if (isBanzaiActive)
                return;

            floorMap = new byte[_room.GetGameMap().Model.MapSizeY, _room.GetGameMap().Model.MapSizeX];
            field = new GameField(floorMap, true);
            timestarted = OblivionServer.GetUnixTimestamp();
            _room.GetGameManager().LockGates();
            for (var i = 1; i < 5; i++)
                _room.GetGameManager().Points[i] = 0;

            foreach (var tile in _banzaiTiles.Values)
            {
                tile.ExtraData = "1";
                tile.value = 0;
                tile.team = TEAM.NONE;
                tile.UpdateState();
            }

            ResetTiles();
            isBanzaiActive = true;

            _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameStarts, null);

            foreach (var user in _room.GetRoomUserManager().GetRoomUsers())
                user.LockedTilesCount = 0;
        }

        public void ResetTiles()
        {
            foreach (var item in _room.GetRoomItemHandler().GetFloor.ToList())
            {
                var type = item.GetBaseItem().InteractionType;

                switch (type)
                {
                    case InteractionType.Banzaiscoreblue:
                    case InteractionType.Banzaiscoregreen:
                    case InteractionType.Banzaiscorered:
                    case InteractionType.Banzaiscoreyellow:
                    {
                        item.ExtraData = "0";
                        item.UpdateState();
                        break;
                    }
                }
            }
        }

        public void BanzaiEnd(bool userTriggered = false)
        {
            //TODO
            isBanzaiActive = false;
            _room.GetGameManager().StopGame();
            floorMap = null;

            if (!userTriggered)
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);

            var winners = _room.GetGameManager().GetWinningTeam();
            _room.GetGameManager().UnlockGates();
            foreach (var tile in _banzaiTiles.Values)
                if (tile.team == winners)
                {
                    tile.interactionCount = 0;
                    tile.interactionCountHelper = 0;
                    tile.UpdateNeeded = true;
                }
                else if (tile.team == TEAM.NONE)
                {
                    tile.ExtraData = "0";
                    tile.UpdateState();
                }

            if (winners != TEAM.NONE)
            {
                var Winners = _room.GetRoomUserManager().GetRoomUsers();

                foreach (var User in Winners.ToList())
                {
                    if (User.Team != TEAM.NONE)
                        if (OblivionServer.GetUnixTimestamp() - timestarted > 5)
                        {
                            OblivionServer.GetGame()
                                .GetAchievementManager()
                                .ProgressAchievement(User.GetClient(), "ACH_BattleBallTilesLocked",
                                    User.LockedTilesCount);
                            OblivionServer.GetGame()
                                .GetAchievementManager()
                                .ProgressAchievement(User.GetClient(), "ACH_BattleBallPlayer", 1);
                        }
                    if (winners == TEAM.BLUE)
                    {
                        if (User.CurrentEffect == 35)
                        {
                            if (OblivionServer.GetUnixTimestamp() - timestarted > 5)
                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            _room.SendMessage(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.RED)
                    {
                        if (User.CurrentEffect == 33)
                        {
                            if (OblivionServer.GetUnixTimestamp() - timestarted > 5)
                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            _room.SendMessage(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.GREEN)
                    {
                        if (User.CurrentEffect == 34)
                        {
                            if (OblivionServer.GetUnixTimestamp() - timestarted > 5)
                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            _room.SendMessage(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                    else if (winners == TEAM.YELLOW)
                    {
                        if (User.CurrentEffect == 36)
                        {
                            if (OblivionServer.GetUnixTimestamp() - timestarted > 5)
                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(User.GetClient(), "ACH_BattleBallWinner", 1);
                            _room.SendMessage(new ActionComposer(User.VirtualId, 1));
                        }
                    }
                }
                field?.Dispose();
            }
        }

        public void MovePuck(Item item, GameClient mover, int newX, int newY, TEAM team)
        {
            if (!_room.GetGameMap().itemCanBePlacedHere(newX, newY))
                return;

            var oldRoomCoord = item.Coordinate;


            if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                return;

            item.ExtraData = Convert.ToInt32(team).ToString();
            item.UpdateNeeded = true;
            item.UpdateState();

            double NewZ = _room.GetGameMap().Model.SqFloorHeight[newX, newY];

            _room.SendMessage(new SlideObjectBundleComposer(item.GetX, item.GetY, item.GetZ, newX, newY, NewZ, 0, 0,
                item.Id));

            _room.GetRoomItemHandler().SetFloorItem(mover, item, newX, newY, item.Rotation, false, false, false);

            if (mover?.GetHabbo() == null)
                return;

            var user = mover.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(mover.GetHabbo().Id);
            if (isBanzaiActive)
                HandleBanzaiTiles(new Point(newX, newY), team, user);
        }

        private void SetTile(Item item, TEAM team, RoomUser user)
        {
            if (item.team == team)
            {
                if (item.value < 3)
                {
                    item.value++;
                    if (item.value == 3)
                    {
                        user.LockedTilesCount++;
                        _room.GetGameManager().AddPointToTeam(item.team, 1);
                        field.updateLocation(item.GetX, item.GetY, (byte) team);
                        var gfield = field.doUpdate();
                        foreach (var gameField in gfield)
                        {
                            var t = (TEAM) gameField.forValue;
                            foreach (var p in gameField.getPoints())
                            {
                                HandleMaxBanzaiTiles(new Point(p.X, p.Y), t);
                                floorMap[p.Y, p.X] = gameField.forValue;
                            }
                        }
                    }
                }
            }
            else
            {
                if (item.value < 3)
                {
                    item.team = team;
                    item.value = 1;
                }
            }


            var newColor = item.value + Convert.ToInt32(item.team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        private void HandleBanzaiTiles(Point coord, TEAM team, RoomUser user)
        {
            if (team == TEAM.NONE)
                return;

            var i = 0;
            foreach (var _item in _banzaiTiles.Values.ToList().Where(_item => _item != null))
            {
                if (_item.GetBaseItem().InteractionType != InteractionType.Banzaifloor)
                {
                    user.Team = TEAM.NONE;
                    user.ApplyEffect(0);
                    continue;
                }

                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                {
                    i++;
                    continue;
                }

                if (_item.GetX != coord.X || _item.GetY != coord.Y)
                    continue;

                SetTile(_item, team, user);
                if (_item.ExtraData.Equals("5") || _item.ExtraData.Equals("8") || _item.ExtraData.Equals("11") ||
                    _item.ExtraData.Equals("14"))
                    i++;
                _item.UpdateState(false, true);
            }
            if (i == _banzaiTiles.Count)
                BanzaiEnd();
        }

        private void HandleMaxBanzaiTiles(Point coord, TEAM team)
        {
            if (team == TEAM.NONE)
                return;

            var items = _room.GetGameMap().GetCoordinatedItems(coord);

            foreach (
                var _item in
                _banzaiTiles.Values.ToList()
                    .Where(_item => _item?.GetBaseItem().InteractionType == InteractionType.Banzaifloor)
                    .Where(_item => _item.GetX == coord.X && _item.GetY == coord.Y))
            {
                SetMaxForTile(_item, team);
                _room.GetGameManager().AddPointToTeam(team, 1);
                _item.UpdateState(false, true);
            }
        }

        private static void SetMaxForTile(Item item, TEAM team)
        {
            if (item.value < 3)
            {
                item.value = 3;
                item.team = team;
            }

            var newColor = item.value + Convert.ToInt32(item.team) * 3 - 1;
            item.ExtraData = newColor.ToString();
        }

        public void Dispose()
        {
            _banzaiTiles.Clear();
            _pucks.Clear();

            if (floorMap != null)
                Array.Clear(floorMap, 0, floorMap.Length);

            field?.Dispose();

            _room = null;
            _banzaiTiles = null;
            _pucks = null;
            floorMap = null;
            field = null;
        }
    }
}