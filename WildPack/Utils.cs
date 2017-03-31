﻿using System;
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
    }
}