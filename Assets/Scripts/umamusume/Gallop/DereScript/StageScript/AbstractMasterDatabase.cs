using Sqlite3Plugin;
using System;

namespace Stage
{
    public abstract class AbstractMasterDatabase
    {
        public abstract void Unload();

        public abstract Query Query(string sql);
    }
}
