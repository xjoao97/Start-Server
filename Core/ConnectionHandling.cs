#region

using System;
using ConnectionManager;
using Oblivion.Net;

#endregion

namespace Oblivion.Core
{
    public class ConnectionHandling
    {
        private readonly SocketManager manager;

        public ConnectionHandling(int port, int maxConnections, int connectionsPerIP, bool enabeNagles)
        {
            manager = new SocketManager();
            manager.init(port, maxConnections, connectionsPerIP, new InitialPacketParser(), !enabeNagles);
        }

        public void Init()
        {
            manager.connectionEvent += manager_connectionEvent;
            manager.initializeConnectionRequests();
        }

        private static void manager_connectionEvent(ConnectionInformation connection)
        {
            connection.connectionChanged += connectionChanged;
            OblivionServer.GetGame()
                .GetClientManager()
                .CreateAndStartClient(Convert.ToInt32(connection.getConnectionID()), connection);
        }

        private static void connectionChanged(ConnectionInformation information, ConnectionState state)
        {
            if (state == ConnectionState.CLOSED)
                CloseConnection(information);
        }

        private static void CloseConnection(ConnectionInformation Connection)
        {
            try
            {
                Connection.Dispose();
                OblivionServer.GetGame()
                    .GetClientManager()
                    .DisposeConnection(Convert.ToInt32(Connection.getConnectionID()));
            }
            catch (Exception e)
            {
                Logging.LogException(e.ToString());
            }
        }

        public void Destroy() => manager.destroy();
    }
}