using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LZ4
{

    public class BinaryReader
    {
        private byte[] m_array;
        private int m_curPos = 0;

        public BinaryReader(byte[] data)
        {
            m_array = new byte[data.Length];
            data.CopyTo(m_array, 0);
        }

        public BinaryReader(string file)
        {
            System.IO.FileStream fs = new System.IO.FileStream(
                file,
                System.IO.FileMode.Open,
                System.IO.FileAccess.Read
            );

            System.IO.FileInfo fi = new System.IO.FileInfo(file);
            m_array = new byte[fi.Length];

            fs.Seek(0, System.IO.SeekOrigin.Begin);
            fs.Read(m_array, 0, (int)fi.Length);

            fs.Close();
        }

        public byte readByte()
        {
            m_curPos++;
            return m_array[m_curPos - 1];
        }

        public ushort readShortLE()
        {
            m_curPos += 2;
            return (ushort)(m_array[m_curPos - 2] | (m_array[m_curPos - 1] << 8));
        }

        public int readIntLE()
        {
            m_curPos += 4;
            return (int)(m_array[m_curPos - 4] | (m_array[m_curPos - 3] << 8) | (m_array[m_curPos - 2] << 16) | (m_array[m_curPos - 1] << 24));
        }

        public void copyBytes(byte[] dst, int offset, int size)
        {
            Array.Copy(m_array, m_curPos, dst, offset, size);
            m_curPos += size;
        }

        public void seekAbs(int pos)
        {
            this.m_curPos = pos;
        }

        public void seekRel(int diff)
        {
            m_curPos += diff;
        }

        public int getPos()
        {
            return m_curPos;
        }
    }
}
