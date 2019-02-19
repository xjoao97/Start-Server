namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class MessengerInitComposer : ServerPacket
    {
        public MessengerInitComposer()
            : base(ServerPacketHeader.MessengerInitMessageComposer)
        {
            WriteInteger(OblivionStaticGameSettings.MessengerFriendLimit); //Friends max.
            WriteInteger(300);
            WriteInteger(800);
            WriteInteger(0); // category count
//            WriteInteger(1); // category id
//            WriteString("Grupos"); // category id
        }
    }
}