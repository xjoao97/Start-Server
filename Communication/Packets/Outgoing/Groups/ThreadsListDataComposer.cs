#region

using System;
using System.Collections.Generic;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Groups.Forums;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class ThreadsListDataComposer : ServerPacket
    {
        public ThreadsListDataComposer(GroupForum Forum, GameClient Session, int StartIndex = 0, int MaxLength = 20)
            : base(ServerPacketHeader.ThreadsListDataMessageComposer)
        {
            WriteInteger(Forum.GroupId); //Group Forum ID
            WriteInteger(StartIndex); //Page Index

            var Threads = Forum.Threads;
            if (Threads.Count - 1 >= StartIndex)
                Threads = Threads.GetRange(StartIndex, Math.Min(MaxLength, Threads.Count - StartIndex));

            WriteInteger(Threads.Count); //Thread Count

            var UnPinneds = new List<GroupForumThread>();

            foreach (var Thread in Threads)
            {
                if (!Thread.Pinned)
                {
                    UnPinneds.Add(Thread);
                    continue;
                }

                Thread.SerializeData(Session, this);
            }

            foreach (var unPinned in UnPinneds)
                unPinned.SerializeData(Session, this);
        }
    }
}