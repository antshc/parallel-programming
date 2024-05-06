using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace Dataflow.Tests;

public class ParallelismTest
{
    private readonly ITestOutputHelper _testOutput;

    public ParallelismTest(ITestOutputHelper testOutput)
    {
        _testOutput = testOutput;
    }

    [Fact]
    public void Test()
    {
        int processorCount = Environment.ProcessorCount;
        int messageCount = processorCount;

        // Print the number of processors on this computer.
        _testOutput.WriteLine("Processor count = {0}.", processorCount);

        TimeSpan elapsed;

        // Perform two dataflow computations and print the elapsed
        // time required for each.

        // This call specifies a maximum degree of parallelism of 1.
        // This causes the dataflow block to process messages serially.
        elapsed = TimeDataflowComputations(1, messageCount);
        _testOutput.WriteLine("Degree of parallelism = {0}; message count = {1}; " +
           "elapsed time = {2}ms.", 1, messageCount, (int)elapsed.TotalMilliseconds);

        // Perform the computations again. This time, specify the number of
        // processors as the maximum degree of parallelism. This causes
        // multiple messages to be processed in parallel.
        elapsed = TimeDataflowComputations(processorCount, messageCount);
        _testOutput.WriteLine("Degree of parallelism = {0}; message count = {1}; " +
           "elapsed time = {2}ms.", processorCount, messageCount, (int)elapsed.TotalMilliseconds);
    }

    static TimeSpan TimeDataflowComputations(int maxDegreeOfParallelism,
   int messageCount)
    {
        // Create an ActionBlock<int> that performs some work.
        var workerBlock = new ActionBlock<int>(
           // Simulate work by suspending the current thread.
           millisecondsTimeout => Thread.Sleep(millisecondsTimeout),
           // Specify a maximum degree of parallelism.
           new ExecutionDataflowBlockOptions
           {
               MaxDegreeOfParallelism = maxDegreeOfParallelism
           });

        // Compute the time that it takes for several messages to
        // flow through the dataflow block.

        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < messageCount; i++)
        {
            workerBlock.Post(1000);
        }
        workerBlock.Complete();

        // Wait for all messages to propagate through the network.
        workerBlock.Completion.Wait();

        // Stop the timer and return the elapsed number of milliseconds.
        stopwatch.Stop();
        return stopwatch.Elapsed;
    }
}
