using BaseNcoding;
using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Assets.Editor.RootMotion
{
    public class Test : MonoBehaviour
    {
        private void Start()
        {
            SHA1CryptoServiceProvider sha = new SHA1CryptoServiceProvider();
            var str = CalHameString(sha, 1541716412357448819, 77, Encoding.UTF8.GetBytes("root"));
            Debug.LogWarning($"{str} Length:{str.Length}");
            Debug.LogWarning($"{"FN4II57IWE274AT5R45G2UE7XVMFDPBD"} Length:{"FN4II57IWE274AT5R45G2UE7XVMFDPBD".Length}");
        }

        public byte[] CalHame(SHA1CryptoServiceProvider sha1, ulong checksum, ulong size, byte[] name)
        {
            if (name == null) return null;
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

        public string CalHameString(SHA1CryptoServiceProvider sha1, ulong checksum, ulong size, byte[] name)
        {
            var bytes = CalHame(sha1, checksum, size, name);
            Base32 base32 = new Base32();
            return base32.Encode(bytes);
        }
    }
}
