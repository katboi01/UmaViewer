using BaseNcoding;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Assets.Editor.RootMotion
{
    public class ManifestEntry 
    {
        public static byte[] CalHame(ulong checksum, ulong size, byte[] name)
        {
            if (name == null) return null;
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] allbyte = new byte[name.Length + 16];
            byte[] checksumByte = BitConverter.GetBytes(checksum);
            byte[] sizeByte = BitConverter.GetBytes(size);
            for(int i=0; i < 16; i++)
            {
                allbyte[i] = (i < 8 ? checksumByte[7 - i] : sizeByte[15 - i]);
            }
            Buffer.BlockCopy(name, 0, allbyte, 16, name.Length);
            return sha1.ComputeHash(allbyte);
        }

        public static string CalHameString(ulong checksum, ulong size, byte[] name)
        {
            var bytes = CalHame(checksum, size, name);
            Base32 base32 = new Base32();
            return base32.Encode(bytes);
        }
    }
}
