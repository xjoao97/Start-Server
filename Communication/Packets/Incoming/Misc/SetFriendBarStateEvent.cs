#region

using Oblivion.Communication.Packets.Outgoing.Sound;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger.FriendBar;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Misc
{
    internal class SetFriendBarStateEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            Session.GetHabbo().FriendbarState = FriendBarStateUtility.GetEnum(Packet.PopInt());
            Session.SendMessage(new SoundSettingsComposer(Session.GetHabbo().ClientVolume,
                Session.GetHabbo().ChatPreference, Session.GetHabbo().AllowMessengerInvites,
                Session.GetHabbo().FocusPreference, FriendBarStateUtility.GetInt(Session.GetHabbo().FriendbarState)));
        }
    }
}