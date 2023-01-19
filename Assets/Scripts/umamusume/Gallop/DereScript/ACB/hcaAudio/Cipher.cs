using System;

namespace DereTore.Exchange.Audio.HCA {
    internal sealed class Cipher {

        public bool Initialize(CipherType type, uint key1, uint key2) {
            if (key1 == 0 && key2 == 0 && type == CipherType.CipherWithAKey) {
                type = CipherType.NoChipher;
            }
            switch (type) {
                case CipherType.NoChipher:
                    Init0();
                    break;
                case CipherType.CipherWithoutKeys:
                    Init1();
                    break;
                case CipherType.CipherWithAKey:
                    Init56(key1, key2);
                    break;
                default:
                    return false;
            }
            InitEncryptTable();
            return true;
        }

        public void Decrypt(byte[] data) {
            Decrypt(data, data.Length);
        }

        public void Decrypt(byte[] data, int length) {
            length = Math.Min(data.Length, length);
            for (var i = 0; i < length; ++i) {
                data[i] = _decryptTable[data[i]];
            }
        }

        public void Encrypt(byte[] data) {
            Encrypt(data, data.Length);
        }

        public void Encrypt(byte[] data, int length) {
            length = Math.Min(data.Length, length);
            for (var i = 0; i < length; ++i) {
                data[i] = _encryptTable[data[i]];
            }
        }

        private void Init0() {
            for (var i = 0; i < _decryptTable.Length; ++i) {
                _decryptTable[i] = (byte)i;
            }
        }

        private void Init1() {
            for (int i = 1, v = 0; i < _decryptTable.Length - 1; ++i) {
                v = (v * 13 + 11) & 0xff;
                if (v == 0 || v == 0xff) {
                    v = (v * 13 + 11) & 0xff;
                }
                _decryptTable[i] = (byte)v;
            }
            _decryptTable[0] = 0;
            _decryptTable[_decryptTable.Length - 1] = 0xff;
        }

        private void Init56(uint key1, uint key2) {
            byte[] t1 = new byte[8];
            if (key1 == 0) {
                --key2;
            }
            --key1;
            for (int i = 0; i < 7; ++i) {
                t1[i] = (byte)key1;
                key1 = (key1 >> 8) | (key2 << 24);
                key2 >>= 8;
            }
            byte[] t2 = new byte[0x10] {
                t1[1], (byte)(t1[1] ^ t1[6]),
                (byte)(t1[2] ^ t1[3]), t1[2],
                (byte)(t1[2] ^ t1[1]), (byte)(t1[3] ^ t1[4]),
                t1[3], (byte)(t1[3] ^ t1[2]),
                (byte)(t1[4] ^ t1[5]), t1[4],
                (byte)(t1[4] ^ t1[3]), (byte)(t1[5] ^ t1[6]),
                t1[5], (byte)(t1[5] ^ t1[4]),
                (byte)(t1[6] ^ t1[1]), t1[6],
            };
            byte[] t3 = new byte[0x100];
            byte[] t31 = new byte[0x10];
            byte[] t32 = new byte[0x10];
            int c = 0;
            Init56CreateTable(t31, t1[0]);
            for (int i = 0; i < t31.Length; ++i) {
                Init56CreateTable(t32, t2[i]);
                byte v = (byte)(t31[i] << 4);
                for (int j = 0; j < t32.Length; ++j) {
                    t3[c] = (byte)(v | t32[j]);
                    ++c;
                }
            }
            c = 1;
            for (int i = 0, v = 0; i < _decryptTable.Length; ++i) {
                v = (v + 0x11) & 0xff;
                byte a = t3[v];
                if (a != 0 && a != 0xff) {
                    _decryptTable[c] = a;
                    ++c;
                }
            }
            _decryptTable[0] = 0;
            _decryptTable[_decryptTable.Length - 1] = 0xff;
        }

        private void Init56CreateTable(byte[] r, byte key) {
            int mul = ((key & 1) << 3) | 5;
            int add = (key & 0xe) | 1;
            key >>= 4;
            for (int i = 0; i < 0x10; ++i) {
                key = (byte)((key * mul + add) & 0xf);
                r[i] = key;
            }
        }

        private void InitEncryptTable() {
            for (var i = 0; i < 0x100; ++i) {
                for (var j = 0; j < 0x100; ++j) {
                    if (_decryptTable[j] == i) {
                        _encryptTable[i] = (byte)(j & 0xff);
                        break;
                    }
                }
            }
        }

        private readonly byte[] _decryptTable = new byte[0x100];
        private readonly byte[] _encryptTable = new byte[0x100];

    }
}