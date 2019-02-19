#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Furni.Moodlight;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Items.Data.Moodlight;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Furni.Moodlight
{
    internal class GetMoodlightConfigEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            if (!Room.CheckRights(Session, true))
                return;

            if (Room.MoodlightData == null)
                foreach (
                    var item in
                    Room.GetRoomItemHandler()
                        .GetWall.ToList()
                        .Where(item => item.GetBaseItem().InteractionType == InteractionType.Moodlight))
                    Room.MoodlightData = new MoodlightData(item.Id);

            if (Room.MoodlightData == null)
                return;

            Session.SendMessage(new MoodlightConfigComposer(Room.MoodlightData));
        }
    }
}