#region

using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Rooms.Session;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class FindNewFriendsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var Instance = OblivionServer.GetGame().GetRoomManager().TryGetRandomLoadedRoom();

            if (Instance != null)
            {
                Session.SendMessage(new FindFriendsProcessResultComposer(true));
                Session.SendMessage(new RoomForwardComposer(Instance.Id));
            }
            else
            {
                Session.SendMessage(new FindFriendsProcessResultComposer(false));
            }
        }
    }
}