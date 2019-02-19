#region

using Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.Catalog.Clothing;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class UseSellableClothingEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            var ItemId = Packet.PopInt();

            var Item = Room.GetRoomItemHandler().GetItem(ItemId);
            if (Item == null)
                return;

            if (Item.Data == null)
                return;

            if (Item.UserID != Session.GetHabbo().Id)
                return;

            if (Item.Data.InteractionType != InteractionType.PurchasableClothing)
            {
                Session.SendNotification("Oops, this item isn't set as a sellable clothing item!");
                return;
            }

            if (Item.Data.ClothingId == 0)
            {
                Session.SendNotification(
                    "Oops, this item doesn't have a linking clothing configuration, please report it!");
                return;
            }

            ClothingItem Clothing = null;
            if (
                !OblivionServer.GetGame()
                    .GetCatalog()
                    .GetClothingManager()
                    .TryGetClothing(Item.Data.ClothingId, out Clothing))
            {
                Session.SendNotification("Oops, we couldn't find this clothing part!");
                return;
            }

            //Quickly delete it from the database.
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("DELETE FROM `items` WHERE `id` = @ItemId LIMIT 1");
                dbClient.AddParameter("ItemId", Item.Id);
                dbClient.RunQuery();
            }

            //Remove the item.
            Room.GetRoomItemHandler().RemoveFurniture(Session, Item.Id);

            Session.GetHabbo().GetClothing().AddClothing(Clothing.ClothingName, Clothing.PartIds);
            Session.SendMessage(new FigureSetIdsComposer(Session.GetHabbo().GetClothing().GetClothingAllParts));
            Session.SendMessage(new RoomNotificationComposer("figureset.redeemed.success"));
            Session.SendWhisper("If for some reason cannot see your new clothing, reload the hotel!");
        }
    }
}