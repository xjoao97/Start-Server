#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class EditRoomEventEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var RoomId = Packet.PopInt();
            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Desc = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            if (Data == null)
                return;

            if (Data.OwnerId != Session.GetHabbo().Id)
                return; //HAX

            if (Data.Promotion == null)
            {
                Session.SendNotification("Oops, it looks like there isn't a room promotion in this room?");
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "UPDATE `room_promotions` SET `title` = @title, `description` = @desc WHERE `room_id` = " + RoomId +
                    " LIMIT 1");
                dbClient.AddParameter("title", Name);
                dbClient.AddParameter("desc", Desc);
                dbClient.RunQuery();
            }

            Room Room;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Convert.ToInt32(RoomId), out Room))
                return;

            Data.Promotion.Name = Name;
            Data.Promotion.Description = Desc;
            Room.SendMessage(new RoomEventComposer(Data, Data.Promotion));
        }
    }
}