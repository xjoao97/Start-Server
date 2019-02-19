#region

using System;
using System.Collections.Generic;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Users
{
    internal class ProfileInformationComposer : ServerPacket
    {
        public ProfileInformationComposer(Habbo Data, GameClient Session, List<Group> Groups, int friendCount)
            : base(ServerPacketHeader.ProfileInformationMessageComposer)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(Data.AccountCreated);

            WriteInteger(Data.Id);
            WriteString(Data.Username);
            WriteString(Data.Look);
            WriteString(Data.Motto);
            WriteString(origin.ToString("dd/MM/yyyy"));
            WriteInteger(Data.GetStats().AchievementPoints);
            WriteInteger(friendCount); // Friend Count
            WriteBoolean(Data.Id != Session.GetHabbo().Id &&
                         Session.GetHabbo().GetMessenger().FriendshipExists(Data.Id)); //  Is friend
            WriteBoolean(Data.Id != Session.GetHabbo().Id &&
                         !Session.GetHabbo().GetMessenger().FriendshipExists(Data.Id) &&
                         Session.GetHabbo().GetMessenger().RequestExists(Data.Id)); // Sent friend request
            WriteBoolean(OblivionServer.GetGame().GetClientManager().GetClientByUserID(Data.Id) != null);

            WriteInteger(Groups.Count);
            foreach (var Group in Groups)
            {
                WriteInteger(Group.Id);
                WriteString(Group.Name);
                WriteString(Group.Badge);
                WriteString(OblivionServer.GetGame().GetGroupManager().GetGroupColour(Group.Colour1, true));
                WriteString(OblivionServer.GetGame().GetGroupManager().GetGroupColour(Group.Colour2, false));
                WriteBoolean(Data.GetStats().FavouriteGroupId == Group.Id); // todo favs
                WriteInteger(0); //what the fuck
                WriteBoolean(Group.ForumEnabled); //HabboTalk
            }

            WriteInteger(Convert.ToInt32(OblivionServer.GetUnixTimestamp() - Data.LastOnline)); // Last online
            WriteBoolean(true); // Show the profile
        }
    }
}