#region

using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.Camera;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Pathfinding;
using Oblivion.HabboHotel.Rooms.AI;
using Oblivion.HabboHotel.Rooms.Games.Freeze;
using Oblivion.HabboHotel.Rooms.Games.Teams;

#endregion

namespace Oblivion.HabboHotel.Rooms
{
    public class RoomUser
    {
        public bool AllowOverride;

        public FreezePowerUp banzaiPowerUp;
        public BotAI BotAI;
        public RoomBot BotData;
        public bool CanWalk;
        public int CarryItemID; //byte
        public int CarryTimer; //byte
        public int ChatSpamCount;
        public int ChatSpamTicks = 16;
        public bool CurrentItemEffect;
        public int DanceId;
        public bool FastWalking = false;
        public int FreezeCounter;
        public bool Freezed;
        public bool FreezeInteracting;
        public int FreezeLives;
        public bool Frozen;
        public int GateId;
        public int GoalX; //byte
        public int GoalY; //byte
        public int HabboId;
        internal int handelingBallStatus = 0;
        public int HorseID = 0;
        public int IdleTime; //byte
        public bool InteractingGate;
        public int InternalRoomID;
        public bool IsAsleep;
        public bool IsJumping;
        public bool isLying = false;


        public bool isRolling = false;
        public bool isSitting = false;
        public bool IsWalking;
        public int LastBubble = 0;
        public double LastInteraction;
        public Item LastItem = null;
        public CameraPhotoPreview LastPhotoPreview;

        public int LLPartner = 0;
        public int LockedTilesCount;
        private GameClient mClient;
        public bool moonwalkEnabled = false;
        private Room mRoom;

        public List<Vector2D> Path = new List<Vector2D>();
        public bool PathRecalcNeeded;
        public int PathStep = 1;
        public Pet PetData;

        public int PrevTime;
        public bool RidingHorse = false;
        public int rollerDelay = 0;
        public int RoomId;
        public int RotBody; //byte
        public int RotHead; //byte

        public Vector2D SetLocation;

        public bool SetStep;
        public int SetX; //byte
        public int SetY; //byte
        public double SetZ;
        public bool shieldActive;
        public int shieldCounter;
        public double SignTime;
        public byte SqState;
        public Dictionary<string, string> Statusses;
        public bool SuperFastWalking = false;
        public TEAM Team;
        public int TeleDelay; //byte
        public bool TeleportEnabled;
        public double TimeInRoom;
        public bool UpdateNeeded;
        public int UserId;
        public int VirtualId;

        public int X; //byte
        public int Y; //byte
        public double Z;

        public RoomUser(int HabboId, int RoomId, int VirtualId, Room room)
        {
            Freezed = false;
            this.HabboId = HabboId;
            this.RoomId = RoomId;
            this.VirtualId = VirtualId;
            IdleTime = 0;

            X = 0;
            Y = 0;
            Z = 0;
            PrevTime = 0;
            RotHead = 0;
            RotBody = 0;
            UpdateNeeded = true;
            Statusses = new Dictionary<string, string>();

            TeleDelay = -1;
            mRoom = room;

            AllowOverride = false;
            CanWalk = true;


            SqState = 3;

            InternalRoomID = 0;
            CurrentItemEffect = false;

            FreezeLives = 0;
            InteractingGate = false;
            GateId = 0;
            LastInteraction = 0;
            LockedTilesCount = 0;

            IsJumping = false;
            TimeInRoom = 0;
        }


        public Point Coordinate => new Point(X, Y);

        public bool IsPet => IsBot && BotData.IsPet;

        public int CurrentEffect => GetClient().GetHabbo().Effects().CurrentEffect;


        public bool IsDancing => DanceId >= 1;

        public bool NeedsAutokick
        {
            get
            {
                if (IsBot)
                    return false;

                if (GetClient() == null || GetClient().GetHabbo() == null)
                    return true;

                if (GetClient().GetHabbo().GetPermissions().HasRight("mod_tool") || GetRoom().OwnerId == HabboId)
                    return false;

                return IdleTime >= 7200;
            }
        }

        public bool IsTrading => !IsBot && Statusses.ContainsKey("trd");

        public bool IsBot => BotData != null;

        public Point SquareInFront
        {
            get
            {
                var Sq = new Point(X, Y);

                switch (RotBody)
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
                var Sq = new Point(X, Y);

                switch (RotBody)
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
                var Sq = new Point(X, Y);

                switch (RotBody)
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
                var Sq = new Point(X, Y);

                switch (RotBody)
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

        public string GetUsername()
        {
            if (IsBot)
                return string.Empty;

            if (GetClient() != null)
                if (GetClient().GetHabbo() != null)
                    return GetClient().GetHabbo().Username;
                else
                    return OblivionServer.GetUsernameById(HabboId);
            return OblivionServer.GetUsernameById(HabboId);
        }

        public void UnIdle()
        {
            if (!IsBot)
                if (GetClient() != null && GetClient().GetHabbo() != null)
                    GetClient().GetHabbo().TimeAFK = 0;

            IdleTime = 0;

            if (IsAsleep)
            {
                IsAsleep = false;
                GetRoom().SendMessage(new SleepComposer(this, false));
            }
        }

        public void Dispose()
        {
            Statusses.Clear();
            mRoom = null;
            mClient = null;
        }

        public void Chat(string Message, bool Shout, int colour = 0)
        {
            if (GetRoom() == null)
                return;

            if (!IsBot)
                return;

            if (IsPet)
                foreach (
                    var User in
                    GetRoom().GetRoomUserManager().GetUserList().ToList().Where(User => User != null && !User.IsBot))
                {
                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;

                    if (!User.GetClient().GetHabbo().AllowPetSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, 0));
                }
            else
                foreach (
                    var User in
                    GetRoom().GetRoomUserManager().GetUserList().ToList().Where(User => User != null && !User.IsBot))
                {
                    if (User.GetClient() == null || User.GetClient().GetHabbo() == null)
                        return;


                    if (Message.ToUpper().Contains("%ROOMNAME%"))
                        Message = Message.Replace("%ROOMNAME%", GetRoom().Name);

                    if (Message.ToUpper().Contains("%USERCOUNT%"))
                        Message = Message.Replace("%USERCOUNT%", GetRoom().UserCount.ToString());

                    if (Message.ToUpper().Contains("%USERSONLINE%"))
                        Message = Message.Replace("%USERSONLINE%",
                            OblivionServer.GetGame().GetClientManager().Count.ToString());

                    if (!User.GetClient().GetHabbo().AllowBotSpeech)
                        User.GetClient().SendMessage(new ChatComposer(VirtualId, Message, 0, colour == 0 ? 2 : colour));
                }
        }

        public void HandleSpamTicks()
        {
            if (ChatSpamTicks >= 0)
            {
                ChatSpamTicks--;

                if (ChatSpamTicks == -1)
                    ChatSpamCount = 0;
            }
        }

        public bool IncrementAndCheckFlood(out int MuteTime)
        {
            MuteTime = 0;

            ChatSpamCount++;
            if (ChatSpamTicks == -1)
            {
                ChatSpamTicks = 8;
            }
            else if (ChatSpamCount >= 6)
            {
                if (GetClient().GetHabbo().GetPermissions().HasRight("events_staff"))
                    MuteTime = 3;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("gold_vip"))
                    MuteTime = 7;
                else if (GetClient().GetHabbo().GetPermissions().HasRight("silver_vip"))
                    MuteTime = 10;
                else
                    MuteTime = 20;

                GetClient().GetHabbo().FloodTime = OblivionServer.GetUnixTimestamp() + MuteTime;

                ChatSpamCount = 0;
                return true;
            }
            return false;
        }

        /* public void OnChat(int Colour, string Message, bool Shout)
         {
             if (GetClient() == null || GetClient().GetHabbo() == null || mRoom == null)
                 return;

             if (mRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), Message))
                 return;
             if (mRoom.GetWired().TriggerEvent(WiredBoxType.TriggerUserSaysCommand, GetClient().GetHabbo(), Message))
                 return;


             GetClient().GetHabbo().HasSpoken = true;

             if (mRoom.WordFilterList.Count > 0 &&
                 !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override"))
                 Message = mRoom.GetFilter().CheckMessage(Message);

             var ColouredMessage = Message;
             if (!string.IsNullOrEmpty(GetClient().GetHabbo().chatColour))
                 ColouredMessage = "@" + GetClient().GetHabbo().chatColour + "@" + Message;

             ServerPacket Packet;
             if (Shout)
                 Packet = new ShoutComposer(VirtualId, ColouredMessage,
                     OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
             else
                 Packet = new ChatComposer(VirtualId, ColouredMessage,
                     OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);


             if (GetClient().GetHabbo().TentId > 0)
             {
                 mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, Packet);

                 Packet = new WhisperComposer(VirtualId, "[Tent Chat] " + Message, 0, Colour);

                 var ToNotify = mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                 if (ToNotify.Count > 0)
                     foreach (
                         var user in
                         ToNotify.Where(
                             user =>
                                 user?.GetClient() != null && user.GetClient().GetHabbo() != null &&
                                 user.GetClient().GetHabbo().TentId != GetClient().GetHabbo().TentId))
                         user.GetClient().SendMessage(Packet);
             }
             else
             {
                 foreach (
                     var User in
                     mRoom.GetRoomUserManager()
                         .GetRoomUsers()
                         .ToList()
                         .Where(
                             User =>
                                 User?.GetClient() != null && User.GetClient().GetHabbo() != null &&
                                 !User.GetClient().GetHabbo().MutedUsers.Contains(mClient.GetHabbo().Id))
                         .Where(
                             User =>
                                 mRoom.ChatDistance <= 0 ||
                                 Gamemap.TileDistance(X, Y, User.X, User.Y) <= mRoom.ChatDistance))
                     User.GetClient().SendMessage(Packet);
             }
             Console.WriteLine(Message);

             #region Pets/Bots responces

             if (Shout)
                 foreach (
                     var User in
                     mRoom.GetRoomUserManager()
                         .GetUserList()
                         .ToList()
                         .Where(User => User.IsBot)
                         .Where(User => User.IsBot))
                     User.BotAI.OnUserShout(this, Message);
             else
                 foreach (
                     var User in
                     mRoom.GetRoomUserManager()
                         .GetUserList()
                         .ToList()
                         .Where(User => User.IsBot)
                         .Where(User => User.IsBot))
                     User.BotAI.OnUserSay(this, Message);

             #endregion
         }*/

        public void OnChat(int Colour, string Message, bool Shout)
        {
            if (GetClient() == null || GetClient().GetHabbo() == null || mRoom == null)
                return;

            if (mRoom.GetWired().OnSayTrigger(WiredBoxType.TriggerUserSays, GetClient().GetHabbo(), Message))
                return;

            if (mRoom.GetWired().OnSayTrigger(WiredBoxType.TriggerUserSaysCommand, GetClient().GetHabbo(), Message))
                return;

            GetClient().GetHabbo().HasSpoken = true;

            if (mRoom.WordFilterList.Count > 0 &&
                !GetClient().GetHabbo().GetPermissions().HasRight("word_filter_override"))
                Message = mRoom.GetFilter().CheckMessage(Message);

            var ColouredMessage = Message;
            if (!string.IsNullOrEmpty(GetClient().GetHabbo().chatColour))
                ColouredMessage = "@" + GetClient().GetHabbo().chatColour + "@" + Message;

            ServerPacket Packet;
            if (Shout)
                Packet = new ShoutComposer(VirtualId, ColouredMessage,
                    OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);
            else
                Packet = new ChatComposer(VirtualId, ColouredMessage,
                    OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message), Colour);


            if (GetClient().GetHabbo().TentId > 0)
            {
                mRoom.SendToTent(GetClient().GetHabbo().Id, GetClient().GetHabbo().TentId, Packet);

                Packet = new WhisperComposer(VirtualId, "[Tent Chat] " + Message, 0, Colour);

                var ToNotify = mRoom.GetRoomUserManager().GetRoomUserByRank(2);

                if (ToNotify.Count > 0)
                    foreach (
                        var user in
                        ToNotify.Where(
                            user =>
                                user?.GetClient() != null && user.GetClient().GetHabbo() != null &&
                                user.GetClient().GetHabbo().TentId != GetClient().GetHabbo().TentId))
                        user.GetClient().SendMessage(Packet);
            }
            else
            {
                foreach (var User in mRoom.GetRoomUserManager().GetRoomUsers().ToList())
                {
                    if (User?.GetClient() == null || User.GetClient().GetHabbo() == null ||
                        User.GetClient().GetHabbo().MutedUsers.Contains(mClient.GetHabbo().Id))
                        continue;


                    if (mRoom.ChatDistance > 0 && Gamemap.TileDistance(X, Y, User.X, User.Y) > mRoom.ChatDistance)
                        continue;

                    if (User.GetClient().GetHabbo().ChatPreference)
                    {
                        ColouredMessage = HttpUtility.HtmlDecode(ColouredMessage);
                        if (Shout)
                            Packet = new ShoutComposer(VirtualId, ColouredMessage,
                                OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message),
                                Colour);
                        else
                            Packet = new ChatComposer(VirtualId, ColouredMessage,
                                OblivionServer.GetGame().GetChatManager().GetEmotions().GetEmotionsForText(Message),
                                Colour);
                    }
                    User.GetClient().SendMessage(Packet);
                }
            }

            #region Pets/Bots responces

            if (Shout)
                foreach (var User in mRoom.GetRoomUserManager().GetUserList().ToList().Where(User => User.IsBot))
                    User.BotAI.OnUserShout(this, Message);
            else
                foreach (var User in mRoom.GetRoomUserManager().GetUserList().ToList().Where(User => User.IsBot))
                    User.BotAI.OnUserSay(this, Message);

            #endregion
        }


        public void ClearMovement(bool Update)
        {
            IsWalking = false;
            Statusses.Remove("mv");
            GoalX = 0;
            GoalY = 0;
            SetStep = false;
            SetX = 0;
            SetY = 0;
            SetZ = 0;

            if (Update)
                UpdateNeeded = true;
        }

        public void MoveTo(Point c) => MoveTo(c.X, c.Y);

        public void MoveTo(int pX, int pY, bool pOverride)
        {
            if (GetRoom().GetGameMap().SquareHasUsers(pX, pY) && !pOverride || Frozen)
                return;

            UnIdle();

            if (TeleportEnabled)
            {
                GetRoom()
                    .SendMessage(GetRoom()
                        .GetRoomItemHandler()
                        .UpdateUserOnRoller(this, new Point(pX, pY), 0,
                            GetRoom().GetGameMap().SqAbsoluteHeight(GoalX, GoalY)));
                if (Statusses.ContainsKey("sit"))
                    Z -= 0.35;
                UpdateNeeded = true;
                return;
            }

            GoalX = pX;
            GoalY = pY;
            IsWalking = true;
            PathRecalcNeeded = true;
            FreezeInteracting = false;
        }

        public void MoveTo(int pX, int pY) => MoveTo(pX, pY, false);

        public void UnlockWalking()
        {
            AllowOverride = false;
            CanWalk = true;
        }


        public void SetPos(int pX, int pY, double pZ)
        {
            X = pX;
            Y = pY;
            Z = pZ;
        }

        public void CarryItem(int Item)
        {
            CarryItemID = Item;

            CarryTimer = Item > 0 ? 240 : 0;

            GetRoom().SendMessage(new CarryObjectComposer(VirtualId, Item));
        }


        public void SetRot(int Rotation, bool HeadOnly)
        {
            if (Statusses.ContainsKey("lay") || IsWalking)
                return;

            var diff = RotBody - Rotation;

            RotHead = RotBody;

            if (Statusses.ContainsKey("sit") || HeadOnly)
            {
                if (RotBody == 2 || RotBody == 4)
                {
                    if (diff > 0)
                        RotHead = RotBody - 1;
                    else if (diff < 0)
                        RotHead = RotBody + 1;
                }
                else if (RotBody == 0 || RotBody == 6)
                {
                    if (diff > 0)
                        RotHead = RotBody - 1;
                    else if (diff < 0)
                        RotHead = RotBody + 1;
                }
            }
            else if (diff <= -2 || diff >= 2)
            {
                RotHead = Rotation;
                RotBody = Rotation;
            }
            else
            {
                RotHead = Rotation;
            }

            UpdateNeeded = true;
        }

        public void SetStatus(string Key, string Value)
        {
            if (Statusses.ContainsKey(Key))
                Statusses[Key] = Value;
            else
                AddStatus(Key, Value);
        }

        public void AddStatus(string Key, string Value) => Statusses[Key] = Value;

        public void RemoveStatus(string Key)
        {
            if (Statusses.ContainsKey(Key))
                Statusses.Remove(Key);
        }

        public void ApplyEffect(int effectID)
        {
            if (IsBot)
            {
                mRoom.SendMessage(new AvatarEffectComposer(VirtualId, effectID));
                return;
            }

            if (GetClient() == null || GetClient().GetHabbo() == null ||
                GetClient().GetHabbo().Effects() == null)
                return;

            GetClient().GetHabbo().Effects().ApplyEffect(effectID);
        }


        public GameClient GetClient()
        {
            if (IsBot)
                return null;
            return mClient ?? (mClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(HabboId));
        }

        private Room GetRoom()
        {
            if (mRoom == null)
                if (OblivionServer.GetGame().GetRoomManager().TryGetRoom(RoomId, out mRoom))
                    return mRoom;

            return mRoom;
        }
    }

    public enum ItemEffectType
    {
        None,
        Swim,
        SwimLow,
        SwimHalloween,
        Iceskates,
        Normalskates,
        PublicPool
        //Skateboard?
    }

    public static class ByteToItemEffectEnum
    {
        public static ItemEffectType Parse(byte pByte)
        {
            switch (pByte)
            {
                case 0:
                    return ItemEffectType.None;
                case 1:
                    return ItemEffectType.Swim;
                case 2:
                    return ItemEffectType.Normalskates;
                case 3:
                    return ItemEffectType.Iceskates;
                case 4:
                    return ItemEffectType.SwimLow;
                case 5:
                    return ItemEffectType.SwimHalloween;
                case 6:
                    return ItemEffectType.PublicPool;
                //case 7:
                //return ItemEffectType.Custom;
                default:
                    return ItemEffectType.None;
            }
        }
    }

    //0 = none
    //1 = pool
    //2 = normal skates
    //3 = ice skates
}