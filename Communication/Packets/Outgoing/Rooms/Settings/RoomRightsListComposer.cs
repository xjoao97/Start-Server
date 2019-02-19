#region

using System.Linq;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class RoomRightsListComposer : ServerPacket
    {
        public RoomRightsListComposer(Room Instance)
            : base(ServerPacketHeader.RoomRightsListMessageComposer)
        {
            WriteInteger(Instance.Id);

            WriteInteger(Instance.UsersWithRights.Count);
            foreach (
                var Data in
                Instance.UsersWithRights.ToList()
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