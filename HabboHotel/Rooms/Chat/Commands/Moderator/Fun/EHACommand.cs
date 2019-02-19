/*#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class EHACommand : IChatCommand
    {
        public string PermissionRequired => "command_event_alert";

        public string Parameters => "%message%";

        public string Description => "Send a hotel alert for your event!";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Session != null)
                if (Room != null)
                {
                    var Message = "" + "Hey there's a event going on right now you might wanna head down there!";
                    if (Params.Length > 2)
                        Message = CommandManager.MergeParams(Params, 1);

                    OblivionServer.GetGame()
                        .GetClientManager()
                        .SendMessage(new RoomNotificationComposer("Evento en marcha",
                            Message + "\r\n- <b>" + Session.GetHabbo().Username + "</b>\r\n<i></i>",
                            "figure/" + Session.GetHabbo().Username + "",
                            "Go to \"" + Session.GetHabbo().CurrentRoom.Name + "\"!",
                            "event:navigator/goto/" + Session.GetHabbo().CurrentRoomId));
                }
        }
    }
}*/