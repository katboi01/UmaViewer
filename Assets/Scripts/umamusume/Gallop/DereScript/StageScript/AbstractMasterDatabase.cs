using Sqlite3Plugin;

namespace Stage
{
    public abstract class AbstractMasterDatabase
    {
        public abstract void Unload();

        public abstract Query Query(string sql);
    }
}
