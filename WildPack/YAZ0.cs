using System;
using System.Text;
using Syroot.NintenTools.Yaz0;
using System.IO;

namespace WildPack
{
    class YAZ0
    {
        // Decode yaz0 file
        public static void DecodeYAZ0(string infile, string outfile)
        {
            Console.WriteLine("Decoding {0}", infile);
            Yaz0Compression.Decompress(infile, outfile);
            byte[] yaz0_data = File.ReadAllBytes(infile);
            byte[] padding = new byte[] { yaz0_data[8], yaz0_data[9], yaz0_data[10], yaz0_data[11] };
            int padding_int = Utils.SwapEndianness(BitConverter.ToInt32(padding, 0));
            if (padding_int > 0) { Console.WriteLine("Important: Padding for this file is 0x{0}. Keep in mind when repacking.", padding_int.ToString("X8")); }
        }

        // Encode yaz0 file
        public static void EncodeYAZ0(string infile, string outfile, int padding)
        {
            Byte codeByte = 0x00;
            byte[] decodedBuffer = File.ReadAllBytes(infile);
            Int32 bulkChunksCount = decodedBuffer.Length / 0x08;
            Int32 extraChunkSize = decodedBuffer.Length % 0x08;
            byte[] chunkBuffer = new byte[0x08];

            Console.WriteLine("Encoding {0}", infile);

            using (FileStream fs = new FileStream(outfile, FileMode.Create, FileAccess.Write))
            {
                fs.Write(Encoding.UTF8.GetBytes("Yaz0"), 0, 4);
                fs.Write(BitConverter.GetBytes(Utils.SwapEndianness((Int32)new FileInfo(infile).Length)), 0, 4);
                fs.Write(BitConverter.GetBytes(Utils.SwapEndianness(padding)), 0, 4);
                fs.Write(BitConverter.GetBytes(Utils.SwapEndianness(0x00000000)), 0, 4);

                for (int i = 0; i < bulkChunksCount; i++)
                {
                    fs.Write(BitConverter.GetBytes(0xFF), 0, 1);
                    Array.Copy(decodedBuffer, (i * 8), chunkBuffer, 0, 8);
                    fs.Write(chunkBuffer, 0, 8);
                }

                if (extraChunkSize != 0)
                {
                    Array.Resize(ref chunkBuffer, extraChunkSize);
                    Array.Copy(decodedBuffer, (0x08 * bulkChunksCount), chunkBuffer, 0, chunkBuffer.Length);
                                        
                    switch (extraChunkSize)
                    {
                        case 1:
                            codeByte = 0x80;
                            break;
                        case 2:
                            codeByte = 0xC0;
                            break;
                        case 3:
                            codeByte = 0xE0;
                            break;
                        case 4:
                            codeByte = 0xF0;
                            break;
                        case 5:
                            codeByte = 0xF8;
                            break;
                        case 6:
                            codeByte = 0xFC;
                            break;
                        case 7:
                            codeByte = 0xFE;
                            break;
                        default:
                            break;
                    }
                }

                fs.Write(BitConverter.GetBytes(codeByte), 0, 1);
                fs.Write(chunkBuffer, 0, chunkBuffer.Length);
            }
        }
    }
}
