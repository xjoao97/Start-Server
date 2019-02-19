#region

using System;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class KillCommand : IChatCommand
    {
        public string PermissionRequired => "command_kill";

        public string Parameters => "%target%";

        public string Description => "Mate um usuário";

        public void Execute(GameClient session, string[] Params) => Command(session, Params[1]);

        public void Command(GameClient session, string TargetUser)
        {
            if (session.GetHabbo().LastCustomCommand + 300 >= OblivionServer.GetUnixTimestamp())
            {
                session.SendWhisper("Espere um pouco!");
                return;
            }

            if (TargetUser.Length == 1)
            {
                session.SendWhisper("Escreva o nome da sua vítima. HAHAHA");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(TargetUser);

            if (targetClient == null)
            {
                session.SendWhisper("Opa, provavelmente o usuário não está online.");
                return;
            }

            var room = session.GetHabbo().CurrentRoom;
            var targetUser = room.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);

            if (targetUser == null)
            {
                session.SendWhisper("Hey, talvez ele não esteja aqui.");
                return;
            }

            if (targetClient.GetHabbo().Username == session.GetHabbo().Username)
            {
                session.SendWhisper("Ora, vamos, você não vai querer se suicidar... :(");
                return;
            }

            if (targetClient.GetHabbo().GetPermissions().HasRight("mod_tool"))
            {
                session.SendWhisper("Você não pode matar este usuário.");
                return;
            }

            if (targetUser.isLying || targetUser.isSitting)
            {
                session.SendWhisper("Não podes matar ele desta forma...");
                return;
            }

            var thisUser = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (thisUser == null)
                return;

            if (!(Math.Abs(targetUser.X - thisUser.X) > 1 || Math.Abs(targetUser.Y - thisUser.Y) > 1))
            {
                thisUser.ApplyEffect(164);
                room.SendMessage(new ChatComposer(thisUser.VirtualId, "*Te matei, " + TargetUser + "*", 32,
                    thisUser.LastBubble));
                room.SendMessage(new ChatComposer(targetUser.VirtualId, "Não pode ser, estou morto :( ", 17,
                    targetUser.LastBubble));
                targetUser.RotBody--; //
                targetUser.Statusses.Add("lay", "1.0 null");
                targetUser.Z -= 0.35;
                targetUser.isLying = true;
                targetUser.UpdateNeeded = true;
                session.GetHabbo().LastCustomCommand = OblivionServer.GetUnixTimestamp();
            }
            else
            {
                session.SendWhisper("Hey, " + TargetUser + " está muito machucado.");
            }
        }
    }
}