#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class UseFurnitureEvent : IPacketEvent
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

            if (Item.GetBaseItem().InteractionType == InteractionType.Banzaitele)
                return;

            if (Item.GetBaseItem().InteractionType == InteractionType.Toner)
            {
                if (!Room.CheckRights(Session, true))
                    return;
                Room.TonerData.Enabled = Room.TonerData.Enabled == 0 ? 1 : 0;

                Room.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));

                Item.UpdateState();

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.runFastQuery("UPDATE `room_items_toner` SET `enabled` = '" + Room.TonerData.Enabled +
                                      "' LIMIT 1");
                }
                return;
            }

            if (Item.Data.InteractionType == InteractionType.GnomeBox && Item.UserID == Session.GetHabbo().Id)
                Session.SendMessage(new GnomeBoxComposer(Item.Id));

            var Toggle = true;
            if (Item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch1 ||
                Item.GetBaseItem().InteractionType == InteractionType.WfFloorSwitch2)
            {
                var User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
                if (User == null)
                    return;

                if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.X, User.Y))
                    Toggle = false;
            }

            var request = Packet.PopInt();

            Item.Interactor.OnTrigger(Session, Item, request, hasRights);

            if (Toggle)
                Item.GetRoom().GetWired().TriggerEvent(WiredBoxType.TriggerStateChanges, Session.GetHabbo(), Item);

            OblivionServer.GetGame()
                .GetQuestManager()
                .ProgressUserQuest(Session, QuestType.EXPLORE_FIND_ITEM, Item.GetBaseItem().Id);
        }
    }
}