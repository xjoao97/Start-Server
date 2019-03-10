#region

using System.Data;
using Oblivion.Communication.Packets.Outgoing.Catalog;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.Catalog.Vouchers;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    public class RedeemVoucherEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var VoucherCode = Packet.PopString().Replace("\r", "");

            if (!OblivionServer.GetGame().GetCatalog().GetVoucherManager().TryGetVoucher(VoucherCode, out Voucher Voucher))
            {
                Session.SendMessage(new VoucherRedeemErrorComposer(0));
                return;
            }

            if (Voucher.CurrentUses >= Voucher.MaxUses)
            {
                Session.SendNotification("Oops, this voucher has reached the maximum usage limit!");
                return;
            }

            DataRow GetRow = null;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `user_vouchers` WHERE `user_id` = '" + Session.GetHabbo().Id +
                                  "' AND `voucher` = @Voucher LIMIT 1");
                dbClient.AddParameter("Voucher", VoucherCode);
                GetRow = dbClient.getRow();
            }

            if (GetRow != null)
            {
                Session.SendNotification("You've already used this voucher code, one per each user, sorry!");
                return;
            }
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO `user_vouchers` (`user_id`,`voucher`) VALUES ('" + Session.GetHabbo().Id +
                                  "', @Voucher)");
                dbClient.AddParameter("Voucher", VoucherCode);
                dbClient.RunQuery();
            }

            Voucher.UpdateUses();

            if (Voucher.Type == VoucherType.CREDIT)
            {
                Session.GetHabbo().Credits += Voucher.Value;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }
            else if (Voucher.Type == VoucherType.DUCKET)
            {
                Session.GetHabbo().Duckets += Voucher.Value;
                Session.SendMessage(new HabboActivityPointNotificationComposer(Session.GetHabbo().Duckets, Voucher.Value));
            }

            Session.SendMessage(new VoucherRedeemOkComposer());
        }
    }
}