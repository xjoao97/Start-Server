#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class RoomAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_room_alert";

        public string Parameters => "%message%";

        public string Description => "Envie um alerta para todo o quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira a mensagem que você deseja enviar.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            if (!session.GetHabbo().GetPermissions().HasRight("mod_alert") && room.OwnerId != session.GetHabbo().Id)
            {
                session.SendWhisper("Você só pode alertar seu próprio quarto.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 1);
            foreach (
                var roomUser in
                room.GetRoomUserManager()
                    .GetRoomUsers()
                    .Where(roomUser => roomUser?.GetClient() != null && session.GetHabbo().Id != roomUser.UserId))
                roomUser.GetClient()
                    .SendNotification(session.GetHabbo().Username + " alertou o quarto com a seguinte mensagem:\n\n" +
                                      message);
            session.SendWhisper("Mensagem enviada para todo o quarto com sucesso.");
        }
    }
}