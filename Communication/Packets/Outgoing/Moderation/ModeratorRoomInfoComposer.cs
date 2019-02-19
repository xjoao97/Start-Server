﻿#region

using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Moderation
{
    internal class ModeratorRoomInfoComposer : ServerPacket
    {
        public ModeratorRoomInfoComposer(RoomData Data, bool OwnerInRoom)
            : base(ServerPacketHeader.ModeratorRoomInfoMessageComposer)
        {
            WriteInteger(Data.Id);
            WriteInteger(Data.UsersNow);
            WriteBoolean(OwnerInRoom); // owner in room
            WriteInteger(Data.OwnerId);
            WriteString(Data.OwnerName);
            WriteBoolean(Data != null);
            WriteString(Data.Name);
            WriteString(Data.Description);

            WriteInteger(Data.Tags.Count);
            foreach (var Tag in Data.Tags)
                WriteString(Tag);

            WriteBoolean(false);
        }
    }
}