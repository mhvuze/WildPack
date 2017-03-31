using System;
using System.IO;
using System.Linq;

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

    struct filedata
    {
        public string filename, realname;
        public int filesize, namesize, filenum;

        public filedata(string _filename, string _realname, int _filesize, int _namesize, int _filenum)
        {
            filename = _filename;
            realname = _realname;
            filesize = _filesize;
            namesize = _namesize;
            filenum = _filenum;
        }
    }

    struct filehash
    {
        public uint hash;
        public int index;

        public filehash(uint fhash, int findex)
        {
            hash = fhash;
            index = findex;
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
        public static void PackSARC(string indir, string outfile, uint sfnt_padding)
        {
            StreamWriter sw = new StreamWriter(outfile);
            uint padding = 4;
            uint lhash;
            int dhi = 0;

            string[] indir_files = Directory.GetFiles(indir == "" ? Environment.CurrentDirectory : indir, "*.*", SearchOption.AllDirectories);
            filedata[] filedatalist = new filedata[indir_files.Length];
            int lenfiles = 0, numfiles = indir_files.Length, lennames = 0;
            uint filesize;

            // Add files to array
            for (int c = 0; c < indir_files.Length; c++)
            {
                string realname = indir_files[c];
                string filenameD = indir_files[c].Replace(indir + Path.DirectorySeparatorChar.ToString(), "");
                string filename = filenameD.Replace("\\", "/");

                // Account for file names in root
                if (filename.Count(f => f == '/') == 0 && sfnt_padding == 0)
                {
                    filename = "/" + filename;
                }

                filesize = Utils.getfilesize(realname);
                if (filesize % padding > 0) filesize += (padding - (filesize % padding));
                int namesize = filename.Length;
                namesize += (4 - (namesize % 4));
                lennames += namesize;
                filedatalist[c] = new filedata(filename, realname, (int)filesize, namesize, numfiles);
            }

            filehash[] hashes_unsorted = new filehash[numfiles];
            for (int c = 0; c < numfiles; c++)
            {
                hashes_unsorted[c] = new filehash(Utils.calchash(filedatalist[c].filename), c);
            }

            bool[] hashes_done = new bool[hashes_unsorted.Length];
            filehash[] hashes = new filehash[hashes_unsorted.Length];
            for (int c = 0; c < hashes.Length; c++)
            {
                lhash = uint.MaxValue;
                for (int cc = 0; cc < hashes_unsorted.Length; cc++)
                {
                    if (hashes_done[cc]) continue;
                    if (hashes_unsorted[cc].hash < lhash)
                    {
                        dhi = cc;
                        lhash = hashes_unsorted[cc].hash;
                    }
                }
                hashes_done[dhi] = true;
                hashes[c] = hashes_unsorted[dhi];
            }

            for (int c = 0; c < numfiles; c++)
            {
                lenfiles += filedatalist[hashes[c].index].filesize;
            }

            //uint lastfile = Utils.getfilesize(filedatalist[hashes[hashes.Length - 1].index].realname);
            //lenfiles += (int)lastfile;
            filesize = (uint)(32 + (16 * numfiles) + 8 + lennames); // SARC header + SFAT header + (SFAT nodes) + SFNT header + file names
            uint padSFAT = 0;
            if (sfnt_padding > 0)
                padSFAT = (sfnt_padding - (filesize % sfnt_padding));          
            uint datastart = padSFAT + filesize;
            filesize += (uint)(padSFAT + lenfiles);

            // Write SARC + SFAT header
            sw.BaseStream.Write(new byte[] { 83, 65, 82, 67, 0x00, 0x14, 0xFE, 0xFF }, 0, 8);
            sw.BaseStream.Write(Utils.breaku32(filesize), 0, 4);
            sw.BaseStream.Write(Utils.breaku32(datastart), 0, 4);            
            sw.BaseStream.Write(new byte[] { 0x01, 0x00, 0x00, 0x00, 83, 70, 65, 84, 0x00, 0x0C }, 0, 10);
            sw.BaseStream.Write(Utils.breaku16((ushort)numfiles), 0, 2);
            sw.BaseStream.Write(Utils.breaku32(0x65), 0, 4);
            int strpos = 0, filepos = 0;

            // Write file nodes
            for (int c = 0; c < numfiles; c++)
            {
                Console.WriteLine("Packing {0}", filedatalist[c].filename);
                sw.BaseStream.Write(Utils.breaku32(hashes[c].hash), 0, 4);
                sw.BaseStream.WriteByte(0x01); // Unknown, see http://mk8.tockdom.com/wiki/SARC_%28File_Format%29
                sw.BaseStream.Write(Utils.breaku32((uint)(strpos >> 2)), 1, 3);
                strpos += filedatalist[hashes[c].index].namesize;
                sw.BaseStream.Write(Utils.breaku32((uint)filepos), 0, 4);
                filesize = Utils.getfilesize(filedatalist[hashes[c].index].realname);
                sw.BaseStream.Write(Utils.breaku32((uint)filepos + (uint)filesize), 0, 4);
                filepos += filedatalist[hashes[c].index].filesize;
            }

            sw.BaseStream.Write(new byte[] { 83, 70, 78, 84, 0x00, 0x08, 0x00, 0x00 }, 0, 8);

            // Write file name table
            for (int c = 0; c < numfiles; c++)
            {
                string tn = filedatalist[hashes[c].index].filename;
                for (int cc = 0; cc < tn.Length; cc++)
                {
                    sw.BaseStream.WriteByte((byte)tn[cc]);
                }
                int numpad0 = filedatalist[hashes[c].index].namesize - filedatalist[hashes[c].index].filename.Length;
                for (int cc = 0; cc < numpad0; cc++)
                    sw.BaseStream.WriteByte(0);
            }

            for (int cc = 0; cc < padSFAT; cc++)
                sw.BaseStream.WriteByte(0);

            byte[] tmp;
            for (int c = 0; c < numfiles; c++)
            {
                tmp = File.ReadAllBytes(filedatalist[hashes[c].index].realname);
                sw.BaseStream.Write(tmp, 0, tmp.Length);
                filesize = (uint)tmp.Length;
                if (c < numfiles - 1)
                {
                    int numpad0 = (int)(filedatalist[hashes[c].index].filesize - filesize);
                    for (int cc = 0; cc < numpad0; cc++)
                        sw.BaseStream.WriteByte(0);
                }
            }

            sw.Close();
            sw.Dispose();
        }
    }
}
