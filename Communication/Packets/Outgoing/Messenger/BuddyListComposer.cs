#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Users;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class BuddyListComposer : ServerPacket
    {
        public BuddyListComposer(ICollection<MessengerBuddy> Friends, Habbo Player)
            : base(ServerPacketHeader.BuddyListMessageComposer)
        {
            var Groups =
                OblivionServer.GetGame()
                    .GetGroupManager()
                    .GetGroupsForUser(Player.Id)
                    .Where(group => group.HasChat && group.ChatUsers.Contains(Player.Id))
                    .ToList();
            WriteInteger(1); // Pages
            WriteInteger(0); // Page

            WriteInteger(Friends.Count + Groups.Count); //items count
            foreach (var Friend in Friends.ToList())
            {
                var Relationship =
                    Player.Relationships.FirstOrDefault(x => x.Value.UserId == Convert.ToInt32(Friend.UserId)).Value;

                WriteInteger(Friend.Id);
                WriteString(Friend.mUsername);
                WriteInteger(1); //Gender.
                WriteBoolean(Friend.IsOnline);
                WriteBoolean(Friend.IsOnline && Friend.InRoom);
                WriteString(Friend.IsOnline ? Friend.mLook : string.Empty);
                WriteInteger(0); // category id
                WriteString(Friend.IsOnline ? Friend.mMotto : string.Empty);
                WriteString(string.Empty); //Alternative name?
                WriteString(string.Empty);
                WriteBoolean(true);
                WriteBoolean(false);
                WriteBoolean(false); //Pocket Habbo user.
                WriteShort(Relationship?.Type ?? 0);
            }
            foreach (var group in Groups)
            {
                WriteInteger(-group.Id);
                WriteString(group.Name);
                WriteInteger(0);
                WriteBoolean(true);
                WriteBoolean(false);
                WriteString(group.Badge);
                WriteInteger(0);
                WriteString(group.Description);
                WriteString(string.Empty);
                WriteString(string.Empty);
                WriteBoolean(false);
                WriteBoolean(false);
                WriteBoolean(false);
                WriteShort(0);
            }
        }
    }
}