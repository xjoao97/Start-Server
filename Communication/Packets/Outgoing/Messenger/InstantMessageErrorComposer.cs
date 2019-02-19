#region

using Oblivion.HabboHotel.Users.Messenger;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Messenger
{
    internal class InstantMessageErrorComposer : ServerPacket
    {
        public InstantMessageErrorComposer(MessengerMessageErrors Error, int Target)
            : base(ServerPacketHeader.InstantMessageErrorMessageComposer)
        {
            WriteInteger(MessengerMessageErrorsUtility.GetMessageErrorPacketNum(Error));
            WriteInteger(Target);
            WriteString("");
        }
    }
}