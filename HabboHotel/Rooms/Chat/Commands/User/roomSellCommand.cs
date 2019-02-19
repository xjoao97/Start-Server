#region

using System.Linq;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User
{
    internal class RoomSellCommand : IChatCommand
    {
        public string PermissionRequired => "command_sss";

        public string Parameters => "%cost%";

        public string Description => "Permite você vender seu quarto por um certo preço [Ex. 1000c OR 200d]";

        public void Execute(GameClient session, string[] Params)
        {
            var currentRoom = session.GetHabbo().CurrentRoom;
            //Gets the current RoomUser
            var user = currentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);

            //If it grabs an invalid user somehow, it returns
            if (user == null)
                return;


            //If does not have any value or the length does not contain a number with letter
            if (Params.Length == 1 || Params[1].Length < 2)
            {
                session.SendWhisper("Por favor, insira o valor do quarto [Ex. 145000c OR 75000d]", 3);
                return;
            }

            //Assigns the input to a variable
            var actualInput = Params[1];

            //If they are not the current room owner
            if (currentRoom.RoomData.OwnerId != user.HabboId)
            {
                session.SendWhisper("Você precisa ser o proprietário!", 3);
                return;
            }
            var roomCostType = actualInput.Substring(actualInput.Length - 1);
            //Declares the variable to be assigned in the try statement if they entered a valid cost
            int roomCost;
            try
            {
                //Great! It's valid if it passes this try
                roomCost = int.Parse(actualInput.Substring(0, actualInput.Length - 1));
            }
            catch
            {
                //Nope, Invalid integer
                user.GetClient().SendWhisper("Você precisa inserir um custo válido para o quarto.", 3);
                return;
            }

            //Forget it, no longer for sale
            if (roomCost == 0)
                return;

            //Is the cost too low or too high?
            if (roomCost < 1 || roomCost > 10000000)
            {
                user.GetClient().SendWhisper("Custo de quarto inválido, muito baixo ou muito alto.", 3);
                return;
            }

            //If the input is coins or diamonds, then it will set the room for sale.
            if (actualInput.EndsWith("c") || actualInput.EndsWith("d"))
            {
                currentRoom.RoomData.RoomForSale = true;
                currentRoom.RoomData.RoomSaleCost = roomCost;
                currentRoom.RoomData.RoomSaleType = roomCostType;
            }
            else
            {
                session.SendWhisper("Preço inválido [Ex. 600c OR 400d]", 3);
                return;
            }

            //Whispers to all of the room users that the room is for sale
            //RoomSellCommand Updated By Hamada Zipto
            foreach (var userInRoom in currentRoom.GetRoomUserManager().GetRoomUsers().ToList())
                userInRoom?.GetClient()?
                    .SendWhisper(
                        "Este quarto está a venda paor " + roomCost + roomCostType + " digite :buyroom " + roomCost +
                        roomCostType + " para comprá-lo.", 5);
        }
    }
}