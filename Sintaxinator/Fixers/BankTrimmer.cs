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

            int bankCount = 256;
            do
            {
                if (maxUsedBank > (bankCount / 2))
                {
                    break;
                }
                bankCount /= 2;
            } while (bankCount > 2);

            rom = rom.Take(0x4000 * bankCount).ToArray();
        }
    }
}