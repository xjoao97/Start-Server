#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class StandCommand : IChatCommand
    {
        public string PermissionRequired => "command_stand";

        public string Parameters => "";


        public string Description => "Hm, e que tal levantar-se?.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Username);
            if (user == null)
                return;

            if (user.isSitting)
            {
                user.Statusses.Remove("sit");
                user.Z += 0.35;
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
            else if (user.isLying)
            {
                user.Statusses.Remove("lay");
                user.Z += 0.35;
                user.isLying = false;
                user.UpdateNeeded = true;
            }
        }
    }
}