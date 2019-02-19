#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class MimicCommand : IChatCommand
    {
        public string PermissionRequired => "command_mimic";

        public string Parameters => "%username%";

        public string Description => "Gostou do estilo de alguém? Copie-o. xD";

        public void Execute(GameClient session, string[] Params)
        {
            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, digite o nome do usuário que você deseja copiar o visual.");
                return;
            }

            var targetClient = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (targetClient == null)
            {
                session.SendWhisper("Aconteceu um erro, o usuário não está online.");
                return;
            }

            if (!targetClient.GetHabbo().AllowMimic)
            {
                session.SendWhisper("Hey, este usuário não deseja ser copiado. :(");
                return;
            }

            var targetUser =
                session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(targetClient.GetHabbo().Id);
            if (targetUser == null)
            {
                session.SendWhisper(
                    "Provavelmente o usuário não está online ou no quarto.");
                return;
            }

            session.GetHabbo().Gender = targetUser.GetClient().GetHabbo().Gender;
            session.GetHabbo().Look = targetUser.GetClient().GetHabbo().Look;

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `gender` = @gender, `look` = @look WHERE `id` = @id LIMIT 1");
                dbClient.AddParameter("gender", session.GetHabbo().Gender);
                dbClient.AddParameter("look", session.GetHabbo().Look);
                dbClient.AddParameter("id", session.GetHabbo().Id);
                dbClient.RunQuery();
            }
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user != null)
            {
                session.SendMessage(new UserChangeComposer(user, true));
                room.SendMessage(new UserChangeComposer(user, false));
            }
        }
    }
}