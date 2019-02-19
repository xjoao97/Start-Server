namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class NewConsoleMessageComposer : ServerPacket
    {
        public NewConsoleMessageComposer(int Sender, string Message, int Time = 0)
            : base(ServerPacketHeader.NewConsoleMessageMessageComposer)
        {
            WriteInteger(Sender);
            WriteString(Message);
            WriteInteger(Time);
        }

        public NewConsoleMessageComposer(int GroupId, string Message, int Time, int UserId, string Username,
            string figure)
            : base(ServerPacketHeader.NewConsoleMessageMessageComposer)
        {
            WriteInteger(-GroupId);
            WriteString(Message);
            WriteInteger(Time);
            WriteString(Username + "/" + figure + "/" + UserId);
        }
    }
}