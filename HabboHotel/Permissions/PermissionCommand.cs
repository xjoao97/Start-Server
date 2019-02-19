namespace Oblivion.HabboHotel.Permissions
{
    internal class PermissionCommand
    {
        public PermissionCommand(string Command, int GroupId)
        {
            this.Command = Command;
            this.GroupId = GroupId;
        }

        public string Command { get; set; }
        public int GroupId { get; set; }
    }
}