#region

using System;
using System.Globalization;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.Catalog.Utilities;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Quests;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class PurchaseFromCatalogAsGiftEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageId = Packet.PopInt();
            var ItemId = Packet.PopInt();
            var Data = Packet.PopString();
            var GiftUser = StringCharFilter.Escape(Packet.PopString());
            var GiftMessage = StringCharFilter.Escape(Packet.PopString().Replace(Convert.ToChar(5), ' '));
            var SpriteId = Packet.PopInt(); //giftspriteid
            var Ribbon = Packet.PopInt();
            var Colour = Packet.PopInt();
            var dnow = Packet.PopBoolean();

            CatalogPage Page;
            if (!OblivionServer.GetGame().GetCatalog().TryGetPage(PageId, out Page))
                return;

            if (!Page.Enabled || !Page.Visible || Page.MinimumRank > Session.GetHabbo().Rank)
                return;

            var Item = (CatalogItem) Page.Items[ItemId];
            if (Item == null)
                if (Page.ItemOffers.Contains(ItemId))
                {
                    Item = (CatalogItem) Page.ItemOffers[ItemId];
                    if (Item == null)
                        return;
                }
                else
                {
                    return;
                }

            if (!ItemUtility.CanGiftItem(Item))
                return;

            var PresentData = OblivionServer.GetGame().GetItemManager().GetItemBySpriteId(SpriteId);
            if (PresentData == null)
                return;

            if (Session.GetHabbo().Credits < Item.CostCredits)
            {
                Session.SendMessage(new PresentDeliverErrorMessageComposer(true, false));
                return;
            }

            if (Session.GetHabbo().Duckets < Item.CostPixels)
            {
                Session.SendMessage(new PresentDeliverErrorMessageComposer(false, true));
                return;
            }

            var Habbo = OblivionServer.GetHabboByUsername(GiftUser);
            if (Habbo == null)
            {
                Session.SendMessage(new GiftWrappingErrorComposer());
                return;
            }

            if (!Habbo.AllowGifts)
            {
                Session.SendNotification("Ops, este usuário não aceita presentes!");
                return;
            }

            if ((DateTime.Now - Session.GetHabbo().LastGiftPurchaseTime).TotalSeconds <= 15.0)
            {
                Session.SendNotification("Espere 15 segundos!");

                Session.GetHabbo().GiftPurchasingWarnings += 1;
                if (Session.GetHabbo().GiftPurchasingWarnings >= 25)
                    Session.GetHabbo().SessionGiftBlocked = true;
                return;
            }

            if (Session.GetHabbo().SessionGiftBlocked)
                return;


            var ED = GiftUser + Convert.ToChar(5) + GiftMessage + Convert.ToChar(5) + Session.GetHabbo().Id +
                     Convert.ToChar(5) + Item.Data.Id + Convert.ToChar(5) + SpriteId + Convert.ToChar(5) + Ribbon +
                     Convert.ToChar(5) + Colour;

            int newItemId;
            Task.Factory.StartNew(() =>
            {
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    //Insert the dummy item.
                    dbClient.SetQuery("INSERT INTO `items` (`base_item`,`user_id`,`extra_data`) VALUES ('" +
                                      PresentData.Id +
                                      "', '" + Habbo.Id + "', @extra_data)");
                    dbClient.AddParameter("extra_data", ED);
                    newItemId = Convert.ToInt32(dbClient.InsertQuery());

                    string ItemExtraData = null;
                    switch (Item.Data.InteractionType)
                    {
                        case InteractionType.None:
                            ItemExtraData = "";
                            break;

                            #region Pet handling

                        case InteractionType.Pet:
                       
                            try
                            {
                                var Bits = Data.Split('\n');
                                var PetName = Bits[0];
                                var Race = Bits[1];
                                var Color = Bits[2];

                                int.Parse(Race); // to trigger any possible errors

                                if (PetUtility.CheckPetName(PetName))
                                    return;

                                if (Race.Length > 2)
                                    return;

                                if (Color.Length != 6)
                                    return;

                                OblivionServer.GetGame()
                                    .GetAchievementManager()
                                    .ProgressAchievement(Session, "ACH_PetLover", 1);
                            }
                            catch
                            {
                                return;
                            }

                            break;

                            #endregion

                        case InteractionType.Floor:
                        case InteractionType.Wallpaper:
                        case InteractionType.Landscape:

                            double Number = 0;
                            try
                            {
                                Number = string.IsNullOrEmpty(Data) ? 0 : double.Parse(Data, OblivionServer.CultureInfo);
                            }
                            catch
                            {
                            }

                            ItemExtraData = Number.ToString(CultureInfo.CurrentCulture).Replace(',', '.');
                            break; // maintain extra data // todo: validate

                        case InteractionType.Postit:
                            ItemExtraData = "FFFF33";
                            break;

                        case InteractionType.Moodlight:
                            ItemExtraData = "1,1,1,#000000,255";
                            break;

                        case InteractionType.Trophy:
                            ItemExtraData = Session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" +
                                            DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + Data;
                            break;

                        case InteractionType.Mannequin:
                            ItemExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) +
                                            "Default Mannequin";
                            break;

                        case InteractionType.BadgeDisplay:
                            if (!Session.GetHabbo().GetBadgeComponent().HasBadge(Data))
                            {
                                Session.SendMessage(
                                    new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                                return;
                            }

                            ItemExtraData = Data + Convert.ToChar(9) + Session.GetHabbo().Username + Convert.ToChar(9) +
                                            DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
                            break;

                        default:
                            ItemExtraData = Data;
                            break;
                    }

                    //Insert the present, forever.
                    dbClient.SetQuery("INSERT INTO `user_presents` (`item_id`,`base_id`,`extra_data`) VALUES ('" +
                                      newItemId +
                                      "', '" + Item.Data.Id + "', @extra_data)");
                    dbClient.AddParameter("extra_data", string.IsNullOrEmpty(ItemExtraData) ? "" : ItemExtraData);
                    dbClient.RunQuery();

                    //Here we're clearing up a record, this is dumb, but okay.
                    dbClient.RunFastQuery("DELETE FROM `items` WHERE `id` = " + newItemId + " LIMIT 1;");
                }

                var GiveItem = ItemFactory.CreateGiftItem(PresentData, Habbo, ED, ED, newItemId);
                if (GiveItem != null)
                {
                    var Receiver = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Habbo.Id);
                    Receiver?.GetHabbo().GetInventoryComponent().TryAddItem(GiveItem);
                    Receiver?.SendMessage(new FurniListNotificationComposer(GiveItem.Id, 1));
                    Receiver?.SendMessage(new PurchaseOkComposer());
                    Receiver?.SendMessage(new FurniListAddComposer(GiveItem));
                    Receiver?.SendMessage(new FurniListUpdateComposer());

                    if (Habbo.Id != Session.GetHabbo().Id)
                    {
                        OblivionServer.GetGame()
                            .GetAchievementManager()
                            .ProgressAchievement(Session, "ACH_GiftGiver", 1);
                        if (Receiver != null)
                            OblivionServer.GetGame()
                                .GetAchievementManager()
                                .ProgressAchievement(Receiver, "ACH_GiftReceiver", 1);
                        OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.GIFT_OTHERS);
                    }
                }
            });

            Session.SendMessage(new PurchaseOkComposer(Item, PresentData));

            if (Item.CostCredits > 0)
            {
                Session.GetHabbo().Credits -= Item.CostCredits;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }

            if (Item.CostPixels > 0)
            {
                Session.GetHabbo().Duckets -= Item.CostPixels;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets,
                    Session.GetHabbo().Duckets));
            }

            Session.GetHabbo().LastGiftPurchaseTime = DateTime.Now;
            
        }
    }
}