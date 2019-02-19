#region

using Oblivion.Communication.Packets.Outgoing.Inventory.Trading;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Inventory.Trading
{
    internal class InitTradeEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;
            if (!Room.CanTradeInRoom)
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Session.GetHabbo().TradingLockExpiry > 0)
                if (Session.GetHabbo().TradingLockExpiry > OblivionServer.GetUnixTimestamp())
                {
                    Session.SendNotification("You're currently banned from trading.");
                    return;
                }
                else
                {
                    Session.GetHabbo().TradingLockExpiry = 0;
                    Session.SendNotification("Your trading ban has now expired, please do not scam again.");

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.RunQuery("UPDATE `user_info` SET `trading_locked` = '0' WHERE `id` = '" +
                                          Session.GetHabbo().Id + "' LIMIT 1");
                    }
                }

            var TargetUser = Room.GetRoomUserManager().GetRoomUserByVirtualId(Packet.PopInt());

            if (TargetUser?.GetClient() == null || TargetUser.GetClient().GetHabbo() == null)
                return;

            if (TargetUser.IsTrading)
            {
                Session.SendMessage(new TradingErrorComposer(8, TargetUser.GetUsername()));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("room_trade_override"))
                if (Room.TradeSettings == 1 && Room.OwnerId != Session.GetHabbo().Id) //Owner only.
                {
                    Session.SendMessage(new TradingErrorComposer(6, TargetUser.GetUsername()));
                    return;
                }
                else if (Room.TradeSettings == 0 && Room.OwnerId != Session.GetHabbo().Id) //Trading is disabled.
                {
                    Session.SendMessage(new TradingErrorComposer(6, TargetUser.GetUsername()));
                    return;
                }

            if (TargetUser.GetClient().GetHabbo().TradingLockExpiry > 0)
            {
                Session.SendNotification("Oops, it appears this user is currently banned from trading!");
                return;
            }

            Room.TryStartTrade(User, TargetUser);
        }
    }
}