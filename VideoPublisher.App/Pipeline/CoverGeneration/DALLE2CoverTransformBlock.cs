using Bogus;
using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.CoverGeneration;

public class DALLE2CoverTransformBlock
{
    private readonly IOutputLogger _logger;

    public DALLE2CoverTransformBlock(IOutputLogger logger)
        => _logger = logger;

    public DALLE2VideoCover Transform(ChatGPTSummary video)
    {
        _logger.Log($"Pipeline=CoverGeneration, ResourceId={video.Id}: DALLE2VideoCover");
        return new DALLE2VideoCover(video.Id, new Faker().Image.Animals());
    }
}
