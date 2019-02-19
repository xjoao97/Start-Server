#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class UseWallItemEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var itemID = Packet.PopInt();
            var Item = Room.GetRoomItemHandler().GetItem(itemID);
            if (Item == null)
                return;

            var hasRights = Room.CheckRights(Session, false, true);

            var oldData = Item.ExtraData;
            var request = Packet.PopInt();

            Item.Interactor.OnTrigger(Session, Item, request, hasRights);
            Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);

            OblivionServer.GetGame()
                .GetQuestManager()
                .ProgressUserQuest(Session, QuestType.EXPLORE_FIND_ITEM, Item.GetBaseItem().Id);
        }
    }
}