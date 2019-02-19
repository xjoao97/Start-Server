#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class EmptyUser : IChatCommand
    {
        public string PermissionRequired => "command_emptyuser";

        public string Parameters => "%username%";

        public string Description => "%USUARIO% - Limpiar Inventario a un Usuario";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Escribe el nombre del usuario que deseas limpiar el inventario.");
                return;
            }

            var TargetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("¡Oops! Probablemente el usuario no se encuentre en linea.");
                return;
            }

            if (TargetClient.GetHabbo().Rank >= Session.GetHabbo().Rank)
            {
                Session.SendWhisper("No puedes limpiar el inventario a este usuario");
                return;
            }


            TargetClient.GetHabbo().GetInventoryComponent().ClearItems();
        }
    }
}