using System;
using System.IO;

namespace RomStuff
{
    class RomLoader
    {
        protected byte[] rom;

        public RomLoader(string inputFilename)
        {
            if (!File.Exists(inputFilename))
            {
                throw new Exception("input file does not exist");
            }
            this.rom = File.ReadAllBytes(inputFilename);
        }
    }
}
