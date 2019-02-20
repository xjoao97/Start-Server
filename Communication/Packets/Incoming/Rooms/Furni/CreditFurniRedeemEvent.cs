#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni
{
    internal class CreditFurniRedeemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session, true))
                return;

            var Exchange = Room.GetRoomItemHandler().GetItem(Packet.PopInt());
            if (Exchange == null)
                return;

            if (!Exchange.GetBaseItem().ItemName.StartsWith("CF_") &&
                !Exchange.GetBaseItem().ItemName.StartsWith("CFC_") &&
                !Exchange.GetBaseItem().ItemName.StartsWith("DF_") &&
                !Exchange.GetBaseItem().ItemName.StartsWith("DFD_"))
                return;

            var Split = Exchange.GetBaseItem().ItemName.Split('_');
            var Value = int.Parse(Split[1]);

            if (Value > 0)
                if (Exchange.GetBaseItem().ItemName.StartsWith("CF_") ||
                    Exchange.GetBaseItem().ItemName.StartsWith("CFC_"))
                {
                    Session.GetHabbo().Credits += Value;
                    Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                }
                else if (Exchange.GetBaseItem().ItemName.StartsWith("DF_") ||
                         Exchange.GetBaseItem().ItemName.StartsWith("DFD_"))
                {
                    Session.GetHabbo().Diamonds += Value;
                    Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Diamonds, Value, 5));
                }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE FROM `items` WHERE `id` = '" + Exchange.Id + "' LIMIT 1");
            }

            Session.SendMessage(new FurniListUpdateComposer());
            Room.GetRoomItemHandler().RemoveFurniture(null, Exchange.Id, false);
            Session.GetHabbo().GetInventoryComponent().RemoveItem(Exchange.Id);
        }
    }
}