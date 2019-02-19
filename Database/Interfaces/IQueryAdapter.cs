#region

using System;

#endregion

namespace Oblivion.Database.Interfaces
{
    public interface IQueryAdapter : IRegularQueryAdapter, IDisposable
    {
        long InsertQuery();
        void RunQuery();
    }
}