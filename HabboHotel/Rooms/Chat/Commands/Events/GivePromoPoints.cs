using System.Threading;
using System.Threading.Tasks;
using Oblivion.Communication.Packets.Outgoing.Inventory.Purse;
using Oblivion.Communication.Packets.Outgoing.Rooms.Notifications;
using Oblivion.HabboHotel.GameClients;

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Events
{
    internal class GivePromoPoints : IChatCommand
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
                const int amount = 150;
                target.GetHabbo().Diamonds += amount;
                target.SendMessage(new HabboActivityPointNotificationComposer(target.GetHabbo().Diamonds, amount,
                    5));

                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("update users set ppoints = ppoints + 1, vip_points = vip_points + " + amount +
                                      " WHERE id = @id");
                    dbClient.AddParameter("id", userId);
                    dbClient.RunQuery();
                }                   
                target.SendNotification("Parabéns, parece que você ganhou uma promoção.");

                session.SendWhisper("Sucesso, você enviou o prêmio para " +
                                    target.GetHabbo().Username + "!");
            });
        }
    }
}