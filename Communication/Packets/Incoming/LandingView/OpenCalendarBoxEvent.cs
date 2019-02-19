using System;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.LandingView;

namespace Oblivion.Communication.Packets.Incoming.LandingView
{
    internal class OpenCalendarBoxEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var eventName = Packet.PopString();
            var giftDay = Packet.PopInt();

            var habboStats = Session?.GetHabbo()?.GetStats();

            var currentDay = DateTime.Now.Day - 1;

            if (habboStats == null ||
                habboStats.openedGifts.Contains(giftDay) || giftDay < currentDay - 2 ||
                giftDay > currentDay || eventName != "xmas16")
                return;

            Item newItem;
            if (!OblivionServer.GetGame().GetLandingManager().GenerateCalendarItem(
                Session.GetHabbo(), eventName, giftDay, out newItem))
                return;

            habboStats.addOpenedGift(giftDay, Session.GetHabbo().Id);
            Session.GetHabbo().GetInventoryComponent().TryAddItem(newItem);
            Session.SendMessage(new FurniListUpdateComposer());
            Session.SendMessage(new CampaignCalendarGiftComposer(newItem.Data.ItemName));
        }
    }
}