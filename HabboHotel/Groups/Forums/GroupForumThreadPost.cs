﻿#region

using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.Users;

#endregion

namespace Oblivion.HabboHotel.Groups.Forums
{
    public class GroupForumThreadPost
    {
        public int DeletedLevel;

        public int DeleterId;
        public int Id;
        public string Message;

        public GroupForumThread ParentThread;
        public int Timestamp;
        public int UserId;

        public GroupForumThreadPost(GroupForumThread parent, int id, int userid, int timestamp, string message,
            int deletedlevel, int deleterid)
        {
            ParentThread = parent;
            Id = id;
            UserId = userid;
            Timestamp = timestamp;
            Message = message;

            DeleterId = deleterid;
            DeletedLevel = deletedlevel;
        }

        public Habbo GetDeleter() => OblivionServer.GetHabboById(DeleterId);

        public Habbo GetAuthor() => OblivionServer.GetHabboById(UserId);

        public void SerializeData(ServerPacket Packet)
        {
            var User = GetAuthor();
            var oculterData = GetDeleter();
            Packet.WriteInteger(Id); //Post Id
            Packet.WriteInteger(ParentThread.Posts.IndexOf(this)); //Post Index

            Packet.WriteInteger(User.Id); //User id
            Packet.WriteString(User.Username); //Username
            Packet.WriteString(User.Look); //User look

            Packet.WriteInteger((int) (OblivionServer.GetUnixTimestamp() - Timestamp)); //User message timestamp
            Packet.WriteString(Message); // Message text
            Packet.WriteByte(DeletedLevel * 10); // User message oculted by - level
            Packet.WriteInteger(oculterData?.Id ?? 0); // User that oculted message ID
            Packet.WriteString(oculterData != null ? oculterData.Username : "Unknown"); //Oculted message user name
            Packet.WriteInteger(242342340);
            Packet.WriteInteger(ParentThread.GetUserPosts(User.Id).Count); //User messages count
        }

        internal void Save()
        {
            using (var adap = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                adap.SetQuery(
                    "UPDATE group_forums_threads SET deleted_level = @dl, deleter_user_id = @duid WHERE id = @id");
                adap.AddParameter("dl", DeletedLevel);
                adap.AddParameter("duid", DeleterId);
                adap.AddParameter("id", Id);
                adap.RunQuery();
            }
        }
    }
}