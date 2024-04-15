using Core.Logger;
using System.Threading.Tasks.Dataflow;
using VideoPublisher.App.Models;
using VideoPublisher.App.Pipeline;
using VideoPublisher.App.Pipeline.CoverGeneration;
using VideoPublisher.App.Pipeline.Main;

namespace Dataflow.Play.App.ReceiptsReportPipeline;

public class VideoCoverPipelineCreator
{
    public static ITargetBlock<Video> Create(IOutputLogger logger, VideoCoverPipelineConfig config)
    {
        var videoTranscriptionTransformBlock =
            new TransformBlock<Video, VideoTranscription>(
                new VideoTranscriptionTransformBlock(logger).Transform,
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = config.TranscriptionMaxParallelism }
            );
        var videoTranscriptionBroadcastBlock = new BroadcastBlock<VideoTranscription>(vt => vt);
        var chatGPTCoverSummaryTransformBlock = new TransformBlock<VideoTranscription, ChatGPTSummary>(new ChatGPTCoverSummaryTransformBlock(logger).Transform);
        var chatGPTSummaryTransformBlock = new TransformBlock<VideoTranscription, ChatGPTSummary>(new ChatGPTSummaryTransformBlock(logger).Transform);
        var dalle2CoverTransformBlock = new TransformBlock<ChatGPTSummary, DALLE2VideoCover>(new DALLE2CoverTransformBlock(logger).Transform);

        var joinArchiveBlock = new JoinBlock<DALLE2VideoCover, ChatGPTSummary>(
            new GroupingDataflowBlockOptions
            {
                Greedy = false
            });

        var archiveCoverActionBlock = new TransformBlock<Tuple<DALLE2VideoCover, ChatGPTSummary>, CoverArchive>(
            new CoverArchiveTransformBlock(logger).Transform);
        var publishCoverArchiveActionBlock = new ActionBlock<CoverArchive>(
            new PublishCoverArchiveActionBlock(logger).Transform,
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = config.PublishCoverMaxParallelism });

        videoTranscriptionTransformBlock.LinkTo(videoTranscriptionBroadcastBlock);
        videoTranscriptionBroadcastBlock.LinkTo(chatGPTCoverSummaryTransformBlock);
        chatGPTCoverSummaryTransformBlock.LinkTo(dalle2CoverTransformBlock);

        videoTranscriptionBroadcastBlock.LinkTo(chatGPTSummaryTransformBlock);

        dalle2CoverTransformBlock.LinkTo(joinArchiveBlock.Target1);
        chatGPTSummaryTransformBlock.LinkTo(joinArchiveBlock.Target2);

        joinArchiveBlock.LinkTo(archiveCoverActionBlock);
        archiveCoverActionBlock.LinkTo(publishCoverArchiveActionBlock);

        return videoTranscriptionTransformBlock;
    }
}
