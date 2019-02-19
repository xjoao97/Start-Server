#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Furni
{
    internal class RequestFurniInventoryEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Items = Session.GetHabbo().GetInventoryComponent().GetItems;

            var enumerable = Items as Item[] ?? Items.ToArray();
            if (Session.GetHabbo().InventoryAlert == false)
            {
                Session.GetHabbo().InventoryAlert = true;
                var TotalCount = enumerable.Length;
                if (TotalCount >= 7000)
                    Session.SendNotification("Hey! Our system has detected that you have a very large inventory!\n\n" +
                                             "The maximum an inventory can load is 8000 items, you have " + TotalCount +
                                             " items loaded now.\n\n" +
                                             "If you have 8000 loaded now then you're probably over the limit and some items will be hidden until you free up space.\n\n" +
                                             "Please note that we are not responsible for you crashing because of too large inventorys!");
            }


            Session.SendMessage(new FurniListComposer(enumerable.ToList()));
        }
    }
}