#region

using System.Linq;
using System.Threading;
using Oblivion.HabboHotel.GameClients;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Pathfinding;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.Moderator
{
    internal class FloorTileCommand : IChatCommand
    {
        public string PermissionRequired => "command_floortile";

        public string Parameters => "%rule%";

        public string Description => "Fill room tile.";

        public void Execute(GameClient Session, string[] Params)
        {
            /*
             * Stand above an item to set as main item. Do not use two items.
             * 
             * Commands Examples:
             * :floortile fill even -> Fills every even tile from the floor
             * :floortile remove all -> remove (DELETE) all floor items.
             * :floortile remove even -> remove (DELETE) all floor items in even positions.
             * :floortile fill all -> Fills every floor tile with the item
             */

            if (Params.Length <= 1)
                return;

            var action = Params[1];

            var mod_empty_tile_only = false;

            var rules = Params.Skip(1);

            if (rules.Contains("empty_tile_only"))
                mod_empty_tile_only = true;
            var Room = Session.GetHabbo().CurrentRoom;

            var RoomUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (action == "set")
            {
                RoomUser.SetLocation = new Vector2D(RoomUser.X, RoomUser.Y);
                Session.SendWhisper("Local selecionado para a clonagem. Só adicione os ítens neste local! X: " +
                                    RoomUser.X + " / Y: " + RoomUser.Y);
                return;
            }
            if (action == "unset")
            {
                RoomUser.SetLocation = null;
                Session.SendWhisper("O local para a clonagem agora é aonde o seu personagem está. X: " + RoomUser.X +
                                    " / Y: " + RoomUser.Y);
                return;
            }

            var squareItems = RoomUser.SetLocation != null
                ? Room.GetGameMap().GetRoomItemForSquare(RoomUser.SetLocation)
                : Room.GetGameMap().GetRoomItemForSquare(RoomUser.X, RoomUser.Y);

            var maxX = Room.GetGameMap().getDynamicRoomModel().MapSizeX;
            var maxY = Room.GetGameMap().getDynamicRoomModel().MapSizeY;

            if (squareItems.Count > 0)
                new Thread(() =>
                {
                    switch (action)
                    {
                        case "all":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if (Room.GetGameMap().itemCanBePlacedHere(x, y) &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any()))
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "rest":
                            for (var x = RoomUser.X; x < maxX; x++)
                            for (var y = RoomUser.Y; y < maxY; y++)
                                if (Room.GetGameMap().itemCanBePlacedHere(x, y) &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any()))
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "even":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if ((x % 2 == 0 && y % 2 != 0 || x % 2 != 0 && y % 2 == 0) &&
                                    Room.GetGameMap().itemCanBePlacedHere(x, y) &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any()))
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "odd":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if ((x % 2 != 0 && y % 2 != 0 || x % 2 == 0 && y % 2 == 0) &&
                                    Room.GetGameMap().itemCanBePlacedHere(x, y) &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any()))
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "border":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if (Room.GetGameMap().itemCanBePlacedHere(x, y)
                                    &&
                                    Room.GetGameMap().getItemsFromSquare(x, y)
                                    &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any())
                                )
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "border_even":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if ((x % 2 == 0 && y % 2 != 0 || x % 2 != 0 && y % 2 == 0) &&
                                    Room.GetGameMap().itemCanBePlacedHere(x, y)
                                    &&
                                    Room.GetGameMap().getItemsFromSquare(x, y)
                                    &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any())
                                )
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        case "border_odd":
                            for (var x = 0; x < maxX; x++)
                            for (var y = 0; y < maxY; y++)
                                if ((x % 2 != 0 && y % 2 != 0 || x % 2 == 0 && y % 2 == 0) &&
                                    Room.GetGameMap().itemCanBePlacedHere(x, y)
                                    &&
                                    Room.GetGameMap().getItemsFromSquare(x, y)
                                    &&
                                    (!mod_empty_tile_only || !Room.GetGameMap().GetRoomItemForSquare(x, y).Any())
                                )
                                    foreach (var item in squareItems)
                                    {
                                        var s = ItemFactory.CreateSingleItem(item.Data, Session.GetHabbo(),
                                            item.ExtraData, item.ExtraData);
                                        var RoomItem = new Item(s.Id, Room.RoomId, s.BaseItem, s.ExtraData, x, y, 0,
                                            s.Rotation, Session.GetHabbo().Id, s.GroupId, s.LimitedNo, s.LimitedTot,
                                            string.Empty, Room);
                                        Room.GetRoomItemHandler()
                                            .SetFloorItem(Session, RoomItem, x, y, s.Rotation, true, false, true);
                                    }
                            break;
                        default:
                            Session.SendNotification(
                                "Opções disponíveis:\n :tile set \n :tile unset \n :tile border \n :tile odd \n :tile even \n :tile all \n :tile border_odd \n :tile boder_even");
                            break;
                    }
                }).Start();
        }
    }
}