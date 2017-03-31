using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.NintenTools.Yaz0;

namespace WildPack
{
    class YAZ0
    {
        // Decode yaz0 file
        public static void DecodeYAZ0(string infile, string outfile)
        {
            Console.WriteLine("Decoding {0}", infile);
            Yaz0Compression.Decompress(infile, outfile);
        }

        // Encode yaz0 file
        public static void EncodeYAZ0(string infile, string outfile)
        {

        }
    }
}
