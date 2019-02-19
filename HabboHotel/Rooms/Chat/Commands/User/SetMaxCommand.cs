#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class SetMaxCommand : IChatCommand
    {
        public string PermissionRequired => "command_setmax";

        public string Parameters => "%valor%";

        public string Description => "Escolha o número de visitantes para seu quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;
            if (!room.CheckRights(session, true))
                return;

            if (Params.Length == 1)
            {
                session.SendWhisper("Por favor, insira um valor limite para seu quarto.");
                return;
            }

            int maxAmount;
            if (int.TryParse(Params[1], out maxAmount))
            {
                if (maxAmount == 0)
                {
                    maxAmount = 10;
                    session.SendWhisper("Quantidade pequena, o número foi definido como 10.");
                }
                else if (maxAmount > 200 && !session.GetHabbo().GetPermissions().HasRight("override_command_setmax_limit"))
                {
                    maxAmount = 200;
                    session.SendWhisper("Número muito grande para seu cargo, o valor definido foi 200.");
                }
                else
                {
                    session.SendWhisper("Valor definido: " + maxAmount + ".");
                }

                room.UsersMax = maxAmount;
                room.RoomData.UsersMax = maxAmount;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `rooms` SET `users_max` = " + maxAmount + " WHERE `id` = '" + room.Id +
                                      "' LIMIT 1");
                }
            }
            else
            {
                session.SendWhisper("Quantidade inválida, por favor insira novamente.");
            }
        }
    }
}