#region

using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands
{
    public interface IChatCommand
    {
        string PermissionRequired { get; }
        string Parameters { get; }
        string Description { get; }
        void Execute(GameClient session, string[] Params);
    }
}