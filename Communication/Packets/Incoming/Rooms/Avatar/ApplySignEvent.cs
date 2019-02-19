#region

using System;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Avatar
{
    internal class ApplySignEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var SignId = Packet.PopInt();
            Room Room;

            if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;


            User.UnIdle();

            User.AddStatus("sign", Convert.ToString(SignId));
            User.UpdateNeeded = true;
            User.SignTime = OblivionServer.GetUnixTimestamp() + 5;
        }
    }
}