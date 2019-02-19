#region

using System;

#endregion

namespace Oblivion.HabboHotel.Users.Authenticator
{
    [Serializable]
    public class IncorrectLoginException : Exception
    {
        public IncorrectLoginException(string Reason) : base(Reason)
        {
        }
    }
}