#region

using System;
using System.Text;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class StatsCommand : IChatCommand
    {
        public string PermissionRequired => "command_stats";

        public string Parameters => "";

        public string Description => "Veja suas estatísticas.";

        public void Execute(GameClient session, string[] Params)
        {
            var minutes = session.GetHabbo().GetStats().OnlineTime / 60;
            var hours = minutes / 60;
            var onlineTime = Convert.ToInt32(hours);
            var s = onlineTime == 1 ? "" : "s";

            var habboInfo = new StringBuilder();
            habboInfo.Append("Status da sua conta:\r\r");

            habboInfo.Append("Créditos: " + session.GetHabbo().Credits + "\r");
            habboInfo.Append("Estrelas do Mar: " + session.GetHabbo().Duckets + "\r");
            habboInfo.Append("Conchas: " + session.GetHabbo().Diamonds + "\r");
            habboInfo.Append("Moedas Promocionais: " + session.GetHabbo().GOTWPoints + "\r");
            habboInfo.Append("Tempo Online: " + onlineTime + " Hora" + s + "\r");
            habboInfo.Append("Respeitos: " + session.GetHabbo().GetStats().Respect + "\r");


            session.SendNotification(habboInfo.ToString());
        }
    }
}