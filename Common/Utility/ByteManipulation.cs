using System.Collections.Generic;
using System.Linq;

namespace Common.Utility
{
    public class ByteManipulation
    {
        public static byte ReorderBits(byte input, byte[] reorder, bool reverse = false)
        {
            Dictionary<byte, byte> reorderingMap = reorder   
                .Select((v, i) => new {Key = reverse ? v : (byte)i, Value = reverse ? (byte)i : v})
                .ToDictionary(kv => kv.Key, kv => kv.Value);
            
            byte newbyte = 0;
            for (byte x = 0; x < 8; x++)
            {
                newbyte += (byte)( (byte)((input >> (7 - reorderingMap[x])) & 1) << (byte)(7 - x) );
            }

            return newbyte;
        }
    }
}