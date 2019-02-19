#region

using Oblivion.Communication.Packets.Outgoing.Groups;
using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    public class InfoRetrieveEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Session.SendMessage(new UserObjectComposer(Session.GetHabbo()));
            Session.SendMessage(new UserPerksComposer());
            Session.SendMessage(
                new UnreadForumThreadPostsComposer(
                    OblivionServer.GetGame().GetGroupForumManager().GetUnreadThreadForumsByUserId(Session.GetHabbo().Id)));
        }
    }
}