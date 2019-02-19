#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Navigator;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Settings
{
    internal class SaveEnforcedCategorySettingsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Room Room = null;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Packet.PopInt(), out Room))
                return;

            if (!Room.CheckRights(Session, true))
                return;

            var CategoryId = Packet.PopInt();
            var TradeSettings = Packet.PopInt();

            if (TradeSettings < 0 || TradeSettings > 2)
                TradeSettings = 0;

            SearchResultList SearchResultList;
            if (!OblivionServer.GetGame().GetNavigator().TryGetSearchResultList(CategoryId, out SearchResultList))
                CategoryId = 36;

            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank)
                CategoryId = 36;
        }
    }
}