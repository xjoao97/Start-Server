#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class AddEventCommand : IChatCommand
    {
        public string PermissionRequired => "command_hal";

        public string Parameters => "%message%";

        public string Description => "Adicionar um evento";

        public void Execute(GameClient Session, string[] Params)
        {
            if (Params.Length <= 2)
            {
                Session.SendWhisper("Digite um nome para o evento.");
                return;
            }
            var name = CommandManager.MergeParams(Params, 1);
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO eventlist (name) VALUE (@name)");
                dbClient.AddParameter("name", name);
                dbClient.RunQuery();
            }
            Session.SendWhisper("Você adicionou o evento " + name);
        }
    }
}