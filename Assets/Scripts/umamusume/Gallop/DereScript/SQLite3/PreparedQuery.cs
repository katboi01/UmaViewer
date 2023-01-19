using System;
using System.Text;
using UnityEngine;

namespace Sqlite3Plugin
{
    public class PreparedQuery : Query
    {
        public PreparedQuery(DBProxy proxy, string sql)
            : base(proxy, sql)
        {
        }

        public bool BindText(int idx, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            int num = Sqlite3LibImport.sqlite3_bind_text(_stmt, idx, bytes, bytes.Length, IntPtr.Zero);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_bind_text error at idx {idx}: {num}");
            }
            return num == 0;
        }

        public bool BindBlob(int idx, byte[] byteArray)
        {
            int num = Sqlite3LibImport.sqlite3_bind_blob(_stmt, idx, byteArray, byteArray.Length, IntPtr.Zero);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_bind_blob error at idx {idx}: {num}");
            }
            return num == 0;
        }

        public bool BindInt(int idx, int iValue)
        {
            int num = Sqlite3LibImport.sqlite3_bind_int(_stmt, idx, iValue);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_bind_int error at idx {idx}: {num}");
            }
            return num == 0;
        }

        public bool BindDouble(int idx, double rValue)
        {
            int num = Sqlite3LibImport.sqlite3_bind_double(_stmt, idx, rValue);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_bind_double error at idx {idx}: {num}");
            }
            return num == 0;
        }

        public bool Reset()
        {
            int num = Sqlite3LibImport.sqlite3_reset(_stmt);
            if (num != 0)
            {
                Debug.LogError($"sqlite3_reset error: {num}");
            }
            return num == 0;
        }
    }
}
