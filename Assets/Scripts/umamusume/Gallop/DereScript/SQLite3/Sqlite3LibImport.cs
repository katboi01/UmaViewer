using System;
using System.Runtime.InteropServices;

namespace Sqlite3Plugin
{

    public static class Sqlite3LibImport
    {
        private const string LibraryName = "libsqlcipher";

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_open(byte[] zFilename, out IntPtr ppDB);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_open_v2(byte[] zFilename, out IntPtr ppDB, int flags, IntPtr zVfs);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_close(IntPtr db);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_exec(IntPtr db, byte[] zSql, IntPtr xCallback, IntPtr pArg, out byte[] pzErrMsg);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_prepare_v2(IntPtr db, byte[] zSql, int nBytes, out IntPtr ppStmt, IntPtr pzTail);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_bind_text(IntPtr pStmt, int i, byte[] zData, int nData, IntPtr xDel);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_bind_blob(IntPtr pStmt, int i, byte[] zData, int nData, IntPtr xDel);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_bind_int(IntPtr pStmt, int i, int iValue);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_bind_double(IntPtr pStmt, int i, double rValue);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_bind_null(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_reset(IntPtr pStmt);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_step(IntPtr pStmt);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_finalize(IntPtr pStmt);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_column_int(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern double sqlite3_column_double(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_column_bytes(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern IntPtr sqlite3_column_blob(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern IntPtr sqlite3_column_text(IntPtr pStmt, int i);

        [DllImport("libsqlcipher")]
        public static extern int sqlite3_column_type(IntPtr pStmt, int i);
    }
}
