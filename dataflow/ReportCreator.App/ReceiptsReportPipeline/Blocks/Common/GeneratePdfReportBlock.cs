using Core.Logger;
using Dataflow.Play.App.Models;

namespace Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Common;

public class GeneratePdfReportBlock
{
    private readonly IOutputLogger _logger;

    public GeneratePdfReportBlock(IOutputLogger logger)
        => _logger = logger;

    public void GeneratePdfReport(Receipt[] receipts)
    {
        foreach (Receipt receipt in receipts)
        {
            _logger.Log($"Pipeline=Common,ResourceId={receipt.Id}: {nameof(GeneratePdfReportBlock)}");
        }
    }
}
