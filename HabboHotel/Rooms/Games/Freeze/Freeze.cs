#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Rooms.Freeze;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Rooms.Games.Teams;

#endregion

namespace Oblivion.HabboHotel.Rooms.Games.Freeze
{
    public class Freeze
    {
        private readonly ConcurrentDictionary<int, Item> _freezeBlocks;
        private readonly ConcurrentDictionary<int, Item> _freezeTiles;
        private Random _random;
        private Room _room;

        public Freeze(Room room)
        {
            _room = room;
            GameIsStarted = false;
            ExitTeleports = new ConcurrentDictionary<int, Item>();
            _random = new Random();
            _freezeTiles = new ConcurrentDictionary<int, Item>();
            _freezeBlocks = new ConcurrentDictionary<int, Item>();
        }

        public bool GameIsStarted { get; private set; }

        public ConcurrentDictionary<int, Item> ExitTeleports { get; }

        public void AddExitTile(Item Item)
        {
            if (!ExitTeleports.ContainsKey(Item.Id))
                ExitTeleports.TryAdd(Item.Id, Item);
        }

        public void RemoveExitTile(int Id)
        {
            Item Temp;
            if (ExitTeleports.ContainsKey(Id))
                ExitTeleports.TryRemove(Id, out Temp);
        }

        public Item GetRandomExitTile()
            => ExitTeleports.Values.ToList()[OblivionServer.GetRandomNumber(0, ExitTeleports.Count - 1)];

        public void StartGame()
        {
            GameIsStarted = true;
            CountTeamPoints();
            ResetGame();

            if (ExitTeleports.Count > 0)
                foreach (var ExitTile in ExitTeleports.Values.ToList())
                {
                    if (ExitTile.ExtraData == "0" || string.IsNullOrEmpty(ExitTile.ExtraData))
                        ExitTile.ExtraData = "1";

                    ExitTile.UpdateState();
                }

            _room.GetGameManager().LockGates();
        }

        public void StopGame(bool userTriggered = false)
        {
            GameIsStarted = false;
            _room.GetGameManager().UnlockGates();
            _room.GetGameManager().StopGame();

            ResetGame();

            if (ExitTeleports.Count > 0)
                foreach (var ExitTile in ExitTeleports.Values.ToList())
                {
                    if (ExitTile.ExtraData == "1" || string.IsNullOrEmpty(ExitTile.ExtraData))
                        ExitTile.ExtraData = "0";

                    ExitTile.UpdateState();
                }

            var Winners = _room.GetGameManager().GetWinningTeam();
            foreach (var User in _room.GetRoomUserManager().GetUserList().ToList())
            {
                User.FreezeLives = 0;
                if (User.Team == Winners)
                {
                    User.UnIdle();
                    User.DanceId = 0;
                    _room.SendMessage(new ActionComposer(User.VirtualId, 1));
                }

                if (ExitTeleports.Count > 0)
                {
                    var tile = _freezeTiles.Values.FirstOrDefault(x => x.GetX == User.X && x.GetY == User.Y);
                    if (tile != null)
                    {
                        var ExitTle = GetRandomExitTile();

                        if (ExitTle != null)
                        {
                            _room.GetGameMap().UpdateUserMovement(User.Coordinate, ExitTle.Coordinate, User);
                            User.SetPos(ExitTle.GetX, ExitTle.GetY, ExitTle.GetZ);
                            User.UpdateNeeded = true;

                            if (User.IsAsleep)
                                User.UnIdle();
                        }
                    }
                }
            }

            if (!userTriggered)
                _room.GetWired().TriggerEvent(WiredBoxType.TriggerGameEnds, null);
        }

        public void CycleUser(RoomUser User)
        {
            if (User.Freezed)
            {
                User.FreezeCounter++;
                if (User.FreezeCounter > 10)
                {
                    User.Freezed = false;
                    User.FreezeCounter = 0;
                    ActivateShield(User);
                }
            }

            if (User.shieldActive)
            {
                User.shieldCounter++;
                if (User.shieldCounter > 10)
                {
                    User.shieldActive = false;
                    User.shieldCounter = 10;
                    User.ApplyEffect(Convert.ToInt32(User.Team) + 39);
                }
            }
        }

        public void ResetGame()
        {
            foreach (var Item in _freezeTiles.Values.ToList().Where(Item => !string.IsNullOrEmpty(Item.ExtraData)))
            {
                Item.interactionCountHelper = 0;
                Item.ExtraData = "";
                Item.UpdateState(false, true);
                _room.GetGameMap().AddItemToMap(Item, false);
            }

            foreach (var Item in _freezeBlocks.Values.Where(Item => !string.IsNullOrEmpty(Item.ExtraData)))
            {
                Item.ExtraData = "";
                Item.UpdateState(false, true);
                _room.GetGameMap().AddItemToMap(Item, false);
            }
        }

        public void OnUserWalk(RoomUser User)
        {
            if (!GameIsStarted || User.Team == TEAM.NONE)
                return;

            foreach (
                var Item in
                _freezeTiles.Values.ToList()
                    .Where(Item => User.GoalX == Item.GetX && User.GoalY == Item.GetY && User.FreezeInteracting)
                    .Where(Item => Item.interactionCountHelper == 0))
            {
                Item.interactionCountHelper = 1;
                Item.ExtraData = "1000";
                Item.UpdateState();
                Item.InteractingUser = User.UserId;
                Item.freezePowerUp = User.banzaiPowerUp;
                Item.RequestUpdate(4, true);

                switch (User.banzaiPowerUp)
                {
                    case FreezePowerUp.GREENARROW:
                    case FreezePowerUp.ORANGESNOWBALL:
                    {
                        User.banzaiPowerUp = FreezePowerUp.NONE;
                        break;
                    }
                }
                break;
            }

            foreach (
                var Item in
                _freezeBlocks.Values.ToList()
                    .Where(Item => User.GoalX == Item.GetX && User.GoalY == Item.GetY)
                    .Where(Item => Item.freezePowerUp != FreezePowerUp.NONE))
                PickUpPowerUp(Item, User);
        }

        private void CountTeamPoints()
        {
            _room.GetGameManager().Reset();

            foreach (
                var User in
                _room.GetRoomUserManager()
                    .GetUserList()
                    .ToList()
                    .Where(User => !User.IsBot && User.Team != TEAM.NONE && User.GetClient() != null))
            {
                User.banzaiPowerUp = FreezePowerUp.NONE;
                User.FreezeLives = 3;
                User.shieldActive = false;
                User.shieldCounter = 11;

                _room.GetGameManager().AddPointToTeam(User.Team, 30);
                User.GetClient().SendMessage(new UpdateFreezeLivesComposer(User.InternalRoomID, User.FreezeLives));
            }
        }

        public void onFreezeTiles(Item item, FreezePowerUp powerUp)
        {
            List<Item> items;

            switch (powerUp)
            {
                case FreezePowerUp.BLUEARROW:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    break;
                }

                case FreezePowerUp.GREENARROW:
                {
                    items = GetDiagonalItems(item.GetX, item.GetY, 5);
                    break;
                }

                case FreezePowerUp.ORANGESNOWBALL:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 5);
                    items.AddRange(GetDiagonalItems(item.GetX, item.GetY, 5));
                    break;
                }

                default:
                {
                    items = GetVerticalItems(item.GetX, item.GetY, 3);
                    break;
                }
            }
            HandleBanzaiFreezeItems(items);
        }

        private static void ActivateShield(RoomUser User)
        {
            User.ApplyEffect(Convert.ToInt32(User.Team + 48));
            User.shieldActive = true;
            User.shieldCounter = 0;
        }

        private void HandleBanzaiFreezeItems(List<Item> items)
        {
            foreach (var item in items.ToList())
                switch (item.GetBaseItem().InteractionType)
                {
                    case InteractionType.FreezeTile:
                    {
                        item.ExtraData = "11000";
                        item.UpdateState(false, true);
                        continue;
                    }

                    case InteractionType.FreezeTileBlock:
                    {
                        SetRandomPowerUp(item);
                        item.UpdateState(false, true);
                        continue;
                    }
                    default:
                    {
                        continue;
                    }
                }
        }

        private void SetRandomPowerUp(Item item)
        {
            if (!string.IsNullOrEmpty(item.ExtraData))
                return;

            var next = _random.Next(1, 14);

            switch (next)
            {
                case 2:
                {
                    item.ExtraData = "2000";
                    item.freezePowerUp = FreezePowerUp.BLUEARROW;
                    break;
                }
                case 3:
                {
                    item.ExtraData = "3000";
                    item.freezePowerUp = FreezePowerUp.SNOWBALLS;
                    break;
                }
                case 4:
                {
                    item.ExtraData = "4000";
                    item.freezePowerUp = FreezePowerUp.GREENARROW;
                    break;
                }
                case 5:
                {
                    item.ExtraData = "5000";
                    item.freezePowerUp = FreezePowerUp.ORANGESNOWBALL;
                    break;
                }
                case 6:
                {
                    item.ExtraData = "6000";
                    item.freezePowerUp = FreezePowerUp.HEART;
                    break;
                }
                case 7:
                {
                    item.ExtraData = "7000";
                    item.freezePowerUp = FreezePowerUp.SHIELD;
                    break;
                }
                default:
                {
                    item.ExtraData = "1000";
                    item.freezePowerUp = FreezePowerUp.NONE;
                    break;
                }
            }

            _room.GetGameMap().RemoveFromMap(item, false);
            item.UpdateState(false, true);
        }

        private void PickUpPowerUp(Item item, RoomUser User)
        {
            switch (item.freezePowerUp)
            {
                case FreezePowerUp.HEART:
                {
                    if (User.FreezeLives < 5)
                    {
                        User.FreezeLives++;
                        _room.GetGameManager().AddPointToTeam(User.Team, 10);
                    }

                    User.GetClient().SendMessage(new UpdateFreezeLivesComposer(User.InternalRoomID, User.FreezeLives));
                    break;
                }
                case FreezePowerUp.SHIELD:
                {
                    ActivateShield(User);
                    break;
                }
                case FreezePowerUp.BLUEARROW:
                case FreezePowerUp.GREENARROW:
                case FreezePowerUp.ORANGESNOWBALL:
                {
                    User.banzaiPowerUp = item.freezePowerUp;
                    break;
                }
            }

            item.freezePowerUp = FreezePowerUp.NONE;
            item.ExtraData = "1" + item.ExtraData;
            item.UpdateState(false, true);
        }

        public void AddFreezeTile(Item Item)
        {
            if (!_freezeTiles.ContainsKey(Item.Id))
                _freezeTiles.TryAdd(Item.Id, Item);
        }

        public void RemoveFreezeTile(int itemID)
        {
            Item Item;
            if (_freezeTiles.ContainsKey(itemID))
                _freezeTiles.TryRemove(itemID, out Item);
        }

        public void AddFreezeBlock(Item Item)
        {
            if (!_freezeBlocks.ContainsKey(Item.Id))
                _freezeBlocks.TryAdd(Item.Id, Item);
        }

        public void RemoveFreezeBlock(int ItemID)
        {
            Item Item;
            _freezeBlocks.TryRemove(ItemID, out Item);
        }

        private void HandleUserFreeze(Point point)
        {
            var user = _room?.GetGameMap().GetRoomUsers(point).FirstOrDefault();
            if (user != null)
            {
                if (user.IsWalking && user.SetX != point.X && user.SetY != point.Y)
                    return;

                FreezeUser(user);
            }
        }

        private void FreezeUser(RoomUser User)
        {
            if (User.IsBot || User.shieldActive || User.Team == TEAM.NONE || User.Freezed)
                return;

            User.Freezed = true;
            User.FreezeCounter = 0;

            User.FreezeLives--;
            if (User.FreezeLives <= 0)
            {
                User.GetClient().SendMessage(new UpdateFreezeLivesComposer(User.InternalRoomID, User.FreezeLives));

                User.ApplyEffect(-1);
                _room.GetGameManager().AddPointToTeam(User.Team, -10);
                var t = _room.GetTeamManagerForFreeze();
                t.OnUserLeave(User);
                User.Team = TEAM.NONE;
                if (ExitTeleports.Count > 0)
                    _room.GetGameMap().TeleportToItem(User, GetRandomExitTile());

                User.Freezed = false;
                User.SetStep = false;
                User.IsWalking = false;
                User.UpdateNeeded = true;

                if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 &&
                    t.YellowTeam.Count > 0)
                    StopGame(); // yellow team win
                else if (t.BlueTeam.Count > 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // blue team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count > 0 && t.GreenTeam.Count <= 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // red team win
                else if (t.BlueTeam.Count <= 0 && t.RedTeam.Count <= 0 && t.GreenTeam.Count > 0 &&
                         t.YellowTeam.Count <= 0)
                    StopGame(); // green team win
                return;
            }

            _room.GetGameManager().AddPointToTeam(User.Team, -10);
            User.ApplyEffect(12);

            User.GetClient().SendMessage(new UpdateFreezeLivesComposer(User.InternalRoomID, User.FreezeLives));
        }

        private List<Item> GetVerticalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y);

                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x, y + i);

                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x - i, y);
                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 1; i < length; i++)
            {
                var point = new Point(x, y - i);
                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetDiagonalItems(int x, int y, int length)
        {
            var totalItems = new List<Item>();

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y + i);

                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x - i, y - i);
                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x - i, y + i);
                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            for (var i = 0; i < length; i++)
            {
                var point = new Point(x + i, y - i);
                var items = GetItemsForSquare(point);
                if (!SquareGotFreezeTile(items))
                    break;

                HandleUserFreeze(point);
                totalItems.AddRange(items);

                if (SquareGotFreezeBlock(items))
                    break;
            }

            return totalItems;
        }

        private List<Item> GetItemsForSquare(Point point) => _room.GetGameMap().GetCoordinatedItems(point);

        private static bool SquareGotFreezeTile(IEnumerable<Item> items) => items.Any(item => item.GetBaseItem().InteractionType == InteractionType.FreezeTile);

        private static bool SquareGotFreezeBlock(IEnumerable<Item> items) => items.Any(item => item.GetBaseItem().InteractionType == InteractionType.FreezeTileBlock);

        public void Dispose()
        {
            _freezeBlocks.Clear();
            _freezeTiles.Clear();
            ExitTeleports.Clear();
           
            _room = null;
            _random = null;
        }
    }
}