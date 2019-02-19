using System.Threading;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class GiveEventPoints : IChatCommand
    {
        public string PermissionRequired => "command_give";

        public string Parameters => "%usuário%";

        public string Description => "";

        public void Execute(GameClient session, string[] Params)
        {
            var target = OblivionServer.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (target == null)
            {
                session.SendWhisper("Opa, não coneseguimos encontrar esse usuário!");
                return;
            }

            var userId = target.GetHabbo().Id;
            if (userId == session.GetHabbo().Id)
                return;


            Task.Factory.StartNew(() =>
            {
                const int amount = 20;
                target.GetHabbo().Diamonds += amount;
                target.GetHabbo().EventPoint += 1;
                target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().Diamonds, amount,
                    5));

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("update users set epoints = epoints + 1, vip_points = vip_points + " + amount +
                                      " WHERE id = @id");
                    dbClient.AddParameter("id", userId);
                    dbClient.RunQuery();
                }

                OblivionServer.GetGame().GetAchievementManager().ProgressAchievement(target, "GAME", 1);
                target.SendMessage(new RoomNotificationComposer("",
                    "Hey, você ganhou o evento, parabéns!",
                    "fig/" + target.GetHabbo().Look,
                    session.GetHabbo().Username, "" + "event:friendbar/user/" + session.GetHabbo().Username, true));
                Thread.Sleep(5000);
                OblivionServer.GetGame()
                    .GetClientManager().SendMessage(new RoomNotificationComposer("",
                        "O usuário " + target.GetHabbo().Username + " ganhou o evento, parabéns!",
                        "fig/" + target.GetHabbo().Look,
                        session.GetHabbo().Username, "" + "event:friendbar/user/" + session.GetHabbo().Username, true));

                session.SendWhisper("Sucesso, você enviou o prêmio para " +
                                    target.GetHabbo().Username + "!");
            });
        }
    }
}