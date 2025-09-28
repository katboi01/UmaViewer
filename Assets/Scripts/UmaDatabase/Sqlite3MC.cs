// Sqlite3MC.cs
// Unity C# wrapper for sqlite3mc_x64.dll (Windows x64).
// Put sqlite3mc_x64.dll into Assets/Plugins (set platform to Windows x86_64).

using System;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class Sqlite3MC
{
    // ---- DLL name: change if you use different file name / arch ----
    private const string DLL = "sqlite3mc_x64"; // ensure sqlite3mc_x64.dll is available for x64 build

    // ---- Common SQLite constants ----
    public const int SQLITE_OK = 0;
    public const int SQLITE_ROW = 100;
    public const int SQLITE_DONE = 101;
    public const int SQLITE_OPEN_READWRITE = 0x00000002;
    public const int SQLITE_OPEN_CREATE = 0x00000004;

    // ---- Native Imports ----
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_open_v2")]
    private static extern int sqlite3_open_v2([MarshalAs(UnmanagedType.LPStr)] string filename, out IntPtr ppDb, int flags, IntPtr zVfs);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_close")]
    private static extern int sqlite3_close(IntPtr db);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_errmsg")]
    private static extern IntPtr sqlite3_errmsg_ptr(IntPtr db);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_exec")]
    private static extern int sqlite3_exec(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string sql, IntPtr callback, IntPtr arg, out IntPtr errMsg);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_free")]
    private static extern void sqlite3_free(IntPtr ptr);

    // sqlite3mc plugin-specific
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3mc_config")]
    private static extern int sqlite3mc_config(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string paramName, int newValue);

    // Key: provide both byte[] and IntPtr signatures (use whichever convenient)
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_key")]
    private static extern int sqlite3_key_bytes(IntPtr db, byte[] pKey, int nKey);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_key")]
    private static extern int sqlite3_key_ptr(IntPtr db, IntPtr pKey, int nKey);

    // Prepare/step/finalize
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_prepare_v2")]
    private static extern int sqlite3_prepare_v2(IntPtr db, [MarshalAs(UnmanagedType.LPStr)] string zSql, int nByte, out IntPtr ppStmt, IntPtr pzTail);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_step")]
    private static extern int sqlite3_step(IntPtr stmt);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_finalize")]
    private static extern int sqlite3_finalize(IntPtr stmt);

    // Column accessors
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_text")]
    private static extern IntPtr sqlite3_column_text(IntPtr stmt, int iCol);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_blob")]
    private static extern IntPtr sqlite3_column_blob(IntPtr stmt, int iCol);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_bytes")]
    private static extern int sqlite3_column_bytes(IntPtr stmt, int iCol);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int")]
    private static extern int sqlite3_column_int(IntPtr stmt, int iCol);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_column_int64")]
    private static extern long sqlite3_column_int64(IntPtr stmt, int iCol);

    // Binders
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_text")]
    private static extern int sqlite3_bind_text(IntPtr stmt, int idx, byte[] text, int n, IntPtr destructor);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_blob")]
    private static extern int sqlite3_bind_blob(IntPtr stmt, int idx, byte[] blob, int n, IntPtr destructor);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int")]
    private static extern int sqlite3_bind_int(IntPtr stmt, int idx, int val);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_bind_int64")]
    private static extern int sqlite3_bind_int64(IntPtr stmt, int idx, long val);

    // Backup API
    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_backup_init")]
    private static extern IntPtr sqlite3_backup_init(IntPtr pDest, [MarshalAs(UnmanagedType.LPStr)] string zDestName, IntPtr pSource, [MarshalAs(UnmanagedType.LPStr)] string zSourceName);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_backup_step")]
    private static extern int sqlite3_backup_step(IntPtr backup, int nPage);

    [DllImport(DLL, CallingConvention = CallingConvention.Cdecl, EntryPoint = "sqlite3_backup_finish")]
    private static extern int sqlite3_backup_finish(IntPtr backup);

    // ---- Helper: convert native C string (UTF-8) to C# string ----
    private static string PtrToStringUTF8(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero) return null;
        // find length
        int len = 0;
        while (Marshal.ReadByte(ptr, len) != 0) len++;
        if (len == 0) return string.Empty;
        byte[] buffer = new byte[len];
        Marshal.Copy(ptr, buffer, 0, len);
        return Encoding.UTF8.GetString(buffer);
    }

    // Get error message for DB handle
    public static string GetErrMsg(IntPtr db)
    {
        IntPtr p = sqlite3_errmsg_ptr(db);
        return PtrToStringUTF8(p) ?? $"(null errmsg, db={db})";
    }

    // ---- High-level wrappers ----

    /// <summary>Open a database. Throws Exception on failure. Flags default to read-write | create.</summary>
    public static IntPtr Open(string path, int flags = SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE)
    {
        int rc = sqlite3_open_v2(path, out IntPtr db, flags, IntPtr.Zero);
        if (rc != SQLITE_OK || db == IntPtr.Zero)
        {
            string em = db != IntPtr.Zero ? GetErrMsg(db) : "(no db handle)";
            throw new InvalidOperationException($"sqlite3_open_v2('{path}') failed rc={rc} errmsg={em}");
        }
        return db;
    }

    /// <summary>Close DB. Returns rc.</summary>
    public static int Close(IntPtr db)
    {
        if (db == IntPtr.Zero) return SQLITE_OK;
        return sqlite3_close(db);
    }

    /// <summary>Execute SQL (no binder). Return code; if errorMsg != null it contains message (freed automatically).</summary>
    public static int Exec(IntPtr db, string sql, out string errorMsg)
    {
        errorMsg = null;
        int rc = sqlite3_exec(db, sql, IntPtr.Zero, IntPtr.Zero, out IntPtr errPtr);
        if (errPtr != IntPtr.Zero)
        {
            errorMsg = PtrToStringUTF8(errPtr);
            // free the error string allocated by sqlite3_exec
            sqlite3_free(errPtr);
        }
        return rc;
    }

    /// <summary>Set sqlite3mc config parameter (plugin). e.g. sqlite3mc_config(db, "cipher", index).</summary>
    public static int MC_Config(IntPtr db, string paramName, int newValue)
    {
        return sqlite3mc_config(db, paramName, newValue);
    }

    /// <summary>Set key from raw bytes.</summary>
    public static int Key_SetBytes(IntPtr db, byte[] keyBytes)
    {
        if (keyBytes == null) return sqlite3_key_bytes(db, null, 0);
        return sqlite3_key_bytes(db, keyBytes, keyBytes.Length);
    }

    /// <summary>Set key from pointer (unsafe). Rarely needed.</summary>
    public static int Key_SetPtr(IntPtr db, IntPtr pKey, int len)
    {
        return sqlite3_key_ptr(db, pKey, len);
    }

    /// <summary>Validate whether DB can be read (try SELECT on sqlite_master).</summary>
    public static bool ValidateReadable(IntPtr db, out string errmsg)
    {
        int rc = Exec(db, "SELECT name FROM sqlite_master LIMIT 1;", out errmsg);
        return rc == SQLITE_OK;
    }

    // Prepare / Step utility: simple single-row single-column string fetch
    public static string QuerySingleString(IntPtr db, string sql)
    {
        int rc = sqlite3_prepare_v2(db, sql, -1, out IntPtr stmt, IntPtr.Zero);
        if (rc != SQLITE_OK) throw new InvalidOperationException($"prepare failed rc={rc} errmsg={GetErrMsg(db)} sql={sql}");
        try
        {
            rc = sqlite3_step(stmt);
            if (rc == SQLITE_ROW)
            {
                IntPtr txt = sqlite3_column_text(stmt, 0);
                return PtrToStringUTF8(txt);
            }
            else if (rc == SQLITE_DONE)
            {
                return null;
            }
            else
            {
                throw new InvalidOperationException($"step failed rc={rc} errmsg={GetErrMsg(db)}");
            }
        }
        finally
        {
            sqlite3_finalize(stmt);
        }
    }

    // Helper: prepare/step loop with callback
    public delegate void RowCallback(IntPtr stmt);
    public static void ForEachRow(string sql, IntPtr db, RowCallback rowCallback)
    {
        int rc = sqlite3_prepare_v2(db, sql, -1, out IntPtr stmt, IntPtr.Zero);
        if (rc != SQLITE_OK)
            throw new InvalidOperationException($"prepare failed rc={rc} errmsg={GetErrMsg(db)} sql={sql}");

        try
        {
            while (true)
            {
                rc = sqlite3_step(stmt);
                if (rc == SQLITE_ROW)
                {
                    rowCallback(stmt);
                }
                else if (rc == SQLITE_DONE)
                {
                    break;
                }
                else
                {
                    throw new InvalidOperationException($"step failed rc={rc} errmsg={GetErrMsg(db)}");
                }
            }
        }
        finally
        {
            sqlite3_finalize(stmt);
        }
    }

    // Column getters (UTF-8 text or blob)
    public static string ColumnText(IntPtr stmt, int col)
    {
        IntPtr p = sqlite3_column_text(stmt, col);
        return PtrToStringUTF8(p);
    }

    public static byte[] ColumnBlob(IntPtr stmt, int col)
    {
        IntPtr p = sqlite3_column_blob(stmt, col);
        if (p == IntPtr.Zero) return null;
        int len = sqlite3_column_bytes(stmt, col);
        if (len <= 0) return new byte[0];
        byte[] buf = new byte[len];
        Marshal.Copy(p, buf, 0, len);
        return buf;
    }

    public static int ColumnInt(IntPtr stmt, int col) => sqlite3_column_int(stmt, col);
    public static long ColumnInt64(IntPtr stmt, int col) => sqlite3_column_int64(stmt, col);

    // Bind helpers (use SQLITE_TRANSIENT to let sqlite copy data)
    private static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);

    public static int BindText(IntPtr stmt, int idx, string text)
    {
        if (text == null) return sqlite3_bind_text(stmt, idx, null, 0, IntPtr.Zero);
        byte[] utf8 = Encoding.UTF8.GetBytes(text);
        return sqlite3_bind_text(stmt, idx, utf8, utf8.Length, SQLITE_TRANSIENT);
    }

    public static int BindBlob(IntPtr stmt, int idx, byte[] blob)
    {
        if (blob == null) return sqlite3_bind_blob(stmt, idx, null, 0, IntPtr.Zero);
        return sqlite3_bind_blob(stmt, idx, blob, blob.Length, SQLITE_TRANSIENT);
    }

    public static int BindInt(IntPtr stmt, int idx, int v) => sqlite3_bind_int(stmt, idx, v);
    public static int BindInt64(IntPtr stmt, int idx, long v) => sqlite3_bind_int64(stmt, idx, v);

    // Backup: copy entire main DB from source to destination path (destination will be created/opened by function)
    // Returns SQLITE_OK on success. Throws on fatal open errors.
    public static int BackupToFile(IntPtr srcDb, string dstPath, int pagesPerStep = 5)
    {
        // Open destination DB (readwrite | create)
        int rcOpen = sqlite3_open_v2(dstPath, out IntPtr dstDb, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE, IntPtr.Zero);
        if (rcOpen != SQLITE_OK)
        {
            throw new InvalidOperationException($"sqlite3_open_v2(dst) failed rc={rcOpen} errmsg={GetErrMsg(dstDb)}");
        }

        IntPtr backup = sqlite3_backup_init(dstDb, "main", srcDb, "main");
        if (backup == IntPtr.Zero)
        {
            int rcClose = sqlite3_close(dstDb);
            throw new InvalidOperationException($"sqlite3_backup_init returned NULL; errmsg(dst)={GetErrMsg(dstDb)} rcClose={rcClose}");
        }

        int rc = SQLITE_OK;
        try
        {
            while (true)
            {
                rc = sqlite3_backup_step(backup, pagesPerStep);
                if (rc == SQLITE_DONE)
                {
                    rc = sqlite3_backup_finish(backup);
                    break;
                }
                else if (rc == SQLITE_OK)
                {
                    // continue
                    continue;
                }
                else
                {
                    // error
                    rc = sqlite3_backup_finish(backup);
                    break;
                }
            }
        }
        finally
        {
            // ensure destination db closed
            sqlite3_close(dstDb);
        }
        return rc;
    }

    // Simple convenience: try to open source with key (raw bytes) and then backup to dstPath (unencrypted).
    // Throws exceptions on fatal errors; returns final sqlite rc from backup.
    public static int DecryptDbToPlainFile(string srcPath, string dstPath, byte[] keyBytes, int cipherIndex = -1)
    {
        IntPtr srcDb = IntPtr.Zero;
        try
        {
            srcDb = Open(srcPath, SQLITE_OPEN_READWRITE | SQLITE_OPEN_CREATE);

            if (cipherIndex >= 0)
            {
                int cfg = MC_Config(srcDb, "cipher", cipherIndex);
                // cfg may return various codes depending implementation; we don't treat non-zero as fatal here
            }

            int rcKey = Key_SetBytes(srcDb, keyBytes);
            if (rcKey != SQLITE_OK)
            {
                throw new InvalidOperationException($"sqlite3_key returned rc={rcKey} errmsg={GetErrMsg(srcDb)}");
            }

            if (!ValidateReadable(srcDb, out string vErr))
            {
                throw new InvalidOperationException($"DB did not validate after key. errmsg={vErr}");
            }

            int rcBackup = BackupToFile(srcDb, dstPath);
            return rcBackup;
        }
        finally
        {
            if (srcDb != IntPtr.Zero) sqlite3_close(srcDb);
        }
    }
}
