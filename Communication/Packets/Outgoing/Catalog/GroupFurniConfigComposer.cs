#region

using System.Collections.Generic;
using Oblivion.HabboHotel.Groups;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Catalog
{
    internal class GroupFurniConfigComposer : ServerPacket
    {
        public GroupFurniConfigComposer(ICollection<Group> Groups)
            : base(ServerPacketHeader.GroupFurniConfigMessageComposer)
        {
            WriteInteger(Groups.Count);
            foreach (var Group in Groups)
            {
                WriteInteger(Group.Id);
                WriteString(Group.Name);
                WriteString(Group.Badge);
                WriteString(OblivionServer.GetGame().GetGroupManager().SymbolColours.Contains(Group.Colour1)
                    ? ((GroupSymbolColours) OblivionServer.GetGame().GetGroupManager().SymbolColours[Group.Colour1]).Colour
                    : "4f8a00"); // Group Colour 1
                WriteString(OblivionServer.GetGame().GetGroupManager().BackGroundColours.Contains(Group.Colour2)
                    ? ((GroupBackGroundColours) OblivionServer.GetGame().GetGroupManager().BackGroundColours[Group.Colour2]).Colour
                    : "4f8a00"); // Group Colour 2            
                WriteBoolean(false);
                WriteInteger(Group.CreatorId);
                WriteBoolean(Group.ForumEnabled);
            }
        }
    }
}