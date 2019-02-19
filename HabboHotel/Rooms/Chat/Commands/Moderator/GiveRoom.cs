#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class GiveRoom : IChatCommand
    {
        public string PermissionRequired => "command_give_room";

        public string Parameters => "%cantidad%";

        public string Description => "Envie um emblema para todos os usuários de um quarto!";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Insira o código do emblema que você deseja enviar.");
                return;
            }
            int amount;
            var room = session.GetHabbo().CurrentRoom;

            if (int.TryParse(Params[1], out amount))

                foreach (
                    var roomUser in
                    room.GetRoomUserManager()
                        .GetRoomUsers()
                        .Where(roomUser => roomUser?.GetClient() != null && session.GetHabbo().Id != roomUser.UserId))
                {
                    roomUser.GetClient().GetHabbo().Credits += amount;
                    roomUser.GetClient().SendMessage(new CreditBalanceComposer(roomUser.GetClient().GetHabbo().Credits));
                }
        }
    }
}