using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
