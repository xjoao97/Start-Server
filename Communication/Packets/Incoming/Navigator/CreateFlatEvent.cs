#region

using Oblivion.Communication.Packets.Outgoing.Navigator;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Navigator;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Navigator
{
    internal class CreateFlatEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            if (Session.GetHabbo().UsersRooms.Count >= 50)
            {
                Session.SendMessage(new CanCreateRoomComposer(true, 500));
                return;
            }

            var Name = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var Description = OblivionServer.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            var ModelName = Packet.PopString();

            var Category = Packet.PopInt();
            var MaxVisitors = Packet.PopInt(); //10 = min, 25 = max.
            var TradeSettings = Packet.PopInt(); //2 = All can trade, 1 = owner only, 0 = no trading.

            if (Name.Length < 3 || Name.Length > 25)
                return;

            RoomModel RoomModel;
            if (!OblivionServer.GetGame().GetRoomManager().TryGetModel(ModelName, out RoomModel))
                return;

            SearchResultList SearchResultList;
            if (!OblivionServer.GetGame().GetNavigator().TryGetSearchResultList(Category, out SearchResultList))
                Category = 36;

            if (SearchResultList.CategoryType != NavigatorCategoryType.CATEGORY ||
                SearchResultList.RequiredRank > Session.GetHabbo().Rank)
                Category = 36;

            if (MaxVisitors < 10 || MaxVisitors > 25)
                MaxVisitors = 10;

            if (TradeSettings < 0 || TradeSettings > 2)
                TradeSettings = 0;

            var NewRoom = OblivionServer.GetGame()
                .GetRoomManager()
                .CreateRoom(Session, Name, Description, ModelName, Category, MaxVisitors, TradeSettings);
            if (NewRoom != null)
            {
                Session.SendMessage(new FlatCreatedComposer(NewRoom.Id, Name));
                Session.SendMessage(new RoomForwardComposer(NewRoom.Id));
            }
        }
    }
}