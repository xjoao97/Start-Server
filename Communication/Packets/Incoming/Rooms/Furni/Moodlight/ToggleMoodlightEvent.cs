#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    internal class ToggleMoodlightEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session, true) || Room.MoodlightData == null)
                return;

            var Item = Room.GetRoomItemHandler().GetItem(Room.MoodlightData.ItemId);
            if (Item == null || Item.GetBaseItem().InteractionType != InteractionType.Moodlight)
                return;

            if (Room.MoodlightData.Enabled)
                Room.MoodlightData.Disable();
            else
                Room.MoodlightData.Enable();

            Item.ExtraData = Room.MoodlightData.GenerateExtraData();
            Item.UpdateState();
        }
    }
}