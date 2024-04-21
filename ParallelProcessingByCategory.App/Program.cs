using Bogus;
using Core.Logger;
using Core.Simulators;
using System.Threading.Tasks.Dataflow;

namespace ParallelProcessingByCategory.App;

public interface IJobPipeline<TQueueItem>
{
    void Post(TQueueItem item);
}

public class PlayWithAnimalsJobPipeline : IJobPipeline<QueueItem>
{
    private static readonly object _lockObj = new object();
    private readonly HashSet<Category> highPriorityCategoryRegister = new HashSet<Category>();
    private readonly HashSet<Category> defaultPriorityCategoryRegister = new HashSet<Category>();
    private readonly HashSet<Priority> priorityRegister = new HashSet<Priority>();
    private BufferBlock<QueueItem> _pipeline;
    private BufferBlock<QueueItem> _defaultPriority;
    private BufferBlock<QueueItem> _highPriority;
    private PlayWithAnimalsService _service;

    public PlayWithAnimalsJobPipeline(PlayWithAnimalsService service)
    {
        _pipeline = new BufferBlock<QueueItem>();
        _defaultPriority = new BufferBlock<QueueItem>();
        _highPriority = new BufferBlock<QueueItem>();
        _service = service;
    }

    public void Post(QueueItem animal)
    {
        lock (_lockObj)
        {
            BuildDynamicPipeline(_pipeline, animal);
        }

        _pipeline.Post(animal);
    }

    private void BuildDynamicPipeline(BufferBlock<QueueItem> pipeline, QueueItem animal)
    {
        if (!priorityRegister.Contains(Priority.High))
        {
            pipeline.LinkTo(_highPriority, q => q.Priority == Priority.High);
            priorityRegister.Add(Priority.High);
            Console.WriteLine($"Register priority: High");
        }

        if (!priorityRegister.Contains(Priority.Default))
        {
            pipeline.LinkTo(_defaultPriority, q => q.Priority == Priority.Default);
            priorityRegister.Add(Priority.Default);
            Console.WriteLine($"Register priority: Default");
        }

        if (!highPriorityCategoryRegister.Contains(animal.Category))
        {
            var batchBlock = new BatchBlock<QueueItem>(5);
            _highPriority.LinkTo(batchBlock, it => it.Category == animal.Category);
            var categoryActionBlock = new ActionBlock<QueueItem[]>(_service.Play);
            batchBlock.LinkTo(categoryActionBlock);
            highPriorityCategoryRegister.Add(animal.Category);
            Console.WriteLine($"Register priority: High, Category: {animal.Category}");
        }

        if (!defaultPriorityCategoryRegister.Contains(animal.Category))
        {
            var batchBlock = new BatchBlock<QueueItem>(5);
            _defaultPriority.LinkTo(batchBlock, it => it.Category == animal.Category);
            var categoryActionBlock = new ActionBlock<QueueItem[]>(_service.Play);
            batchBlock.LinkTo(categoryActionBlock);
            defaultPriorityCategoryRegister.Add(animal.Category);
            Console.WriteLine($"Register priority: Default, Category: {animal.Category}");
        }
    }
}

public class PlayWithAnimalsService
{
    public Task Play(QueueItem[] animals)
    {
        foreach (var animal in animals)
        {
            Console.WriteLine($"Handle message from Category: {animal.Category}, Id: {animal.Id}");
        }

        return Task.CompletedTask;
    }
}

public enum Priority
{
    High,
    Default
}

public enum Category
{
    Cat,
    Dog,
    Fish
}

public record QueueItem(Guid Id, Category Category, Priority Priority);
public record BatchQueueItem(IEnumerable<QueueItem> QueueItems);

public class Program
{
    public static void Main(string[] args)
    {
        var queueBlock = new PlayWithAnimalsJobPipeline(new PlayWithAnimalsService());

        var producer = CreateMessageSimulator(queueBlock);
        producer.Start();

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

    private static MessageReceiverSimulator CreateMessageSimulator(IJobPipeline<QueueItem> queue)
    {
        MessageReceiverSimulator _receiptCreationSimulator = new MessageReceiverSimulator(2000);
        _receiptCreationSimulator.Elapsed += (sender, e) =>
        {
            var qi = new QueueItem(Guid.NewGuid(), new Faker().PickRandom<Category>(), new Faker().PickRandom<Priority>());
            Console.WriteLine($"Post: {qi}");
            queue.Post(qi);
        };
        return _receiptCreationSimulator;
    }
}