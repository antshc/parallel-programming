using Bogus;
using Core.Logger;
using Dataflow.Play.App.Models;

namespace Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Cashier;

public class AddLocationBlock
{
    private readonly IOutputLogger _logger;

    public AddLocationBlock(IOutputLogger logger)
        => _logger = logger;

    public Receipt AddLocation(Receipt receipt)
    {
        _logger.Log($"Pipeline=Cashier, ResourceId={receipt.Id}: Get location from GPS");
        if (receipt is CashierReceipt cashierReceipt)
        {
            cashierReceipt.Location = new Faker().Address.City();
        }
        return receipt;
    }
}
