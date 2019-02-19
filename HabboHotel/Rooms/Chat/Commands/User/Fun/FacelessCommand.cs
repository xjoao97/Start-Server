#region

using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class FacelessCommand : IChatCommand
    {
        public string PermissionRequired => "command_faceless";

        public string Parameters => "";

        public string Description => "Hey, quer ficar sem rosto? Utilize este comando.";

        public void Execute(GameClient session, string[] Params)
        {
            var room = session.GetHabbo().CurrentRoom;

            var user = room.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (user?.GetClient() == null)
                return;

            var figureParts = session.GetHabbo().Look.Split('.');
            foreach (var part in figureParts)
                if (part.StartsWith("hd"))
                {
                    var headParts = part.Split('-');
                    if (!headParts[1].Equals("99999"))
                        headParts[1] = "99999";
                    else
                        return;

                    session.GetHabbo().Look = session.GetHabbo()
                        .Look.Replace(part, "hd-" + headParts[1] + "-" + headParts[2]);
                    break;
                }
            session.GetHabbo().Look = OblivionServer.GetGame().GetAntiMutant().RunLook(session.GetHabbo().Look);
            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("UPDATE `users` SET `look` = @Look WHERE `id` = '" + session.GetHabbo().Id +
                                  "' LIMIT 1");
                dbClient.AddParameter("look", session.GetHabbo().Look);
                dbClient.RunQuery();
            }

            session.SendMessage(new UserChangeComposer(user, true));
            session.GetHabbo().CurrentRoom.SendMessage(new UserChangeComposer(user, false));
        }
    }
}