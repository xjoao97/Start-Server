#region

using System.Threading;
using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class PickAllCommand : IChatCommand
    {
        public string PermissionRequired => "command_pickall";

        public string Parameters => "";

        public string Description => "Remover todos os itens do quarto";

        public void Execute(GameClient session, string[] Params) => Task.Factory.StartNew(() =>
        {
            var room = session.GetHabbo().CurrentRoom;

            var roomItemList = room.GetRoomItemHandler().RemoveAllFurniture(session);

            if (session.GetHabbo().GetInventoryComponent() == null)
                return;
            room.GetRoomItemHandler().RemoveItemsByOwner(ref roomItemList, ref session);
        });
    }
}