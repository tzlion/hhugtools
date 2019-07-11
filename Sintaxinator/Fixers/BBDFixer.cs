using System;
using System.Linq;
using Common.Utility;
using Common.Rom;

namespace Sintaxinator.Fixers
{
    class BBDFixer : RomManipulator
    {
        public BBDFixer(string inputFilename, string outputFilename) : base(inputFilename, outputFilename) { }

        public void ReorderAllBytes(byte[] reordering)
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

        public byte[] getBBDDataReorderings(byte? reorderMode)
        {
            // these are from hhugboy
            byte[] noReordering = {0,1,2,3,4,5,6,7};
            byte[] reordering04 = {0,1,5,3,4,6,2,7};
            byte[] reordering05 = {0,1,2,6,4,5,3,7};
            byte[] reordering07 = {0,1,5,3,4,2,6,7};
            
            switch(reorderMode & 0x07) {
                case 0x00:
                    return noReordering;
                case 0x04:
                    return reordering04;
                case 0x05:
                    return reordering05;
                case 0x07:
                    return reordering07;
                default:
                    throw new Exception("unsupported reordering type");
            }
        }

        public byte[] getBBDBankReorderings(byte? reorderMode)
        {
            // these are from hhugboy
            byte[] noReordering = {0,1,2,3,4,5,6,7};
            byte[] reordering03 = {0,1,2,6,7,5,3,4};
            byte[] reordering05 = {0,1,2,7,3,4,5,6};
            
            switch(reorderMode & 0x07) {
                case 0x00:
                    return noReordering;
                case 0x03:
                    return reordering03;
                case 0x05:
                    return reordering05;
                default:
                    throw new Exception("unsupported reordering type");
            }
        }

    }
}
