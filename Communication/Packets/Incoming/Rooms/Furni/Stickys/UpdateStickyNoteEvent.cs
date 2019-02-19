#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    internal class UpdateStickyNoteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var Item = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.Postit)
                return;

            var Color = Packet.PopString();
            var Text = Packet.PopString();

            if (!Room.CheckRights(Session))
                if (!Text.StartsWith(Item.ExtraData))
                    return; // we can only ADD stuff! older stuff changed, this is not allowed

            switch (Color)
            {
                case "FFFF33":
                case "FF9CFF":
                case "9CCEFF":
                case "9CFF9C":

                    break;

                default:

                    return; // invalid color
            }

            Item.ExtraData = Color + " " + Text;
            Item.UpdateState(true, true);
        }
    }
}