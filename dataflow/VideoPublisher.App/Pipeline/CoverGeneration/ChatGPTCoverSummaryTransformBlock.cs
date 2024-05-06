using Bogus;
using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.CoverGeneration;

public class ChatGPTCoverSummaryTransformBlock
{
    private readonly IOutputLogger _logger;

    public ChatGPTCoverSummaryTransformBlock(IOutputLogger logger)
        => _logger = logger;

    public ChatGPTSummary Transform(VideoTranscription transcription)
    {
        _logger.Log($"Pipeline=CoverGeneration, ResourceId={transcription.Id}: ChatGPTCoverSummary");
        return new ChatGPTSummary(transcription.Id, new Faker().Lorem.Sentence(20));
    }
}
