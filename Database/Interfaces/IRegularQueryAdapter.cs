#region

using System.Data;

#endregion

namespace Oblivion.Database.Interfaces
{
    public interface IRegularQueryAdapter
    {
        void AddParameter(string name, object query);
        bool findsResult();
        int getInteger();
        DataRow getRow();
        string getString();
        DataTable getTable();
        void runFastQuery(string query);
        void RunQuery(string query);
        void SetQuery(string query);
    }
}