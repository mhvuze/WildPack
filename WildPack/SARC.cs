using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WildPack
{
    struct sarcnode
    {
        public uint hash;
        public byte unk;
        public uint off, srt, end;

        public sarcnode(uint fhash, byte unknown, uint foffset, uint fstart, uint fend)
        {
            hash = fhash;
            unk = unknown;
            off = foffset;
            srt = fstart;
            end = fend;
        }
    }

    class SARC
    {
        // Unpack sarc
        public static void UnpackSARC(string infile, string outdir)
        {
            byte[] input = File.ReadAllBytes(infile);
            int pos = 4;
            StreamWriter sw;

            if (input[0] != 'S' || input[1] != 'A' || input[2] != 'R' || input[3] != 'C')
            {
                Console.WriteLine("ERR: The specified input file is not a SARC archive.");
                return;
            }

            ushort hdr = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;
            ushort order = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;

            if (order != 65279)
            {
                Console.WriteLine("ERR: The specified input file is a LE SARC archive.");
                return;
            }

            uint size = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
            pos += 4;
            uint doff = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
            pos += 4;
            uint unknown = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
            pos += 4;

            if (input[pos] != 'S' || input[pos + 1] != 'F' || input[pos + 2] != 'A' || input[pos + 3] != 'T')
            {
                Console.WriteLine("ERR: Unknown file section encountered.");
                return;
            }
            pos += 4;

            ushort hdr2 = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;
            ushort nodec = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;
            uint hashr = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
            pos += 4;

            sarcnode[] nodes = new sarcnode[nodec];
            sarcnode tmpnode = new sarcnode();

            for (int c = 0; c < nodec; c++)
            {
                tmpnode.hash = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
                pos += 4;
                tmpnode.unk = input[pos];
                pos += 1;
                tmpnode.off = Utils.makeu32(0, input[pos], input[pos + 1], input[pos + 2]);
                pos += 3;
                tmpnode.srt = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
                pos += 4;
                tmpnode.end = Utils.makeu32(input[pos], input[pos + 1], input[pos + 2], input[pos + 3]);
                pos += 4;
                nodes[c] = tmpnode;
            }

            if (input[pos] != 'S' || input[pos + 1] != 'F' || input[pos + 2] != 'N' || input[pos + 3] != 'T')
            {
                Console.WriteLine("ERR: Unknown file section encountered.");
                return;
            }
            pos += 4;

            ushort hdr3 = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;
            ushort unk2 = Utils.makeu16(input[pos], input[pos + 1]);
            pos += 2;

            string[] fnames = new string[nodec];
            string tmpstr;

            for (int c = 0; c < nodec; c++)
            {
                tmpstr = "";
                while (input[pos] != 0)
                {
                    tmpstr = tmpstr + ((char)input[pos]).ToString();
                    pos += 1;
                }
                while (input[pos] == 0)
                    pos += 1;

                fnames[c] = tmpstr;
            }

            if (!Directory.Exists(outdir)) Directory.CreateDirectory(outdir);
            for (int c = 0; c < nodec; c++)
            {
                Console.WriteLine("Extracing {0}", fnames[c]);
                Utils.makedirexist(Path.GetDirectoryName(outdir + "/" + fnames[c]));
                sw = new StreamWriter(outdir + "/" + fnames[c]);
                sw.BaseStream.Write(input, (int)(nodes[c].srt + doff), (int)(nodes[c].end - nodes[c].srt));
                sw.Close();
                sw.Dispose();
            }
        }

        // Pack sarc
        public static void PackSARC(string folder)
        {

        }
    }
}
