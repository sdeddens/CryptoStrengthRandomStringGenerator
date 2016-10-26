using System;
using SecretStringMaker;
using System.Diagnostics;

namespace SecretMakerTest
{
    class Program
    {
        static void Main()
        {
            Stopwatch stopWatch = new Stopwatch();

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
                    stopWatch.Start();
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

                stopWatch.Stop();
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = stopWatch.Elapsed;

                // Format and display the TimeSpan value.
                string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                    ts.Hours, ts.Minutes, ts.Seconds,
                    ts.Milliseconds / 10);
                Console.WriteLine(" RunTime: " + elapsedTime);
                stopWatch.Reset();

                Console.WriteLine("\n ctrl-c to quit");
            }
        }
    }
}
