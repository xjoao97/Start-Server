using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    public class CatalogIndexComposer : ServerPacket
    {
        public CatalogIndexComposer(GameClient Session, ICollection<CatalogPage> Pages)
            : base(ServerPacketHeader.CatalogIndexMessageComposer)
        {
            WriteRootIndex(Session, Pages);


            foreach (var Page in Pages.Where(Page => Page.ParentId == -1 && Page.MinimumRank <= Session.GetHabbo().Rank)
            )
            {
                WritePage(Page, CalcTreeSize(Session, Pages, Page.Id));


                foreach (
                    var child in
                    Pages.Where(child => child.ParentId == Page.Id && child.MinimumRank <= Session.GetHabbo().Rank))
                {
                    WritePage(child, CalcTreeSize(Session, Pages, child.Id));


                    foreach (
                        var baby in
                        Pages.Where(baby => baby.ParentId == child.Id && baby.MinimumRank <= Session.GetHabbo().Rank))
                        WritePage(baby, 0);
                }
            }


            WriteBoolean(false);
            WriteString("NORMAL");
        }


        public void WriteRootIndex(GameClient Session, ICollection<CatalogPage> Pages)
        {
            WriteBoolean(true);
            WriteInteger(0);
            WriteInteger(-1);
            WriteString("root");
            WriteString(string.Empty);
            WriteInteger(0);
            WriteInteger(CalcTreeSize(Session, Pages, -1));
        }


        public void WritePage(CatalogPage Page, int TreeSize)
        {
            WriteBoolean(Page.Visible);
            WriteInteger(Page.Icon);
            WriteInteger(!Page.Enabled ? -1 : Page.Id);
            WriteString(Page.PageLink);
            WriteString(Page.Caption);
            WriteInteger(Page.ItemOffers.Count);
            foreach (int i in Page.ItemOffers.Keys)
                WriteInteger(i);
            WriteInteger(TreeSize);
        }


        public int CalcTreeSize(GameClient Session, ICollection<CatalogPage> Pages, int ParentId)
            =>
                Pages.Where(Page => Page.MinimumRank <= Session.GetHabbo().Rank && Page.ParentId == ParentId)
                    .Count(Page => Page.ParentId == ParentId);
    }
}