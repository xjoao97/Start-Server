#region

using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Oblivion.Core;
using Oblivion.HabboHotel.GameClients;

#endregion

namespace Oblivion.HabboHotel.Rooms.AI.Types.Events
{
    public class TextBot : BotAI
    {
        private readonly int _virtualId;
//        private int _actionTimer;
        private int _speechTimer;
        private int _minUsers = 0;
        private bool _eventgoing;
        private string _word;
        private string _winner;

        public TextBot(int virtualId)
        {
            _virtualId = virtualId;
        }

        public override void OnSelfEnterRoom()
        {
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }

        public override void OnUserEnterRoom(RoomUser User)
        {
            var room = GetRoom();
            var usersnow = room.UserCount;
            if (usersnow - 1 == 0)
            {
                BotConfig.Init("soletrando");
            }

            GetRoomUser()
                .Chat(
                    usersnow < _minUsers
                        ? $"Olá {User.GetUsername()}, aguarde os outros chegarem! :D"
                        : "Atenção: O Evento vai começar em alguns segundos!", false, GetBotData().ChatBubble);
        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
        }

        public override void OnUserSay(RoomUser User, string Message)
        {
            Message = HttpUtility.HtmlDecode(Message.ToLower());
            var data = Encoding.GetEncoding("utf-8").GetBytes(Message);
            Message = Encoding.UTF8.GetString(data);



            if (_eventgoing)
            {
                if (Message == _word)
                    return;

                var spaced = string.Join(string.Empty, _word.Select((x, i) => i > 0 && i % 1 == 0
                    ? $" {x}"
                    : x.ToString())).ToLower();
                var split = string.Join(string.Empty, _word.Select((x, i) => i > 0 && i % 1 == 0
                    ? $"-{x}"
                    : x.ToString())).ToLower();

                if (User.GetUsername() == _winner)
                {
                    var tokick = GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Message);
                    if (tokick != null)
                    {
                        GetRoomUser().Chat("Usuário kikado!", false, 31);
                        GetRoom().GetRoomUserManager().RemoveUserFromRoom(tokick.GetClient(), true, true);
                        _winner = null;
                        _speechTimer = 2;
                        return;
                    }
                    GetRoomUser().Chat("Não encontrei esse usuário, digite novamente!", false, 31);
                    return;
                }

                if ((split == Message || spaced == Message) && _winner == null)
                {
                    _winner = User.GetUsername();
                    _word = null;
                    GetRoomUser().Chat($"Hey {_winner}, kike um.", false, 31);
                    return;
                }
            }
            else
            {
                if (Message == "começa" && User.GetClient().GetHabbo().Rank > 6)
                {
                    _eventgoing = true;
                    _speechTimer = 10;
                    return;
                }
                if (Message.ToLower().Contains("começa"))
                {
                    var room = GetRoom();
                    var usersneed = _minUsers - room.UserCount;
                    GetRoomUser()
                        .Chat(
                            usersneed > 0
                                ? $"Ainda faltam {usersneed} usuários para começar!"
                                : "O evento começara em alguns minutos, aguarde", true, 31);
                    return;
                }
            }
        }

        public override void OnUserShout(RoomUser User, string Message) => OnUserSay(User, Message);

        public override void OnTimerTick()
        {
            if (GetBotData() == null)
                return;
            var room = GetRoom();

            var user = GetRoomUser();

            if (_speechTimer <= 0)
            {
                if (!_eventgoing)
                {
                    if (room.UserCount > _minUsers)
                    {
                        new Thread(() =>
                        {
                            _eventgoing = true; //here we start
                            _speechTimer = 300;
                            user.Chat("Olá a todos, o meu nome é Baianin, sou o bot oficial de eventos!", false, 31);
                            Thread.Sleep(3000);
                            user.Chat("O evento começará em alguns segundos!", false, 31);
                            Thread.Sleep(25000);
                            user.Chat("Quarto trancado, começando!", false, 31);
                            room.RoomData.Access = RoomAccess.DOORBELL;
                            Thread.Sleep(3000);
                            user.Chat("O Evento será: Soletrando! Instruções no bloco de notas (parede)", false, 31);
                            _speechTimer = 1;
                        }).Start();
                    }
                    _speechTimer = 8; //wait 8 secs and check again
                }
                else
                {
                    if (_word == null && _winner == null)
                    {
                        _word = BotConfig.GetRandomValue();
                        user.Chat($"A palavra é {_word}", true, 31);
                        return;
                    }
                }

                _speechTimer = 10; // wait + 10 secs to check again xdd
            }
            else
            {
                _speechTimer--;
            }
        }
    }
}