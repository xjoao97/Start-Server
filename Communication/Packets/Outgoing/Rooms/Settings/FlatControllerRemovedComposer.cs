#region

using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Settings
{
    internal class FlatControllerRemovedComposer : ServerPacket
    {
        public FlatControllerRemovedComposer(Room Instance, int UserId)
            : base(ServerPacketHeader.FlatControllerRemovedMessageComposer)
        {
            WriteInteger(Instance.Id);
            WriteInteger(UserId);
        }
    }
}