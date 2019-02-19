#region

using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;
using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class PurchaseRoomPromotionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var PageId = Packet.PopInt();
            var ItemId = Packet.PopInt();
            var RoomId = Packet.PopInt();
            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var junk3 = Packet.PopBoolean();
            var Desc = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var CategoryId = Packet.PopInt();

            var Data = OblivionServer.GetGame().GetRoomManager().GenerateRoomData(RoomId);

            if (Data?.OwnerId != Session.GetHabbo().Id)
                return;

            if (Data.Promotion == null)
            {
                Data.Promotion = new RoomPromotion(Name, Desc, CategoryId);
            }
            else
            {
                Data.Promotion.Name = Name;
                Data.Promotion.Description = Desc;
                Data.Promotion.TimestampExpires += 7200;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery(
                    "REPLACE INTO `room_promotions` (`room_id`,`title`,`description`,`timestamp_start`,`timestamp_expire`,`category_id`) VALUES (@room_id, @title, @description, @start, @expires, @CategoryId)");
                dbClient.AddParameter("room_id", RoomId);
                dbClient.AddParameter("title", Name);
                dbClient.AddParameter("description", Desc);
                dbClient.AddParameter("start", Data.Promotion.TimestampStarted);
                dbClient.AddParameter("expires", Data.Promotion.TimestampExpires);
                dbClient.AddParameter("CategoryId", CategoryId);
                dbClient.RunQuery();
            }

            if (!Session.GetHabbo().GetBadgeComponent().HasBadge("RADZZ"))
                Session.GetHabbo().GetBadgeComponent().GiveBadge("RADZZ", true, Session);

            Session.SendMessage(new PurchaseOkComposer());
            if (Session.GetHabbo().InRoom && Session.GetHabbo().CurrentRoomId == RoomId)
                Session.GetHabbo().CurrentRoom.SendMessage(new RoomEventComposer(Data, Data.Promotion));

//            Session.GetHabbo()
//                .GetMessenger()
//                .BroadcastAchievement(Session.GetHabbo().Id, MessengerEventTypes.EVENT_STARTED, Name);
        }
    }
}