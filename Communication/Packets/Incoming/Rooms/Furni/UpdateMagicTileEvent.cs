#region

using System;
using System.Drawing;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class UpdateMagicTileEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (!Room.CheckRights(Session, false, true) &&
                !Session.GetHabbo().GetPermissions().HasRight("room_item_use_any_stack_tile"))
                return;

            var ItemId = Packet.PopInt();
            var DecimalHeight = Packet.PopInt();

            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if (DecimalHeight > 10000)
                DecimalHeight = 10000;
            else if (DecimalHeight < 0)
            {
                Item item;
                if (Room.GetGameMap().GetHighestItemForSquare(new Point(Item.GetX, Item.GetY), out item))
                {
                    DecimalHeight = Convert.ToInt32(item.GetZ) * 10;
                }

            }

            Item.GetZ = DecimalHeight / 100.0;
            Room.SendMessage(new ObjectUpdateComposer(Item, Convert.ToInt32(Session.GetHabbo().Id)));
            Room.SendMessage(new UpdateMagicTileComposer(ItemId, DecimalHeight));
        }
    }
}