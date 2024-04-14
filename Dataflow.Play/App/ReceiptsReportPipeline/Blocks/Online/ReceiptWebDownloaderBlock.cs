using Bogus;
using Dataflow.Play.App.Logger;
using Dataflow.Play.App.Models;

namespace Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Online;

public class ReceiptWebDownloaderBlock
{
    private static readonly Faker<OnlineReceipt> _faker = new Faker<OnlineReceipt>()
             .RuleFor(u => u.Id, f => f.Random.Uuid())
             .RuleFor(u => u.CustomerName, f => f.Name.FirstName() + " " + f.Name.LastName())
             .RuleFor(u => u.Products, f => Enumerable.Range(1, 3).Select(x => f.Vehicle.Model()).ToList())
             .RuleFor(u => u.Total, f => f.Random.Decimal(10, 1000));

    private readonly IOutputLogger _logger;

    public ReceiptWebDownloaderBlock(IOutputLogger logger)
        => _logger = logger;

    public Receipt ReadReceipt(ReceiptResource resource)
    {
        _logger.Log($"Pipeline=Online,ResourceId={resource.Resource} Download receipt: {resource.Resource}");
        var receipt = _faker.Generate();
        receipt.Id = Guid.Parse(resource.Resource);
        return receipt;
    }
}
