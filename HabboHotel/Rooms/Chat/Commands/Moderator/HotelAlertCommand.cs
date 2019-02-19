#region

using Oblivion.Communication.Packets.Outgoing.Moderation;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class HotelAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_hotel_alert";

        public string Parameters => "%message%";

        public string Description => "Envie um alerta para todo o Hotel";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira uma mensagem.");
                return;
            }

            var message = CommandManager.MergeParams(Params, 1);
            OblivionServer.GetGame()
                .GetClientManager()
                .SendMessage(new BroadcastMessageAlertComposer(message + "\r\n" + "- " + session.GetHabbo().Username));
        }
    }
}