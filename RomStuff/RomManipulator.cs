using System;
using System.IO;

namespace RomStuff
{
    class RomManipulator : RomLoader
    {
        protected string outputFilename;

        public RomManipulator(string inputFilename, string outputFilename) : base (inputFilename)
        {
            if (!Directory.Exists(new FileInfo(outputFilename).DirectoryName))
            {
                throw new Exception("output path does not exist");
            }
            this.outputFilename = outputFilename;
        }

        public void save(bool openAfterwards = false)
        {
            File.WriteAllBytes(this.outputFilename, this.rom);
            if (openAfterwards)
            {
                System.Diagnostics.Process.Start(this.outputFilename);
            }
        }
    }
}
