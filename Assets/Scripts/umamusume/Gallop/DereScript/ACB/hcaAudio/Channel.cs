using System;
using System.Runtime.InteropServices;
using DereTore.Common;
using System.Linq.Expressions;

namespace DereTore.Exchange.Audio.HCA {
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct Channel {

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 0x80)]
        public float[] Block;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 0x80)]
        public float[] Base;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 0x80)]
        public byte[] Value;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 0x80)]
        public byte[] Scale;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
        public byte[] Value2;
        [MarshalAs(UnmanagedType.I4)]
        public int Type;
        // Original type: public char *
        [MarshalAs(UnmanagedType.U4)]
        public uint Value3;
        public uint Count;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 0x80)]
        public float[] Wav1;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 0x80)]
        public float[] Wav2;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 0x80)]
        public float[] Wav3;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8, SizeConst = 8 * 0x80)]
        public float[] Wave;

        public static string NameOf<T>(Expression<Func<T>> e)
        {
            return ((MemberExpression)e.Body).Member.Name;
        }
        public static Channel CreateDefault() {
            var v = default(Channel);
            v.Block = new float[0x80];
            v.Base = new float[0x80];
            v.Value = new byte[0x80];
            v.Scale = new byte[0x80];
            v.Value2 = new byte[8];
            v.Type = 0;
            v.Value3 = 0;
            v.Count = 0;
            v.Wav1 = new float[0x80];
            v.Wav2 = new float[0x80];
            v.Wav3 = new float[0x80];
            v.Wave = new float[8 * 0x80];
            return v;
        }

        public void Decode1(DataBits data, uint a, int b, byte[] ath) {
            int v = data.GetBit(3);
            if (v >= 6) {
                for (uint i = 0; i < Count; ++i) {
                    Value[i] = (byte)data.GetBit(6);
                }
            } else if (v != 0) {
                int v1 = data.GetBit(6);
                int v2 = (1 << v) - 1;
                int v3 = v2 >> 1;
                Value[0] = (byte)v1;
                for (uint i = 1; i < Count; ++i) {
                    int v4 = data.GetBit(v);
                    if (v4 != v2) {
                        v1 += v4 - v3;
                    } else {
                        v1 = data.GetBit(6);
                    }
                    Value[i] = (byte)v1;
                }
            } else {
                Value.ZeroMem();
            }
            if (Type == 2) {
                v = data.CheckBit(4);
                Value2[0] = (byte)v;
                if (v < 15) {
                    for (var i = 0; i < 8; ++i) {
                        Value2[i] = (byte)data.GetBit(4);
                    }
                }
            } else {
                for (uint i = 0; i < a; ++i) {
                    //Value3[i] = (byte)data.GetBit(6);
                    SetValue3(i, (byte)data.GetBit(6));
                }
            }
            for (uint i = 0; i < Count; ++i) {
                v = Value[i];
                if (v != 0) {
                    v = (int)(ath[i] + ((b + i) >> 8) - ((v * 5) >> 1) + 1);
                    if (v < 0) {
                        v = 15;
                    } else if (v >= 0x39) {
                        v = 1;
                    } else {
                        v = ChannelTables.Decode1ScaleList[v];
                    }
                }
                Scale[i] = (byte)v;
            }
            for (var i = Count; i < Scale.Length; ++i) {
                Scale[i] = 0;
            }
            for (uint i = 0; i < Count; ++i) {
                Base[i] = ChannelTables.Decode1ValueSingle[Value[i]] * ChannelTables.Decode1ScaleSingle[Scale[i]];
            }
        }

        public void Decode2(DataBits data) {
            for (uint i = 0; i < Count; ++i) {
                float f;
                int s = Scale[i];
                int bitSize = ChannelTables.Decode2List1[s];
                int v = data.GetBit(bitSize);
                if (s < 8) {
                    v += s << 4;
                    data.AddBit(ChannelTables.Decode2List2[v] - bitSize);
                    f = ChannelTables.Decode2List3[v];
                } else {
                    v = (1 - ((v & 1) << 1)) * (v >> 1);
                    if (v == 0) {
                        data.AddBit(-1);
                    }
                    f = v;
                }
                Block[i] = Base[i] * f;
            }
            for (var i = Count; i < Block.Length; ++i) {
                Block[i] = 0;
            }
        }

        public void Decode3(uint a, uint b, uint c, uint d) {
            if (Type != 2 && b != 0) {
                float[] listFloat = ChannelTables.Decode3ListSingle;
                int offset = ChannelTables.Decode3ListOffset;
                for (uint i = 0, k = c, l = c - 1; i < a; ++i) {
                    for (uint j = 0; j < b && k < d; ++j, --l) {
                        Block[k++] = listFloat[GetValue3(i) - Value[l] + offset] * Block[l];
                    }
                }
                Block[0x80 - 1] = 0;
            }
        }

        public static void Decode4(ref Channel @this, ref Channel next, int index, uint a, uint b, uint c) {
            if (@this.Type == 1 && c != 0) {
                var f1 = ChannelTables.Decode4ListSingle[next.Value2[index]];
                var f2 = f1 - 2f;
                float[] s = @this.Block;
                float[] d = next.Block;
                int sIndex, dIndex;
                sIndex = (int)b;
                dIndex = (int)b;
                for (uint i = 0; i < a; ++i) {
                    // Don't know why, but it just happened.
                    // See se_live_flic_perfect.hca
                    // original:
                    /*
                     * (no 'break')
                     * d[dIndex++] = s[sIndex] * f2;
                     * s[sIndex++] = s[sIndex] * f1;
                     */
                    if (sIndex >= s.Length || dIndex >= d.Length) {
                        break;
                    }
                    d[dIndex] = s[sIndex] * f2;
                    dIndex++;
                    s[sIndex] = s[sIndex] * f1;
                    sIndex++;
                }
            }
        }

        public void Decode5(int index) {
            float[] s;
            float[] d;
            s = Block;
            d = Wav1;
            int sIndex = 0, dIndex = 0;
            int s1Index, s2Index;
            for (int i = 0, count1 = 1, count2 = 0x40; i < 7; ++i, count1 <<= 1, count2 >>= 1) {
                int dIndex1 = dIndex, dIndex2 = dIndex + count2;
                for (int j = 0; j < count1; ++j) {
                    for (int k = 0; k < count2; ++k) {
                        float a = s[sIndex++];
                        float b = s[sIndex++];
                        d[dIndex1++] = b + a;
                        d[dIndex2++] = a - b;
                    }
                    dIndex1 += count2;
                    dIndex2 += count2;
                }
                sIndex -= 0x80;
                HcaHelper.Exchange(ref sIndex, ref dIndex);
                HcaHelper.Exchange(ref s, ref d);
            }
            s = Wav1;
            d = Block;
            sIndex = dIndex = 0;
            for (int i = 0, count1 = 0x40, count2 = 1; i < 7; ++i, count1 >>= 1, count2 <<= 1) {
                // The original array is a 2-rank array, [7][0x40].
                int list1FloatIndex = i * 0x40;
                // The original array is a 2-rank array, [7][0x40].
                int list2FloatIndex = i * 0x40;
                s1Index = sIndex;
                s2Index = sIndex + count2;
                int dIndex1 = dIndex;
                int dIndex2 = dIndex + count2 * 2 - 1;
                for (int j = 0; j < count1; ++j) {
                    for (int k = 0; k < count2; ++k) {
                        float fa = s[s1Index++];
                        float fb = s[s2Index++];
                        float fc = ChannelTables.Decode5List1Single[list1FloatIndex++];
                        float fd = ChannelTables.Decode5List2Single[list2FloatIndex++];
                        d[dIndex1++] = fa * fc - fb * fd;
                        d[dIndex2--] = fa * fd + fb * fc;
                    }
                    s1Index += count2;
                    s2Index += count2;
                    dIndex1 += count2;
                    dIndex2 += count2 * 3;
                }
                HcaHelper.Exchange(ref sIndex, ref dIndex);
                HcaHelper.Exchange(ref s, ref d);
            }
            d = Wav2;
            for (int i = 0; i < 0x80; ++i) {
                d[i] = s[i];
            }
            s = ChannelTables.Decode5List3Single;
            sIndex = 0;
            d = Wave;
            // The original array is [8][0x80].
            dIndex = index * 0x80;
            float[] s1 = Wav2;
            s1Index = 0x40;
            float[] s2 = Wav3;
            s2Index = 0;
            for (int i = 0; i < 0x40; ++i) {
                d[dIndex++] = s1[s1Index++] * s[sIndex++] + s2[s2Index++];
            }
            for (int i = 0; i < 0x40; ++i) {
                d[dIndex++] = s[sIndex++] * s1[--s1Index] - s2[s2Index++];
            }
            s1 = Wav2;
            s2 = Wav3;
            s1Index = 0x40 - 1;
            s2Index = 0;
            for (int i = 0; i < 0x40; ++i) {
                s2[s2Index++] = s1[s1Index--] * s[--sIndex];
            }
            for (int i = 0; i < 0x40; ++i) {
                s2[s2Index++] = s[--sIndex] * s1[++s1Index];
            }
        }

        private byte GetValue3(int refIndex) {
            int index = (int)(refIndex + Value3);
            if (0 <= index && index < 0x80) {
                return Value[index];
            } else if (0x80 <= index && index < 0x80 + 0x80) {
                return Scale[index - 0x80];
            } else {
                throw new ArgumentOutOfRangeException(NameOf(() => refIndex));
            }
        }

        private byte GetValue3(uint refIndex) {
            var index = refIndex + Value3;
            if (index < 0x80) {
                return Value[index];
            } else if (index < 0x80 + 0x80) {
                return Scale[index - 0x80];
            } else {
                throw new ArgumentOutOfRangeException(NameOf(() => refIndex));
            }
        }

        private void SetValue3(int refIndex, byte value) {
            Value[refIndex + Value3] = value;
        }

        private void SetValue3(uint refIndex, byte value) {
            Value[refIndex + Value3] = value;
        }

        private void SetValue3(byte value) {
            Value[Value3] = value;
        }

    }
}
