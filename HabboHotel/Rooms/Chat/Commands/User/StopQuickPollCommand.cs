#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class StopQuickPollCommand : IChatCommand
    {
        public string PermissionRequired => "command_stop_quick_poll";

        public string Parameters => "";

        public string Description => "Encerre sua question!";

        public void Execute(GameClient Session, string[] Params)
        {
            var Room = Session.GetHabbo().CurrentRoom;

            if (!Room.CheckRights(Session, false, true))
                return;
            if (Room.QuickPoll == null)
            {
                Session.SendWhisper("Nenhuma votação encontrada");
                return;
            }
            Room.QuickPoll.Stop();
        }
    }
}