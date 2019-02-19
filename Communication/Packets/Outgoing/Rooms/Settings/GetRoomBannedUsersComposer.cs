#region

using System.Linq;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class GetRoomBannedUsersComposer : ServerPacket
    {
        public GetRoomBannedUsersComposer(Room Instance)
            : base(ServerPacketHeader.GetRoomBannedUsersMessageComposer)
        {
            WriteInteger(Instance.Id);

            WriteInteger(Instance.BannedUsers().Count); //Count
            foreach (
                var Data in
                Instance.BannedUsers()
                    .ToList()
                    .Select(Id => OblivionServer.GetGame().GetCacheManager().GenerateUser(Id)))
                if (Data == null)
                {
                    WriteInteger(0);
                    WriteString("Unknown Error");
                }
                else
                {
                    WriteInteger(Data.Id);
                    WriteString(Data.Username);
                }
        }
    }
}