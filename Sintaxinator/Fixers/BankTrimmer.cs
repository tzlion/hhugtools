using System.Linq;
using Common.Rom;

namespace Sintaxinator.Fixers
{
    class BankTrimmer : RomManipulator
    {
        public BankTrimmer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void TrimUnusedBanks(bool[] usedBanks)
        {
            int maxUsedBank = 0;
            
            for (int curBank = 0; curBank < usedBanks.Length; curBank++)
            {
                if (usedBanks[curBank])
                {
                    maxUsedBank = curBank;
                }
            }

            int bankCount;
            if (maxUsedBank < 32)
            {
                bankCount = 32;
            }
            else if (maxUsedBank < 64)
            {
                bankCount = 64;
            }
            else if (maxUsedBank < 128)
            {
                bankCount = 128;
            }
            else
            {
                bankCount = 256;
            }

            rom = rom.Take(0x4000 * bankCount).ToArray();
        }
    }
}