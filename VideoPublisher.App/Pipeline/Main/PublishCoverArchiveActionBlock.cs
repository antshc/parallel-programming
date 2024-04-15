using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.Main;

public class PublishCoverArchiveActionBlock
{
    private readonly IOutputLogger _logger;

    public PublishCoverArchiveActionBlock(IOutputLogger logger)
        => _logger = logger;

    public async Task Transform(CoverArchive cover)
    {
        _logger.Log($"Pipeline=Main, ResourceId={cover.Id}: Publish");
        await Task.Delay(1000);
    }
}
