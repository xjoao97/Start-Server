#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class EventRadio : IChatCommand
    {
        public string PermissionRequired => "command_radiopoins";

        public string Parameters => "%message%";

        public string Description => "Envie um alerta de evento da rádio.";

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
                    "alertradio",
                    session.GetHabbo().Username,
                    "Clique aqui para acessar o evento!" + "event:navigator/goto/" + session.GetHabbo().CurrentRoomId,
                    true));
        }
    }
}