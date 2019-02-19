#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class PullCommand : IChatCommand
    {
        public string PermissionRequired => "command_pull";

        public string Parameters => "%target%";

        public string Description => "Puxe um usuário para perto de você.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário que você deseja puxar.");
                return;
            }


            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online ou neste quarto.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper(
                    "O usuário não está online ou neste quarto.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Ei, você não vai gostar de puxar a si mesmo.");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("Você não pode puxar este usuário.");
                return;
            }

            if (targetUser.TeleportEnabled)
            {
                session.SendWhisper("O usuário está com o modo teleporte ativo, você não pode puxá-lo.");
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (thisUser.SetX - 1 == room.GetGameMap().Model.DoorX)
            {
                session.SendWhisper("Ei, não empurre-o para fora do quarto. :(!");
                return;
            }


            var pushDirection = "down";
            if (targetClient.GetHabbo().CurrentRoomId == session.GetHabbo().CurrentRoomId &&
                Math.Abs(thisUser.X - targetUser.X) < 3 && Math.Abs(thisUser.Y - targetUser.Y) < 3)
            {
                room.SendMessage(new ChatComposer(thisUser.VirtualId, "*pulls " + Params[1] + " to them*", 0,
                    thisUser.LastBubble));

                if (thisUser.RotBody == 0)
                    pushDirection = "up";
                if (thisUser.RotBody == 2)
                    pushDirection = "right";
                if (thisUser.RotBody == 4)
                    pushDirection = "down";
                if (thisUser.RotBody == 6)
                    pushDirection = "left";

                if (pushDirection == "up")
                    targetUser.MoveTo(thisUser.X, thisUser.Y - 1);

                if (pushDirection == "right")
                    targetUser.MoveTo(thisUser.X + 1, thisUser.Y);

                if (pushDirection == "down")
                    targetUser.MoveTo(thisUser.X, thisUser.Y + 1);

                if (pushDirection == "left")
                    targetUser.MoveTo(thisUser.X - 1, thisUser.Y);
            }
            else
            {
                session.SendWhisper("O usuário não está perto o suficiente para ser puxado.!");
            }
        }
    }
}