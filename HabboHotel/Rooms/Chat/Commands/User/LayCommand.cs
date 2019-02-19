#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class LayCommand : IChatCommand
    {
        public string PermissionRequired => "command_lay";

        public string Parameters => "";

        public string Description => "Deite na sala sem precisar de uma cama.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user == null)
                return;

            if (!room.GetGameMap().ValidTile(user.X + 2, user.Y + 2) &&
                !room.GetGameMap().ValidTile(user.X + 1, user.Y + 1))
            {
                session.SendWhisper("Opa, você não pode se deitar aqui, vá para outro lugar!");
                return;
            }

            if (user.Statusses.ContainsKey("sit") || user.isSitting || user.RidingHorse || user.IsWalking)
                return;

            if (session.GetHabbo().Effects().CurrentEffect > 0)
                session.GetHabbo().Effects().ApplyEffect(0);

            if (!user.Statusses.ContainsKey("lay"))
            {
                if (user.RotBody % 2 == 0)
                {
                    try
                    {
                        user.Statusses.Add("lay", "1.0 null");
                        user.Z -= 0.35;
                        user.isLying = true;
                        user.UpdateNeeded = true;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    user.RotBody--; //
                    user.Statusses.Add("lay", "1.0 null");
                    user.Z -= 0.35;
                    user.isLying = true;
                    user.UpdateNeeded = true;
                }
            }
            else
            {
                user.Z += 0.35;
                user.Statusses.Remove("lay");
                user.Statusses.Remove("1.0");
                user.isLying = false;
                user.UpdateNeeded = true;
            }
        }
    }
}