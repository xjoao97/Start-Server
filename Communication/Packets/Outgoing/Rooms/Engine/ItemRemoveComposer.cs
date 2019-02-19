#region

using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class ItemRemoveComposer : ServerPacket
    {
        public ItemRemoveComposer(Item Item, int UserId)
            : base(ServerPacketHeader.ItemRemoveMessageComposer)
        {
            WriteString(Item.Id.ToString());
            WriteBoolean(false);
            WriteInteger(UserId);
        }
    }
}