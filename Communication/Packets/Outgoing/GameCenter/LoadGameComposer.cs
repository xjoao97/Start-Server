#region

using Oblivion.HabboHotel.Games;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.GameCenter
{
    internal class LoadGameComposer : ServerPacket
    {
        public LoadGameComposer(GameData GameData, string SSOTicket)
            : base(ServerPacketHeader.LoadGameMessageComposer)
        {
            WriteInteger(GameData.GameId);
            WriteString("1365260055982");
            WriteString(GameData.ResourcePath + GameData.GameSWF);
            WriteString("best");
            WriteString("showAll");
            WriteInteger(60); //FPS?
            WriteInteger(10);
            WriteInteger(8);
            WriteInteger(6); //Asset count
            WriteString("assetUrl");
            WriteString(GameData.ResourcePath + GameData.GameAssets);
            WriteString("habboHost");
            WriteString("http://fuseus-private-httpd-fe-1");
            WriteString("accessToken");
            WriteString(SSOTicket);
            WriteString("gameServerHost");
            WriteString(GameData.GameServerHost);
            WriteString("gameServerPort");
            WriteString(GameData.GameServerPort);
            WriteString("socketPolicyPort");
            WriteString(GameData.GameServerHost);
        }
    }
}