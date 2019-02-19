#region

using System;
using Oblivion.Utilities;

#endregion

namespace Oblivion.Communication.Packets.Incoming
{
    public class ClientPacket : IDisposable
    {
        private byte[] Body;
        private int Pointer;

        public ClientPacket(int messageID, byte[] body)
        {
            Init(messageID, body);
        }

        public int Id { get; private set; }

        public int RemainingLength => Body.Length - Pointer;

        public int Header => Id;

        public void Init(int messageID, byte[] body)
        {
            if (body == null)
                body = new byte[0];

            Id = messageID;
            Body = body;

            Pointer = 0;
        }

        public override string ToString() => "[" + Header + "] BODY: " +
                                             OblivionServer.GetDefaultEncoding()
                                                 .GetString(Body)
                                                 .Replace(Convert.ToChar(0).ToString(), "[0]");


        public byte[] ReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
                Bytes = RemainingLength;

            var data = new byte[Bytes];

            for (var i = 0; i < Bytes; i++)
                data[i] = Body[Pointer++];

            return data;
        }

        internal byte[] GetBytes(int len)
        {
            var arrayBytes = new byte[len];
            var pos = Pointer;

            for (var i = 0; i < len; i++)
            {
                arrayBytes[i] = Body[pos];

                pos++;
            }

            return arrayBytes;
        }

        public byte[] PlainReadBytes(int Bytes)
        {
            if (Bytes > RemainingLength)
                Bytes = RemainingLength;

            var data = new byte[Bytes];

            for (int x = 0, y = Pointer; x < Bytes; x++, y++)
                data[x] = Body[y];

            return data;
        }

        public byte[] ReadFixedValue()
        {
            var len = HabboEncoding.DecodeInt16(ReadBytes(2));
            return ReadBytes(len);
        }

        public string PopString() => OblivionServer.GetDefaultEncoding().GetString(ReadFixedValue());

        public bool PopBoolean() => RemainingLength > 0 && Body[Pointer++] == Convert.ToChar(1);

        public int PopInt()
        {
            if (RemainingLength < 1)
                return 0;

            var Data = PlainReadBytes(4);

            var i = HabboEncoding.DecodeInt32(Data);

            Pointer += 4;

            return i;
        }
        public void Dispose()
        {
            ClientMessageFactory.ObjectCallback(this);
            GC.SuppressFinalize(this);
        }
    }
}