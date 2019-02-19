#region

using Oblivion.Communication.Packets.Outgoing.Notifications;
using Oblivion.Communication.Packets.Outgoing.Rooms.Engine;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.Chat.Commands.User.Fun
{
    internal class PetCommand : IChatCommand
    {
        public string PermissionRequired => "command_pet";

        public string Parameters => "";

        public string Description => "Permite que você se torne um pet...";

        public void Execute(GameClient session, string[] Params)
        {
            var roomUser = session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(session.GetHabbo().Id);
            if (roomUser == null)
                return;

            if (Params.Length == 1)
            {
                session.SendWhisper(
                    "Opa, você se esqueceu de escolher o tipo de pet que você gostaria de se transformar em! Use :pet list para ver os pets disponíveis!");
                return;
            }

            if (Params[1].ToLower() == "list")
            {
                session.SendMessage(new MotdNotificationComposer(
                    "Esta é a lista de pets disponíveis para você usar: \n Hiddo\n Cachorro\n Gato\n Terrier\n Croc\n Urso\n Porco\n Lion\n Rino\n Aranha\n Tartaruga\n Galinha\n Sapo\n Drago\n Macaco\n Cavalo\n Coelho\n Bebe1\n Bebe2\n Coruja\n Porquinho\n Pikachu\n Lobo\n Pterossauro\n Mario\n Pedra\n Elefante\nVaca"));
                return;
            }

            var targetPetId = GetPetIdByString(Params[1]);
            if (targetPetId == 0)
            {
                session.SendWhisper("Este nome de pet não existe.");
                return;
            }

            //Change the users Pet Id.
            session.GetHabbo().PetId = targetPetId == -1 ? 0 : targetPetId;
            var room = session.GetHabbo().CurrentRoom;

            //Quickly remove the old user instance.
            room.SendMessage(new UserRemoveComposer(roomUser.VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            room.SendMessage(new UsersComposer(roomUser));

            //Tell them a quick message.
            if (session.GetHabbo().PetId > 0)
                session.SendWhisper("Use ':pet hiddo' para voltar ao normal!");
        }

        private static int GetPetIdByString(string pet)
        {
            switch (pet.ToLower())
            {
                default:
                    return 0;
                case "hiddo":
                case "habbo":
                    return -1;
                case "cachorro":
                    return 60; //This should be 0.
                case "gato":
                    return 1;
                case "terrier":
                    return 2;
                case "croc":
                case "croco":
                    return 3;
                case "urso":
                    return 4;
                case "porco":
                case "pig":
                    return 5;
                case "lion":
                case "leão":
                    return 6;
                case "rino":
                    return 7;
                case "aranha":
                    return 8;
                case "tartaruga":
                case "tuga":
                    return 9;
                case "chick":
                case "galinha":
                    return 10;
                case "sapo":
                    return 11;
                case "dragon":
                case "drago":
                    return 12;
                case "macaco":
                    return 14;
                case "cavalo":
                    return 15;
                case "coelho":
                    return 17;
                case "pigeon":
                    return 21;
                case "demon":
                    return 23;
                case "gnome":
                    return 26;
                case "bebe1":
                    return 34;
                case "bebe2":
                    return 35;
                case "coruja":
                    return 36;
                case "lobo":
                    return 37;
                case "porquinho":
                    return 38;
                case "pedra":
                    return 39;
                case "mew":
                    return 40;
                case "pikachu":
                    return 41;
                case "mario":
                    return 42;
                case "entei":
                    return 43;
                case "pterossauro":
                    return 44;
                case "elefante":
                    return 45;
                case "vaca":
                    return 46;
                case "velociraptor":
                    return 47;
            }
        }
    }
}