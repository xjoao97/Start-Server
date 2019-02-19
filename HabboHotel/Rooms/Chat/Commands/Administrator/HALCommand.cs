#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class HalCommand : IChatCommand
    {
        public string PermissionRequired => "command_hal";

        public string Parameters => "%message%";

        public string Description => "Envie uma mensagem para todo o Hotel, com um link..";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 2)
            {
                session.SendWhisper("Insira a mensagem e depois o link.");
                return;
            }

            var url = Params[1];

            var message = CommandManager.MergeParams(Params, 2);
            OblivionServer.GetGame()
                .GetClientManager()
                .SendMessage(new RoomNotificationComposer("Alerta!",
                    message + "\r\n" + "- " + session.GetHabbo().Username, "", url, url));
        }
    }
}