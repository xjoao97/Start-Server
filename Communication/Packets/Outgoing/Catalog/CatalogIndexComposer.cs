using System.Collections.Generic;
using System.Linq;
using Oblivion.HabboHotel.Catalog;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    public class CatalogIndexComposer : ServerPacket
    {
        public CatalogIndexComposer(GameClient session, ICollection<CatalogPage> pages)
            : base(ServerPacketHeader.CatalogIndexMessageComposer)
        {
            WriteBoolean(true);
            WriteInteger(0);
            WriteInteger(-1);
            WriteString("root");
            WriteString("");
            WriteInteger(0);

            WriteInteger(pages.Count);
            foreach (var page in pages)
            {
                Append(session, page);
            }

            WriteBoolean(false);
            WriteString("NORMAL");
        }

        public void Append(GameClient session, CatalogPage page)
        {
            var pages = OblivionServer.GetGame().GetCatalog().GetPages(session, page.Id);

            WriteBoolean(page.Visible);
            WriteInteger(page.Icon);
            WriteInteger(page.Enabled ? page.Id : -1);
            WriteString(page.PageLink);
            WriteString(page.Caption);

            WriteInteger(page.ItemOffers.Count);
            foreach (int key in page.ItemOffers.Keys)
            {
                WriteInteger(key);
            }

            WriteInteger(pages.Count);
            foreach (var nextPage in pages)
            {
                Append(session, nextPage);
            }
        }
    }
}