# CryptoStrengthRandomStringGenerator
Generates cryptographic strength random strings (secrets / keys / passwords ); written in C#. The string generated is insured to have at least one character from each of four separate sub-pools of characters; numbers, lower case, uppercase, and special.  The minimum length of a string is four characters.  File CryptoRandomIndexProvider.cs contains the CryptoRandomIndexProvider class that is the source used to provide cryptographic stingth random numbers for the random character selection.

# CryptoRandomIndexProvider
A library utility that generates a cryptographic strength random number in the range of the arguments supplied.  
Discussion: This class uses the .NET RNGCryptoServiceProvider as its random number source. There is a problem with this source as it only returns byte chunks of random bits.  This can introduce a bias into a solution when using the bytes as an index into a range.  (ref: http://stackoverflow.com/questions/32932679/using-rngcryptoserviceprovider-to-generate-random-string)  This issue is overcome by using a subset of the random returned bits that is large enough to hold the range needed.  When a returned subset has a numerical value larger than the range, it is discarded and a new random set is retrieved.

The program.cs file in the SecretMakerTest folder is a command line program that exercises RandomStringGenerator.  Open SecretStringMaker.sln in VisualStudio.  Open the solution's  properties and on the left select: "Common Properties/Startup Project".  Then on the right select: "Single startup project/SecretMakerTest.  Start Debugging.  The solution will compile then open a command prompt.  Answer the first two questions and you will be presented with a random string and a ton of statistics.  Please enjoy.

