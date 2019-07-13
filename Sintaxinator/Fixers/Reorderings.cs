using System;

namespace Sintaxinator.Fixers
{
    public class Reorderings
    {
        public static byte[] GetSintaxBankReorderings(byte? reorderMode)
        {
            // these are from hhugboy
            byte[] reordering00 = {0,7,2,1,4,3,6,5};
            byte[] reordering01 = {7,6,1,0,3,2,5,4};
            byte[] reordering05 = {0,1,6,7,4,5,2,3};
            byte[] reordering07 = {6,7,4,5,2,3,0,1};
            byte[] reordering09 = {3,2,5,4,7,6,1,0};
            byte[] reordering0b = {5,4,7,6,1,0,3,2};
            byte[] reordering0d = {6,7,0,1,2,3,4,5};
            byte[] noReordering = {0,1,2,3,4,5,6,7};
    
            switch(reorderMode & 0x0f) {
                case 0x0D:
                    return reordering0d;
                case 0x09:
                    return reordering09;
                case 0x00:
                    return reordering00;
                case 0x01:
                    return reordering01;
                case 0x05:
                    return reordering05;
                case 0x07:
                    return reordering07;
                case 0x0B:
                    return reordering0b;
                case 0x0F:
                    return noReordering;
                default:
                    throw new Exception("unsupported reordering type");
            }
        }
        
        public static byte[] GetBbdDataReorderings(byte? reorderMode)
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

        public static byte[] GetBbdBankReorderings(byte? reorderMode)
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