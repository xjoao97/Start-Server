using System;
using System.Data;
using System.Linq;
using Oblivion.Core;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.Catalog.Utilities;
using Oblivion.HabboHotel.Items;

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    public class CatalogPageComposer : ServerPacket
    {
        public CatalogPageComposer(CatalogPage Page, string CataMode)
            : base(ServerPacketHeader.CatalogPageMessageComposer)
        {
            WriteInteger(Page.Id);
            WriteString(CataMode);
            WriteString(Page.Template);

            WriteInteger(Page.PageStrings1.Count);
            foreach (var s in Page.PageStrings1)
                WriteString(s);

            WriteInteger(Page.PageStrings2.Count);
            foreach (var s in Page.PageStrings2)
                WriteString(s);

            if (!Page.Template.Equals("frontpage") && !Page.Template.Equals("club_buy"))
            {
                WriteInteger(Page.Items.Count);
                foreach (CatalogItem Item in Page.Items.Values)
                {
                    WriteInteger(Item.Id);
                    WriteString(Item.Name);
                    WriteBoolean(false); //IsRentable
                    WriteInteger(Item.CostCredits);

                    if (Item.CostDiamonds > 0)
                    {
                        WriteInteger(Item.CostDiamonds);
                        WriteInteger(5); // Diamonds
                    }
                    else
                    {
                        WriteInteger(Item.CostPixels);
                        WriteInteger(0); // Type of PixelCost
                    }

                    WriteBoolean(ItemUtility.CanGiftItem(Item));

                    if (Item.Data.InteractionType == InteractionType.Deal)
                    {
                        foreach (var Deal in Page.Deals.Values)
                        {
                            WriteInteger(Deal.ItemDataList.Count); //Count

                            foreach (var DealItem in Deal.ItemDataList.ToList())
                            {
                                WriteString(DealItem.Data.Type.ToString());
                                WriteInteger(DealItem.Data.SpriteId);
                                WriteString("");
                                WriteInteger(1);
                                WriteBoolean(false);
                            }
                            WriteInteger(0); //club_level
                            WriteBoolean(ItemUtility.CanSelectAmount(Item));
                        }
                    }
                    else
                    {
                        WriteInteger(string.IsNullOrEmpty(Item.Badge) ? 1 : 2);
                        //Count 1 item if there is no badge, otherwise count as 2.
                        {
                            if (!string.IsNullOrEmpty(Item.Badge))
                            {
                                WriteString("b");
                                WriteString(Item.Badge);
                            }

                            WriteString(Item.Data.Type.ToString());
                            if (Item.Data.Type.ToString().ToLower() == "b")
                            {
                                //This is just a badge, append the name.
                                WriteString(Item.Data.ItemName);
                            }
                            else
                            {
                                WriteInteger(Item.Data.SpriteId);
                                if (Item.Data.InteractionType == InteractionType.Wallpaper ||
                                    Item.Data.InteractionType == InteractionType.Floor ||
                                    Item.Data.InteractionType == InteractionType.Landscape)
                                {
                                    WriteString(Item.Name.Split('_')[2]);
                                }
                                else if (Item.Data.InteractionType == InteractionType.Bot) //Bots
                                {
                                    CatalogBot CatalogBot;
                                    WriteString(
                                        !OblivionServer.GetGame().GetCatalog().TryGetBot(Item.ItemId, out CatalogBot)
                                            ? "hd-180-7.ea-1406-62.ch-210-1321.hr-831-49.ca-1813-62.sh-295-1321.lg-285-92"
                                            : CatalogBot.Figure);
                                }
                                else if (Item.ExtraData != null)
                                {
                                    WriteString(Item.ExtraData ?? string.Empty);
                                }
                                WriteInteger(Item.Amount);
                                WriteBoolean(Item.IsLimited); // IsLimited
                                if (Item.IsLimited)
                                {
                                    WriteInteger(Item.LimitedEditionStack);
                                    WriteInteger(Item.LimitedEditionStack - Item.LimitedEditionSells);
                                }
                            }
                            WriteInteger(0); //club_level
                            WriteBoolean(ItemUtility.CanSelectAmount(Item));
                            WriteBoolean(true);
                            WriteString("");
                        }
                    }
                }
            }

            else
            {
                WriteInteger(0);
            }
            WriteInteger(-1);
            WriteBoolean(false);

            if (Page.Template.Equals("frontpage4"))
            {
                var list = OblivionServer.GetGame().GetCatalog().IndexText;
                WriteInteger(list.Count); // count
                foreach (DataRow Catalog in list)
                {

                    WriteInteger(1); // id
                    WriteString(Convert.ToString(Catalog["title"])); // name
                    WriteString(Convert.ToString(Catalog["image"])); // image
                    WriteInteger(0);
                    WriteString(Convert.ToString(Catalog["page_link"])); // page link?
                    WriteInteger(Convert.ToInt32(Catalog["page_id"])); // page id?
                }
            }
        }
    }
}