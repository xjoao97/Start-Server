namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class NavigatorCollapsedCategoriesComposer : ServerPacket
    {
        public NavigatorCollapsedCategoriesComposer()
            : base(ServerPacketHeader.NavigatorCollapsedCategoriesMessageComposer)
        {
            WriteInteger(0);
        }
    }
}