using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WildPack
{
    class Utils
    {
        public static ushort makeu16(byte b1, byte b2)
        {
            return (ushort)(((ushort)b1 << 8) | (ushort)b2);
        }

        public static uint makeu32(byte b1, byte b2, byte b3, byte b4)
        {
            return ((uint)b1 << 24) | ((uint)b2 << 16) | ((uint)b3 << 8) | (uint)b4;
        }

        public static byte[] breaku16(ushort u16)
        {
            return new byte[] { (byte)(u16 >> 8), (byte)(u16 & 0xFF) };
        }

        public static byte[] breaku32(uint u32)
        {
            return new byte[] { (byte)(u32 >> 24), (byte)((u32 >> 16) & 0xFF), (byte)((u32 >> 8) & 0xFF), (byte)(u32 & 0xFF) };
        }

        public static void makedirexist(string dir)
        {
            string dpath = System.IO.Path.GetFullPath(dir);
            int numdirs = 0;
            for (int c = 0; c < dpath.Length; c++)
                if (dpath[c] == '\\') numdirs++;

            for (int c = numdirs; c >= 0; c--)
            {
                string tmp = dpath;
                for (int cc = 0; cc < c; cc++)
                    tmp = System.IO.Path.GetDirectoryName(tmp);
                if (!System.IO.Directory.Exists(tmp))
                    System.IO.Directory.CreateDirectory(tmp);
            }
        }

        public static uint getfilesize(string fpath)
        {
            System.IO.StreamReader sr = new System.IO.StreamReader(fpath);
            uint fs = (uint)sr.BaseStream.Length;
            sr.Close();
            sr.Dispose();
            return fs;
        }

        public static uint calchash(string name)
        {
            ulong result = 0;
            for (int c = 0; c < name.Length; c++)
            {
                result = (((byte)name[c]) + (result * 0x65)) & 0xFFFFFFFF;
            }
            return (uint)(result & 0xFFFFFFFF);
        }

        public static byte[] ArrayReverse(byte[] array)
        {
            Array.Reverse(array);
            return array;
        }

        public static Int32 SwapEndianness(Int32 value)
        {
            return BitConverter.ToInt32(ArrayReverse(BitConverter.GetBytes(value)), 0);
        }
    }
}
