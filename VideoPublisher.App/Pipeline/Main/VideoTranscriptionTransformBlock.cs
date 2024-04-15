using Bogus;
using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.Main;

public class VideoTranscriptionTransformBlock
{
    private readonly IOutputLogger _logger;

    public VideoTranscriptionTransformBlock(IOutputLogger logger)
        => _logger = logger;

    public VideoTranscription Transform(Video video)
    {
        _logger.Log($"Pipeline=Main, ResourceId={video.Id}: Transcript");
        Thread.Sleep(1000);
        return new VideoTranscription(video.Id, new Faker().Lorem.Sentence(1000));
    }
}
