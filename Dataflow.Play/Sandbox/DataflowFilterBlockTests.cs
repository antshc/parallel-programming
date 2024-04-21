using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace Dataflow.Play.Sandbox;

public class DataflowFilterBlockTests
{
    private readonly ITestOutputHelper _output;

    public DataflowFilterBlockTests(ITestOutputHelper output)
        => _output = output;

    [Fact]
    public void FilterBlockTest()
    {
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var queueBlock = new BufferBlock<int>();

        var catHandlerBlock = new TransformBlock<int, int>((cat) =>
        {
            Thread.Sleep(1000);
            _output.WriteLine($"Cat: {cat}");
            return cat;
        });

        var dogHandlerBlock = new TransformBlock<int, int>((dog) =>
        {
            Thread.Sleep(1000);
            _output.WriteLine($"Dog: {dog}");
            return dog;
        });

        var finishHandlerBlock = new ActionBlock<int>((animal) =>
        {
            _output.WriteLine($"Finish animal: {animal}");
        });
        queueBlock.LinkTo(catHandlerBlock, linkOptions, (animal) => animal == 1);
        queueBlock.LinkTo(dogHandlerBlock, linkOptions, (animal) => animal == 2);

        catHandlerBlock.LinkTo(finishHandlerBlock, linkOptions);
        dogHandlerBlock.LinkTo(finishHandlerBlock, linkOptions);

        queueBlock.Post(1);
        queueBlock.Post(1);
        queueBlock.Post(2);
        queueBlock.Post(2);
        queueBlock.Post(1);
        queueBlock.Post(2);
        queueBlock.Complete();

        Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
        finishHandlerBlock.Completion.Wait();

        stopWatch.Stop();
        TimeSpan ts = stopWatch.Elapsed;
        // Format and display the TimeSpan value.
        string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
            ts.Hours, ts.Minutes, ts.Seconds,
            ts.Milliseconds / 10);
        _output.WriteLine("RunTime " + elapsedTime);
    }
}
