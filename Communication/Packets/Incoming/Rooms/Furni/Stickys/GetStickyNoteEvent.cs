#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Stickys;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    internal class GetStickyNoteEvent : IPacketEvent
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

            Session.SendMessage(new StickyNoteComposer(Item.Id.ToString(), Item.ExtraData));
        }
    }
}