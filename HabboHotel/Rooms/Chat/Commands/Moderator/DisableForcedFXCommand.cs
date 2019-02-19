#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class DisableForcedFxCommand : IChatCommand
    {
        public string PermissionRequired => "command_forced_effects";

        public string Parameters => "";

        public string Description => "Dá a habilidade de desabilitar efeitos forçados.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().DisableForcedEffects = !session.GetHabbo().DisableForcedEffects;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `disable_forced_effects` = @DisableForcedEffects WHERE `id` = '" +
                                  session.GetHabbo().Id + "' LIMIT 1");
                dbClient.AddParameter("DisableForcedEffects",
                    (session.GetHabbo().DisableForcedEffects ? 1 : 0).ToString());
                dbClient.RunQuery();
            }

            session.SendWhisper("Modo de efeito forçado agora está " +
                                (session.GetHabbo().DisableForcedEffects ? "desativado!" : "ativado!"));
        }
    }
}