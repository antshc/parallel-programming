namespace VideoPublisher.App.Pipeline;

public record struct VideoCoverPipelineConfig(int TranscriptionMaxParallelism = 1, int PublishCoverMaxParallelism = 1);
