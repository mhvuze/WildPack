using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace WildPack
{
    class Program
    {        
        static void Main(string[] args)
        {
            string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine("WildPack v{0} by MHVuze", version);

            if (args.Count() >= 3 )
            {
                switch(args[0])
                {
                    case "sarc":
                        switch(args[1])
                        {
                            case "x":
                                if (File.Exists(args[2]))
                                {
                                    SARC.UnpackSARC(args[2], new FileInfo(args[2]).Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(args[2]));
                                }
                                else
                                {
                                    Console.WriteLine("ERR: The specified input file does not exist.");
                                }
                                break;

                            case "p":
                                if (Directory.Exists(args[2]))
                                {
                                    uint padding = 0;
                                    if (args.Count() == 4) { padding = Convert.ToUInt32(args[3], 16); }
                                    SARC.PackSARC(args[2], new FileInfo(args[2]).Directory.FullName + "\\" + Path.GetFileName(args[2]) + "_new.sarc", padding);
                                }
                                else
                                {
                                    Console.WriteLine("ERR: The specified directory does not exist.");
                                }
                                break;
                        }
                        break;

                    case "yaz0":
                        switch(args[1])
                        {
                            case "d":
                                if (File.Exists(args[2]))
                                {
                                    YAZ0.DecodeYAZ0(args[2], new FileInfo(args[2]).Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(args[2]) + "_dec" + Path.GetExtension(args[2]));
                                }
                                else
                                {
                                    Console.WriteLine("ERR: The specified input file does not exist.");
                                }
                                break;

                            case "e":
                                if (File.Exists(args[2]))
                                {
                                    int padding = 0;
                                    if (args.Count() == 4) { padding = Convert.ToInt32(args[3], 16); }
                                    YAZ0.EncodeYAZ0(args[2], new FileInfo(args[2]).Directory.FullName + "\\" + Path.GetFileNameWithoutExtension(args[2]) + "_enc" + Path.GetExtension(args[2]), padding);
                                }
                                else
                                {
                                    Console.WriteLine("ERR: The specified input file does not exist.");
                                }
                                break;
                        }
                        break;
                }
            }
            else
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("Usage: WildPack sarc [x|p] <input_file>");
                Console.WriteLine("Usage: WildPack yaz0 [d|e] <input_file>");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
