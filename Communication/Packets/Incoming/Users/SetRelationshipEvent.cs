#region

using System;
using Oblivion.Communication.Packets.Outgoing.Messenger;
using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Messenger;
using Oblivion.HabboHotel.Users.Relationships;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class SetRelationshipEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var User = Packet.PopInt();
            var Type = Packet.PopInt();

            if (!Session.GetHabbo().GetMessenger().FriendshipExists(User))
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer("Oops, you can only set a relationship where a friendship exists."));
                return;
            }

            if (Type < 0 || Type > 3)
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer("Oops, you've chosen an invalid relationship type."));
                return;
            }

            if (Session.GetHabbo().Relationships.Count > 2000)
            {
                Session.SendMessage(
                    new BroadcastMessageAlertComposer("Sorry, you're limited to a total of 2000 relationships."));
                return;
            }

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                if (Type == 0)
                {
                    dbClient.SetQuery("SELECT `id` FROM `user_relationships` WHERE `user_id` = '" +
                                      Session.GetHabbo().Id + "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", User);
                    var Id = dbClient.getInteger();

                    dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = '" + Session.GetHabbo().Id +
                                      "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", User);
                    dbClient.RunQuery();

                    if (Session.GetHabbo().Relationships.ContainsKey(User))
                        Session.GetHabbo().Relationships.Remove(User);
                }
                else
                {
                    dbClient.SetQuery("SELECT id FROM `user_relationships` WHERE `user_id` = '" + Session.GetHabbo().Id +
                                      "' AND `target` = @target LIMIT 1");
                    dbClient.AddParameter("target", User);
                    var Id = dbClient.getInteger();

                    if (Id > 0)
                    {
                        dbClient.SetQuery("DELETE FROM `user_relationships` WHERE `user_id` = '" + Session.GetHabbo().Id +
                                          "' AND `target` = @target LIMIT 1");
                        dbClient.AddParameter("target", User);
                        dbClient.RunQuery();

                        if (Session.GetHabbo().Relationships.ContainsKey(Id))
                            Session.GetHabbo().Relationships.Remove(Id);
                    }

                    dbClient.SetQuery("INSERT INTO `user_relationships` (`user_id`,`target`,`type`) VALUES ('" +
                                      Session.GetHabbo().Id + "', @target, @type)");
                    dbClient.AddParameter("target", User);
                    dbClient.AddParameter("type", Type);
                    var newId = Convert.ToInt32(dbClient.InsertQuery());

                    if (!Session.GetHabbo().Relationships.ContainsKey(User))
                        Session.GetHabbo().Relationships.Add(User, new Relationship(newId, User, Type));
                }

                var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(User);
                if (Client != null)
                {
                    Session.GetHabbo().GetMessenger().UpdateFriend(User, Client, true);
                }
                else
                {
                    var Habbo = OblivionServer.GetHabboById(User);
                    if (Habbo == null) return;
                    MessengerBuddy Buddy;
                    if (Session.GetHabbo().GetMessenger().TryGetFriend(User, out Buddy))
                        Session.SendMessage(new FriendListUpdateComposer(Session, Buddy));
                }
            }
        }
    }
}