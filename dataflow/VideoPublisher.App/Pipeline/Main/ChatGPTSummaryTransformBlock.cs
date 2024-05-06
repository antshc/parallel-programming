using Bogus;
using Core.Logger;
using VideoPublisher.App.Models;

namespace VideoPublisher.App.Pipeline.Main;

public class ChatGPTSummaryTransformBlock
{
    private readonly IOutputLogger _logger;

    public ChatGPTSummaryTransformBlock(IOutputLogger logger)
        => _logger = logger;

    public ChatGPTSummary Transform(VideoTranscription transcription)
    {
        _logger.Log($"Pipeline=Main, ResourceId={transcription.Id}: ChatGPTSummary");
        return new ChatGPTSummary(transcription.Id, new Faker().Lorem.Sentence(40));
    }
}
