namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorSettingsComposer : ServerPacket
    {
        public NavigatorSettingsComposer(int Homeroom)
            : base(ServerPacketHeader.NavigatorSettingsMessageComposer)
        {
            WriteInteger(Homeroom);
            WriteInteger(Homeroom);
        }
    }
}