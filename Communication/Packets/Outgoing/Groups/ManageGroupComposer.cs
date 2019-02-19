#region

using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Groups
{
    internal class ManageGroupComposer : ServerPacket
    {
        public ManageGroupComposer(Group Group)
            : base(ServerPacketHeader.ManageGroupMessageComposer)
        {
            WriteInteger(0);
            WriteBoolean(true);
            WriteInteger(Group.Id);
            WriteString(Group.Name);
            WriteString(Group.Description);
            WriteInteger(1);
            WriteInteger(Group.Colour1);
            WriteInteger(Group.Colour2);
            WriteInteger(Group.GroupType == GroupType.OPEN ? 0 : Group.GroupType == GroupType.LOCKED ? 1 : 2);
            WriteInteger(Group.AdminOnlyDeco);
            WriteBoolean(false);
            WriteString("");

            var BadgeSplit = Group.Badge.Replace("b", "").Split('s');
            WriteInteger(5);
            var Req = 5 - BadgeSplit.Length;
            var Final = 0;
            var array2 = BadgeSplit;
            foreach (var Symbol in array2)
            {
                WriteInteger(Symbol.Length >= 6 ? int.Parse(Symbol.Substring(0, 3)) : int.Parse(Symbol.Substring(0, 2)));
                WriteInteger(Symbol.Length >= 6 ? int.Parse(Symbol.Substring(3, 2)) : int.Parse(Symbol.Substring(2, 2)));
                WriteInteger(Symbol.Length < 5
                    ? 0
                    : Symbol.Length >= 6 ? int.Parse(Symbol.Substring(5, 1)) : int.Parse(Symbol.Substring(4, 1)));
            }

            while (Final != Req)
            {
                WriteInteger(0);
                WriteInteger(0);
                WriteInteger(0);
                Final++;
            }

            WriteString(Group.Badge);
            WriteInteger(Group.MemberCount);
        }
    }
}