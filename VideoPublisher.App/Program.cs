using Bogus;
using Core.Logger;
using Core.Simulators;
using Dataflow.Play.App.ReceiptsReportPipeline;
using System.Threading.Tasks.Dataflow;
using VideoPublisher.App.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var videoCoverCreationPipeline = VideoCoverPipelineCreator.Create(new ConsoleOutputLogger());

        MessageReceiverSimulator videoGenerationSimulator = CreateResourcesSimulator(videoCoverCreationPipeline);
        videoGenerationSimulator.Start();

        Console.ReadLine();
    }
    private static MessageReceiverSimulator CreateResourcesSimulator(ITargetBlock<Video> pipeline)
    {
        var receiptCreationSimulator = new MessageReceiverSimulator(5000);
        receiptCreationSimulator.Elapsed += (sender, e) => pipeline.Post(new Video(Guid.NewGuid(), new Faker().Music.Genre()));
        return receiptCreationSimulator;
    }
}