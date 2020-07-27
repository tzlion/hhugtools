using System.Linq;
using Common.Rom;

namespace Sintaxinator.Fixers
{
    class BankBlanker : RomManipulator
    {
        public BankBlanker(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void BlankUnusedBanks(bool[] usedBanks)
        {

            byte[] blankrompart = new byte[0x4000];

            byte[][] superdata = new byte[256][];

            for(int x=1;x<=255;x++) {
                superdata[x] = blankrompart;
            }

            int bankCount = this.rom.Length / 0x4000;

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();
                
                if (usedBanks[curBank])
                {
                    superdata[curBank] = bankData;
                }
            }

            byte[] newrom = {};

            foreach(byte[] datapart in superdata) {
                newrom = newrom.Concat(datapart).ToArray();
            }

            this.rom = newrom;

        }
    }
}