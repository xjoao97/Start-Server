#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class GetGroupFurniSettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var ItemId = Packet.PopInt();
            var GroupId = Packet.PopInt();

            var Item = Session.GetHabbo().CurrentRoom.GetRoomItemHandler().GetItem(ItemId);

            if (Item?.Data.InteractionType != InteractionType.GuildGate)
                return;

            Group Group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(GroupId);
            if (Group == null)
                return;

            Session.SendMessage(new GroupFurniSettingsComposer(Group, ItemId, Session.GetHabbo().Id));
            Session.SendMessage(new GroupInfoComposer(Group, Session));
        }
    }
}