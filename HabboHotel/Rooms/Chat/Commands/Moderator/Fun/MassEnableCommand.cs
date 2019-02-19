#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class MassEnableCommand : IChatCommand
    {
        public string PermissionRequired => "command_massenable";

        public string Parameters => "%EffectId%";

        public string Description => "Give every user in the room a specific enable ID.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter an effect ID.");
                return;
            }

            int enableId;
            if (int.TryParse(Params[1], out enableId))
            {
                if (enableId == 102 || enableId == 178)
                {
                    session.Disconnect();
                    return;
                }
                var room = session.GetHabbo().CurrentRoom;

                if (!session.GetHabbo().GetPermissions().HasCommand("command_override_massenable") &&
                    room.OwnerId != session.GetHabbo().Id)
                {
                    session.SendWhisper("You can only use this command in your own room.");
                    return;
                }

                var users = room.GetRoomUserManager().GetRoomUsers();
                if (users.Count > 0)
                    foreach (var u in users.ToList().Where(u => u != null && !u.RidingHorse))
                        u.ApplyEffect(enableId);
            }
            else
            {
                session.SendWhisper("Please enter an effect ID.");
            }
        }
    }
}