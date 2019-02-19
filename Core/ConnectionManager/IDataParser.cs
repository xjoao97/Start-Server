#region

using System;

#endregion

namespace SharedPacketLib
{
    public interface IDataParser : IDisposable, ICloneable
    {
        void handlePacketData(byte[] packet);
    }
}