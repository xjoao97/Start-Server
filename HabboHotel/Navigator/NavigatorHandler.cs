#region

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Oblivion.Communication.Packets.Outgoing;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Rooms;

#endregion

namespace Oblivion.HabboHotel.Navigator
{
    internal static class NavigatorHandler
    {
        public static void Search(ServerPacket Message, SearchResultList SearchResult, string SearchData,
            GameClient Session, int FetchLimit)
        {
            //Switching by categorys.
            switch (SearchResult.CategoryType)
            {
                default:
                    Message.WriteInteger(0);
                    break;

                case NavigatorCategoryType.QUERY:
                {
                    #region Query

                    if (SearchData.ToLower().StartsWith("owner:"))
                    {
                        if (SearchData.Length > 0)
                        {
                            //  var UserId = 0;
                            DataTable GetRooms = null;
                            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                            {
                                if (SearchData.ToLower().StartsWith("owner:"))
                                {
                                    dbClient.SetQuery(
                                        "SELECT r.* FROM rooms r, users u WHERE u.username = @username AND r.owner = u.id AND r.state != 'invisible' ORDER BY r.users_now DESC LIMIT 50;");
                                    dbClient.AddParameter("username", SearchData.Remove(0, 6));
                                    GetRooms = dbClient.getTable();
                                }
                            }

                            var Results = new List<RoomData>();
                            if (GetRooms != null)
                                foreach (
                                    var RoomData in GetRooms.Rows.Cast<DataRow>().Select(Row => OblivionServer.GetGame()
                                            .GetRoomManager()
                                            .FetchRoomData(Convert.ToInt32(Row["id"]), Row))
                                        .Where(RoomData => RoomData != null && !Results.Contains(RoomData)))
                                    Results.Add(RoomData);

                            Message.WriteInteger(Results.Count);
                            foreach (var Data in Results.ToList())
                                RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                    }
                    else if (SearchData.ToLower().StartsWith("tag:"))
                    {
                        SearchData = SearchData.Remove(0, 4);
                        ICollection<RoomData> TagMatches =
                            OblivionServer.GetGame().GetRoomManager().SearchTaggedRooms(SearchData);

                        Message.WriteInteger(TagMatches.Count);
                        foreach (var Data in TagMatches.ToList())
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    else if (SearchData.ToLower().StartsWith("group:"))
                    {
                        SearchData = SearchData.Remove(0, 6);
                        ICollection<RoomData> GroupRooms =
                            OblivionServer.GetGame().GetRoomManager().SearchGroupRooms(SearchData);

                        Message.WriteInteger(GroupRooms.Count);
                        foreach (var Data in GroupRooms.ToList())
                            RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    }
                    else
                    {
                        if (SearchData.Length > 0)
                        {
                            DataTable Table;
                            using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                            {
                                dbClient.SetQuery(
                                    "SELECT `id`,`caption`,`description`,`roomtype`,`owner`,`state`,`category`,`users_now`,`users_max`,`model_name`,`score`,`allow_pets`,`allow_pets_eat`,`room_blocking_disabled`,`allow_hidewall`,`password`,`wallpaper`,`floor`,`landscape`,`floorthick`,`wallthick`,`mute_settings`,`kick_settings`,`ban_settings`,`chat_mode`,`chat_speed`,`chat_size`,`trade_settings`,`group_id`,`tags` FROM rooms WHERE `caption` LIKE @query ORDER BY `users_now` DESC LIMIT 50");
                                if (SearchData.ToLower().StartsWith("roomname:"))
                                    dbClient.AddParameter("query", "%" + SearchData.Split(new[] {':'}, 2)[1] + "%");
                                else
                                    dbClient.AddParameter("query", "%" + SearchData + "%");
                                Table = dbClient.getTable();
                            }

                            var Results = new List<RoomData>();
                            if (Table != null)
                                foreach (var RData in from DataRow Row in Table.Rows
                                    where Convert.ToString(Row["state"]) != "invisible"
                                    select OblivionServer.GetGame()
                                        .GetRoomManager()
                                        .FetchRoomData(Convert.ToInt32(Row["id"]), Row)
                                    into RData
                                    where RData != null && !Results.Contains(RData)
                                    select RData)
                                    Results.Add(RData);

                            Message.WriteInteger(Results.Count);
                            foreach (var Data in Results.ToList())
                                RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                        }
                    }

                    #endregion

                    break;
                }

                case NavigatorCategoryType.FEATURED:

                    #region Featured

                    var Rooms = new List<RoomData>();
                    var Featured = OblivionServer.GetGame().GetNavigator().GetFeaturedRooms();
                    foreach (var Data in from FeaturedItem in Featured.ToList()
                        where FeaturedItem != null
                        select OblivionServer.GetGame().GetRoomManager().GenerateRoomData(FeaturedItem.RoomId)
                        into Data
                        where Data != null
                        where !Rooms.Contains(Data)
                        select Data)
                        Rooms.Add(Data);

                    Message.WriteInteger(Rooms.Count);
                    foreach (var Data in Rooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);

                    #endregion

                    break;

                case NavigatorCategoryType.POPULAR:
                {
                    var PopularRooms = OblivionServer.GetGame().GetRoomManager().GetPopularRooms(-1, FetchLimit);

                    Message.WriteInteger(PopularRooms.Count);
                    foreach (var Data in PopularRooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;
                }

                case NavigatorCategoryType.RECOMMENDED:
                {
                    var RecommendedRooms = OblivionServer.GetGame().GetRoomManager().GetRecommendedRooms(FetchLimit);

                    Message.WriteInteger(RecommendedRooms.Count);
                    foreach (var Data in RecommendedRooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;
                }

                case NavigatorCategoryType.CATEGORY:
                {
                    var GetRoomsByCategory = OblivionServer.GetGame()
                        .GetRoomManager()
                        .GetRoomsByCategory(SearchResult.Id, FetchLimit);

                    Message.WriteInteger(GetRoomsByCategory.Count);
                    foreach (var Data in GetRoomsByCategory.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;
                }

                case NavigatorCategoryType.MY_ROOMS:

                    Message.WriteInteger(Session.GetHabbo().UsersRooms.Count);
                    foreach (var Data in Session.GetHabbo().UsersRooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;

                case NavigatorCategoryType.MY_FAVORITES:
                    var Favourites = new List<RoomData>();
                    foreach (var Room in from int Id in Session.GetHabbo().FavoriteRooms.ToArray()
                        select OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Id)
                        into Room
                        where Room != null
                        where !Favourites.Contains(Room)
                        select Room)
                        Favourites.Add(Room);

                    Favourites = Favourites.Take(FetchLimit).ToList();

                    Message.WriteInteger(Favourites.Count);
                    foreach (var Data in Favourites.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;

                case NavigatorCategoryType.MY_GROUPS:
                    var MyGroups = new List<RoomData>();

                    foreach (
                        var Data in
                        from Group in
                        OblivionServer.GetGame().GetGroupManager().GetGroupsForUser(Session.GetHabbo().Id).ToList()
                        where Group != null
                        select OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Group.RoomId)
                        into Data
                        where Data != null
                        where !MyGroups.Contains(Data)
                        select Data)
                        MyGroups.Add(Data);

                    MyGroups = MyGroups.Take(FetchLimit).ToList();

                    Message.WriteInteger(MyGroups.Count);
                    foreach (var Data in MyGroups.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;

                case NavigatorCategoryType.MY_FRIENDS_ROOMS:
                    var MyFriendsRooms = new List<RoomData>();
                    foreach (
                        var buddy in
                        Session.GetHabbo()
                            .GetMessenger()
                            .GetFriends()
                            .Where(p => p.InRoom)
                            .Where(buddy => buddy.InRoom && buddy.UserId != Session.GetHabbo().Id)
                            .Where(buddy => !MyFriendsRooms.Contains(buddy.CurrentRoom.RoomData)))
                        MyFriendsRooms.Add(buddy.CurrentRoom.RoomData);

                    Message.WriteInteger(MyFriendsRooms.Count);
                    foreach (var Data in MyFriendsRooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;

                case NavigatorCategoryType.MY_RIGHTS:
                    var MyRights = new List<RoomData>();

                    using (var dbClient = OblivionServer.GetDatabaseManager().GetQueryReactor())
                    {
                        dbClient.SetQuery(
                            "SELECT `room_id` FROM `room_rights` WHERE `user_id` = @UserId LIMIT @FetchLimit");
                        dbClient.AddParameter("UserId", Session.GetHabbo().Id);
                        dbClient.AddParameter("FetchLimit", FetchLimit);
                        var GetRights = dbClient.getTable();

                        foreach (var Data in from DataRow Row in GetRights.Rows
                            select
                            OblivionServer.GetGame().GetRoomManager().GenerateRoomData(Convert.ToInt32(Row["room_id"]))
                            into Data
                            where Data != null
                            where !MyRights.Contains(Data)
                            select Data)
                            MyRights.Add(Data);
                    }

                    Message.WriteInteger(MyRights.Count);
                    foreach (var Data in MyRights.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;

                case NavigatorCategoryType.TOP_PROMOTIONS:
                {
                    var GetPopularPromotions = OblivionServer.GetGame()
                        .GetRoomManager()
                        .GetOnGoingRoomPromotions(16, FetchLimit);

                    Message.WriteInteger(GetPopularPromotions.Count);
                    foreach (var Data in GetPopularPromotions.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;
                }

                case NavigatorCategoryType.PROMOTION_CATEGORY:
                {
                    var GetPromotedRooms = OblivionServer.GetGame()
                        .GetRoomManager()
                        .GetPromotedRooms(SearchResult.Id, FetchLimit);

                    Message.WriteInteger(GetPromotedRooms.Count);
                    foreach (var Data in GetPromotedRooms.ToList())
                        RoomAppender.WriteRoom(Message, Data, Data.Promotion);
                    break;
                }
            }
        }
    }
}