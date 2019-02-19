using Oblivion.Communication.Packets.Outgoing;

namespace Oblivion.Communication.Packets.Incoming.Catalog
{
    internal class FurniMaticRewardsEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            var response = new ServerPacket(ServerPacketHeader.FurniMaticRewardsComposer);
            response.WriteInteger(5);

            for (var i = 5; i >= 1; i--)
            {
                response.WriteInteger(i);
                response.WriteInteger(i);

                var rewards = OblivionServer.GetGame().GetFurniMaticRewardsManager().GetRewardsByLevel(i);

                response.WriteInteger(rewards.Count);
                foreach (var reward in rewards)
                {
                    response.WriteString(reward.GetBaseItem().ItemName);
                    response.WriteInteger(1);
                    response.WriteString(reward.GetBaseItem().Type.ToString().ToLower());
                    response.WriteInteger(reward.GetBaseItem().SpriteId);
                }
            }

            Session.SendMessage(response);
        }
    }
}

