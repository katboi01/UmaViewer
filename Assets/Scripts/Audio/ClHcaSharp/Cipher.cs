using System;

namespace ClHcaSharp
{
    internal static class Cipher
    {
        public static void Decrypt(byte[] cipherData, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = cipherData[data[i]];
            }
        }

        public static byte[] Init(int type, ulong keyCode)
        {
            if (type == 56 && keyCode == 0) type = 0;

            return type switch
            {
                0 => Init0(),
                1 => Init1(),
                56 => Init56(keyCode),
                _ => throw new ArgumentException("Invalid cipher type."),
            };
        }

        private static byte[] Init0()
        {
            byte[] cipherTable = new byte[256];

            for (int i = 0; i < 256; i++)
            {
                cipherTable[i] = (byte)i;
            }

            return cipherTable;
        }

        private static byte[] Init1()
        {
            byte[] cipherTable = new byte[256];

            const int mul = 13;
            const int add = 11;

            uint v = 0;
            for (int i = 1; i < 256 - 1; i++)
            {
                v = (v * mul + add) & 0xFF;
                if (v == 0 || v == 0xFF)
                    v = (v * mul + add) & 0xFF;
                cipherTable[i] = (byte)v;
            }
            cipherTable[0] = 0;
            cipherTable[0xFF] = 0xFF;

            return cipherTable;
        }

        private static void Init56CreateTable(byte[] table, byte key)
        {
            int mul = ((key & 1) << 3) | 5;
            int add = (key & 0xE) | 1;

            key >>= 4;

            for (int i = 0; i < 16; i++)
            {
                key = (byte)((key * mul + add) & 0xF);
                table[i] = key;
            }
        }

        private static byte[] Init56(ulong keyCode)
        {
            byte[] cipherTable = new byte[256];

            byte[] kc = new byte[8];
            byte[] seed = new byte[16];
            byte[] baseTable = new byte[256];
            byte[] baseTableR = new byte[16];
            byte[] baseTableC = new byte[16];

            if (keyCode != 0) keyCode--;

            for (int r = 0; r < (8 - 1); r++)
            {
                kc[r] = (byte)(keyCode & 0xFF);
                keyCode >>= 8;
            }

            seed[0x00] = kc[1];
            seed[0x01] = (byte)(kc[1] ^ kc[6]);
            seed[0x02] = (byte)(kc[2] ^ kc[3]);
            seed[0x03] = kc[2];
            seed[0x04] = (byte)(kc[2] ^ kc[1]);
            seed[0x05] = (byte)(kc[3] ^ kc[4]);
            seed[0x06] = kc[3];
            seed[0x07] = (byte)(kc[3] ^ kc[2]);
            seed[0x08] = (byte)(kc[4] ^ kc[5]);
            seed[0x09] = kc[4];
            seed[0x0A] = (byte)(kc[4] ^ kc[3]);
            seed[0x0B] = (byte)(kc[5] ^ kc[6]);
            seed[0x0C] = kc[5];
            seed[0x0D] = (byte)(kc[5] ^ kc[4]);
            seed[0x0E] = (byte)(kc[6] ^ kc[1]);
            seed[0x0F] = kc[6];

            Init56CreateTable(baseTableR, kc[0]);

            for (int r = 0; r < 16; r++)
            {
                byte nb;
                Init56CreateTable(baseTableC, seed[r]);
                nb = (byte)(baseTableR[r] << 4);
                for (int c = 0; c < 16; c++)
                {
                    baseTable[r * 16 + c] = (byte)(nb | baseTableC[c]);
                }
            }

            uint x = 0;
            uint pos = 1;

            for (int i = 0; i < 256; i++)
            {
                x = (x + 17) & 0xFF;
                if (baseTable[x] != 0 && baseTable[x] != 0xFF)
                    cipherTable[pos++] = baseTable[x];
            }

            cipherTable[0] = 0;
            cipherTable[0xFF] = 0xFF;

            return cipherTable;
        }
    }
}