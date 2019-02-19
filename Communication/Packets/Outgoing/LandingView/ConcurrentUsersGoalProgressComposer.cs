namespace Oblivion.Communication.Packets.Outgoing.LandingView
{
    internal class ConcurrentUsersGoalProgressComposer : ServerPacket
    {
        public ConcurrentUsersGoalProgressComposer(int UsersNow)
            : base(ServerPacketHeader.ConcurrentUsersGoalProgressMessageComposer)
        {
            //todo: code when reach the online users, packet when click: 2295

            WriteInteger(UsersNow >= 500 ? 2 : 0); //0/1 = Not done, 2 = Done & can claim, 3 = claimed.
            //WriteInteger(0);
            WriteInteger(UsersNow);
            WriteInteger(500);
        }
    }
}