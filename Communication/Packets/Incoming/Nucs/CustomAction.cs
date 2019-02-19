#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Nucs
{
    internal class CustomAction : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var userId = Packet.PopInt();
            if (Session.GetHabbo().LastCustomCommand + 300 >= OblivionServer.GetUnixTimestamp())
            {
                Session.SendWhisper(Language.GetValue("command.wait"));
                return;
            }
            var blockedcommands = Session.GetHabbo().CurrentRoom.RoomData.BlockedCommands;

            if (blockedcommands.Contains("fumar") && Session.GetHabbo().Rank < 5)
            {
                Session.SendWhisper(Language.GetValue("command.blocked"));
                return;
            }
            var TargetClient = OblivionServer.GetGame().GetClientManager().GetClientByUserID(userId);

            if (TargetClient == null)
            {
                Session.SendWhisper("Opa, provavelmente o usuário não está online.");
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            var TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(TargetClient.GetHabbo().Id);

            if (TargetUser == null)
            {
                Session.SendWhisper("Opa, provavelmente ele não está na sala.");
                return;
            }

            if (TargetClient.GetHabbo().Username == Session.GetHabbo().Username)
            {
                Session.SendWhisper("Não se beije :(");
                return;
            }


            if (TargetUser.isLying || TargetUser.isSitting)
            {
                Session.SendWhisper("Não pode beijá-lo assim...");
                return;
            }

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (ThisUser == null)
                return;

            var name = TargetClient.GetHabbo().Username;
            if (!(Math.Abs(TargetUser.X - ThisUser.X) > 1 || Math.Abs(TargetUser.Y - ThisUser.Y) > 1))
            {
                TargetUser.ApplyEffect(168);
                ThisUser.ApplyEffect(168);
                Room.SendMessage(new ChatComposer(ThisUser.VirtualId, " *Beijando " + name + "*", 0, 16));
                Room.SendMessage(new ChatComposer(TargetUser.VirtualId, "*Amei |*", 0, 16));
                Session.GetHabbo().LastCustomCommand = OblivionServer.GetUnixTimestamp();
            }
            else
            {
                Session.SendWhisper("Ops! " + name + " está muito longe.");
            }
        }
    }
}