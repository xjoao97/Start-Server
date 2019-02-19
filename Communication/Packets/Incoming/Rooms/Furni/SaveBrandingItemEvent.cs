#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class SaveBrandingItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.CheckRights(Session, true) ||
                !Session.GetHabbo().GetPermissions().HasRight("room_item_save_branding_items"))
                return;

            var ItemId = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if (Item.Data.InteractionType == InteractionType.Background)
            {
                var Data = Packet.PopInt();
                var BrandData = "state" + Convert.ToChar(9) + "0";

                for (var i = 1; i <= Data; i++)
                    BrandData = BrandData + Convert.ToChar(9) + Packet.PopString();

                Item.ExtraData = BrandData;
            }


            Room.GetRoomItemHandler()
                .SetFloorItem(Session, Item, Item.GetX, Item.GetY, Item.Rotation, false, false, true);
        }
    }
}