#region

using Oblivion.Communication.Packets.Outgoing.Handshake;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Handshake
{
    public class GenerateSecretKeyEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            Packet.PopString();

//            var SharedKey = HabboEncryptionV2.CalculateDiffieHellmanSharedKey(CipherPublickey);
//            if (SharedKey != 0)
//            {
//                Session.RC4Client = new ARC4(SharedKey.getBytes());
            Session.SendMessage(new SecretKeyComposer());
//            }
//            else
//            {
//                Session.SendNotification("There was an error logging you in, please try again!");
//            }
        }
    }
}