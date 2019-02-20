#region

using Oblivion.Communication.Packets.Outgoing;
using Oblivion.Communication.Packets.Outgoing.Nux;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Nucs
{
    internal class RoomNucsAlerta : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var habbo = Session.GetHabbo();
            if (!habbo.PassedNuxNavigator && !habbo.PassedNuxCatalog && !habbo.PassedNuxChat && !habbo.PassedNuxDuckets &&
                !habbo.PassedNuxItems)
            {
                Session.SendMessage(
                    new NuxAlertComposer(
                        "helpBubble/add/BOTTOM_BAR_NAVIGATOR/Este é o navegador de quartos, aqui você poderá encontrar novas salas."));
                habbo.PassedNuxNavigator = true;
            }

            if (habbo.PassedNuxNavigator && !habbo.PassedNuxCatalog && !habbo.PassedNuxChat && !habbo.PassedNuxDuckets &&
                !habbo.PassedNuxItems)
            {
                Session.SendMessage(
                    new NuxAlertComposer(
                        "helpBubble/add/BOTTOM_BAR_CATALOG/Aqui é a loja, você pode comprar mobílias e decorar seus quartos aqui!"));
                habbo.PassedNuxCatalog = true;
            }
            else if (habbo.PassedNuxNavigator && habbo.PassedNuxCatalog && !habbo.PassedNuxChat && !habbo.PassedNuxDuckets &&
                     !habbo.PassedNuxItems)
            {
                Session.SendMessage(
                    new NuxAlertComposer(
                        "helpBubble/add/CHAT_INPUT/Este é o chat, aqui você pode conversar com os outros usuários."));
                habbo.PassedNuxChat = true;
            }
            else if (habbo.PassedNuxNavigator && habbo.PassedNuxCatalog && habbo.PassedNuxChat && !habbo.PassedNuxDuckets &&
                     !habbo.PassedNuxItems)
            {
                Session.SendMessage(
                    new NuxAlertComposer(
                        "helpBubble/add/DUCKETS_BUTTON/Nesta caixa você pode ver sua economia no Hotel."));
                habbo.PassedNuxDuckets = true;
            }
            else if (habbo.PassedNuxNavigator && habbo.PassedNuxCatalog && habbo.PassedNuxChat && habbo.PassedNuxDuckets &&
                     !habbo.PassedNuxItems)
            {
                Session.SendMessage(
                    new NuxAlertComposer(
                        "helpBubble/add/BOTTOM_BAR_INVENTORY/Aqui é o inventário, aqui fica armazenado seus emblemas, mobílias, mascotes e bots."));
                habbo.PassedNuxItems = true;
            }

            if (habbo.PassedNuxNavigator && habbo.PassedNuxCatalog && habbo.PassedNuxChat && habbo.PassedNuxDuckets &&
                habbo.PassedNuxItems)
            {
                Session.SendMessage(new NuxAlertComposer("nux/lobbyoffer/show"));
                habbo._NUX = false;
                using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.runFastQuery("UPDATE users SET nux_user = 'false' WHERE id = " + Session.GetHabbo().Id + ";");
                }
                var nuxStatus = new ServerPacket(ServerPacketHeader.NuxUserStatus);
                nuxStatus.WriteInteger(0);
                Session.SendMessage(nuxStatus);
            }
        }
    }
}