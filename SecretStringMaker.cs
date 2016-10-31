using System;
using System.Text;

namespace SecretStringMaker
{

    /// <summary>
    ///     Generates cryptographic strength random strings(secrets). RNGCryptoServiceProvider is used as the
    ///     random index source.  The string generated is insured to have at least one char from each of four
    ///     separate sub-pools of characters; numbers, lower case, uppercase, and special.
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
        private int secretStringLength;
        private int minSecretLength = 4;

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
        /// Sets length of generated secrets to the same length as the selection pool.
        /// </summary>
        public SecretMaker()
        {
            secretStringLength = allCharsPool.Length;
        }

        /// <summary>
        /// Sets length of generated secrets. any argument less than 4 will return a sting of length 4.
        /// </summary>
        /// <param name="length">Desired length of new secret.</param>
        public SecretMaker(int length)
        {
            if ( length < minSecretLength )
            {
                secretStringLength = minSecretLength;
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

            // Object where the secret is assembled.
            var sBuilder = new StringBuilder(secretStringLength);

            // Object that generates the random indexes.
            var newRandomSource = new CryptoRandomIndexProvider();

            // Add one char from each of the four sub-pools ensuring the secret
            // contains at least one char from each sub-pool.
            addCharsToBuilder(numberPool, 1, sBuilder, newRandomSource, false);
            addCharsToBuilder(lowerPool, 1, sBuilder, newRandomSource, false);
            addCharsToBuilder(upperPool, 1, sBuilder, newRandomSource, false);
            addCharsToBuilder(specialPool, 1, sBuilder, newRandomSource, false);

            // Add the remaining chars from allCharsPool.
            // Note: Bias introduce in previous steps is mitigated in this step.
            addCharsToBuilder(allCharsPool, secretStringLength - minSecretLength, sBuilder, newRandomSource, true);

            // Shuffle the entire string randomly relocating the first four chars added.
            shuffle(sBuilder, newRandomSource);

            // Return the assembled string.
            return sBuilder.ToString();
        }

        /// <summary>
        ///     Adds randomly selected chars from a selection pool to the string builder.
        /// </summary>
        /// <param name="pool">Selection pool</param>
        /// <param name="numberOfCharsToAdd">Number of chars to add</param>
        /// <param name="builder">string builder chars are added to</param>
        /// <param name="provider">random generator provider</param>
        /// <prram name="compensate">Flag to check if compensation is necessary</prram>
        private void addCharsToBuilder(
            string pool, 
            int numberOfCharsToAdd, 
            StringBuilder builder, 
            CryptoRandomIndexProvider provider, 
            bool compensate)
        {

            for (int i = 0; i < numberOfCharsToAdd; i++)
            {
                // load randomIndex with a random number.
                var randomIndex = provider.getRandomIndex(pool.Length-1);

                // Discussion: The program starts by selecting one random character from each of the four sub-strings.  This
                // ensures the secret generated will contain at least one character from each of the sub-strings.  This, however
                // introduces a bias.  Disproportionately more characters are generated from the shorter sub-strings than the 
                // longer ones.  The bias can be significant and increases with shorter pools and shorter generated secrets.  The
                // bias is reduced by ignoring the next reference to each sub-string after the initial selections.  The bias
                // becomes insignificant falling below three sigma when generating secrets of 64 characters or more.

                // The following block of code implements a trap that bypasses the second instance of a character being selected
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

        /// Performs an in place shuffle on a StringBuilder's array.
        /// The stack of randomly selected chars builds from the end of the array.
        /// The top of the stack tracks with i as i gets smaller.
        /// </summary>
        /// <param name="builder">string builder that gets shuffled</param>
        /// <param name="provider">random generator provider</param>
        private void shuffle(StringBuilder builder, CryptoRandomIndexProvider provider)
        {
            char copiedChar;

            for (int i = builder.Length - 1; i > 1; i--)
            {
                // Get a random number in the range of 0 to i.
                //processRandomIndex(provider, i, getBitsToShift(i + 1));
                var randomIndex = provider.getRandomIndex(i-1);

                // Copy the char at i making room above the stack for the randomly selected char.
                copiedChar = builder[i];

                // Grab the char at that random position and place it on the stack.
                builder[i] = builder[randomIndex];

                // Place the char that was at i in the position the random char came from.
                builder[randomIndex] = copiedChar;
            }
        }
    }
}
