using System;
using System.IO;

namespace CommonStuff
{
    abstract class RomLoader
    {
        protected byte[] rom;

        protected RomLoader(string inputFilename)
        {
            if (!File.Exists(inputFilename))
            {
                throw new Exception("input file does not exist");
            }
            this.rom = File.ReadAllBytes(inputFilename);
        }
    }
}
