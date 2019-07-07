using System;
using System.Security.Cryptography;

namespace Common.Utility
{
    class Hashing
    {
        public static string GetMd5Hash(byte[] data)
        {
            byte[] computedHash = new MD5CryptoServiceProvider().ComputeHash(data);
            return BitConverter.ToString(computedHash).Replace("-", "").ToLower();
        }
    }
}
