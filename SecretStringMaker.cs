using System;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;

namespace SecretStringMaker
{

    /// Generates cryptographic strength random strings (secrets). RNGCryptoServiceProvider is used as the
    /// random index source. The string generated is insured to have at least one char from each of four separate 
    /// sub-pools of characters; numbers, lower case, uppercase, and special.  
    /// 
    /// Discussion: There is a problem with the random source as it only returns byte chunks of random bits. This
    /// can introduce a bias into a solution when using the bytes as an index into a selection string. 
    /// (ref: http://stackoverflow.com/questions/32932679/using-rngcryptoserviceprovider-to-generate-random-string)
    /// This issue is overcome by using a subset of the random returned bits. The subset must be large enough to
    /// hold the largest index needed.  Any returned subset that has a numerical value larger than the max index
    /// is discarded and a new random set is retrieved.
    /// 
    /// </summary>
    public class SecretMaker
    {
        public const string numberPool = "1234567890";
        public const string lowerPool = "abcdefghijklmnopqrstuvwxyz";
        public const string upperPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        // specialCharactersPool excludes: space - " ", double quote -'"', back slash - "\", and DEL - "0x7F 
        public const string specialPool = "!#$%&'()*+,-./:;<=>?@[]^_`{|}~";
        public const string allCharsPool = numberPool + lowerPool + upperPool + specialPool;
        private int numberPoolEnd = numberPool.Length - 1;
        private int lowerPoolEnd = numberPool.Length + lowerPool.Length - 1;
        private int upperPoolEnd = numberPool.Length + lowerPool.Length + upperPool.Length - 1;

        private int bitsToShift = 0;
        private byte[] twoBytes = new byte[2];
        private ushort randomIndex;

        private int minSecretLength = 4;
        private int secretStringLength;

        private int numberOfPoolsCompensated = 0;
        private bool numberPoolCompensated = false;
        private bool lowerPoolCompensated = false;
        private bool upperPoolCompensated = false;
        private bool specialPoolCompensated = false;
        private bool allPoolsCompensated
        {
            get { return numberOfPoolsCompensated < minSecretLength ? false : true; }
        }

        /// <summary>
        /// Constructor: Sets length of generated secrets to the same length as the selection pool.
        /// </summary>
        public SecretMaker()
        {
            secretStringLength = allCharsPool.Length;
        }

        /// <summary>
        /// Constructor: Sets length of generated secrets. Minimum length is 4. Maximum length is 65536 chars
        /// </summary>
        /// <param name="length">Desired length of new secret.</param>
        public SecretMaker(int length)
        {
            if (length < minSecretLength)
            {
                throw new Exception($"String length too short; must be {minSecretLength} or more characters.");
            }
            else if (length > ushort.MaxValue)
            {
                throw new Exception($"String length too long; cannot exceed {ushort.MaxValue} characters.");
            }
            else
            {
                secretStringLength = length;
            }
        }

        /// <summary>
        ///     Assembles a randomly generated string from the characters provided by allChars string.
        ///     The secret is assured to have at least one instance of a character from each of the
        ///     four sub-pools: integers, uppercase letters, lowercase letters, and special characters.
        /// </summary>
        /// <returns>Returns a string of length "length" with a minimum length of 4.</returns>
        public string getNewSecret()
        {
            // Initialize global variables.
            numberPoolCompensated = false;
            lowerPoolCompensated = false;
            upperPoolCompensated = false;
            specialPoolCompensated = false;
            numberOfPoolsCompensated = 0;

            // Important: Type RNGCryptoServiceProvider uses IDisposable interface.
            // The following 'using' construct insures that it will be disposed.  
            using (RNGCryptoServiceProvider rngProvider = new RNGCryptoServiceProvider())
            {
                // Object where the secret is assembled.
                StringBuilder sBuilder = new StringBuilder(secretStringLength);

                // Add one char from each of the four sub-pools ensuring the secret
                // contains at least one char from each sub-pool.
                addCharsToBuilder(numberPool, 1, sBuilder, rngProvider, false);
                addCharsToBuilder(lowerPool, 1, sBuilder, rngProvider, false);
                addCharsToBuilder(upperPool, 1, sBuilder, rngProvider, false);
                addCharsToBuilder(specialPool, 1, sBuilder, rngProvider, false);

                // Add the remaining chars from allCharsPool.
                // Note: Bias introduce in previous steps is mitigated in this step.
                addCharsToBuilder(allCharsPool, secretStringLength - minSecretLength, sBuilder, rngProvider, true);

                // Shuffle the string to randomly relocate the first four chars added.
                shuffle(sBuilder, rngProvider);

                // Return the assembled string.
                return sBuilder.ToString();
            }
        }


        /// <summary>
        ///     Adds randomly selected chars from a selection pool to the string builder.
        /// </summary>
        /// <param name="pool">Selection pool</param>
        /// <param name="numberOfCharsToAdd">Number of chars to add</param>
        /// <param name="builder">string builder chars are added to</param>
        /// <param name="provider">random generator provider</param>
        /// <prram name="compensate">Flag to check if compensation is necessary</prram>
        private void addCharsToBuilder(string pool, int numberOfCharsToAdd, StringBuilder builder, RNGCryptoServiceProvider provider, bool compensate)
        {
            bitsToShift = getBitsToShift(pool.Length);

            for (int i = 0; i < numberOfCharsToAdd; i++)
            {
                // load randomIndex with a random number.
                processRandomIndex(provider, pool.Length, bitsToShift);

                // Discussion: The program selects one random character from each of the four sub-strings to ensure
                // the secret generated contains a character from each of the sub-strings. This introduces a bias that
                // disproportionately generates more characters from shorter sub-strings than longer ones. The bias
                // can be significant and increases with shorter pools and shorter generated secrets. The bias can be
                // reduced by ignoring the next reference to each sub-string after the initial selections. The bias
                // becomes insignificant with secrets of 64 characters or more.

                // The following code implements a trap that bypasses the second instance of a character being selected
                // from each of the four sub-strings. See discussion above.
                if (!allPoolsCompensated && compensate)
                {
                    if (randomIndex <= numberPoolEnd)
                    {
                        if (!numberPoolCompensated)
                        {
                            numberPoolCompensated = true;
                            numberOfPoolsCompensated++;
                            i--;
                            continue;
                        }
                    }
                    else if (randomIndex <= lowerPoolEnd)
                    {
                        if (!lowerPoolCompensated)
                        {
                            lowerPoolCompensated = true;
                            numberOfPoolsCompensated++;
                            i--;
                            continue;
                        }
                    }
                    else if (randomIndex <= upperPoolEnd)
                    {
                        if (!upperPoolCompensated)
                        {
                            upperPoolCompensated = true;
                            numberOfPoolsCompensated++;
                            i--;
                            continue;
                        }
                    }
                    else if (true) // (randomSort <= specialPoolEnd)
                    {
                        if (!specialPoolCompensated)
                        {
                            specialPoolCompensated = true;
                            numberOfPoolsCompensated++;
                            i--;
                            continue;
                        }
                    }
                }

                // Add the selected character to the builder.
                builder.Append(pool[randomIndex]);
            }
        }

        
        // Finds the minimum number of bits required to hold an integer large enough to index into a string
        // of length 'length' then returns the number of bits a ushort has in excess of the required bits.
        private int getBitsToShift(int length)
        {
            var bits = 15;
            while ((length >>= 1) > 0)
            {
                bits--;
            }
            return bits;

        //    return 16 - (int)Math.Ceiling(Math.Log(length, 2));
        }

        /// Performs an in place shuffle on a StringBuilder's array.
        /// The stack of randomly selected chars builds from the end of the array.
        /// The top of the stack tracks with i as i gets smaller.
        /// </summary>
        /// <param name="builder">string builder that gets shuffled</param>
        /// <param name="provider">random generator provider</param>
        private void shuffle(StringBuilder builder, RNGCryptoServiceProvider provider)
        {
            char copiedChar;

            for (int i = builder.Length - 1; i > 0; i--)
            {
                // Get a random number in the range of 0 to i.
                processRandomIndex(provider, i, getBitsToShift(i + 1));

                // Copy the char at i making room above the stack for the randomly selected char.
                copiedChar = builder[i];

                // Grab the char at that random position and place it on the stack.
                builder[i] = builder[randomIndex];

                // Place the char that was at i in the position the random char came from.
                builder[randomIndex] = copiedChar;
            }
        }

        /// <summary>
        ///     Reduces unsigned integer value in global variable 'randomIndex'
        /// </summary>
        /// <param name="provider">Random generator provider</param>
        /// <param name="length">Length of the string that variable randomIndex will operate on</param>
        /// <param name="bitsToShift">Value used to reduce effective bits in variable randomIndex </param>
        private void processRandomIndex(RNGCryptoServiceProvider provider, int length, int bitsToShift)
        {
            // Keep trying until we get a random number in range of the strings length.;
            do
            {
                provider.GetBytes(twoBytes);
                randomIndex = BitConverter.ToUInt16(twoBytes, 0);

                // Collapse the randomShort so we use only the necessary number of bits.
                // The number is smaller and still random. Helps the do-while loop.
                randomIndex >>= bitsToShift;
            }
            while (randomIndex >= length);
        }
    }
}
