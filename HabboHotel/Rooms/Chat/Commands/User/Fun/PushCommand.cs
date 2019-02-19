#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class PushCommand : IChatCommand
    {
        public string PermissionRequired => "command_push";

        public string Parameters => "%target%";

        public string Description => "Empurre um usuário.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o nome do usuário que você deseja empurrar.");
                return;
            }


            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online, ou não está no quarto.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper(
                    "O usuário não está online, ou não está no quarto.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Você não vai gostar de se empurrar.");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("Este usuário não pode ser empurrado.");
                return;
            }

            if (targetUser.TeleportEnabled)
            {
                session.SendWhisper("O usuário está usando o modo teleporte, você não pode empurrá-lo.");
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (!(Math.Abs(targetUser.X - thisUser.X) >= 2 || Math.Abs(targetUser.Y - thisUser.Y) >= 2))
            {
                if (targetUser.SetX - 1 == room.GetGameMap().Model.DoorX)
                {
                    session.SendWhisper("Não empurre-o para fora do quarto. :(!");
                    return;
                }

                switch (thisUser.RotBody)
                {
                    case 0:
                        targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                        break;
                    case 4:
                        targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                        break;
                    case 6:
                        targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                        break;
                    case 2:
                        targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                        break;
                    case 3:
                        targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                        targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                        break;
                    case 1:
                        targetUser.MoveTo(targetUser.X + 1, targetUser.Y);
                        targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                        break;
                    case 7:
                        targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                        targetUser.MoveTo(targetUser.X, targetUser.Y - 1);
                        break;
                    case 5:
                        targetUser.MoveTo(targetUser.X - 1, targetUser.Y);
                        targetUser.MoveTo(targetUser.X, targetUser.Y + 1);
                        break;
                }

                room.SendMessage(new ChatComposer(thisUser.VirtualId, "*empurrei " + Params[1] + "*", 0,
                    thisUser.LastBubble));
            }
            else
            {
                session.SendWhisper("Hey, " + Params[1] + " não está perto o suficiente!");
            }
        }
    }
}