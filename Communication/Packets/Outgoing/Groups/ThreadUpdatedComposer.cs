#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups.Forums;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class ThreadUpdatedComposer : ServerPacket
    {
        public ThreadUpdatedComposer(GameClient Session, GroupForumThread Thread)
            : base(ServerPacketHeader.ThreadUpdatedMessageComposer)
        {
            WriteInteger(Thread.ParentForum.Id);

            Thread.SerializeData(Session, this);
        }
    }
}