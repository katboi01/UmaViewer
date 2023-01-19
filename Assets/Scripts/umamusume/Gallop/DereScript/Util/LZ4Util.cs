using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using LZ4Sharp;
using LZ4ps;

namespace LZ4
{
    /// <summary>
    /// 置換
    /// </summary>
    public class LZ4Util
    {
        public static byte[] decompress(byte[] indata)
        {
            int m_curPos = 4;
            int token =  (int)(indata[m_curPos - 4] | (indata[m_curPos - 3] << 8) | (indata[m_curPos - 2] << 16) | (indata[m_curPos - 1] << 24));
            m_curPos += 4;
            int decompSize = (int)(indata[m_curPos - 4] | (indata[m_curPos - 3] << 8) | (indata[m_curPos - 2] << 16) | (indata[m_curPos - 1] << 24));
            m_curPos += 4;
            int compSize = (int)(indata[m_curPos - 4] | (indata[m_curPos - 3] << 8) | (indata[m_curPos - 2] << 16) | (indata[m_curPos - 1] << 24));
            m_curPos += 4;
            int tmp4 = (int)(indata[m_curPos - 4] | (indata[m_curPos - 3] << 8) | (indata[m_curPos - 2] << 16) | (indata[m_curPos - 1] << 24));

            //byte[] srcBytes = new byte[compSize];
            //Array.Copy(indata, 0x10, srcBytes, 0, compSize);

            if (IntPtr.Size == 4)
            {
                return LZ4Codec.Decode32(indata, 0x10, compSize, decompSize);
            }
            else
            {
                return LZ4Codec.Decode64(indata, 0x10, compSize, decompSize);
            }
        }
    }
    /*
    public class LZ4Util
    {

        public static byte[] decompress(byte[] indata)
        {
            LZ4.BinaryReader r = new LZ4.BinaryReader(indata);

            byte[] dec = decompress(r);

            return dec;
        }

        public static byte[] decompress(BinaryReader r)
        {
            byte[] retArray;
            var dataSize = 0;
            var decompressedSize = 0;

            var token = 0;
            var sqSize = 0;
            var matchSize = 0;
            var offset = 0;
            var retCurPos = 0;
            var endPos = 0;

            r.seekAbs(4);
            decompressedSize = r.readIntLE();
            dataSize = r.readIntLE();
            endPos = dataSize + 16;
            retArray = new byte[decompressedSize];

            r.seekAbs(16);

            //Start reading sequences
            while (true)
            {
                //read the LiteralSize and the MatchSize
                token = r.readByte();
                sqSize = token >> 4;
                matchSize = (token & 0x0F) + 4;
                if (sqSize == 15)
                    sqSize += readAdditionalSize(r);

                //copy the literal
                r.copyBytes(retArray, retCurPos, sqSize);
                retCurPos += sqSize;

                if (r.getPos() >= endPos - 1)
                    break;

                //read the offset
                offset = r.readShortLE();

                //read the additional MatchSize
                if (matchSize == 19)
                    matchSize += readAdditionalSize(r);

                //copy the match properly
                if (matchSize > offset)
                {
                    var matchPos = retCurPos - offset;
                    while (true)
                    {
                        Array.Copy(retArray, matchPos, retArray, retCurPos, offset);
                        retCurPos += offset;
                        matchSize -= offset;
                        if (matchSize < offset)
                            break;
                    }
                }
                Array.Copy(retArray, retCurPos - offset, retArray, retCurPos, matchSize);
                retCurPos += matchSize;
            }


            return retArray;
        }

        public static int readAdditionalSize(BinaryReader reader)
        {
            var size = reader.readByte();
            if (size == 255)
                return size + readAdditionalSize(reader);
            else
                return size;
        }
    }
    */
}
