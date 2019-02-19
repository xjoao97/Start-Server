#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator.Fun
{
    internal class ForceSitCommand : IChatCommand
    {
        public string PermissionRequired => "command_forcesit";

        public string Parameters => "%username%";

        public string Description => "Force um usuário a sentar.";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Opa, não encontramos este usuário.");
                return;
            }
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (user == null)
                return;

            if (user.Statusses.ContainsKey("lie") || user.isLying || user.RidingHorse || user.IsWalking)
                return;

            if (!user.Statusses.ContainsKey("sit"))
            {
                if (user.RotBody % 2 == 0)
                {
                    try
                    {
                        user.Statusses.Add("sit", "1.0");
                        user.Z -= 0.35;
                        user.isSitting = true;
                        user.UpdateNeeded = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    user.RotBody--;
                    user.Statusses.Add("sit", "1.0");
                    user.Z -= 0.35;
                    user.isSitting = true;
                    user.UpdateNeeded = true;
                }
            }
            else if (user.isSitting)
            {
                user.Z += 0.35;
                user.Statusses.Remove("sit");
                user.Statusses.Remove("1.0");
                user.isSitting = false;
                user.UpdateNeeded = true;
            }
        }
    }
}