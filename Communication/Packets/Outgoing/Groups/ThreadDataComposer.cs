﻿#region

using Oblivion.HabboHotel.Groups.Forums;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class ThreadDataComposer : ServerPacket
    {
        public ThreadDataComposer(GroupForumThread Thread, int StartIndex, int MaxLength)
            : base(ServerPacketHeader.ThreadDataMessageComposer)
        {
            WriteInteger(Thread.ParentForum.Id);
            WriteInteger(Thread.Id);
            WriteInteger(StartIndex);
            WriteInteger(Thread.Posts.Count); //Messages count

            foreach (var Post in Thread.Posts)
                Post.SerializeData(this);
        }
    }
}