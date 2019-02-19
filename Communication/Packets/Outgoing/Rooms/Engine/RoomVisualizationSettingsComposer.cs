﻿namespace Oblivion.Communication.Packets.Outgoing.Rooms.Engine
{
    internal class RoomVisualizationSettingsComposer : ServerPacket
    {
        public RoomVisualizationSettingsComposer(int Walls, int Floor, bool HideWalls)
            : base(ServerPacketHeader.RoomVisualizationSettingsMessageComposer)
        {
            WriteBoolean(HideWalls);
            WriteInteger(Walls);
            WriteInteger(Floor);
        }
    }
}