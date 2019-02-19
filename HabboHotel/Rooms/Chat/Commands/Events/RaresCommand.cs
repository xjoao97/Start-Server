using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class CatalogUpdateAlert : IChatCommand
    {
        public string PermissionRequired => "command_hotel_alert";

        public string Parameters => "%message%";

        public string Description => "Link -> .";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length <= 1)
                return;

            var Message = CommandManager.MergeParams(Params, 1);
            if (Message == null) session.SendWhisper("Coloque a link page.");

            OblivionServer.GetGame().GetClientManager().SendMessage(new RoomNotificationComposer("Raros no Catálogo!",
                "O catálogo do <font color=\"#2E9AFE\"><b>Hiddo</b></font> acabou de ser atualizado. Se quiser ver <b>as novidades</b>, clique no botão abaixo.<br>",
                "raresltd", "Ir para a página!", "event:catalog/open/" + Message));
        }
    }
}