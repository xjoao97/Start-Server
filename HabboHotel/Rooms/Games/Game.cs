#region

using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Linq;
using Oblivion.HabboHotel.Items;
using Oblivion.HabboHotel.Rooms.Games.Teams;

#endregion

namespace Oblivion.HabboHotel.Rooms.Games
{
    public class GameManager
    {
        private ConcurrentDictionary<int, Item> _blueTeamItems;
        private ConcurrentDictionary<int, Item> _greenTeamItems;
        private ConcurrentDictionary<int, Item> _redTeamItems;
        private Room _room;
        private ConcurrentDictionary<int, Item> _yellowTeamItems;

        public GameManager(Room room)
        {
            _room = room;
            Points = new int[5];

            _redTeamItems = new ConcurrentDictionary<int, Item>();
            _blueTeamItems = new ConcurrentDictionary<int, Item>();
            _greenTeamItems = new ConcurrentDictionary<int, Item>();
            _yellowTeamItems = new ConcurrentDictionary<int, Item>();
        }

        public int[] Points { get; set; }

        public TEAM GetWinningTeam()
        {
            var winning = 1;
            var highestScore = 0;

            for (var i = 1; i < 5; i++)
                if (Points[i] > highestScore)
                {
                    highestScore = Points[i];
                    winning = i;
                }
            return (TEAM) winning;
        }

        public void AddPointToTeam(TEAM team, int points)
        {
            var newPoints = Points[Convert.ToInt32(team)] += points;
            if (newPoints < 0)
                newPoints = 0;

            Points[Convert.ToInt32(team)] = newPoints;

            foreach (
                var item in
                GetFurniItems(team).Values.ToList().Where(item => !IsFootballGoal(item.GetBaseItem().InteractionType)))
            {
                item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                item.UpdateState();
            }

            foreach (var item in _room.GetRoomItemHandler().GetFloor.ToList())
                if (team == TEAM.BLUE && item.Data.InteractionType == InteractionType.Banzaiscoreblue)
                {
                    item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                    item.UpdateState();
                }
                else if (team == TEAM.RED && item.Data.InteractionType == InteractionType.Banzaiscorered)
                {
                    item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                    item.UpdateState();
                }
                else if (team == TEAM.GREEN && item.Data.InteractionType == InteractionType.Banzaiscoregreen)
                {
                    item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                    item.UpdateState();
                }
                else if (team == TEAM.YELLOW && item.Data.InteractionType == InteractionType.Banzaiscoreyellow)
                {
                    item.ExtraData = Points[Convert.ToInt32(team)].ToString();
                    item.UpdateState();
                }
        }

        public void Reset()
        {
            AddPointToTeam(TEAM.BLUE, GetScoreForTeam(TEAM.BLUE) * -1);
            AddPointToTeam(TEAM.GREEN, GetScoreForTeam(TEAM.GREEN) * -1);
            AddPointToTeam(TEAM.RED, GetScoreForTeam(TEAM.RED) * -1);
            AddPointToTeam(TEAM.YELLOW, GetScoreForTeam(TEAM.YELLOW) * -1);
        }

        private int GetScoreForTeam(TEAM team) => Points[Convert.ToInt32(team)];

        private ConcurrentDictionary<int, Item> GetFurniItems(TEAM team)
        {
            switch (team)
            {
                default:
                    return new ConcurrentDictionary<int, Item>();
                case TEAM.BLUE:
                    return _blueTeamItems;
                case TEAM.GREEN:
                    return _greenTeamItems;
                case TEAM.RED:
                    return _redTeamItems;
                case TEAM.YELLOW:
                    return _yellowTeamItems;
            }
        }

        private static bool IsFootballGoal(InteractionType type)
            => type == InteractionType.FootballGoalBlue || type == InteractionType.FootballGoalGreen ||
               type == InteractionType.FootballGoalRed || type == InteractionType.FootballGoalYellow;

        public void AddFurnitureToTeam(Item item, TEAM team)
        {
            switch (team)
            {
                case TEAM.BLUE:
                    _blueTeamItems.TryAdd(item.Id, item);
                    break;
                case TEAM.GREEN:
                    _greenTeamItems.TryAdd(item.Id, item);
                    break;
                case TEAM.RED:
                    _redTeamItems.TryAdd(item.Id, item);
                    break;
                case TEAM.YELLOW:
                    _yellowTeamItems.TryAdd(item.Id, item);
                    break;
            }
        }

        public void RemoveFurnitureFromTeam(Item item, TEAM team)
        {
            switch (team)
            {
                case TEAM.BLUE:
                    _blueTeamItems.TryRemove(item.Id, out item);
                    break;
                case TEAM.GREEN:
                    _greenTeamItems.TryRemove(item.Id, out item);
                    break;
                case TEAM.RED:
                    _redTeamItems.TryRemove(item.Id, out item);
                    break;
                case TEAM.YELLOW:
                    _yellowTeamItems.TryRemove(item.Id, out item);
                    break;
            }
        }

        public void StopGame() => _room.lastTimerReset = DateTime.Now;

        public void Dispose()
        {
            Array.Clear(Points, 0, Points.Length);
            _redTeamItems.Clear();
            _blueTeamItems.Clear();
            _greenTeamItems.Clear();
            _yellowTeamItems.Clear();

            Points = null;
            _redTeamItems = null;
            _blueTeamItems = null;
            _greenTeamItems = null;
            _yellowTeamItems = null;
            _room = null;
        }

        #region Gates

        public void LockGates()
        {
            foreach (var item in _redTeamItems.Values.ToList())
                LockGate(item);

            foreach (var item in _greenTeamItems.Values.ToList())
                LockGate(item);

            foreach (var item in _blueTeamItems.Values.ToList())
                LockGate(item);

            foreach (var item in _yellowTeamItems.Values.ToList())
                LockGate(item);
        }

        public void UnlockGates()
        {
            foreach (var item in _redTeamItems.Values.ToList())
                UnlockGate(item);

            foreach (var item in _greenTeamItems.Values.ToList())
                UnlockGate(item);

            foreach (var item in _blueTeamItems.Values.ToList())
                UnlockGate(item);

            foreach (var item in _yellowTeamItems.Values.ToList())
                UnlockGate(item);
        }

        private void LockGate(Item item)
        {
            var type = item.GetBaseItem().InteractionType;
            if (type == InteractionType.FreezeBlueGate || type == InteractionType.FreezeGreenGate ||
                type == InteractionType.FreezeRedGate || type == InteractionType.FreezeYellowGate
                || type == InteractionType.Banzaigateblue || type == InteractionType.Banzaigatered ||
                type == InteractionType.Banzaigategreen || type == InteractionType.Banzaigateyellow)
            {
                foreach (var user in _room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                    user.SqState = 0;
                _room.GetGameMap().GameMap[item.GetX, item.GetY] = 0;
            }
        }

        private void UnlockGate(Item item)
        {
            var type = item.GetBaseItem().InteractionType;
            if (type == InteractionType.FreezeBlueGate || type == InteractionType.FreezeGreenGate ||
                type == InteractionType.FreezeRedGate || type == InteractionType.FreezeYellowGate
                || type == InteractionType.Banzaigateblue || type == InteractionType.Banzaigatered ||
                type == InteractionType.Banzaigategreen || type == InteractionType.Banzaigateyellow)
            {
                foreach (var user in _room.GetGameMap().GetRoomUsers(new Point(item.GetX, item.GetY)))
                    user.SqState = 1;
                _room.GetGameMap().GameMap[item.GetX, item.GetY] = 1;
            }
        }

        #endregion
    }
}