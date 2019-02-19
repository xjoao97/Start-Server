#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Quests;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class MoveObjectEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var ItemId = Packet.PopInt();
            if (ItemId == 0)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            Item Item;
            if (Room.Group != null)
            {
                if (!Room.CheckRights(Session, false, true))
                {
                    Item = Room.GetRoomItemHandler().GetItem(ItemId);
                    if (Item == null)
                        return;

                    Session.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                    return;
                }
            }
            else
            {
                if (!Room.CheckRights(Session))
                    return;
            }

            Item = Room.GetRoomItemHandler().GetItem(ItemId);

            if (Item == null)
                return;

            var x = Packet.PopInt();
            var y = Packet.PopInt();
            var Rotation = Packet.PopInt();

            if (x != Item.GetX || y != Item.GetY)
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_MOVE);

            if (Rotation != Item.Rotation)
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_ROTATE);

            if (!Room.GetRoomItemHandler().SetFloorItem(Session, Item, x, y, Rotation, false, false, true))
            {
                Room.SendMessage(new ObjectUpdateComposer(Item, Room.OwnerId));
                return;
            }

            if (Item.GetZ >= 0.1)
                OblivionServer.GetGame().GetQuestManager().ProgressUserQuest(Session, QuestType.FURNI_STACK);
        }
    }
}