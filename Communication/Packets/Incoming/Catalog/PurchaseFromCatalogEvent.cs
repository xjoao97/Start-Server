#region

using System;
using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Oblivion.Communication.Packets.Outgoing.Inventory.Bots;
using Oblivion.Communication.Packets.Outgoing.Inventory.Furni;
using Oblivion.Communication.Packets.Outgoing.Inventory.Pets;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.Core;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.Catalog.Utilities;
using Oblivion.HabboHotel.GameClients;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Users.Effects;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class PurchaseFromCatalogEvent : IPacketEvent //todo: improve
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var PageId = Packet.PopInt();
            var ItemId = Packet.PopInt();
            var ExtraData = Packet.PopString();
            var Amount = Packet.PopInt();


            if (!OblivionServer.GetGame().GetCatalog().TryGetPage(PageId, out CatalogPage Page))
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

            if (Item.Data != null)
            {
                // NOMBRES DE COLORES
                if (Item.Data.InteractionType == InteractionType.NAME_COLOR)
                {
                    if (Item.CostCredits > Session.GetHabbo().Credits)
                        return;

                    if (Item.CostCredits > 0)
                    {
                        Session.GetHabbo().Credits -= Item.CostCredits;
                        Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                    }

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("UPDATE users SET name_color = '" + Item.Name + "' WHERE id = '" + Session.GetHabbo().Id + "'");
                    }

                    Session.GetHabbo().NameColor = Item.Name;
                    Session.SendMessage(new ScrSendUserInfoComposer(Session.GetHabbo()));
                    Session.SendMessage(new PurchaseOkComposer(Item, Item.Data));
                    Session.SendMessage(new FurniListUpdateComposer());
                    return;
                }

                // COLOR DE PREFIJO
                if (Item.Data.InteractionType == InteractionType.PREFIX_COLOR)
                {
                    if (Item.CostCredits > Session.GetHabbo().Credits)
                        return;

                    if (Item.CostCredits > 0)
                    {
                        Session.GetHabbo().Credits -= Item.CostCredits;
                        Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                    }

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("UPDATE users SET prefix_color = '" + Item.Name + "' WHERE id = '" + Session.GetHabbo().Id + "'");
                    }

                    Session.GetHabbo().PrefixColor = Item.Name;
                    Session.SendMessage(new ScrSendUserInfoComposer(Session.GetHabbo()));
                    Session.SendMessage(new PurchaseOkComposer(Item, Item.Data));
                    Session.SendMessage(new FurniListUpdateComposer());
                    return;
                }

                // PREFIJO
                if (Item.Data.InteractionType == InteractionType.PREFIX_NAME)
                {
                    if (Item.CostCredits > Session.GetHabbo().Credits)
                        return;

                    if (Item.CostCredits > 0)
                    {
                        Session.GetHabbo().Credits -= Item.CostCredits;
                        Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
                    }

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunFastQuery("UPDATE users SET prefix_name = '" + ExtraData + "' WHERE id = '" + Session.GetHabbo().Id + "'");
                    }

                    Session.GetHabbo().PrefixName = ExtraData;
                    Session.SendMessage(new ScrSendUserInfoComposer(Session.GetHabbo()));
                    Session.SendMessage(new PurchaseOkComposer(Item, Item.Data));
                    Session.SendMessage(new FurniListUpdateComposer());
                    return;
                }
            }

            if (Amount < 1 || Amount > 100 || !Item.HaveOffer)
                Amount = 1;

            var AmountPurchase = Item.Amount > 1 ? Item.Amount : Amount;

            var TotalCreditsCost = Amount > 1
                ? Item.CostCredits * Amount - (int) Math.Floor((double) Amount / 6) * Item.CostCredits
                : Item.CostCredits;
            var TotalPixelCost = Amount > 1
                ? Item.CostPixels * Amount - (int) Math.Floor((double) Amount / 6) * Item.CostPixels
                : Item.CostPixels;
            var TotalDiamondCost = Amount > 1
                ? Item.CostDiamonds * Amount - (int) Math.Floor((double) Amount / 6) * Item.CostDiamonds
                : Item.CostDiamonds;
            var TotalGotwCost = Amount > 1
                ? Item.CostGotw * Amount - (int) Math.Floor((double) Amount / 6) * Item.CostGotw
                : Item.CostGotw;

            if (Session.GetHabbo().Credits < TotalCreditsCost || Session.GetHabbo().Duckets < TotalPixelCost ||
                Session.GetHabbo().Diamonds < TotalDiamondCost || Session.GetHabbo().GOTWPoints < TotalGotwCost)
                return;

            var LimitedEditionSells = 0;
            var LimitedEditionStack = 0;

            #region Create the extradata

            switch (Item.Data.InteractionType)
            {
                case InteractionType.None:
                    ExtraData = "";
                    break;

                case InteractionType.GuildItem:
                case InteractionType.GuildForum:
                case InteractionType.GuildGate:
                    int groupId;

                    int.TryParse(ExtraData, out groupId);

                    var group = OblivionServer.GetGame().GetGroupManager().TryGetGroup(groupId);

                    ItemFactory.CreateMultipleItems(Item.Data, Session.GetHabbo(), "0", AmountPurchase, groupId);
                    break;

                    #region Pet handling

                case InteractionType.Pet:
                    try
                    {
                        var Bits = ExtraData.Split('\n');
                        var PetName = Bits[0];
                        var Race = Bits[1];
                        var Color = Bits[2];

                        int.Parse(Race); // to trigger any possible errors

                        if (!PetUtility.CheckPetName(PetName))
                            return;

                        if (Race.Length > 2)
                            return;

                        if (Color.Length != 6)
                            return;

                        OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(Session, "ACH_PetLover", 1);
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e.ToString());
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
                        Number = string.IsNullOrEmpty(ExtraData)
                            ? 0
                            : double.Parse(ExtraData, OblivionServer.CultureInfo);
                    }
                    catch (Exception e)
                    {
                        Logging.HandleException(e, "Catalog.HandlePurchase: " + ExtraData);
                    }

                    ExtraData = Number.ToString().Replace(',', '.');
                    break; // maintain extra data // todo: validate

                case InteractionType.Postit:
                    ExtraData = "FFFF33";
                    break;

                case InteractionType.Moodlight:
                    ExtraData = "1,1,1,#000000,255";
                    break;

                case InteractionType.Trophy:
                    ExtraData = Session.GetHabbo().Username + Convert.ToChar(9) + DateTime.Now.Day + "-" +
                                DateTime.Now.Month + "-" + DateTime.Now.Year + Convert.ToChar(9) + ExtraData;
                    break;

                case InteractionType.Mannequin:
                    ExtraData = "m" + Convert.ToChar(5) + ".ch-210-1321.lg-285-92" + Convert.ToChar(5) +
                                "Default Mannequin";
                    break;

                case InteractionType.BadgeDisplay:
                    if (!Session.GetHabbo().GetBadgeComponent().HasBadge(ExtraData))
                    {
                        Session.SendMessage(
                            new BroadcastMessageAlertComposer("Oops, it appears that you do not own this badge."));
                        return;
                    }

                    ExtraData = ExtraData + Convert.ToChar(9) + Session.GetHabbo().Username + Convert.ToChar(9) +
                                DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year;
                    break;

                case InteractionType.Badge:
                {
                    if (Session.GetHabbo().GetBadgeComponent().HasBadge(Item.Data.ItemName))
                    {
                        Session.SendMessage(new PurchaseErrorComposer(1));
                        return;
                    }
                    break;
                }

                default:
                    ExtraData = "";
                    break;
            }

            

            #endregion

            if (Item.IsLimited)
            {
                if (Item.LimitedEditionStack <= Item.LimitedEditionSells)
                {
                    Session.SendNotification("This item has sold out!\n\n" +
                                             "Please note, you have not recieved another item (You have also not been charged for it!)");
                    Session.SendMessage(new CatalogUpdatedComposer());
                    Session.SendMessage(new PurchaseOkComposer());
                    return;
                }

                Item.LimitedEditionSells++;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunFastQuery("UPDATE `catalog_items` SET `limited_sells` = '" + Item.LimitedEditionSells +
                                      "' WHERE `id` = '" + Item.Id + "' LIMIT 1");
                    LimitedEditionSells = Item.LimitedEditionSells;
                    LimitedEditionStack = Item.LimitedEditionStack;
                }
            }

            if (Item.CostCredits > 0)
            {
                Session.GetHabbo().Credits -= TotalCreditsCost;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }

            if (Item.CostPixels > 0)
            {
                Session.GetHabbo().Duckets -= TotalPixelCost;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets,
                    Session.GetHabbo().Duckets)); //Love you, Tom.
            }

            if (Item.CostDiamonds > 0)
            {
                Session.GetHabbo().Diamonds -= TotalDiamondCost;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Diamonds, 0, 5));
            }
            if (Item.CostGotw > 0)
            {
                Session.GetHabbo().GOTWPoints -= TotalGotwCost;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().GOTWPoints, 0, 103));
            }

            switch (Item.Data.Type.ToString().ToLower())
            {
                default:
                    var GeneratedGenericItems = new List<Item>();

                    Item NewItem;
                    switch (Item.Data.InteractionType)
                    {
                        default:
                            if (AmountPurchase > 1)
                            {
                                var Items = ItemFactory.CreateMultipleItems(Item.Data, Session.GetHabbo(), ExtraData,
                                    AmountPurchase);

                                if (Items != null)
                                    GeneratedGenericItems.AddRange(Items);
                            }
                            else
                            {
                                NewItem = ItemFactory.CreateSingleItemNullable(Item.Data, Session.GetHabbo(), ExtraData,
                                    ExtraData, 0, LimitedEditionSells, LimitedEditionStack);

                                if (NewItem != null)
                                    GeneratedGenericItems.Add(NewItem);
                            }
                            break;

                        case InteractionType.GuildForum:
                        case InteractionType.GuildGate:
                        case InteractionType.GuildItem:

                            break;

                        case InteractionType.Arrow:
                        case InteractionType.Teleport:
                            for (var i = 0; i < AmountPurchase; i++)
                            {
                                var TeleItems = ItemFactory.CreateTeleporterItems(Item.Data, Session.GetHabbo());

                                if (TeleItems != null)
                                    GeneratedGenericItems.AddRange(TeleItems);
                            }
                            break;

                        case InteractionType.Moodlight:
                        {
                            if (AmountPurchase > 1)
                            {
                                var Items = ItemFactory.CreateMultipleItems(Item.Data, Session.GetHabbo(), ExtraData,
                                    AmountPurchase);

                                if (Items != null)
                                {
                                    GeneratedGenericItems.AddRange(Items);
                                    foreach (var I in Items)
                                        ItemFactory.CreateMoodlightData(I);
                                }
                            }
                            else
                            {
                                NewItem = ItemFactory.CreateSingleItemNullable(Item.Data, Session.GetHabbo(), ExtraData,
                                    ExtraData);

                                if (NewItem != null)
                                {
                                    GeneratedGenericItems.Add(NewItem);
                                    ItemFactory.CreateMoodlightData(NewItem);
                                }
                            }
                        }
                            break;

                        case InteractionType.Toner:
                        {
                            if (AmountPurchase > 1)
                            {
                                var Items = ItemFactory.CreateMultipleItems(Item.Data, Session.GetHabbo(), ExtraData,
                                    AmountPurchase);

                                if (Items != null)
                                {
                                    GeneratedGenericItems.AddRange(Items);
                                    foreach (var I in Items)
                                        ItemFactory.CreateTonerData(I);
                                }
                            }
                            else
                            {
                                NewItem = ItemFactory.CreateSingleItemNullable(Item.Data, Session.GetHabbo(), ExtraData,
                                    ExtraData);

                                if (NewItem != null)
                                {
                                    GeneratedGenericItems.Add(NewItem);
                                    ItemFactory.CreateTonerData(NewItem);
                                }
                            }
                        }
                            break;

                        case InteractionType.Deal:
                        {
                            //Fetch the deal where the ID is this items ID.
                            var DealItems = from d in Page.Deals.Values.ToList() where d.Id == Item.Id select d;

                            //This bit, iterating ONE item? How can I make this simpler
                            foreach (
                                var Items in
                                DealItems.SelectMany(
                                    DealItem =>
                                        DealItem.ItemDataList.ToList()
                                            .Select(
                                                CatalogItem =>
                                                    ItemFactory.CreateMultipleItems(CatalogItem.Data, Session.GetHabbo(),
                                                        "",
                                                        AmountPurchase)).Where(Items => Items != null)))
                                GeneratedGenericItems.AddRange(Items);
                            break;
                        }
                    }

                    foreach (
                        var PurchasedItem in
                        GeneratedGenericItems.Where(
                            PurchasedItem => Session.GetHabbo().GetInventoryComponent().TryAddItem(PurchasedItem)))
                        Session.SendMessage(new FurniListNotificationComposer(PurchasedItem.Id, 1));
                    break;

                case "e":
                    AvatarEffect Effect;

                    if (Session.GetHabbo().Effects().HasEffect(Item.Data.SpriteId))
                    {
                        Effect = Session.GetHabbo().Effects().GetEffectNullable(Item.Data.SpriteId);

                        Effect?.AddToQuantity();
                    }
                    else
                    {
                        Effect = AvatarEffectFactory.CreateNullable(Session.GetHabbo(), Item.Data.SpriteId, 3600);
                    }

                    if (Effect != null) // && Session.GetHabbo().Effects().TryAdd(Effect))
                        Session.SendMessage(new AvatarEffectAddedComposer(Item.Data.SpriteId, 3600));
                    break;

                case "r":
                    var Bot = BotUtility.CreateBot(Item.Data, Session.GetHabbo().Id);
                    if (Bot != null)
                    {
                        Session.GetHabbo().GetInventoryComponent().TryAddBot(Bot);
                        Session.SendMessage(
                            new BotInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetBots()));
                        Session.SendMessage(new FurniListNotificationComposer(Bot.Id, 5));
                    }
                    else
                    {
                        Session.SendNotification(
                            "Oops! There was an error whilst purchasing this bot. It seems that there is no bot data for the bot!");
                    }
                    break;

                case "b":
                {
                    Session.GetHabbo().GetBadgeComponent().GiveBadge(Item.Data.ItemName, true, Session);
                    Session.SendMessage(new FurniListNotificationComposer(0, 4));
                    break;
                }

                case "p":
                {

                        var PetData = ExtraData.Split('\n');
                        var GeneratedPet = PetUtility.CreatePet(Session.GetHabbo().Id, PetData[0], Item.Data.BehaviourData, PetData[1],
                            PetData[2]);

                        Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet);

                      /*  switch (Item.Data.InteractionType)
                    {
                            #region Pets


                            #region Pet 0

                        case InteractionType.Pet0:
                            var PetData = ExtraData.Split('\n');
                            var GeneratedPet = PetUtility.CreatePet(Session.GetHabbo().Id, PetData[0], 0, PetData[1],
                                PetData[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet);

                            break;

                            #endregion

                            #region Pet 1

                        case InteractionType.Pet1:
                            var PetData1 = ExtraData.Split('\n');
                            var GeneratedPet1 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData1[0], 1, PetData1[1],
                                PetData1[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet1);

                            break;

                            #endregion

                            #region Pet 2

                        case InteractionType.Pet2:
                            var PetData5 = ExtraData.Split('\n');
                            var GeneratedPet5 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData5[0], 2, PetData5[1],
                                PetData5[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet5);

                            break;

                            #endregion

                            #region Pet 3

                        case InteractionType.Pet3:
                            var PetData2 = ExtraData.Split('\n');
                            var GeneratedPet2 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData2[0], 3, PetData2[1],
                                PetData2[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet2);

                            break;

                            #endregion

                            #region Pet 4

                        case InteractionType.Pet4:
                            var PetData3 = ExtraData.Split('\n');
                            var GeneratedPet3 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData3[0], 4, PetData3[1],
                                PetData3[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet3);

                            break;

                            #endregion

                            #region Pet 5

                        case InteractionType.Pet5:
                            var PetData7 = ExtraData.Split('\n');
                            var GeneratedPet7 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData7[0], 5, PetData7[1],
                                PetData7[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet7);

                            break;

                            #endregion

                            #region Pet 6 (wrong?)

                        case InteractionType.Pet6:
                            var PetData4 = ExtraData.Split('\n');
                            var GeneratedPet4 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData4[0], 6, PetData4[1],
                                PetData4[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet4);

                            break;

                            #endregion

                            #region Pet 7 (wrong?)

                        case InteractionType.Pet7:
                            var PetData6 = ExtraData.Split('\n');
                            var GeneratedPet6 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData6[0], 7, PetData6[1],
                                PetData6[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet6);

                            break;

                            #endregion

                            #region Pet 8

                        case InteractionType.Pet8:
                            var PetData8 = ExtraData.Split('\n');
                            var GeneratedPet8 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData8[0], 8, PetData8[1],
                                PetData8[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet8);

                            break;

                            #endregion

                            #region Pet 8

                        case InteractionType.Pet9:
                            var PetData9 = ExtraData.Split('\n');
                            var GeneratedPet9 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData9[0], 9, PetData9[1],
                                PetData9[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet9);

                            break;

                            #endregion

                            #region Pet 10

                        case InteractionType.Pet10:
                            var PetData10 = ExtraData.Split('\n');
                            var GeneratedPet10 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData10[0], 10,
                                PetData10[1], PetData10[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet10);

                            break;

                            #endregion

                            #region Pet 11

                        case InteractionType.Pet11:
                            var PetData11 = ExtraData.Split('\n');
                            var GeneratedPet11 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData11[0], 11,
                                PetData11[1], PetData11[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet11);

                            break;

                            #endregion

                            #region Pet 12

                        case InteractionType.Pet12:
                            var PetData12 = ExtraData.Split('\n');
                            var GeneratedPet12 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData12[0], 12,
                                PetData12[1], PetData12[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet12);

                            break;

                            #endregion

                            #region Pet 13

                        case InteractionType.Pet13: //Caballo - Horse
                            var PetData13 = ExtraData.Split('\n');
                            var GeneratedPet13 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData13[0], 13,
                                PetData13[1], PetData13[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet13);

                            break;

                            #endregion

                            #region Pet 14

                        case InteractionType.Pet14:
                            var PetData14 = ExtraData.Split('\n');
                            var GeneratedPet14 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData14[0], 14,
                                PetData14[1], PetData14[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet14);

                            break;

                            #endregion

                            #region Pet 15

                        case InteractionType.Pet15:
                            var PetData15 = ExtraData.Split('\n');
                            var GeneratedPet15 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData15[0], 15,
                                PetData15[1], PetData15[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet15);

                            break;

                            #endregion

                            #region Pet 16

                        case InteractionType.Pet16: // Mascota Agregada
                            var PetData16 = ExtraData.Split('\n');
                            var GeneratedPet16 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData16[0], 16,
                                PetData16[1], PetData16[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet16);

                            break;

                            #endregion

                            #region Pet 17

                        case InteractionType.Pet17: // Mascota Agregada
                            var PetData17 = ExtraData.Split('\n');
                            var GeneratedPet17 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData17[0], 17,
                                PetData17[1], PetData17[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet17);

                            break;

                            #endregion

                            #region Pet 18

                        case InteractionType.Pet18: // Mascota Agregada
                            var PetData18 = ExtraData.Split('\n');
                            var GeneratedPet18 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData18[0], 18,
                                PetData18[1], PetData18[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet18);

                            break;

                            #endregion

                            #region Pet 19

                        case InteractionType.Pet19: // Mascota Agregada
                            var PetData19 = ExtraData.Split('\n');
                            var GeneratedPet19 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData19[0], 19,
                                PetData19[1], PetData19[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet19);

                            break;

                            #endregion

                            #region Pet 20

                        case InteractionType.Pet20: // Mascota Agregada
                            var PetData20 = ExtraData.Split('\n');
                            var GeneratedPet20 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData20[0], 20,
                                PetData20[1], PetData20[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet20);

                            break;

                            #endregion

                            #region Pet 21

                        case InteractionType.Pet21: // Mascota Agregada
                            var PetData21 = ExtraData.Split('\n');
                            var GeneratedPet21 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData21[0], 21,
                                PetData21[1], PetData21[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet21);

                            break;

                            #endregion

                            #region Pet 22

                        case InteractionType.Pet22: // Mascota Agregada
                            var PetData22 = ExtraData.Split('\n');
                            var GeneratedPet22 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData22[0], 22,
                                PetData22[1], PetData22[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet22);

                            break;

                            #endregion

                            #region Pet 28

                        case InteractionType.Pet28: // Mascota Agregada
                            var PetData28 = ExtraData.Split('\n');
                            var GeneratedPet28 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData28[0], 28,
                                PetData28[1], PetData28[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet28);

                            break;

                            #endregion

                            #region Pet 29

                        case InteractionType.Pet29:
                            var PetData29 = ExtraData.Split('\n');
                            var GeneratedPet29 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData29[0], 29,
                                PetData29[1], PetData29[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet29);

                            break;

                            #endregion

                            #region Pet 30

                        case InteractionType.Pet30:
                            var PetData30 = ExtraData.Split('\n');
                            var GeneratedPet30 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData30[0], 30,
                                PetData30[1], PetData30[2]);

                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet30);

                            break;

                            #endregion

                            #region Pet 34

                        case InteractionType.Pet34:
                            var PetData34 = ExtraData.Split('\n');
                            var GeneratedPet34 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData34[0], 34,
                                PetData34[1], PetData34[2]);
                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet34);

                            break;

                            #endregion

                            #region Pet 35

                        case InteractionType.Pet35:
                            var PetData35 = ExtraData.Split('\n');
                            var GeneratedPet35 = PetUtility.CreatePet(Session.GetHabbo().Id, PetData35[0], 35,
                                PetData35[1], PetData35[2]);
                            Session.GetHabbo().GetInventoryComponent().TryAddPet(GeneratedPet35);

                            break;

                            #endregion

                            #endregion
                    }
*/
                    Session.SendMessage(new FurniListNotificationComposer(0, 3));
                    Session.SendMessage(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));

                    var PetFood = OblivionServer.GetGame().GetItemManager().GetItem(320);
                    if (PetFood != null)
                    {
                        var Food = ItemFactory.CreateSingleItemNullable(PetFood, Session.GetHabbo(), "", "");
                        if (Food != null)
                        {
                            Session.GetHabbo().GetInventoryComponent().TryAddItem(Food);
                            Session.SendMessage(new FurniListNotificationComposer(Food.Id, 1));
                        }
                    }
                    break;
                }
            }


            Session.SendMessage(new PurchaseOkComposer(Item, Item.Data));
            Session.SendMessage(new FurniListUpdateComposer());
        }
    }
}