#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Avatar
{
    internal class SaveWardrobeOutfitEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var SlotId = Packet.PopInt();
            var Look = OblivionServer.GetGame().GetAntiMutant().RunLook(Packet.PopString());
            var Gender = Packet.PopString();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT null FROM `user_wardrobe` WHERE `user_id` = @id AND `slot_id` = @slot");
                dbClient.AddParameter("id", Session.GetHabbo().Id);
                dbClient.AddParameter("slot", SlotId);

                using (var reader = dbClient.ExecuteReader())
                    if (reader.HasRows)
                    {
                        dbClient.SetQuery("UPDATE `user_wardrobe` SET `look` = @look, `gender` = @gender WHERE `user_id` = @id AND `slot_id` = @slot LIMIT 1");
                        dbClient.AddParameter("id", Session.GetHabbo().Id);
                        dbClient.AddParameter("slot", SlotId);
                        dbClient.AddParameter("look", Look);
                        dbClient.AddParameter("gender", Gender.ToUpper());
                        dbClient.RunQuery();
                    }
                    else
                    {
                        dbClient.SetQuery("INSERT INTO `user_wardrobe` (`user_id`,`slot_id`,`look`,`gender`) VALUES (@id,@slot,@look,@gender)");
                        dbClient.AddParameter("id", Session.GetHabbo().Id);
                        dbClient.AddParameter("slot", SlotId);
                        dbClient.AddParameter("look", Look);
                        dbClient.AddParameter("gender", Gender.ToUpper());
                        dbClient.RunQuery();
                    }
            }
        }
    }
}
