using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace Sqlite3Plugin
{
    public class Query : IDisposable
    {
        protected DBProxy _proxy;

        protected IntPtr _stmt = IntPtr.Zero;

        public Query(DBProxy proxy, string sql)
        {
            _Setup(proxy, sql);
        }

        protected void _Setup(DBProxy proxy, string sql)
        {
            _proxy = proxy;
            byte[] bytes = Encoding.UTF8.GetBytes(sql);
            IntPtr ppStmt;
            int num = Sqlite3LibImport.sqlite3_prepare_v2(proxy.DBHandle, bytes, bytes.Length, out ppStmt, IntPtr.Zero);
            if (num != 0)
            {
                throw new Exception($"sqlite3_prepare_v2 failed({num}) with sql: {sql}");
            }
            _stmt = ppStmt;
        }

        public virtual void Dispose()
        {
            if (_stmt != IntPtr.Zero)
            {
                int num = Sqlite3LibImport.sqlite3_finalize(_stmt);
                if (num != 0)
                {
                    Debug.LogError("sqlite3_finalize error: " + num);
                }
                _stmt = IntPtr.Zero;
            }
        }

        public bool Step()
        {
            return Sqlite3LibImport.sqlite3_step(_stmt) == 100;
        }

        public bool Exec()
        {
            return Sqlite3LibImport.sqlite3_step(_stmt) == 101;
        }

        public int GetInt(int idx)
        {
            return Sqlite3LibImport.sqlite3_column_int(_stmt, idx);
        }

        public double GetDouble(int idx)
        {
            return Sqlite3LibImport.sqlite3_column_double(_stmt, idx);
        }

        public string GetText(int idx)
        {
            return Marshal.PtrToStringAnsi(Sqlite3LibImport.sqlite3_column_text(_stmt, idx));
        }

        public byte[] GetBlob(int idx)
        {
            int num = Sqlite3LibImport.sqlite3_column_bytes(_stmt, idx);
            if (num == 0)
            {
                Debug.LogError("null blob at idx: " + idx);
                return null;
            }
            IntPtr source = Sqlite3LibImport.sqlite3_column_blob(_stmt, idx);
            byte[] array = new byte[num];
            Marshal.Copy(source, array, 0, num);
            return array;
        }

        public bool IsNull(int idx)
        {
            return Sqlite3LibImport.sqlite3_column_type(_stmt, idx) == 5;
        }
    }
}
