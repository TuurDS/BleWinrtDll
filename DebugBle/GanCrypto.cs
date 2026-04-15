using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DebugBle
{
    public static class GanCrypto
    {
        // ORIGINAL LZSTRING COMPRESSED KEYS:
        // "NoRgnAHANATADDWJYwMxQOxiiEcfYgSK6Hpr4TYCs0IG1OEAbDszALpA"
        // "NoNg7ANATFIQnARmogLBRUCs0oAYN8U5J45EQBmFADg0oJAOSlUQF0g"
        private static readonly byte[][] KEYS = new byte[][]
        {
        new byte[] { 198, 202, 21, 223, 79, 110, 19, 182, 119, 13, 230, 89, 58, 175, 186, 162 },
        new byte[] { 67, 226, 91, 214, 125, 220, 120, 216, 7, 96, 163, 218, 130, 60, 1, 241 }
        };

        //  Derive AES key
        public static byte[] DeriveKey(byte[] fwBytes, byte[] hwBytes)
        {
            int version = (fwBytes[0] << 16) | (fwBytes[1] << 8) | fwBytes[2];
            int keyIndex = (version >> 8) & 0xFF;

            if (keyIndex >= KEYS.Length)
                throw new Exception($"Unsupported GAN cube version: {version:X}");

            byte[] key = (byte[])KEYS[keyIndex].Clone();

            for (int i = 0; i < 6; i++)
            {
                key[i] = (byte)((key[i] + hwBytes[5 - i]) & 0xFF);
            }

            return key;
        }

        private static byte[] DecryptBlock(ICryptoTransform dec, byte[] block)
        {
            return dec.TransformFinalBlock(block, 0, block.Length);
        }

        public static byte[] Decode(byte[] value, byte[] key)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = key;
                aes.Mode = CipherMode.ECB;
                aes.Padding = PaddingMode.None;

                using (var dec = aes.CreateDecryptor())
                {
                    byte[] ret = new byte[value.Length];
                    Buffer.BlockCopy(value, 0, ret, 0, value.Length);

                    // --- decrypt last 16 bytes if length > 16 ---
                    if (ret.Length > 16)
                    {
                        byte[] lastBlock = new byte[16];
                        Buffer.BlockCopy(ret, ret.Length - 16, lastBlock, 0, 16);

                        byte[] decryptedLast = DecryptBlock(dec, lastBlock);

                        Buffer.BlockCopy(decryptedLast, 0, ret, ret.Length - 16, 16);
                    }

                    // --- decrypt first 16 bytes (or full first block) ---
                    if (ret.Length >= 16)
                    {
                        byte[] firstBlock = new byte[16];
                        Buffer.BlockCopy(ret, 0, firstBlock, 0, 16);

                        byte[] decryptedFirst = DecryptBlock(dec, firstBlock);

                        Buffer.BlockCopy(decryptedFirst, 0, ret, 0, 16);
                    }

                    return ret;
                }
            }
        }
    }
}
