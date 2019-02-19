#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users.Relationships;

#endregion

namespace Oblivion.HabboHotel.Users.Messenger
{
    public class MessengerBuddy
    {
        #region Constructor

        public MessengerBuddy(int UserId, string pUsername, string pLook, string pMotto, int pLastOnline,
            bool pAppearOffline, bool pHideInroom)
        {
            this.UserId = UserId;
            mUsername = pUsername;
            mLook = pLook;
            mMotto = pMotto;
            mLastOnline = pLastOnline;
            mAppearOffline = pAppearOffline;
            mHideInroom = pHideInroom;
        }

        #endregion

        #region Fields

        public int UserId;
        public bool mAppearOffline;
        public bool mHideInroom;
        public int mLastOnline;
        public string mLook;
        public string mMotto;

        public GameClient client;
        public string mUsername;

        #endregion

        #region Return values

        public int Id => UserId;

        public bool IsOnline
            =>
                client?.GetHabbo() != null && client.GetHabbo().GetMessenger() != null &&
                !client.GetHabbo().GetMessenger().AppearOffline;

        public bool InRoom => CurrentRoom != null;

        public Room CurrentRoom { get; set; }

        #endregion

        #region Methods

        public void UpdateUser(GameClient client)
        {
            this.client = client;
            if (client?.GetHabbo() != null)
                CurrentRoom = client.GetHabbo().CurrentRoom;
        }

        public void Serialize(ServerPacket Message, GameClient Session)
        {
            Relationship Relationship = null;

            if (Session?.GetHabbo() != null && Session.GetHabbo().Relationships != null)
                Relationship =
                    Session.GetHabbo()
                        .Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(UserId))
                        .Value;

            var y = Relationship?.Type ?? 0;

            Message.WriteInteger(UserId);
            Message.WriteString(mUsername);
            Message.WriteInteger(1);
            Message.WriteBoolean(!mAppearOffline || Session.GetHabbo().GetPermissions().HasRight("mod_tool")
                ? IsOnline
                : false);
            Message.WriteBoolean(!mHideInroom || Session.GetHabbo().GetPermissions().HasRight("mod_tool")
                ? InRoom
                : false);
            Message.WriteString(IsOnline ? mLook : "");
            Message.WriteInteger(0); // categoryid
            Message.WriteString(mMotto);
            Message.WriteString(string.Empty); // Facebook username
            Message.WriteString(string.Empty);
            Message.WriteBoolean(true); // Allows offline messaging
            Message.WriteBoolean(false); // ?
            Message.WriteBoolean(false); // Uses phone
            Message.WriteShort(y);
        }

        #endregion
    }
}