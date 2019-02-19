using System.Collections;
using Oblivion.Communication.Packets.Incoming;

namespace Oblivion.Communication.Packets
{
    static class ClientMessageFactory
    {
        private static Queue _freeObjects;

        public static int ObjectsSize() => _freeObjects.Count;

        public static void FlushCache()
        {
            lock (_freeObjects.SyncRoot)
            {
                _freeObjects.Clear();
            }
        }

        public static void Init() => _freeObjects = new Queue();

        public static ClientPacket GetClientMessage(int messageId, byte[] body)
        {
            if (_freeObjects.Count <= 0)
                return new ClientPacket(messageId, body);
            ClientPacket message;

            lock (_freeObjects.SyncRoot)
            {
                message = (ClientPacket)_freeObjects.Dequeue();
            }
            if (message == null)
                return new ClientPacket(messageId, body);

            message.Init(messageId, body);
            return message;
        }

        public static void ObjectCallback(ClientPacket message)
        {
            lock (_freeObjects.SyncRoot)
            {
                _freeObjects.Enqueue(message);
            }
        }
    }
}