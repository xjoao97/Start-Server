#region

using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.Communication.Packets.Outgoing.Navigator
{
    internal class GetGuestRoomResultComposer : ServerPacket
    {
        public GetGuestRoomResultComposer(GameClient Session, RoomData Data, bool isLoading, bool checkEntry)
            : base(ServerPacketHeader.GetGuestRoomResultMessageComposer)
        {
            WriteBoolean(isLoading);
            WriteInteger(Data.Id);
            WriteString(Data.Name);
            WriteInteger(Data.OwnerId);
            WriteString(Data.OwnerName);
            WriteInteger(RoomAccessUtility.GetRoomAccessPacketNum(Data.Access));
            WriteInteger(Data.UsersNow);
            WriteInteger(Data.UsersMax);
            WriteString(Data.Description);
            WriteInteger(Data.TradeSettings);
            WriteInteger(Data.Score);
            WriteInteger(0); //Top rated room rank.
            WriteInteger(Data.Category);

            WriteInteger(Data.Tags.Count);
            foreach (var Tag in Data.Tags)
                WriteString(Tag);

            if (Data.Group != null && Data.Promotion != null)
            {
                WriteInteger(62); //What?

                WriteInteger((int) Data.Group?.Id);
                WriteString(Data.Group == null ? "" : Data.Group.Name);
                WriteString(Data.Group == null ? "" : Data.Group.Badge);

                WriteString(Data.Promotion != null ? Data.Promotion.Name : "");
                WriteString(Data.Promotion != null ? Data.Promotion.Description : "");
                WriteInteger(Data.Promotion?.MinutesLeft ?? 0);
            }
            else if (Data.Group != null && Data.Promotion == null)
            {
                WriteInteger(58); //What?
                WriteInteger((int) Data.Group?.Id);
                WriteString(Data.Group == null ? "" : Data.Group.Name);
                WriteString(Data.Group == null ? "" : Data.Group.Badge);
            }
            else if (Data.Group == null && Data.Promotion != null)
            {
                WriteInteger(60); //What?
                WriteString(Data.Promotion != null ? Data.Promotion.Name : "");
                WriteString(Data.Promotion != null ? Data.Promotion.Description : "");
                WriteInteger(Data.Promotion?.MinutesLeft ?? 0);
            }
            else
            {
                WriteInteger(56); //What?
            }


            WriteBoolean(checkEntry);
            WriteBoolean(false);
            WriteBoolean(false);
            WriteBoolean(false);

            WriteInteger(Data.WhoCanMute);
            WriteInteger(Data.WhoCanKick);
            WriteInteger(Data.WhoCanBan);

            WriteBoolean(Session.GetHabbo().GetPermissions().HasRight("mod_tool") ||
                         Data.OwnerName == Session.GetHabbo().Username); //Room muting.
            WriteInteger(Data.ChatMode);
            WriteInteger(Data.ChatSize);
            WriteInteger(Data.ChatSpeed);
            WriteInteger(Data.ExtraFlood); //Hearing distance
            WriteInteger(Data.ChatDistance); //Flood!!
        }
    }
}