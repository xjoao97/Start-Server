/*#region

using Oblivion.HabboHotel.Helpers;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Help.Helpers
{
    internal class HandleHelperToolComposer : ServerPacket
    {
        public HandleHelperToolComposer(bool onDuty, int helperAmount, int guideAmount, int guardianAmount)
            : base(ServerPacketHeader.HandleHelperToolMessageComposer)
        {
            WriteBoolean(onDuty);
            WriteInteger(guideAmount);
            WriteInteger(helperAmount);
            WriteInteger(guardianAmount);
        }

        public HandleHelperToolComposer(bool onDuty)
            : base(ServerPacketHeader.HandleHelperToolMessageComposer)
        {
            WriteBoolean(onDuty);
            WriteInteger( OblivionServer.GetGame().GetHelperManager().GuideCount);
            WriteInteger( OblivionServer.GetGame().GetHelperManager().HelperCount);
            WriteInteger( OblivionServer.GetGame().GetHelperManager().GuardianCount);
        }
    }
}*/

