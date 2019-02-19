#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Groups
{
    internal class UpdateForumReadMarkerEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var length = Packet.PopInt();
            for (var i = 0; i < length; i++)
            {
                var forumid = Packet.PopInt(); //Forum ID
                var postid = Packet.PopInt(); //Post ID
                var readall = Packet.PopBoolean(); //Make all read

                var forum = OblivionServer.GetGame().GetGroupForumManager().GetForum(forumid);

                var post = forum?.GetPost(postid);

                var thread = post?.ParentThread;
                var index = thread?.Posts.IndexOf(post);
                thread?.AddView(Session.GetHabbo().Id, index.Value + 1);
            }
            //Thread.AddView(Session.GetHabbo().Id);
        }
    }
}