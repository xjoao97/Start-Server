namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorPreferencesComposer : ServerPacket
    {
        public NavigatorPreferencesComposer()
            : base(ServerPacketHeader.NavigatorPreferencesMessageComposer)
        {
            WriteInteger(80); //X
            WriteInteger(42); //Y
            WriteInteger(425); //Width
            WriteInteger(510); //Height
            WriteBoolean(false); //Show or hide saved searches.
            WriteInteger(0); //No idea?
        }
    }
}