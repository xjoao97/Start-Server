#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorUserRoomVisitsComposer : ServerPacket
    {
        public ModeratorUserRoomVisitsComposer(Habbo Data, Dictionary<double, RoomData> Visits)
            : base(ServerPacketHeader.ModeratorUserRoomVisitsMessageComposer)
        {
            WriteInteger(Data.Id);
            WriteString(Data.Username);
            WriteInteger(Visits.Count);

            foreach (var Visit in Visits)
            {
                WriteInteger(Visit.Value.Id);
                WriteString(Visit.Value.Name);
                WriteInteger(UnixTimestamp.FromUnixTimestamp(Visit.Key).Hour);
                WriteInteger(UnixTimestamp.FromUnixTimestamp(Visit.Key).Minute);
            }
        }
    }
}