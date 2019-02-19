#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class EmptyItems : IChatCommand
    {
        public string PermissionRequired => "command_empty_items";

        public string Parameters => "";

        public string Description => "Seu inventário está cheio? Limpe-o com este comando.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().GetInventoryComponent().ClearItems();
            session.SendNotification("Seu inventário foi limpo!");
        }
    }
}