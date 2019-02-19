#region

using System;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Administrator
{
    internal class CarryCommand : IChatCommand
    {
        public string PermissionRequired => "command_sit";

        public string Parameters => "%ItemId%";

        public string Description => "Handitem";

        public void Execute(GameClient Session, string[] Params)
        {
            int ItemId;
            if (!int.TryParse(Convert.ToString(Params[1]), out ItemId))
            {
                Session.SendWhisper("Por favor, escolha um item válido.");
                return;
            }

            var Room = Session.GetHabbo().CurrentRoom;
            var User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            User?.CarryItem(ItemId);
        }
    }
}