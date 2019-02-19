#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class UnFreezeCommand : IChatCommand
    {
        public string PermissionRequired => "command_unfreeze";

        public string Parameters => "%username%";

        public string Description => "Allow another user to walk again.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Please enter the username of the user you wish to un-freeze.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("O usuário não está online.");
                return;
            }

            var targetUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (targetUser != null)
                targetUser.Frozen = false;

            session.SendWhisper("Successfully unfroze " + targetClient.GetHabbo().Username + "!");
        }
    }
}