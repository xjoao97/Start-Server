#region

using System;
using System.Linq;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Stickys
{
    internal class AddStickyNoteEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var itemId = Packet.PopInt();
            var locationData = Packet.PopString();

            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session))
                return;

            var Item = Session.GetHabbo().GetInventoryComponent().GetItem(itemId);
            if (Item == null)
                return;

            try
            {
                var WallPossition = WallPositionCheck(":" + locationData.Split(':')[1]);

                var RoomItem = new Item(Item.Id, Room.RoomId, Item.BaseItem, Item.ExtraData, 0, 0, 0, 0,
                    Session.GetHabbo().Id, Item.GroupId, 0, 0, WallPossition, Room);
                if (Room.GetRoomItemHandler().SetWallItem(Session, RoomItem))
                    Session.GetHabbo().GetInventoryComponent().RemoveItem(itemId);
            }
            catch
            {
                //TODO: Send a packet
            }
        }

        private static string WallPositionCheck(string wallPosition)
        {
            //:w=3,2 l=9,63 l
            try
            {
                if (wallPosition.Contains(Convert.ToChar(13)))
                    return null;
                if (wallPosition.Contains(Convert.ToChar(9)))
                    return null;

                var posD = wallPosition.Split(' ');
                if (posD[2] != "l" && posD[2] != "r")
                    return null;

                var widD = posD[0].Substring(3).Split(',');
                var widthX = int.Parse(widD[0]);
                var widthY = int.Parse(widD[1]);
                if (widthX < 0 || widthY < 0 || widthX > 200 || widthY > 200)
                    return null;

                var lenD = posD[1].Substring(2).Split(',');
                var lengthX = int.Parse(lenD[0]);
                var lengthY = int.Parse(lenD[1]);
                if (lengthX < 0 || lengthY < 0 || lengthX > 200 || lengthY > 200)
                    return null;
                return ":w=" + widthX + "," + widthY + " " + "l=" + lengthX + "," + lengthY + " " + posD[2];
            }
            catch
            {
                return null;
            }
        }
    }
}