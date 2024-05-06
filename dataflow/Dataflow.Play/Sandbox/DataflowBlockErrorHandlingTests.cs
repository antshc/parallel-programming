using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace Dataflow.Play.Sandbox;

public class DataflowBlockErrorHandlingTests
{
    private readonly ITestOutputHelper _testOutHelper;

    public DataflowBlockErrorHandlingTests(ITestOutputHelper testOutHelper)
        => _testOutHelper = testOutHelper;

    [Fact]
    public void ActionBlockErrorTest()
    {
        var transformBlock = new TransformBlock<int, int>((n) =>
        {
            _testOutHelper.WriteLine($"Transform: {n}");
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
            return n * 1;
        });

        ActionBlock<int> throwIfNegative = new ActionBlock<int>(n =>
        {
            _testOutHelper.WriteLine("n = {0}", n);
            if (n < 0)
            {
                throw new ArgumentOutOfRangeException();
            }
        });

        transformBlock.LinkTo(throwIfNegative);

        // Create a continuation task that prints the overall
        // task status to the console when the block finishes.
        transformBlock.Completion.ContinueWith(task =>
        {
            _testOutHelper.WriteLine("The status of the completion task is '{0}'.",
               task.Status);
        });

        transformBlock.Post(0);
        transformBlock.Post(-1);
        transformBlock.Post(1);
        transformBlock.Post(-2);
        transformBlock.Complete();

        try
        {
            transformBlock.Completion.Wait();
        }
        catch (AggregateException ae)
        {
            // If an unhandled exception occurs during dataflow processing, all
            // exceptions are propagated through an AggregateException object.
            ae.Handle(e =>
            {
                _testOutHelper.WriteLine("Encountered {0}: {1}",
                   e.GetType().Name, e.Message);
                return true;
            });
        }
    }
}
