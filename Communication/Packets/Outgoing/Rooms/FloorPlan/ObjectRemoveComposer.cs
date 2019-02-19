#region

using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ObjectRemoveComposer : ServerPacket
    {
        public ObjectRemoveComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ObjectRemoveMessageComposer)
        {
            WriteString(Item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(UserId);
            WriteInteger(0);
        }
    }
}