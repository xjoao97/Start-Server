#region

using System;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Inventory.Bots;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Users.Inventory.Bots;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class KickBotsCommand : IChatCommand
    {
        public string PermissionRequired => "command_kickbots";

        public string Parameters => "";

        public string Description => "Expulse todos os bots do quarto.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            if (!room.CheckRights(session, true))
            {
                session.SendWhisper("Opa, apenas o dono do quarto pode usar este comando.!");
                return;
            }

            foreach (
                var user in
                room.GetRoomUserManager().GetUserList().ToList().Where(user => user?.IsPet == true && user.IsBot))
            {
                RoomUser botUser;
                if (!room.GetRoomUserManager().TryGetBot(user.BotData.Id, out botUser))
                    return;

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `bots` SET `room_id` = '0' WHERE `id` = @id LIMIT 1");
                    dbClient.AddParameter("id", user.BotData.Id);
                    dbClient.RunQuery();
                }

                session.GetHabbo()
                    .GetInventoryComponent()
                    .TryAddBot(new Bot(Convert.ToInt32(botUser.BotData.Id), Convert.ToInt32(botUser.BotData.ownerID),
                        botUser.BotData.Name, botUser.BotData.Motto, botUser.BotData.Look, botUser.BotData.Gender));
                session.SendMessage(new BotInventoryComposer(session.GetHabbo().GetInventoryComponent().GetBots()));
                room.GetRoomUserManager().RemoveBot(botUser.VirtualId, false);
            }

            session.SendWhisper("Successo, todos os bots foram expulsos.");
        }
    }
}