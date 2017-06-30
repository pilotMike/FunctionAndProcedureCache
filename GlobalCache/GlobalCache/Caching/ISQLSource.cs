using System.Data;

namespace GlobalCache.Caching
{
    public interface ISQLSource
    {
        DataTable GetSPDataTable(string procedureName, object[] parameters);
        object GetSPField(string procedureName, object[] parameters);
    }
}
