#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class NotificationAlert : IChatCommand
    {
        public string PermissionRequired => "command_notification";

        public string Parameters => "%message%";

        public string Description => "Envie um alerta para todo o Hotel";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira a mensagem.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 1);
            OblivionServer.GetGame()
                .GetClientManager().SendMessage(new RoomNotificationComposer("",
                    message,
                    "alerta",
                    session.GetHabbo().Username, "", true));
        }
    }
}