using Bogus;
using Core.Logger;
using Core.Simulators;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;

namespace ParallelProcessingByCategory.App;

public class Program
{
    enum Category
    {
        Cat,
        Dog,
        Fish
    }

    record QueueItem(Guid Id, Category Category);

    public static void Main(string[] args)
    {
        var categoryPipelinesPool = new ConcurrentDictionary<Category, Category>();
        var _output = new ConsoleOutputLogger();
        var linkOptions = new DataflowLinkOptions { PropagateCompletion = true };
        var queueBlock = new BufferBlock<QueueItem>();
        var animalCategoryQueueBlock = new BufferBlock<QueueItem>();

        var producer = CreateMessageSimulator(queueBlock);
        producer.Start();

        var consumerTask = Task.Run(async () =>
        {
            await foreach (QueueItem queueItem in queueBlock.ReceiveAllAsync())
            {
                if (categoryPipelinesPool.TryAdd(queueItem.Category, queueItem.Category))
                {
                    _output.Log($"Link Category: {queueItem.Category}");
                    var animalHandlerBlock = CreateAnimalCategoryPipeline(_output, linkOptions);
                    animalCategoryQueueBlock.LinkTo(animalHandlerBlock, linkOptions, (animal) => animal.Category == queueItem.Category);
                }
                animalCategoryQueueBlock.Post(queueItem);
            }
        });

        consumerTask.Wait();

        Console.WriteLine("Click Enter to close.");
        Console.ReadLine();
    }

    private static ActionBlock<QueueItem> CreateAnimalCategoryPipeline(ConsoleOutputLogger _output, DataflowLinkOptions linkOptions)
    {
        var animalHandlerBlock = new ActionBlock<QueueItem>((animal) =>
          {
              Thread.Sleep(1000);
              _output.Log($"Handle message from Category: {animal.Category}, Id: {animal.Id}");
          });
        return animalHandlerBlock;
    }

    private static MessageReceiverSimulator CreateMessageSimulator(ITargetBlock<QueueItem> queue)
    {
        MessageReceiverSimulator _receiptCreationSimulator = new MessageReceiverSimulator(2000);
        _receiptCreationSimulator.Elapsed += (sender, e) =>
        {
            var qi = new QueueItem(Guid.NewGuid(), new Faker().PickRandom<Category>());
            Console.WriteLine($"Post: {qi}");
            queue.Post(qi);
        };
        return _receiptCreationSimulator;
    }
}