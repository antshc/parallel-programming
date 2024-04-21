using Bogus;
using Core.Logger;
using Core.Simulators;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks.Dataflow;
using System.Timers;

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
        _pipeline.LinkTo(_highPriority, q => q.Priority == Priority.High);
        Console.WriteLine($"Register priority: High");
        _pipeline.LinkTo(_defaultPriority, q => q.Priority == Priority.Default);
        Console.WriteLine($"Register priority: Default");
        _service = service;
    }

    public void Post(QueueItem animal)
    {

        BuildDynamicPipeline(animal);


        _pipeline.Post(animal);
    }

    private void BuildDynamicPipeline(QueueItem animal)
    {
        if (!highPriorityCategoryRegister.Contains(animal.Category))
        {
            lock (_lockObj)
            {
                if (highPriorityCategoryRegister.Contains(animal.Category))
                {
                    return;
                }

                var bufferBatchBlock = new BufferBatchBlockFactory<QueueItem>(5).Create();
                _highPriority.LinkTo(bufferBatchBlock, it => it.Category == animal.Category);
                var categoryActionBlock = new ActionBlock<QueueItem[]>(_service.Play);
                bufferBatchBlock.LinkTo(categoryActionBlock);
                highPriorityCategoryRegister.Add(animal.Category);
                Console.WriteLine($"Register priority: High, Category: {animal.Category}");
            }
        }

        if (!defaultPriorityCategoryRegister.Contains(animal.Category))
        {
            lock (_lockObj)
            {
                if (defaultPriorityCategoryRegister.Contains(animal.Category))
                {
                    return;
                }

                var bufferBatchBlock = new BufferBatchBlockFactory<QueueItem>(5).Create();
                _defaultPriority.LinkTo(bufferBatchBlock, it => it.Category == animal.Category);
                var categoryActionBlock = new ActionBlock<QueueItem[]>(_service.Play);
                bufferBatchBlock.LinkTo(categoryActionBlock);
                defaultPriorityCategoryRegister.Add(animal.Category);
                Console.WriteLine($"Register priority: Default, Category: {animal.Category}");
            }
        }
    }
}

public class PlayWithAnimalsService
{
    public Task Play(QueueItem[] animals)
    {
        if (animals.Length > 0)
        {
            Console.WriteLine($"Handle message from Category: {animals.First().Category}, Count: {animals.Length}");
        }

        return Task.CompletedTask;
    }
}

public class BufferBatchBlockFactory<T> : IDisposable
{
    private readonly ConcurrentQueue<T> _buffer;
    private readonly BufferBlock<T[]> _source;
    private System.Timers.Timer _timer;
    private readonly int size;

    public BufferBatchBlockFactory(int size)
    {
        // Create a queue to hold messages.
        _buffer = new ConcurrentQueue<T>();
        // The source part of the propagator holds arrays of size windowSize
        // and propagates data out to any connected targets.
        _source = new BufferBlock<T[]>();
        ResetTimer();
        this.size = size;
    }

    private void ResetTimer()
    {
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += TimerElapsed;
        _timer.AutoReset = false;
        _timer.Start();
    }

    public IPropagatorBlock<T, T[]> Create()
    {
        // The target part receives data and adds them to the queue.
        var target = new ActionBlock<T>(item =>
        {
            // Post the data in the queue to the source block when the queue size
            // equals the window size.
            if (_buffer.Count == size)
            {
                _source.Post(_buffer.ToArray());
                _buffer.Clear();
                ResetTimer();
            }

            // Add the item to the queue.
            _buffer.Enqueue(item);
        });

        // When the target is set to the completed state, propagate out any
        // remaining data and set the source to the completed state.
        target.Completion.ContinueWith(delegate
        {
            if (_buffer.Count > 0 && _buffer.Count < size)
                _source.Post(_buffer.ToArray());
            _source.Complete();
        });

        // Return a IPropagatorBlock<T, T[]> object that encapsulates the
        // target and source blocks.
        return DataflowBlock.Encapsulate(target, _source);
    }

    private void TimerElapsed(object sender, ElapsedEventArgs e)
    {
        if (_buffer.Count > 0)
        {
            Console.WriteLine("TimerElapsed");
            _source.Post(_buffer.ToArray());
        }
    }

    public void Dispose()
    {
        _timer.Elapsed -= TimerElapsed;
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

    private static MessageReceiverSimulator CreateMessageSimulator(IJobPipeline<QueueItem> queue)
    {
        MessageReceiverSimulator _receiptCreationSimulator = new MessageReceiverSimulator(250);
        _receiptCreationSimulator.Elapsed += (sender, e) =>
        {
            var qi = new QueueItem(Guid.NewGuid(), new Faker().PickRandom<Category>(), new Faker().PickRandom<Priority>());
            Console.WriteLine($"Post: {qi}");
            queue.Post(qi);
        };
        return _receiptCreationSimulator;
    }
}