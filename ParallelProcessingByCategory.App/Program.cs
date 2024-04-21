using Bogus;
using Core.Simulators;
using System.Threading.Tasks.Dataflow;

namespace ParallelProcessingByCategory.App;

public interface IJobPipeline<TQueueItem>
{
    void Post(TQueueItem item);
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

public record Animal(Guid Id);
public record BatchQueueItem(Category Category, Priority Priority, IEnumerable<Animal> Animals);

public class PlayWithAnimalsJobPipeline : IJobPipeline<BatchQueueItem>
{
    private static readonly object _lockObj = new object();
    private readonly HashSet<Category> highPriorityCategoryRegister = new HashSet<Category>();
    private readonly HashSet<Category> defaultPriorityCategoryRegister = new HashSet<Category>();
    private readonly HashSet<Priority> priorityRegister = new HashSet<Priority>();
    private BufferBlock<BatchQueueItem> _pipeline;
    private BufferBlock<BatchQueueItem> _defaultPriority;
    private BufferBlock<BatchQueueItem> _highPriority;
    private PlayWithAnimalsService _service;

    public PlayWithAnimalsJobPipeline(PlayWithAnimalsService service)
    {
        _pipeline = new BufferBlock<BatchQueueItem>();
        _defaultPriority = new BufferBlock<BatchQueueItem>();
        _highPriority = new BufferBlock<BatchQueueItem>();
        _pipeline.LinkTo(_highPriority, q => q.Priority == Priority.High);

        Console.WriteLine($"Register priority: High");
        _pipeline.LinkTo(_defaultPriority, q => q.Priority == Priority.Default);
        Console.WriteLine($"Register priority: Default");

        _service = service;
    }

    public void Post(BatchQueueItem batchAnimals)
    {

        BuildDynamicPipeline(batchAnimals);


        _pipeline.Post(batchAnimals);
    }

    private void BuildDynamicPipeline(BatchQueueItem batchAnimals)
    {
        if (!highPriorityCategoryRegister.Contains(batchAnimals.Category))
        {
            lock (_lockObj)
            {
                if (highPriorityCategoryRegister.Contains(batchAnimals.Category))
                {
                    return;
                }

                var categoryActionBlock = new ActionBlock<BatchQueueItem>(bqi => _service.Play(bqi.Priority, bqi.Category, bqi.Animals));
                _highPriority.LinkTo(categoryActionBlock, it => it.Category == batchAnimals.Category);
                highPriorityCategoryRegister.Add(batchAnimals.Category);
                Console.WriteLine($"Register priority: High, Category: {batchAnimals.Category}");
            }
        }

        if (!defaultPriorityCategoryRegister.Contains(batchAnimals.Category))
        {
            lock (_lockObj)
            {
                if (defaultPriorityCategoryRegister.Contains(batchAnimals.Category))
                {
                    return;
                }

                var categoryActionBlock = new ActionBlock<BatchQueueItem>(bqi => _service.Play(bqi.Priority, bqi.Category, bqi.Animals));
                _defaultPriority.LinkTo(categoryActionBlock, it => it.Category == batchAnimals.Category);
                defaultPriorityCategoryRegister.Add(batchAnimals.Category);
                Console.WriteLine($"Register priority: Default, Category: {batchAnimals.Category}");
            }
        }
    }
}

public class PlayWithAnimalsService
{
    public Task Play(Priority priority, Category category, IEnumerable<Animal> animals)
    {
        if (animals.Count() > 0)
        {
            Console.WriteLine($"Handle message from Priority: {priority}, Category: {category}, Count: {animals.Count()}");
        }

        return Task.CompletedTask;
    }
}


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

    private static MessageReceiverSimulator CreateMessageSimulator(IJobPipeline<BatchQueueItem> queue)
    {
        MessageReceiverSimulator _receiptCreationSimulator = new MessageReceiverSimulator(500);
        _receiptCreationSimulator.Elapsed += (sender, e) =>
        {
            var random = new Faker().Random.Int(1, 6);
            var animals = Enumerable.Range(1, random)
            .Select(i => new Animal(Guid.NewGuid())).ToList();
            var bqi = new BatchQueueItem(
                new Faker().PickRandom<Category>(),
                new Faker().PickRandom<Priority>(),
               animals);
            Console.WriteLine($"Post: Priority: {bqi.Priority}, Category: {bqi.Category}, Count: {bqi.Animals.Count()} ");

            queue.Post(bqi);
        };
        return _receiptCreationSimulator;
    }
}