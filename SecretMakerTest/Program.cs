using System;
using SecretStringMaker;

namespace SecretMakerTest
{
    class Program
    {
        static void Main()
        {

            Console.WriteLine(
                " You are running a testing application. It is designed to expose any weakness in a\n" +
                " specific method used to generate random secrets (character strings) of a given length.\n" +
                " The first step of the test consists of generating multiple strings and tabulating the\n" +
                " individual character locations within the strings. The second step consists of displaying\n" +
                " the last secret generated; then, calculating and displaying the standard deviations of each\n" +
                " character's total usage and distribution within the generated stings.\n");

            while (true)
            {
                try
                {
                    Console.WriteLine(" Enter number of strings to generate: (default is 100)  (ctrl-c to quit)");
                    string reps = Console.ReadLine();
                    Console.WriteLine(" Enter the length of generated string: (default is 92)  (ctrl - c to quit");
                    string secretLength = Console.ReadLine();
                    var runner = new TestRunner();

                    if (reps == "" && secretLength == "")
                    {
                        runner.runTest();
                    }
                    else if (secretLength == "")
                    {
                        runner.runTest(repetitions: int.Parse(reps));
                    }
                    else if (reps == "")
                    {
                        runner.runTest(secretStringLength: int.Parse(secretLength));
                    }
                    else
                    {
                        runner.runTest(int.Parse(reps), int.Parse(secretLength));
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }

                Console.WriteLine("\n ctrl-c to quit");
            }
        }
    }
}
