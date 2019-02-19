#region

using System;
using Oblivion.Database.Interfaces;

#endregion

namespace Oblivion.Database.Adapter
{
    public class NormalQueryReactor : QueryAdapter, IQueryAdapter, IRegularQueryAdapter, IDisposable
    {
        public NormalQueryReactor(IDatabaseClient Client)
            : base(Client)
        {
            command = Client.createNewCommand();
        }

        public void Dispose()
        {
            command.Dispose();
            client.reportDone();
            GC.SuppressFinalize(this);
        }
    }
}