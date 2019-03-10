#region

using System.Collections.Generic;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Users;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Users
{
    internal class CheckValidNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            var InUse = false;
            var Name = Packet.PopString();

            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT COUNT(0) FROM `users` WHERE `username` = @name LIMIT 1");
                dbClient.AddParameter("name", Name);
                InUse = dbClient.GetInteger() == 1;
            }

            var Letters = Name.ToLower().ToCharArray();
            var AllowedCharacters = "abcdefghijklmnopqrstuvwxyz.,_-;:?!1234567890";

            foreach (var Chr in Letters)
                if (!AllowedCharacters.Contains(Chr))
                {
                    Session.SendMessage(new NameChangeUpdateComposer(Name, 4));
                    return;
                }

            if (OblivionServer.GetGame().GetChatManager().GetFilter().IsFiltered(Name))
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 4));
                return;
            }

            if (!Session.GetHabbo().GetPermissions().HasRight("mod_tool") && Name.ToLower().Contains("mod") ||
                Name.ToLower().Contains("adm") || Name.ToLower().Contains("admin") || Name.ToLower().Contains("m0d"))
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 4));
            }
            else if (!Name.ToLower().Contains("mod") && (Session.GetHabbo().Rank == 2 || Session.GetHabbo().Rank == 3))
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 4));
            }
            else if (Name.Length > 15)
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 3));
            }
            else if (Name.Length < 3)
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 2));
            }
            else if (InUse)
            {
                ICollection<string> Suggestions = new List<string>();
                for (var i = 100; i < 103; i++)
                    Suggestions.Add(i.ToString());

                Session.SendMessage(new NameChangeUpdateComposer(Name, 5, Suggestions));
            }
            else
            {
                Session.SendMessage(new NameChangeUpdateComposer(Name, 0));
            }
        }
    }
}