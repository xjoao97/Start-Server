#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Wired;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Wired
{
    internal class SaveWiredConfigEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (!Session.GetHabbo().InRoom)
                return;

            var ItemId = Packet.PopInt();

            Session.SendMessage(new HideWiredConfigComposer());

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null || !Room.CheckRights(Session, false, true))
                return;

            var SelectedItem = Room.GetRoomItemHandler().GetItem(ItemId);
            if (SelectedItem == null)
                return;

            var Box = Room.GetWired().GetWired(ItemId);
            if (Box == null)
                return;


            if (Box.Type == WiredBoxType.EffectGiveUserBadge &&
                !Session.GetHabbo().GetPermissions().HasRight("room_item_wired_rewards"))
            {
                Session.SendNotification("Você não tem permissão para fazer isto.");
                return;
            }

            Box.HandleSave(Packet);
            Session.GetHabbo().CurrentRoom.GetWired().SaveBox(Box);
        }
    }
}