using System;
using System.Security.Cryptography;

namespace SecretStringMaker
{
    // Discussion: There is a problem with the random source as it only returns byte chunks of random bits. This
    // can introduce a bias into a solution when using the bytes as an index into a selection string. 
    // (ref: http://stackoverflow.com/questions/32932679/using-rngcryptoserviceprovider-to-generate-random-string)
    // This issue is overcome by using a subset of the returned random bits. The subset must be large enough to
    // hold the largest index needed.  Any returned subset that has a numerical value larger than the max index
    // is discarded and a new random set is retrieved.

    /// <summary>
    ///     Implements a cryptographic strength random index generator using
    ///     RNGCryptoServiceProvider as the random byte source.
    /// </summary>
    public class CryptoRandomIndexProvider
    {

        /// <summary>
        ///     Returns an int32 value in the inclusive range of firstValue to secondValue;
        /// </summary>
        /// <param name="firstValue">Any Int32 value</param>
        /// <param name="secondValue">Any Int32 value</param>
        /// <returns></returns>
        public int getRandomIndex(int firstValue, int secondValue = 0)
        {
            // Use Int64 because value of range can overflow Int32.
            long range = firstValue - (long)secondValue;

            // Assuming Abs.range is < 2, significant zero bits of uint will be 31.
            int shiftBitsRight = 31;

            // Using absolute value removes any high order twos-compliment bit.
            // Impossible for absolute value of range to overflow a uint.
            uint absRange = (uint)Math.Abs(range);
            uint testRange = absRange;
            uint random;

            // Make a place to park 32 bits
            byte[] fourBytes = new byte[4];

            // Use testRange to find significant zero bits in absRange;
            while ((testRange >>= 1) > 0)
            {
                // Each time testRange is found to be larger, the significant zero bits decrease.
                shiftBitsRight--;
            }

            // Set up the byte provider.
            // Important: Type RNGCryptoServiceProvider uses IDisposable interface.
            // The following 'using' construct insures that it will be disposed.  
            using (RNGCryptoServiceProvider byteSource = new RNGCryptoServiceProvider())
            {
                // Get random.
                do
                {
                    byteSource.GetBytes(fourBytes);
                    random = BitConverter.ToUInt32(fourBytes, 0);
                    
                    // Cutting random "down to size" gives each attempt at least a 50% chance of succeeding.
                    random >>= shiftBitsRight;
                }
                // Keep trying until our random number is <= range;
                while (random > absRange);
            }

            return (firstValue > secondValue) ? secondValue + (int)random : firstValue + (int)random;
        }
    }
}
