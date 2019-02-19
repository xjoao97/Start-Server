#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class EnqueteCommand : IChatCommand
    {
        public string PermissionRequired => "command_hotel_alert";

        public string Parameters => "%message%";

        public string Description => "Uma nova enquete.";

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
                .SendMessage(new RoomNotificationComposer("Há uma nova enquete!",
                   message + "\r\n" + "- " + session.GetHabbo().Username, "facebookenquete", url, url));
        }
    }
}