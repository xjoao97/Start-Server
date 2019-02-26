#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Interactor;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Rooms.Games.Football;
using Oblivion.HabboHotel.Rooms.Games.Freeze;
using Oblivion.HabboHotel.Rooms.Games.Teams;
using Oblivion.Utilities;

#endregion

namespace Oblivion.HabboHotel.Items
{
    public class Item
    {
        private Room _room;
        public int BaseItem;
        public string ExtraData;
        public string Figure;
        public FreezePowerUp freezePowerUp;
        public string Gender;
        public int GroupId;
        public int Id;
        public int interactingBallUser;
        public int InteractingUser;
        public int InteractingUser2;
        public byte interactionCount;
        public byte interactionCountHelper;
        public int LimitedNo;
        public int LimitedTot;
        public bool MagicRemove = false;
        public bool pendingReset = false;
        public int RoomId;
        public int Rotation;

        //ball
        internal bool ballIsMoving;
        public Direction Direction;
        internal RoomUser ballMover;
        internal int _iBallValue;

        public TEAM team;
        public int UpdateCounter;
        private bool updateNeeded;
        public int UserID;
        public string Username;


        public int value;
        public string wallCoord;

        public Item(int Id, int RoomId, int BaseItem, string ExtraData, int X, int Y, double Z, int Rot, int Userid,
            int Group, int limitedNumber, int limitedStack, string wallCoord, Room Room = null)
        {
            ItemData Data;
            if (!OblivionServer.GetGame().GetItemManager().TryGetItem(BaseItem, out Data))
                return;

            this.Id = Id;

            this.RoomId = RoomId;
            _room = Room;
            //  this.Data = Data;
            this.BaseItem = BaseItem;
            this.ExtraData = ExtraData;
            GroupId = Group;

            GetX = X;
            GetY = Y;
            if (!double.IsInfinity(Z))
                GetZ = Z;
            Rotation = Rot;
            UpdateNeeded = false;
            UpdateCounter = 0;
            InteractingUser = 0;
            InteractingUser2 = 0;
            interactingBallUser = 0;
            interactionCount = 0;
            value = 0;

            UserID = Userid;
            Username = OblivionServer.GetUsernameById(Userid);


            LimitedNo = limitedNumber;
            LimitedTot = limitedStack;

            switch (GetBaseItem().InteractionType)
            {
                case InteractionType.Teleport:
                    RequestUpdate(0, true);
                    break;

                case InteractionType.Hopper:
                    RequestUpdate(0, true);
                    break;

                case InteractionType.Roller:
                    IsRoller = true;
                    if (RoomId > 0)
                        GetRoom().GetRoomItemHandler().GotRollers = true;
                    break;

                case InteractionType.Banzaiscoreblue:
                case InteractionType.Footballcounterblue:
                case InteractionType.Banzaigateblue:
                case InteractionType.FreezeBlueGate:
                case InteractionType.Freezebluecounter:
                    team = TEAM.BLUE;
                    break;

                case InteractionType.Banzaiscoregreen:
                case InteractionType.Footballcountergreen:
                case InteractionType.Banzaigategreen:
                case InteractionType.Freezegreencounter:
                case InteractionType.FreezeGreenGate:
                    team = TEAM.GREEN;
                    break;

                case InteractionType.Banzaiscorered:
                case InteractionType.Footballcounterred:
                case InteractionType.Banzaigatered:
                case InteractionType.Freezeredcounter:
                case InteractionType.FreezeRedGate:
                    team = TEAM.RED;
                    break;

                case InteractionType.Banzaiscoreyellow:
                case InteractionType.Footballcounteryellow:
                case InteractionType.Banzaigateyellow:
                case InteractionType.Freezeyellowcounter:
                case InteractionType.FreezeYellowGate:
                    team = TEAM.YELLOW;
                    break;

                case InteractionType.Banzaitele:
                {
                    this.ExtraData = "";
                    break;
                }
            }

            IsWallItem = GetBaseItem().Type.ToString().ToLower() == "i";
            IsFloorItem = GetBaseItem().Type.ToString().ToLower() == "s";

            if (IsFloorItem)
            {
                GetAffectedTiles = Gamemap.GetAffectedTiles(GetBaseItem().Length, GetBaseItem().Width, GetX, GetY,
                    Rot);
            }
            else if (IsWallItem)
            {
                this.wallCoord = wallCoord;
                IsWallItem = true;
                IsFloorItem = false;
                GetAffectedTiles = new Dictionary<int, ThreeDCoord>();
            }
        }

        public ItemData Data { get; set; }

        public Dictionary<int, ThreeDCoord> GetAffectedTiles { get; private set; }

        public int GetX { get; set; }

        public int GetY { get; set; }

        public double GetZ { get; set; }

        public bool UpdateNeeded
        {
            get { return updateNeeded; }
            set
            {
                if (value && GetRoom() != null)
                    GetRoom().GetRoomItemHandler().QueueRoomItemUpdate(this);
                updateNeeded = value;
            }
        }

        public bool IsRoller { get; }

        public Point Coordinate => new Point(GetX, GetY);

        public List<Point> GetCoords
        {
            get
            {
                var toReturn = new List<Point> {Coordinate};

                toReturn.AddRange(GetAffectedTiles.Values.Select(tile => new Point(tile.X, tile.Y)));

                return toReturn;
            }
        }

        public double TotalHeight
        {
            get
            {
                var CurHeight = 0.0;
                int num2;

                if (GetBaseItem().AdjustableHeights.Count > 1)
                    if (int.TryParse(ExtraData, out num2) && GetBaseItem().AdjustableHeights.Count - 1 >= num2)
                        CurHeight = GetZ + GetBaseItem().AdjustableHeights[num2];

                if (CurHeight <= 0.0)
                    CurHeight = GetZ + GetBaseItem().Height;

                return CurHeight;
            }
        }

        public bool IsWallItem { get; }

        public bool IsFloorItem { get; }

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                switch (Rotation)
                {
                    case 0:
                        Sq.Y--;
                        break;
                    case 2:
                        Sq.X++;
                        break;
                    case 4:
                        Sq.Y++;
                        break;
                    case 6:
                        Sq.X--;
                        break;
                }

                return Sq;
            }
        }

        public Point SquareBehind
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                switch (Rotation)
                {
                    case 0:
                        Sq.Y++;
                        break;
                    case 2:
                        Sq.X--;
                        break;
                    case 4:
                        Sq.Y--;
                        break;
                    case 6:
                        Sq.X++;
                        break;
                }

                return Sq;
            }
        }

        public Point SquareLeft
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                switch (Rotation)
                {
                    case 0:
                        Sq.X++;
                        break;
                    case 2:
                        Sq.Y--;
                        break;
                    case 4:
                        Sq.X--;
                        break;
                    case 6:
                        Sq.Y++;
                        break;
                }

                return Sq;
            }
        }

        public Point SquareRight
        {
            get
            {
                var Sq = new Point(GetX, GetY);

                switch (Rotation)
                {
                    case 0:
                        Sq.X--;
                        break;
                    case 2:
                        Sq.Y++;
                        break;
                    case 4:
                        Sq.X++;
                        break;
                    case 6:
                        Sq.Y--;
                        break;
                }
                return Sq;
            }
        }

        public IFurniInteractor Interactor
        {
            get
            {
                if (IsWired)
                    return new InteractorWired();

                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.Gate:
                        return new InteractorGate();

                    case InteractionType.Teleport:
                        return new InteractorTeleport();

                    case InteractionType.Hopper:
                        return new InteractorHopper();

                    case InteractionType.Bottle:
                        return new InteractorSpinningBottle();

                    case InteractionType.Dice:
                        return new InteractorDice();

                    case InteractionType.HabboWheel:
                        return new InteractorHabboWheel();

                    case InteractionType.LoveShuffler:
                        return new InteractorLoveShuffler();

                    case InteractionType.Jukebox:
                        return new InteractorJukebox();

                    case InteractionType.MusicDisc:
                        return new InteractorMusicDisc();

                    case InteractionType.OneWayGate:
                        return new InteractorOneWayGate();

                    case InteractionType.Alert:
                        return new InteractorAlert();

                    case InteractionType.VendingMachine:
                        return new InteractorVendor();

                    case InteractionType.Scoreboard:
                        return new InteractorScoreboard();

                    case InteractionType.PuzzleBox:
                        return new InteractorPuzzleBox();

                    case InteractionType.Mannequin:
                        return new InteractorMannequin();

                    case InteractionType.Banzaicounter:
                        return new InteractorBanzaiTimer();

                    case InteractionType.Freezetimer:
                        return new InteractorFreezeTimer();

                    case InteractionType.FreezeTileBlock:
                    case InteractionType.FreezeTile:
                        return new InteractorFreezeTile();

                    case InteractionType.Footballcounterblue:
                    case InteractionType.Footballcountergreen:
                    case InteractionType.Footballcounterred:
                    case InteractionType.Footballcounteryellow:
                        return new InteractorScoreCounter();

                    case InteractionType.Banzaiscoreblue:
                    case InteractionType.Banzaiscoregreen:
                    case InteractionType.Banzaiscorered:
                    case InteractionType.Banzaiscoreyellow:
                        return new InteractorBanzaiScoreCounter();

                    case InteractionType.WfFloorSwitch1:
                    case InteractionType.WfFloorSwitch2:
                    case InteractionType.WalkSwitch:
                        return new InteractorSwitch();

                    case InteractionType.Lovelock:
                        return new InteractorLoveLock();

                    case InteractionType.Cannon:
                        return new InteractorCannon();

                    case InteractionType.Counter:
                        return new InteractorCounter();

                    case InteractionType.None:
                    default:
                        return new InteractorGenericSwitch();
                }
            }
        }

        public bool IsWired
        {
            get
            {
                switch (GetBaseItem().InteractionType)
                {
                    case InteractionType.WiredEffect:
                    case InteractionType.WiredTrigger:
                    case InteractionType.WiredCondition:
                    case InteractionType.WiredCustom:
                        return true;
                }

                return false;
            }
        }

        public List<Point> GetSides()
        {
            var toReturn = new List<Point> {SquareBehind, SquareInFront, SquareLeft, SquareRight, Coordinate};
            return toReturn;
        }

        public void SetState(int pX, int pY, double pZ, Dictionary<int, ThreeDCoord> Tiles)
        {
            GetX = pX;
            GetY = pY;
            if (!double.IsInfinity(pZ))
                GetZ = pZ;
            GetAffectedTiles = Tiles;
        }

        public void Destroy()
        {
            _room = null;
            Data = null;
            GetAffectedTiles.Clear();
        }

        public void ProcessUpdates()
        {
            try
            {
                UpdateCounter--;

                if (UpdateCounter > 0) return;
                UpdateNeeded = false;
                UpdateCounter = 0;

                RoomUser User;
                RoomUser User2;

                switch (GetBaseItem().InteractionType)
                {
                        #region Group Gates

                    case InteractionType.GuildGate:
                    {
                        if (ExtraData == "1")
                            if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            else
                            {
                                RequestUpdate(2, false);
                            }
                        break;
                    }

                        #endregion

                        #region Item Effects

                    case InteractionType.Effect:
                    {
                        if (ExtraData == "1")
                            if (GetRoom().GetRoomUserManager().GetUserForSquare(GetX, GetY) == null)
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                            else
                            {
                                RequestUpdate(2, false);
                            }
                        break;
                    }

                        #endregion

                        #region One way gates

                    case InteractionType.OneWayGate:

                        User = null;

                        if (InteractingUser > 0)
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                        if (User != null && User.X == GetX && User.Y == GetY)
                        {
                            ExtraData = "1";

                            User.MoveTo(SquareBehind);
                            User.InteractingGate = false;
                            User.GateId = 0;
                            RequestUpdate(1, false);
                            UpdateState(false, true);
                        }
                        else if (User != null && User.Coordinate == SquareBehind)
                        {
                            User.UnlockWalking();

                            ExtraData = "0";
                            InteractingUser = 0;
                            User.InteractingGate = false;
                            User.GateId = 0;
                            UpdateState(false, true);
                        }
                        else if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }

                        if (User == null)
                            InteractingUser = 0;

                        break;

                        #endregion

                        #region VIP Gate

                    case InteractionType.GateVip:

                        User = null;


                        if (InteractingUser > 0)
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                        var NewY = 0;
                        var NewX = 0;

                        if (User != null && User.X == GetX && User.Y == GetY)
                        {
                            switch (User.RotBody)
                            {
                                case 4:
                                    NewY = 1;
                                    break;
                                case 0:
                                    NewY = -1;
                                    break;
                                case 6:
                                    NewX = -1;
                                    break;
                                case 2:
                                    NewX = 1;
                                    break;
                            }


                            User.MoveTo(User.X + NewX, User.Y + NewY);
                            RequestUpdate(1, false);
                        }
                        else if (User != null &&
                                 (User.Coordinate == SquareBehind || User.Coordinate == SquareInFront))
                        {
                            User.UnlockWalking();

                            ExtraData = "0";
                            InteractingUser = 0;
                            UpdateState(false, true);
                        }
                        else if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }

                        if (User == null)
                            InteractingUser = 0;

                        break;

                        #endregion

                        #region Hopper

                    case InteractionType.Hopper:
                    {
                        // User = null;
                        //User2 = null;
                        var showHopperEffect = false;
                        var keepDoorOpen = false;
                        var Pause = 0;


                        // Do we have a primary user that wants to go somewhere?
                        if (InteractingUser > 0)
                        {
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                            // Is this user okay?
                            if (User != null)
                                if (User.Coordinate == Coordinate)
                                {
                                    //Remove the user from the square
                                    User.AllowOverride = false;
                                    if (User.TeleDelay == 0)
                                    {
                                        var RoomHopId = ItemHopperFinder.GetAHopper(User.RoomId);
                                        var NextHopperId = ItemHopperFinder.GetHopperId(RoomHopId);

                                        if (!User.IsBot && User.GetClient() != null &&
                                            User.GetClient().GetHabbo() != null)
                                        {
                                            User.GetClient().GetHabbo().IsHopping = true;
                                            User.GetClient().GetHabbo().HopperId = NextHopperId;
                                            User.GetClient().GetHabbo().PrepareRoom(RoomHopId, "");
                                            //User.GetClient().SendMessage(new RoomForwardComposer(RoomHopId));
                                            InteractingUser = 0;
                                        }
                                    }
                                    else
                                    {
                                        User.TeleDelay--;
                                        showHopperEffect = true;
                                    }
                                }
                                // Is he in front of the tele?
                                else if (User.Coordinate == SquareInFront)
                                {
                                    User.AllowOverride = true;
                                    keepDoorOpen = true;

                                    // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                    if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                        User.ClearMovement(true);

                                    User.CanWalk = false;
                                    User.AllowOverride = true;

                                    // Move into the tele
                                    User.MoveTo(Coordinate.X, Coordinate.Y, true);
                                }
                                // Not even near, do nothing and move on for the next user.
                                else
                                {
                                    InteractingUser = 0;
                                }
                            else
                                InteractingUser = 0;
                        }

                        if (InteractingUser2 > 0)
                        {
                            User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                            // Is this user okay?
                            if (User2 != null)
                            {
                                // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                keepDoorOpen = true;
                                User2.UnlockWalking();
                                User2.MoveTo(SquareInFront);
                            }

                            // This is a one time thing, whether the user's valid or not.
                            InteractingUser2 = 0;
                        }

                        // Set the new item state, by priority
                        if (keepDoorOpen)
                        {
                            if (ExtraData != "1")
                            {
                                ExtraData = "1";
                                UpdateState(false, true);
                            }
                        }
                        else if (showHopperEffect)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                        }
                        else
                        {
                            if (ExtraData != "0")
                                if (Pause == 0)
                                {
                                    ExtraData = "0";
                                    UpdateState(false, true);
                                    //Pause = 2;
                                }
                                else
                                {
                                    Pause--;
                                }
                        }

                        // We're constantly going!
                        RequestUpdate(1, false);
                        break;
                    }

                        #endregion

                        #region Teleports

                    case InteractionType.Teleport:
                    {
                        var keepDoorOpen = false;
                        var showTeleEffect = false;

                        // Do we have a primary user that wants to go somewhere?
                        if (InteractingUser > 0)
                        {
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);

                            // Is this user okay?
                            if (User != null)
                                if (User.Coordinate == Coordinate)
                                {
                                    //Remove the user from the square
                                    User.AllowOverride = false;

                                    if (ItemTeleporterFinder.IsTeleLinked(Id, GetRoom()))
                                    {
                                        showTeleEffect = true;

                                        if (true)
                                        {
                                            // Woop! No more delay.
                                            var TeleId = ItemTeleporterFinder.GetLinkedTele(Id, GetRoom());
                                            var RoomId = ItemTeleporterFinder.GetTeleRoomId(TeleId, GetRoom());

                                            // Do we need to tele to the same room or gtf to another?
                                            if (RoomId == this.RoomId)
                                            {
                                                var Item = GetRoom().GetRoomItemHandler().GetItem(TeleId);

                                                if (Item == null)
                                                {
                                                    User.UnlockWalking();
                                                }
                                                else
                                                {
                                                    // Set pos
                                                    User.SetPos(Item.GetX, Item.GetY, Item.GetZ);
                                                    User.SetRot(Item.Rotation, false);

                                                    // Force tele effect update (dirty)
                                                    Item.ExtraData = "2";
                                                    Item.UpdateState(false, true);

                                                    // Set secondary interacting user
                                                    Item.InteractingUser2 = InteractingUser;
                                                    GetRoom()
                                                        .GetGameMap()
                                                        .RemoveUserFromMap(User, new Point(GetX, GetY));

                                                    InteractingUser = 0;
                                                }
                                            }
                                            else
                                            {
                                                if (User.TeleDelay == 0)
                                                {
                                                    // Let's run the teleport delegate to take futher care of this.. WHY DARIO?!
                                                    if (!User.IsBot && User.GetClient() != null &&
                                                        User.GetClient().GetHabbo() != null)
                                                    {
                                                        User.GetClient().GetHabbo().IsTeleporting = true;
                                                        User.GetClient().GetHabbo().TeleportingRoomID = RoomId;
                                                        User.GetClient().GetHabbo().TeleporterId = TeleId;
                                                        User.GetClient().GetHabbo().PrepareRoom(RoomId, "");
                                                        //User.GetClient().SendMessage(new RoomForwardComposer(RoomId));
                                                        InteractingUser = 0;
                                                    }
                                                }
                                                else
                                                {
                                                    User.TeleDelay--;
                                                }
                                                //OblivionServer.GetGame().GetRoomManager().AddTeleAction(new TeleUserData(User.GetClient().GetMessageHandler(), User.GetClient().GetHabbo(), RoomId, TeleId));
                                            }
                                            GetRoom().GetGameMap().GenerateMaps();
                                            // We're done with this tele. We have another one to bother.
                                        }
                                    }
                                    else
                                    {
                                        // This tele is not linked, so let's gtfo.
                                        User.UnlockWalking();
                                        InteractingUser = 0;
                                    }
                                }
                                // Is he in front of the tele?
                                else if (User.Coordinate == SquareInFront)
                                {
                                    User.AllowOverride = true;
                                    // Open the door
                                    keepDoorOpen = true;

                                    // Lock his walking. We're taking control over him. Allow overriding so he can get in the tele.
                                    if (User.IsWalking && (User.GoalX != GetX || User.GoalY != GetY))
                                        User.ClearMovement(true);

                                    User.CanWalk = false;
                                    User.AllowOverride = true;

                                    // Move into the tele
                                    User.MoveTo(Coordinate.X, Coordinate.Y, true);
                                }
                                // Not even near, do nothing and move on for the next user.
                                else
                                {
                                    InteractingUser = 0;
                                }
                            else
                                InteractingUser = 0;
                        }

                        // Do we have a secondary user that wants to get out of the tele?
                        if (InteractingUser2 > 0)
                        {
                            User2 = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser2);

                            // Is this user okay?
                            if (User2 != null)
                            {
                                // If so, open the door, unlock the user's walking, and try to push him out in the right direction. We're done with him!
                                keepDoorOpen = true;
                                User2.UnlockWalking();
                                User2.MoveTo(SquareInFront);
                            }

                            // This is a one time thing, whether the user's valid or not.
                            InteractingUser2 = 0;
                        }

                        // Set the new item state, by priority
                        if (showTeleEffect)
                        {
                            if (ExtraData != "2")
                            {
                                ExtraData = "2";
                                UpdateState(false, true);
                            }
                        }
                        else if (keepDoorOpen)
                        {
                            if (ExtraData != "1")
                            {
                                ExtraData = "1";
                                UpdateState(false, true);
                            }
                        }
                        else
                        {
                            if (ExtraData != "0")
                            {
                                ExtraData = "0";
                                UpdateState(false, true);
                            }
                        }

                        // We're constantly going!
                        RequestUpdate(1, false);
                        break;
                    }

                        #endregion

                        #region Bottle

                    case InteractionType.Bottle:
                        ExtraData = RandomNumber.GenerateNewRandom(0, 7).ToString();
                        UpdateState();
                        break;

                        #endregion

                        #region Dice

                    case InteractionType.Dice:
                    {
                        string[] numbers = {"1", "2", "3", "4", "5", "6"};
                        if (ExtraData == "-1")
                            ExtraData = RandomizeStrings(numbers)[0];
                        UpdateState();
                    }
                        break;

                        #endregion

                        #region Habbo Wheel

                    case InteractionType.HabboWheel:
                        ExtraData = RandomNumber.GenerateRandom(1, 10).ToString();
                        UpdateState();
                        break;

                        #endregion

                        #region Love Shuffler

                    case InteractionType.LoveShuffler:

                        if (ExtraData == "0")
                        {
                            ExtraData = RandomNumber.GenerateNewRandom(1, 4).ToString();
                            RequestUpdate(20, false);
                        }
                        else
                        {
                            ExtraData = "-1";
                        }

                        UpdateState(false, true);
                        break;

                        #endregion

                        #region Alert

                    case InteractionType.Alert:
                        if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                        break;

                        #endregion

                        #region Vending Machine

                    case InteractionType.VendingMachine:

                        if (ExtraData == "1")
                        {
                            User = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(InteractingUser);
                            if (User == null)
                                break;
                            User.UnlockWalking();
                            if (GetBaseItem().VendingIds.Count > 0)
                            {
                                var randomDrink =
                                    GetBaseItem().VendingIds[
                                        RandomNumber.GenerateRandom(0, GetBaseItem().VendingIds.Count - 1)];
                                User.CarryItem(randomDrink);
                            }


                            InteractingUser = 0;
                            ExtraData = "0";

                            UpdateState(false, true);
                        }
                        break;

                        #endregion

                        #region Scoreboard

                    case InteractionType.Scoreboard:
                    {
                        if (string.IsNullOrEmpty(ExtraData))
                            break;


                        var seconds = 0;

                        try
                        {
                            seconds = int.Parse(ExtraData);
                        }
                        catch
                        {
                        }

                        if (seconds > 0)
                        {
                            if (interactionCountHelper == 1)
                            {
                                seconds--;
                                interactionCountHelper = 0;

                                ExtraData = seconds.ToString();
                                UpdateState();
                            }
                            else
                            {
                                interactionCountHelper++;
                            }

                            UpdateCounter = 1;
                        }
                        else
                        {
                            UpdateCounter = 0;
                        }

                        break;
                    }

                        #endregion

                        #region Banzai Counter

                    case InteractionType.Banzaicounter:
                    {
                        if (string.IsNullOrEmpty(ExtraData))
                            break;

                        var seconds = 0;

                        try
                        {
                            seconds = int.Parse(ExtraData);
                        }
                        catch
                        {
                        }

                        if (seconds > 0)
                        {
                            if (interactionCountHelper == 1)
                            {
                                seconds--;
                                interactionCountHelper = 0;

                                if (GetRoom().GetBanzai().isBanzaiActive)
                                {
                                    ExtraData = seconds.ToString();
                                    UpdateState();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                interactionCountHelper++;
                            }

                            UpdateCounter = 1;
                        }
                        else
                        {
                            UpdateCounter = 0;
                            GetRoom().GetBanzai().BanzaiEnd();
                        }

                        break;
                    }

                        #endregion

                        #region Banzai Tele

                    case InteractionType.Banzaitele:
                    {
                        ExtraData = string.Empty;
                        UpdateState();
                        break;
                    }

                        #endregion

                        #region Banzai Floor

                    case InteractionType.Banzaifloor:
                    {
                        if (value == 3)
                        {
                            if (interactionCountHelper == 1)
                            {
                                interactionCountHelper = 0;

                                switch (team)
                                {
                                    case TEAM.BLUE:
                                    {
                                        ExtraData = "11";
                                        break;
                                    }

                                    case TEAM.GREEN:
                                    {
                                        ExtraData = "8";
                                        break;
                                    }

                                    case TEAM.RED:
                                    {
                                        ExtraData = "5";
                                        break;
                                    }

                                    case TEAM.YELLOW:
                                    {
                                        ExtraData = "14";
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                ExtraData = "";
                                interactionCountHelper++;
                            }

                            UpdateState();

                            interactionCount++;

                            UpdateCounter = interactionCount < 16 ? 1 : 0;
                        }
                        break;
                    }

                        #endregion

                        #region Banzai Puck

                    case InteractionType.Banzaipuck:
                    {
                        if (interactionCount > 4)
                        {
                            interactionCount++;
                            UpdateCounter = 1;
                        }
                        else
                        {
                            interactionCount = 0;
                            UpdateCounter = 0;
                        }

                        break;
                    }

                        #endregion

                        #region Freeze Tile

                    case InteractionType.FreezeTile:
                    {
                        if (InteractingUser > 0)
                        {
                            ExtraData = "11000";
                            UpdateState(false, true);
                            GetRoom().GetFreeze().onFreezeTiles(this, freezePowerUp);
                            InteractingUser = 0;
                            interactionCountHelper = 0;
                        }
                        break;
                    }

                        #endregion

                        #region Football Counter

                    case InteractionType.Counter:
                    {
                        if (string.IsNullOrEmpty(ExtraData))
                            break;

                        var seconds = 0;

                        try
                        {
                            seconds = int.Parse(ExtraData);
                        }
                        catch
                        {
                        }

                        if (seconds > 0)
                        {
                            if (interactionCountHelper == 1)
                            {
                                seconds--;
                                interactionCountHelper = 0;
                                if (GetRoom().GetSoccer().GameIsStarted)
                                {
                                    ExtraData = seconds.ToString();
                                    UpdateState();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                interactionCountHelper++;
                            }

                            UpdateCounter = 1;
                        }
                        else
                        {
                            UpdateNeeded = false;
                            GetRoom().GetSoccer().StopGame();
                        }

                        break;
                    }

                        #endregion

                        #region Freeze Timer

                    case InteractionType.Freezetimer:
                    {
                        if (string.IsNullOrEmpty(ExtraData))
                            break;

                        var seconds = 0;

                        try
                        {
                            seconds = int.Parse(ExtraData);
                        }
                        catch
                        {
                        }

                        if (seconds > 0)
                        {
                            if (interactionCountHelper == 1)
                            {
                                seconds--;
                                interactionCountHelper = 0;
                                if (GetRoom().GetFreeze().GameIsStarted)
                                {
                                    ExtraData = seconds.ToString();
                                    UpdateState();
                                }
                                else
                                {
                                    break;
                                }
                            }
                            else
                            {
                                interactionCountHelper++;
                            }

                            UpdateCounter = 1;
                        }
                        else
                        {
                            UpdateNeeded = false;
                            GetRoom().GetFreeze().StopGame();
                        }

                        break;
                    }

                        #endregion

                        #region Pressure Pad

                    case InteractionType.PressurePad:
                    {
                        ExtraData = "1";
                        UpdateState();
                        break;
                    }

                        #endregion

                        #region Wired

                    case InteractionType.WiredEffect:
                    case InteractionType.WiredCustom:
                    case InteractionType.WiredTrigger:
                    case InteractionType.WiredCondition:
                    {
                        if (ExtraData == "1")
                        {
                            ExtraData = "0";
                            UpdateState(false, true);
                        }
                    }
                        break;

                        #endregion

                        #region Cannon

                    case InteractionType.Cannon:
                    {
                        if (ExtraData != "1")
                            break;

                        #region Target Calculation

                        var TargetStart = Coordinate;
                        var TargetSquares = new List<Point>();
                        switch (Rotation)
                        {
                            case 0:
                            {
                                TargetStart = new Point(GetX - 1, GetY);

                                if (!TargetSquares.Contains(TargetStart))
                                    TargetSquares.Add(TargetStart);

                                for (var I = 1; I <= 3; I++)
                                {
                                    var TargetSquare = new Point(TargetStart.X - I, TargetStart.Y);

                                    if (!TargetSquares.Contains(TargetSquare))
                                        TargetSquares.Add(TargetSquare);
                                }

                                break;
                            }

                            case 2:
                            {
                                TargetStart = new Point(GetX, GetY - 1);

                                if (!TargetSquares.Contains(TargetStart))
                                    TargetSquares.Add(TargetStart);

                                for (var I = 1; I <= 3; I++)
                                {
                                    var TargetSquare = new Point(TargetStart.X, TargetStart.Y - I);

                                    if (!TargetSquares.Contains(TargetSquare))
                                        TargetSquares.Add(TargetSquare);
                                }

                                break;
                            }

                            case 4:
                            {
                                TargetStart = new Point(GetX + 2, GetY);

                                if (!TargetSquares.Contains(TargetStart))
                                    TargetSquares.Add(TargetStart);

                                for (var I = 1; I <= 3; I++)
                                {
                                    var TargetSquare = new Point(TargetStart.X + I, TargetStart.Y);

                                    if (!TargetSquares.Contains(TargetSquare))
                                        TargetSquares.Add(TargetSquare);
                                }

                                break;
                            }

                            case 6:
                            {
                                TargetStart = new Point(GetX, GetY + 2);


                                if (!TargetSquares.Contains(TargetStart))
                                    TargetSquares.Add(TargetStart);

                                for (var I = 1; I <= 3; I++)
                                {
                                    var TargetSquare = new Point(TargetStart.X, TargetStart.Y + I);

                                    if (!TargetSquares.Contains(TargetSquare))
                                        TargetSquares.Add(TargetSquare);
                                }

                                break;
                            }
                        }

                        #endregion

                        if (TargetSquares.Count > 0)
                            foreach (var Target in from Square in TargetSquares.ToList()
                                select _room.GetGameMap().GetRoomUsers(Square).ToList()
                                into affectedUsers
                                where affectedUsers.Count != 0
                                from Target in affectedUsers
                                where Target != null && !Target.IsBot && !Target.IsPet
                                where Target.GetClient() != null && Target.GetClient().GetHabbo() != null
                                where !_room.CheckRights(Target.GetClient(), true)
                                select Target)
                            {
                                Target.ApplyEffect(4);
                                Target.GetClient()
                                    .SendMessage(new RoomNotificationComposer("Kicked from room",
                                        "You were hit by a cannonball!", "room_kick_cannonball", ""));
                                Target.ApplyEffect(0);
                                _room.GetRoomUserManager().RemoveUserFromRoom(Target.GetClient(), true);
                            }

                        ExtraData = "2";
                        UpdateState(false, true);
                    }
                        break;

                        #endregion
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        public static string[] RandomizeStrings(string[] arr)
        {
            var _random = new Random();

            var list = arr.Select(s => new KeyValuePair<int, string>(_random.Next(), s)).ToList();

            var sorted = from item in list
                orderby item.Key
                select item;
            var result = new string[arr.Length];
            var index = 0;
            foreach (var pair in sorted)
            {
                result[index] = pair.Value;
                index++;
            }
            return result;
        }

        public void RequestUpdate(int Cycles, bool setUpdate)
        {
            UpdateCounter = Cycles;
            if (setUpdate)
                UpdateNeeded = true;
        }

        public void UpdateState() => UpdateState(true, true);

        public void UpdateState(bool inDb, bool inRoom)
        {
            if (GetRoom() == null)
                return;

            if (inDb)
                GetRoom().GetRoomItemHandler().UpdateItem(this);

            if (!inRoom) return;
            if (IsFloorItem)
                GetRoom().SendMessage(new ObjectUpdateComposer(this, GetRoom().OwnerId));
            else
                GetRoom().SendMessage(new ItemUpdateComposer(this, GetRoom().OwnerId));
        }

        public void ResetBaseItem()
        {
            Data = null;
            Data = GetBaseItem();
        }

        public ItemData GetBaseItem()
        {
            if (Data == null)
            {
                ItemData I = OblivionServer.GetGame().GetItemManager().GetItem(BaseItem);
                if (I != null)
                    Data = I;
            }

            return Data;
        }

        public void RegenerateBlock(string NewMode, Gamemap Tile)
        {
            try
            {
                var list = new List<RoomUser>();

                if (!int.TryParse(NewMode, out int CurrentMode))
                {
                }

                if (CurrentMode <= 0)
                {
                    foreach (RoomUser user in _room.GetGameMap().GetRoomUsers(new Point(this.GetX, this.GetY)))
                    {
                        user.SqState = 0;
                    }
                    _room.GetGameMap().GameMap[this.GetX, this.GetY] = 0;
                }
            }
            catch
            {
            }
        }

        public Room GetRoom()
        {
            if (_room != null)
                return _room;

            Room Room;
            return OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out Room) ? Room : null;
        }

        public void UserFurniCollision(RoomUser user)
        {
            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerUserFurniCollision, user.GetClient().GetHabbo(), this);
        }

        public void UserWalksOnFurni(RoomUser user)
        {
            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.WalkSwitch)
            {
                Interactor.OnTrigger(user.GetClient(), this, 0, false);
            }

            if (GetBaseItem().InteractionType == InteractionType.Tent ||
                GetBaseItem().InteractionType == InteractionType.TentSmall)
                GetRoom().AddUserToTent(Id, user, this);

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOnFurni, user.GetClient().GetHabbo(), this);
            user.LastItem = this;
        }

        public void UserWalksOffFurni(RoomUser user)
        {
            if (user?.GetClient() == null || user.GetClient().GetHabbo() == null)
                return;

            if (GetBaseItem().InteractionType == InteractionType.WalkSwitch)
            {
                Interactor.OnTrigger(user.GetClient(), this, 0, false);
            }

            if (GetBaseItem().InteractionType == InteractionType.Tent ||
                GetBaseItem().InteractionType == InteractionType.TentSmall)
                GetRoom().RemoveUserFromTent(Id, user, this);

            GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerWalkOffFurni, user.GetClient().GetHabbo(), this);
        }
    }
}