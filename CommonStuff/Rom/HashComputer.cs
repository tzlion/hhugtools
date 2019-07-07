using System;
using System.IO;
using System.Linq;
using CommonStuff.Utility;

namespace CommonStuff.Rom
{
    class HashComputer: RomLoader
    {
        string textFilename;

        public HashComputer(string inputFilename, string textFilename) : base(inputFilename) {
            this.textFilename = textFilename;
        }

        public void bankChecksums()
        {
            int bankCount = this.rom.Length / 0x4000;

            StreamWriter txt = File.CreateText(textFilename);

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();

                txt.WriteLine(curBank.ToString("X2") + " " + Hashing.GetMd5Hash(bankData));
            }

            txt.Close();

            System.Diagnostics.Process.Start(textFilename);
        }
    }
}
