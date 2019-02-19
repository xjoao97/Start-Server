#region

using System.Threading.Tasks;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class StartQuickPollCommand : IChatCommand
    {
        public string PermissionRequired => "command_start_quick_poll";

        public string Parameters => "%question% [%time%]";

        public string Description => "Começe uma votação!";

        public void Execute(GameClient session, string[] Params)
        {
            var Room = session.GetHabbo().CurrentRoom;
            if (!Room.CheckRights(session, false, true))
                return;
            if (Params.Length < 2)
            {
                session.SendWhisper("Erro");
                return;
            }

            var question = CommandManager.MergeParams(Params, 1);
            const int duration = 60000;

            Task.Factory.StartNew(() =>
            {
                Room.GenerateQPoll(duration, question);
                Room.QuickPoll.Start();
            });
        }
    }
}