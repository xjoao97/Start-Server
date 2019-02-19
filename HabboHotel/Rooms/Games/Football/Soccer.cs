#region

using System;
using System.Collections.Concurrent;
using System.Drawing;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Rooms.Games.Teams;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Rooms.Games.Football
{
    public enum Direction
    {
        Up,
        UpRight,
        Right,
        DownRight,
        Down,
        DownLeft,
        Left,
        UpLeft,
        Null
    }


    public class Soccer
    {
        // private ConcurrentDictionary<int, Item> _balls;
        private Room _room;
        // internal Item Balls { get; private set; }
        private ConcurrentDictionary<int, Item> Balls;
        private Item[] gates;
//        private readonly object _lock = new object();

        public Soccer(Room room)
        {
            _room = room;
            gates = new Item[4];
            Balls = new ConcurrentDictionary<int, Item>();
            GameIsStarted = false;
        }

        public bool GameIsStarted { get; private set; }

        public void StopGame(bool userTriggered = false)
        {
            GameIsStarted = false;

            if (!userTriggered)
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
        }

        public void StartGame() => GameIsStarted = true;

        public void AddBall(Item item)
        {
            if (!OblivionServer.GetGame().GetRoomManager().LoadedBallRooms.Contains(_room))
                OblivionServer.GetGame().GetRoomManager().LoadedBallRooms.Add(_room);

            Balls.TryAdd(item.Id, item);
        }

        public void RemoveBall(int itemID)
        {
            var item = _room.GetRoomItemHandler().GetItem(itemID);
            if (item == null)
                return;
            item.ballIsMoving = false;
            Balls.TryRemove(itemID, out item);

            if (OblivionServer.GetGame().GetRoomManager().LoadedBallRooms.Contains(_room))
                OblivionServer.GetGame().GetRoomManager().LoadedBallRooms.Remove(_room);
        }

        internal void OnCycle()
        {
            if (Balls == null)
                return;
            foreach (var ball in Balls.Values)
            {
                if (ball == null)
                    return;
                if (ball.ballIsMoving)
                {
                    MoveBallProcess(ball, null);
                }
            }
        }


        internal void MoveBallProcess(Item item, RoomUser user)
        {
            {
                var tryes = 0;
                var newX = item.Coordinate.X;
                var newY = item.Coordinate.Y;

                if (item.ballMover == null && item.ballMover != user)
                    item.ballMover = user;

                while (tryes < 3)
                {
                    if (_room == null)
                    {
                        if (_room.GetGameMap() == null)
                            _room.FixGameMap();
                        return;
                    }

                    if (item.Direction == Direction.Null)
                    {
                        item.ballIsMoving = false;
                        break;
                    }

                    var resetX = newX;
                    var resetY = newY;

                    ComeDirection.GetNewCoords(item.Direction, ref newX, ref newY);

                    var trollface = false;

                    if (_room.GetGameMap().SquareHasUsers(newX, newY)) // break 100 %, cannot return the ball
                    {
                        if (item.ExtraData != "55" && item.ExtraData != "44")
                        {
                            item.ballIsMoving = false;
                            break;
                        }
                        trollface = true;
                    }

                    if (trollface == false)
                        if (!_room.GetGameMap().itemCanBePlacedHere(newX, newY))
                        {
                            item.Direction = ComeDirection.InverseDirections(_room, item.Direction, newX, newY);
                            newX = resetX;
                            newY = resetY;
                            tryes++;
                            if (tryes > 2)
                                item.ballIsMoving = false;
                            continue;
                        }

                    if (MoveBall(item, item.ballMover, newX, newY))
                    {
                        item.ballIsMoving = false;
                        break;
                    }

                    int Number;
                    int.TryParse(item.ExtraData, out Number);
                    if (Number > 11)
                        item.ExtraData = (int.Parse(item.ExtraData) - 11).ToString();

                    item._iBallValue++;

                    if (item._iBallValue > 6)
                    {
                        item.ballIsMoving = false;
                        item._iBallValue = 1;
                        item.ballMover = null;
                    }
                    break;
                }
            }
            //            }).Start();
        }

        internal void OnUserWalk(RoomUser User)
        {
            if (User == null)
                return;

            foreach (var ball in Balls.Values)
            {
                if (ball == null)
                    return;

                if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.GoalX == ball.GetX &&
                    User.GoalY == ball.GetY && User.handelingBallStatus == 0) // super chute.
                {
                    var userPoint = new Point(User.X, User.Y);
                    ball.ExtraData = "55";
                    ball.ballIsMoving = true;
                    ball._iBallValue = 1;
                    MoveBall(ball, User, userPoint);
                }
                else if (User.X == ball.GetX && User.Y == ball.GetY && User.handelingBallStatus == 0)
                {
                    var userPoint = new Point(User.SetX, User.SetY);
                    ball.ExtraData = "55";
                    ball.ballIsMoving = true;
                    ball._iBallValue = 1;
                    MoveBall(ball, User, userPoint);
                }
                else
                {
                    if (User.handelingBallStatus == 0 && User.GoalX == ball.GetX && User.GoalY == ball.GetY)
                        return;

                    if (User.SetX == ball.GetX && User.SetY == ball.GetY && User.IsWalking &&
                        (User.X != User.GoalX || User.Y != User.GoalY))
                    {
                        User.handelingBallStatus = 1;
                        var _comeDirection = ComeDirection.GetComeDirection(new Point(User.X, User.Y), ball.Coordinate);
                        if (_comeDirection != Direction.Null)
                        {
                            var NewX = User.SetX;
                            var NewY = User.SetY;

                            ComeDirection.GetNewCoords(_comeDirection, ref NewX, ref NewY);
                            if (ball.GetRoom().GetGameMap().ValidTile(NewX, NewY))
                            {
                                ball.ExtraData = "11";
                                MoveBall(ball, User, NewX, NewY);
                            }
                        }
                    }
                }
            }
        }


        public void RegisterGate(Item item)
        {
            if (gates[0] == null)
            {
                item.team = TEAM.BLUE;
                gates[0] = item;
            }
            else if (gates[1] == null)
            {
                item.team = TEAM.RED;
                gates[1] = item;
            }
            else if (gates[2] == null)
            {
                item.team = TEAM.GREEN;
                gates[2] = item;
            }
            else if (gates[3] == null)
            {
                item.team = TEAM.YELLOW;
                gates[3] = item;
            }
        }

        public void UnRegisterGate(Item item)
        {
            switch (item.team)
            {
                case TEAM.BLUE:
                {
                    gates[0] = null;
                    break;
                }
                case TEAM.RED:
                {
                    gates[1] = null;
                    break;
                }
                case TEAM.GREEN:
                {
                    gates[2] = null;
                    break;
                }
                case TEAM.YELLOW:
                {
                    gates[3] = null;
                    break;
                }
            }
        }

        public void onGateRemove(Item item)
        {
            switch (item.GetBaseItem().InteractionType)
            {
                case InteractionType.FootballGoalRed:
                case InteractionType.Footballcounterred:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.RED);
                    break;
                }
                case InteractionType.FootballGoalGreen:
                case InteractionType.Footballcountergreen:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.GREEN);
                    break;
                }
                case InteractionType.FootballGoalBlue:
                case InteractionType.Footballcounterblue:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.BLUE);
                    break;
                }
                case InteractionType.FootballGoalYellow:
                case InteractionType.Footballcounteryellow:
                {
                    _room.GetGameManager().RemoveFurnitureFromTeam(item, TEAM.YELLOW);
                    break;
                }
            }
        }

        internal bool MoveBall(Item item, RoomUser mover, int newX, int newY)
        {
//            lock (_lock)
            {
                if (item?.GetBaseItem() == null /*|| mover == null || mover.GetHabbo() == null*/)
                    return false;
                if (item.ballIsMoving)
                {
                    if (item.ExtraData == "55" || item.ExtraData == "44") // puede ser un cañito? o.O
                    {
                        var randomValue = new Random().Next(1, 7);
                        if (randomValue != 5) // no cañito de CR7
                            if (!_room.GetGameMap().itemCanBePlacedHere(newX, newY))
                                return false;
                    }
                }
                else
                {
                    if (!_room.GetGameMap().itemCanBePlacedHere(newX, newY))
                        return false;
                }

                var oldRoomCoord = item.Coordinate;

                var mMessage2 = new ServerPacket(ServerPacketHeader.ObjectUpdateMessageComposer); // Cf
                mMessage2.WriteInteger(item.Id);
                mMessage2.WriteInteger(item.GetBaseItem().SpriteId);
                mMessage2.WriteInteger(newX);
                mMessage2.WriteInteger(newY);
                mMessage2.WriteInteger(4); // rot;
                mMessage2.WriteString($"{TextHandling.GetString(item.GetZ):0.00}");
                mMessage2.WriteString($"{TextHandling.GetString(item.GetZ):0.00}");
                mMessage2.WriteInteger(0);
                mMessage2.WriteInteger(0);
                mMessage2.WriteString(item.ExtraData);
                mMessage2.WriteInteger(-1);
                mMessage2.WriteInteger(0);
                mMessage2.WriteInteger(0); //owner id
                _room.SendFastMessage(mMessage2);

                if (oldRoomCoord.X == newX && oldRoomCoord.Y == newY)
                    return false;

                item.SetState(newX, newY, item.GetZ,
                    Gamemap.GetAffectedTiles(item.GetBaseItem().Length, item.GetBaseItem().Width, newX, newY,
                        item.Rotation));

                if (mover != null)
                    _room.OnUserShoot(mover, item);

                return false;
            }
        }

        internal void MoveBall(Item item, RoomUser mover, Point user)
        {
            try
            {
                item.Direction = ComeDirection.GetComeDirection(user, item.Coordinate);
                if (item.Direction != Direction.Null)
                    MoveBallProcess(item, mover);

                _room.OnUserShoot(mover, item);
            }
            catch
            {
            }
        }

        public void Dispose()
        {
            Array.Clear(gates, 0, gates.Length);
            gates = null;
            _room = null;
            Balls = null;
        }
    }
}