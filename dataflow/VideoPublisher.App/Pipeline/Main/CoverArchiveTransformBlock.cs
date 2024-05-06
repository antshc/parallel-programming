using Bogus;
using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.Main;

public class CoverArchiveTransformBlock
{
    private readonly IOutputLogger _logger;

    public CoverArchiveTransformBlock(IOutputLogger logger)
        => _logger = logger;

    public CoverArchive Transform(Tuple<DALLE2VideoCover, ChatGPTSummary> coverAndSummary)
    {
        _logger.Log($"Pipeline=Main, ResourceId={coverAndSummary.Item1.Id}: Archive");
        return new CoverArchive(coverAndSummary.Item1.Id, new Faker().Lorem.Sentence(1000));
    }
}
