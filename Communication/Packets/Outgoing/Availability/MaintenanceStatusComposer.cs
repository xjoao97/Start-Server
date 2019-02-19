namespace Oblivion.Communication.Packets.Outgoing.Availability
{
    internal class MaintenanceStatusComposer : ServerPacket
    {
        public MaintenanceStatusComposer(int Minutes, int Duration)
            : base(ServerPacketHeader.MaintenanceStatusMessageComposer)
        {
            WriteBoolean(false);
            WriteInteger(Minutes); //Time till shutdown.
            WriteInteger(Duration); //Duration
        }
    }
}