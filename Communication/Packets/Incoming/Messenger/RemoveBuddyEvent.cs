#region

using System;
using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Messenger
{
    internal class RemoveBuddyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null || Session.GetHabbo().GetMessenger() == null)
                return;

            var Amount = Packet.PopInt();
            if (Amount > 100)
                Amount = 100;
            else if (Amount < 0)
                return;

            for (var i = 0; i < Amount; i++)
            {
                var Id = Packet.PopInt();

                if (Session.GetHabbo().Relationships.Where(x => x.Value.UserId == Id).ToList().Count > 0)
                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery(
                            "DELETE FROM `user_relationships` WHERE `user_id` = @id AND `target` = @target OR `target` = @id AND `user_id` = @target");
                        dbClient.AddParameter("id", Session.GetHabbo().Id);
                        dbClient.AddParameter("target", Id);
                        dbClient.RunQuery();
                    }


                if (Session.GetHabbo().Relationships.ContainsKey(Convert.ToInt32(Id)))
                    Session.GetHabbo().Relationships.Remove(Convert.ToInt32(Id));

                var Target = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Id);
                if (Target != null)
                    if (Target.GetHabbo().Relationships.ContainsKey(Convert.ToInt32(Session.GetHabbo().Id)))
                        Target.GetHabbo().Relationships.Remove(Convert.ToInt32(Session.GetHabbo().Id));

                Session.GetHabbo().GetMessenger().DestroyFriendship(Id);
            }
        }
    }
}