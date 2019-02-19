/*#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class PubliAlertCommand : IChatCommand
    {
        public string PermissionRequired => "command_event_publi";

        public string Parameters => "";

        public string Description => "Oleada de publicidad";

        public void Execute(GameClient session, string[] Params)
        {
            if (Session != null)
                if (Room != null)
                {
                    var Message = "<img src='http://i.imgur.com/zDhgzeV.png'></img>\n\n" +
                                  "Hay una oleada de publicidad en marcha, recuerda que si nos ayudas, recibirás recompensas, <font color='#58D3F7'>como diamantes, rares, o incluso pertenecer al equipo staff</font>.";
                    if (Params.Length > 2)
                        Message = CommandManager.MergeParams(Params, 1);

                    OblivionServer.GetGame()
                        .GetClientManager()
                        .SendMessage(new RoomNotificationComposer("¡Oleada de publicidad!",
                            Message + "\r\n  - <b>" + Session.GetHabbo().Username + "</b>\r\n<i></i>",
                            "figure/" + Session.GetHabbo().Username + "", "Ir a la oleada\"",
                            "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                }
        }
    }
}*/