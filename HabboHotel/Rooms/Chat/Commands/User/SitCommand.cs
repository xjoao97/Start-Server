#region

using System;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SitCommand : IChatCommand
    {
        public string PermissionRequired => "command_sit";

        public string Parameters => "";

        public string Description => "Cansado demais? Sente-se.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            if (user == null || user.Statusses.ContainsKey("lie") || user.isLying || user.RidingHorse || user.IsWalking)
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
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
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