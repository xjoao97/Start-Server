#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.AI.Bots;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.AI.Bots
{
    internal class OpenBotActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            var BotId = Packet.PopInt();
            var ActionId = Packet.PopInt();

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            RoomUser BotUser = null;
            if (!Room.GetRoomUserManager().TryGetBot(BotId, out BotUser))
                return;

            var BotSpeech = "";
            foreach (var Speech in BotUser.BotData.RandomSpeech.ToList())
                BotSpeech += Speech.Message + "\n";

            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.AutomaticChat;
            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.SpeakingInterval;
            BotSpeech += ";#;";
            BotSpeech += BotUser.BotData.MixSentences;

            if (ActionId == 2 || ActionId == 5)
                Session.SendMessage(new OpenBotActionComposer(BotUser, ActionId, BotSpeech));
        }
    }
}