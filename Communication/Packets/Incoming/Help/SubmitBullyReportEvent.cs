#region

using System;
using Oblivion.Communication.Packets.Outgoing.Help;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Help
{
    internal class SubmitBullyReportEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            //0 = sent, 1 = blocked, 2 = no chat, 3 = already reported.
            if (Session == null)
                return;

            var UserId = Packet.PopInt();
            if (UserId == Session.GetHabbo().Id) //Hax
                return;

            if (Session.GetHabbo().AdvertisingReportedBlocked)
            {
                Session.SendMessage(new SubmitBullyReportComposer(1)); //This user is blocked from reporting.
                return;
            }

            var Client = OblivionServer.GetGame().GetClientManager().GetClientByUserID(Convert.ToInt32(UserId));
            if (Client == null)
            {
                Session.SendMessage(new SubmitBullyReportComposer(0)); //Just say it's sent, the user isn't found.
                return;
            }

            if (Session.GetHabbo().LastAdvertiseReport > OblivionServer.GetUnixTimestamp())
            {
                Session.SendNotification("Reports can only be sent per 5 minutes!");
                return;
            }

            if (Client.GetHabbo().GetPermissions().HasRight("mod_tool")) //Reporting staff, nope!
            {
                Session.SendNotification("Sorry, you cannot report staff members via this tool.");
                return;
            }

            //This user hasn't even said a word, nope!
            if (!Client.GetHabbo().HasSpoken)
            {
                Session.SendMessage(new SubmitBullyReportComposer(2));
                return;
            }

            //Already reported, nope.
            if (Client.GetHabbo().AdvertisingReported && Session.GetHabbo().Rank < 2)
            {
                Session.SendMessage(new SubmitBullyReportComposer(3));
                return;
            }

            if (Session.GetHabbo().Rank <= 1)
                Session.GetHabbo().LastAdvertiseReport = OblivionServer.GetUnixTimestamp() + 300;
            else
                Session.GetHabbo().LastAdvertiseReport = OblivionServer.GetUnixTimestamp();

            Client.GetHabbo().AdvertisingReported = true;
            Session.SendMessage(new SubmitBullyReportComposer(0));
            OblivionServer.GetGame().GetClientManager().DoAdvertisingReport(Session, Client);
        }
    }
}