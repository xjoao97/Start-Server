using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class DjOnline : IChatCommand
    {
        public string PermissionRequired => "command_djalert";

        public string Parameters => "%message%";

        public string Description => "Avisa que o locutor x está online.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Ô animal, digita o nome do locutor.");
                return;
            }

            var Message = CommandManager.MergeParams(Params, 1);
            OblivionServer.GetGame()
                .GetClientManager()
                .SendMessage(new RoomNotificationComposer("",
                    "Locutor " + Message + " está ao vivo na rádio Hiddo, venha curtir eventos e músicas.", "DJAlertNew",
                    "", "", true));
        }
    }
}