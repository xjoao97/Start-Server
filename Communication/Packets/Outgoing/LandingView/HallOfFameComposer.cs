#region

using System.Collections.Generic;
using Oblivion.HabboHotel.LandingView.Items;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.LandingView
{
    internal class HallOfFameComposer : ServerPacket
    {
        public HallOfFameComposer(ICollection<HallOfFameUser> Users)
            : base(ServerPacketHeader.UpdateHallOfFameListMessageComposer)
        {
            WriteString("");
            WriteInteger(Users.Count);
            foreach (var user in Users)
            {
                var habbo = OblivionServer.GetHabboById(user.UserId);
                if (habbo == null)
                {
                    WriteInteger(0);
                    WriteString("Error");
                    WriteString("Error");
                    WriteInteger(0);
                    WriteInteger(0);
                    continue;
                }

                WriteInteger(habbo.Id);
                WriteString(habbo.Username);
                WriteString(habbo.Look);
                WriteInteger(2);
                WriteInteger(user.Score);
            }
        }
    }
}