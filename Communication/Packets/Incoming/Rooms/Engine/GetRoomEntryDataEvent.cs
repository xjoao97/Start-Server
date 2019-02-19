#region

using System.Linq;
using Oblivion.Communication.Packets.Outgoing.Rooms.Chat;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items.Wired;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Incoming.Rooms.Engine
{
    internal class GetRoomEntryDataEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (Session?.GetHabbo() == null)
                return;

            var Room = Session.GetHabbo().CurrentRoom;
            if (Room == null)
                return;

            if (Session.GetHabbo().InRoom)
            {
                Room OldRoom;

                if (!OblivionServer.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out OldRoom))
                    return;

                if (OldRoom.GetRoomUserManager() != null)
                    OldRoom.GetRoomUserManager().RemoveUserFromRoom(Session, false);
            }

            if (!Room.GetRoomUserManager().AddAvatarToRoom(Session))
            {
                Room.GetRoomUserManager().RemoveUserFromRoom(Session, false);
                return; //TODO: Remove?
            }

            Room.SendObjects(Session);
            //Status updating for messenger, do later as buggy.


            try
            {
                if (Session.GetHabbo().GetMessenger() != null)
                    Session.GetHabbo().GetMessenger().OnStatusChanged(true);
            }
            catch
            {
            }

            if (Session.GetHabbo().GetStats().QuestID > 0)
                OblivionServer.GetGame().GetQuestManager().QuestReminder(Session, Session.GetHabbo().GetStats().QuestID);

            Session.SendMessage(new RoomEntryInfoComposer(Room.RoomId, Room.CheckRights(Session, true)));
            Session.SendMessage(new RoomVisualizationSettingsComposer(Room.WallThickness, Room.FloorThickness,
                OblivionServer.EnumToBool(Room.Hidewall.ToString())));

            var ThisUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Username);

            if (ThisUser != null && Session.GetHabbo().PetId == 0)
                Room.SendMessage(new UserChangeComposer(ThisUser, false));

            Session.SendMessage(new RoomEventComposer(Room.RoomData, Room.RoomData.Promotion));

            if (Room.GetWired() != null)
                Room.GetWired().TriggerEvent(WiredBoxType.TriggerRoomEnter, Session.GetHabbo());

            foreach (var Bot in Room.GetRoomUserManager().GetBots().ToList().Where(Bot => Bot.IsBot || Bot.IsPet))
                Bot.BotAI.OnUserEnterRoom(ThisUser);

            if (OblivionServer.GetUnixTimestamp() < Session.GetHabbo().FloodTime && Session.GetHabbo().FloodTime != 0)
                Session.SendMessage(
                    new FloodControlComposer((int) Session.GetHabbo().FloodTime -
                                             (int) OblivionServer.GetUnixTimestamp()));
        }
    }
}