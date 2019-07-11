using System.Linq;
using Common.Utility;
using Common.Rom;

namespace Sintaxinator.Fixers
{
    class BBDFixer : RomManipulator
    {
        public BBDFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void TestFix(byte[] reordering)
        {

            byte[] processed = { };

            int bankCount = this.rom.Length / 0x4000;

            for (int curBank = 0; curBank < bankCount; curBank++)
            {
                byte[] bankData = this.rom.Skip(0x4000 * curBank).Take(0x4000).ToArray();
                if (curBank == 0) // Header
                {
                    processed = processed.Concat(bankData).ToArray();
                }
                else
                {
                    processed = processed.Concat(ManipData(bankData, reordering)).ToArray();
                }
            }

            this.rom = processed;
        }

        private byte[] ManipData(byte[] origData, byte[] reordering)
        {
            byte[] newData = new byte[origData.Length];
            for (int x = 0; x < origData.Length; x++)
            {
                newData[x] = ByteManipulation.ReorderBits(origData[x], reordering);
            }
            return newData;
        }
    }
}
