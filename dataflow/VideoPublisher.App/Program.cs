using Bogus;
using Core.Logger;
using Dataflow.Play.App.ReceiptsReportPipeline;
using System.Diagnostics;
using System.Threading.Tasks.Dataflow;
using VideoPublisher.App.Models;
using VideoPublisher.App.Pipeline;

internal class Program
{
    private static void Main(string[] args)
    {
        var elapsed = RunPipeline(maxParallelism: 1);

        Console.WriteLine("Degree of parallelism = {0}; " +
           "elapsed time = {1}ms.", 1, (int)elapsed.TotalMilliseconds);

        elapsed = RunPipeline(maxParallelism: 8);

        Console.WriteLine("Degree of parallelism = {0}; " +
           "elapsed time = {1}ms.", 8, (int)elapsed.TotalMilliseconds);

        Console.ReadLine();
    }

    private static TimeSpan RunPipeline(int maxParallelism)
    {
        var videoCoverCreationPipeline = VideoCoverPipelineCreator.Create(
            new ConsoleOutputLogger(),
            new VideoCoverPipelineConfig(maxParallelism, maxParallelism));

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        for (int i = 0; i < 10; i++)
        {
            videoCoverCreationPipeline.Post(new Video(Guid.NewGuid(), new Faker().Music.Genre()));
        }

        videoCoverCreationPipeline.Complete();
        videoCoverCreationPipeline.Completion.Wait();
        stopwatch.Stop();

        return stopwatch.Elapsed;
    }
}