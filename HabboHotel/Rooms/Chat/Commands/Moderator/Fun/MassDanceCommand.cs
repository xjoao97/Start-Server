#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Avatar;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class MassDanceCommand : IChatCommand
    {
        public string PermissionRequired => "command_massdance";

        public string Parameters => "%DanceId%";

        public string Description => "Force everyone in the room to dance to a dance of your choice.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }

            var danceId = Convert.ToInt32(Params[1]);
            if (danceId < 0 || danceId > 4)
            {
                session.SendWhisper("Please enter a dance ID. (1-4)");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var users = room.GetRoomUserManager().GetRoomUsers();
            if (users.Count > 0)
                foreach (var u in users.ToList().Where(u => u != null))
                {
                    if (u.CarryItemID > 0)
                        u.CarryItemID = 0;

                    u.DanceId = danceId;
                    room.SendMessage(new DanceComposer(u, danceId));
                }
        }
    }
}