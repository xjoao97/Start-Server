#region

using Oblivion.Communication.Packets.Outgoing.Campaigns;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class GiftListCommand : IChatCommand
    {
        public string PermissionRequired => "command_sit";

        public string Parameters => "";

        public string Description => "Ver os presentes";

        public void Execute(GameClient session, string[] Params) => session.SendMessage(new CampaignCalendarDataComposer(session.GetHabbo().GetStats().openedGifts));
    }
}

