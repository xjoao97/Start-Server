#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class SuperFastwalkCommand : IChatCommand
    {
        public string PermissionRequired => "command_super_fastwalk";

        public string Parameters => "";

        public string Description => "Gives you the ability to walk very very fast.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            user.SuperFastWalking = !user.SuperFastWalking;

            if (user.FastWalking)
                user.FastWalking = false;

            session.SendWhisper("Walking mode updated.");
        }
    }
}