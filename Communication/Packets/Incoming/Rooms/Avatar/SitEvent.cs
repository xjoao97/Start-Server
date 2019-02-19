#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Avatar
{
    internal class SitEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || !Session.GetHabbo().InRoom)
                return;

            var user = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (user == null)
                return;

            if (user.Statusses.ContainsKey("lie") || user.isLying || user.RidingHorse || user.IsWalking)
                return;

            if (!user.Statusses.ContainsKey("sit"))
            {
                if (user.RotBody % 2 == 0)
                {
                    try
                    {
                        user.Statusses.Add("sit", "1.0");
                        user.Z -= 0.35;
                        user.isSitting = true;
                        user.UpdateNeeded = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.isSitting = true;
                    user.UpdateNeeded = true;
                }
            }
            else if (user.isSitting)
            {
                user.Z += 0.35;
                user.Statusses.Remove("sit");
                user.Statusses.Remove("1.0");
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
        }
    }
}