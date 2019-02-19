#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups.Forums;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class PostUpdatedComposer : ServerPacket
    {
        public PostUpdatedComposer(GameClient Session, GroupForumThreadPost Post)
            : base(ServerPacketHeader.PostUpdatedMessageComposer)
        {
            WriteInteger(Post.ParentThread.ParentForum.Id);
            WriteInteger(Post.ParentThread.Id);

            Post.SerializeData(this);
        }
    }
}