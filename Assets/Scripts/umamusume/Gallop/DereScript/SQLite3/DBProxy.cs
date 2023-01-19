using System;
using System.Text;
using UnityEngine;

namespace Sqlite3Plugin
{
    public class DBProxy : IDisposable
    {
        public IntPtr DBHandle { get; private set; }

        public string dbPath { get; private set; }

        public bool IsOpened()
        {
            return DBHandle != IntPtr.Zero;
        }

        ~DBProxy()
        {
            CloseDB();
        }

        public DBProxy()
        {
            dbPath = null;
            DBHandle = IntPtr.Zero;
        }

        public bool Open(string fileName)
        {
            IntPtr ppDB = IntPtr.Zero;
            int num = Sqlite3LibImport.sqlite3_open_v2(Encoding.UTF8.GetBytes(fileName + "\0"), out ppDB, 1, IntPtr.Zero);
            DBHandle = ppDB;
            bool num2 = num == 0;
            if (num2)
            {
                Exec("pragma journal_mode=OFF");
                Exec("pragma synchronous=0");
                Exec("pragma locking_mode=EXCLUSIVE");
            }
            else
            {
                Debug.LogError("sqlite3_open failed: " + num);
            }
            dbPath = fileName;
            return num2;
        }

        public bool OpenWritable(string fileName)
        {
            IntPtr ppDB = IntPtr.Zero;
            byte[] bytes = Encoding.UTF8.GetBytes(fileName + "\0");
            bool flag = true;
            try
            {
                int num = Sqlite3LibImport.sqlite3_open(bytes, out ppDB);
                DBHandle = ppDB;
                flag = num == 0;
                if (flag)
                {
                    Exec("pragma journal_mode=MEMORY");
                    Exec("pragma synchronous=1");
                    Exec("pragma locking_mode=EXCLUSIVE");
                }
                else
                {
                    Debug.LogError("sqlite3_open failed: " + num);
                }
                dbPath = fileName;
                return flag;
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                return false;
            }
        }

        public bool Begin()
        {
            return Exec("BEGIN;");
        }

        public bool Commit()
        {
            return Exec("COMMIT;");
        }

        public bool Rollback()
        {
            return Exec("ROLLBACK;");
        }

        public bool Vacuum()
        {
            return Exec("VACUUM;");
        }

        public virtual void Dispose()
        {
            CloseDB();
        }

        protected virtual void Dispose(bool disposing)
        {
            CloseDB();
        }

        private void Terminate()
        {
            CloseDB();
        }

        public void CloseDB()
        {
            if (DBHandle != IntPtr.Zero)
            {
                int num = Sqlite3LibImport.sqlite3_close(DBHandle);
                if (num != 0)
                {
                    Debug.LogError("failed to close db at " + dbPath + ": " + num);
                }
                DBHandle = IntPtr.Zero;
            }
        }

        public bool Exec(string sql)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(sql + "\0");
            byte[] pzErrMsg;
            int num = Sqlite3LibImport.sqlite3_exec(DBHandle, bytes, IntPtr.Zero, IntPtr.Zero, out pzErrMsg);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_exec failed({num}) with sql: {sql}");
            }
            return num == 0;
        }

        public Query Query(string sql)
        {
            return new Query(this, sql);
        }

        public PreparedQuery PreparedQuery(string sql)
        {
            return new PreparedQuery(this, sql);
        }
    }
}
