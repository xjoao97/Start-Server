using System.Drawing;

namespace Oblivion.HabboHotel.Rooms.Games.Football
{
    public class ComeDirection
    {
        internal static Direction InverseDirections(Room room, Direction comeWith, int x, int y)
        {
            try
            {
                if (comeWith == Direction.Up)
                    return Direction.Down;
                if (comeWith == Direction.UpRight)
                    if (room.GetGameMap().StaticModel.SqState[x, y] == SquareState.BLOCKED)
                        return room.GetGameMap().StaticModel.SqState[x + 1, y] == SquareState.BLOCKED ? Direction.DownRight : Direction.UpLeft;
                    else
                        return Direction.DownRight;
                if (comeWith == Direction.Right)
                    return Direction.Left;
                if (comeWith == Direction.DownRight)
                    if (room.GetGameMap().StaticModel.SqState[x, y] == SquareState.BLOCKED)
                        return room.GetGameMap().StaticModel.SqState[x + 1, y] == SquareState.BLOCKED ? Direction.UpRight : Direction.DownLeft;
                    else
                        return Direction.UpRight;
                if (comeWith == Direction.Down)
                    return Direction.Up;
                if (comeWith == Direction.DownLeft)
                    return room.GetGameMap().Model.MapSizeX - 1 <= x ? Direction.DownRight : Direction.UpLeft;
                if (comeWith == Direction.Left)
                    return Direction.Right;
                if (comeWith == Direction.UpLeft)
                    return room.GetGameMap().Model.MapSizeX - 1 <= x ? Direction.UpRight : Direction.DownLeft;
                return Direction.Null;
            }
            catch
            {
                return Direction.Null;
            }
        }


        internal static Direction GetInverseDirectionEasy(Direction comeWith)
        {
            try
            {
                switch (comeWith)
                {
                    case Direction.Up:
                        return Direction.Down;
                    case Direction.UpRight:
                        return Direction.DownLeft;
                    case Direction.Right:
                        return Direction.Left;
                    case Direction.DownRight:
                        return Direction.UpLeft;
                    case Direction.Down:
                        return Direction.Up;
                    case Direction.DownLeft:
                        return Direction.UpRight;
                    case Direction.Left:
                        return Direction.Right;
                    case Direction.UpLeft:
                        return Direction.DownRight;
                }
                return Direction.Null;
            }
            catch
            {
                return Direction.Null;
            }
        }

        internal static void GetNewCoords(Direction comeWith, ref int newX, ref int newY)
        {
            try
            {
                switch (comeWith)
                {
                    case Direction.Up:
                        newY++;
                        break;
                    case Direction.UpRight:
                        newX--;
                        newY++;
                        break;
                    case Direction.Right:
                        newX--;
                        break;
                    case Direction.DownRight:
                        newX--;
                        newY--;
                        break;
                    case Direction.Down:
                        // newX = newX;
                        newY--;
                        break;
                    case Direction.DownLeft:
                        newX++;
                        newY--;
                        break;
                    case Direction.Left:
                        newX++;
                        break;
                    // newY = newY;
                    case Direction.UpLeft:
                        newX++;
                        newY++;
                        break;
                }
            }
            catch
            {
            }
        }

        internal static Direction GetComeDirection(Point user, Point ball)
        {
            try
            {
                if (user.X == ball.X && user.Y - 1 == ball.Y)
                    return Direction.Down;
                if (user.X + 1 == ball.X && user.Y - 1 == ball.Y)
                    return Direction.DownLeft;
                if (user.X + 1 == ball.X && user.Y == ball.Y)
                    return Direction.Left;
                if (user.X + 1 == ball.X && user.Y + 1 == ball.Y)
                    return Direction.UpLeft;
                if (user.X == ball.X && user.Y + 1 == ball.Y)
                    return Direction.Up;
                if (user.X - 1 == ball.X && user.Y + 1 == ball.Y)
                    return Direction.UpRight;
                if (user.X - 1 == ball.X && user.Y == ball.Y)
                    return Direction.Right;
                if (user.X - 1 == ball.X && user.Y - 1 == ball.Y)
                    return Direction.DownRight;
                return Direction.Null;
            }
            catch
            {
                return Direction.Null;
            }
        }
    }
}