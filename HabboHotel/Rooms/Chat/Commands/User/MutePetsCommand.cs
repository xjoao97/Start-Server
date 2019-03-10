#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class MutePetsCommand : IChatCommand
    {
        public string PermissionRequired => "command_mute_pets";

        public string Parameters => "";

        public string Description => "Ignore os pets.";

        public void Execute(GameClient session, string[] Params)
        {
            session.GetHabbo().AllowPetSpeech = !session.GetHabbo().AllowPetSpeech;
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.RunFastQuery("UPDATE `users` SET `pets_muted` = '" + (session.GetHabbo().AllowPetSpeech ? 1 : 0) +
                                  "' WHERE `id` = '" + session.GetHabbo().Id + "' LIMIT 1");
            }

            if (session.GetHabbo().AllowPetSpeech)
                session.SendWhisper("Pronto, agora você não vê as mensagens do pet.");
            else
                session.SendWhisper("Opa, você pode ver novamente as mensagens do pet.");
        }
    }
}