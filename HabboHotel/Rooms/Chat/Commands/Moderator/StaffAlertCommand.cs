#region

using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class StaffAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_staff_alert";

        public string Parameters => "%message%";

        public string Description => "Envie uma mensagem que aparecerá para todos os staffs.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira uma mensagem para todos os membros da staff que estão onlines.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 1);
            OblivionServer.GetGame()
                .GetClientManager()
                .StaffAlert(
                    new MotdNotificationComposer("Mensagem Staff:\r\r" + message + "\r\n" + "- " +
                                                 session.GetHabbo().Username));
        }
    }
}