using Bogus;
using Core.Logger;
using Dataflow.Play.App.Models;

namespace Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Cashier;

public class ReceiptFileReaderBlock
{
    private static readonly Faker<CashierReceipt> _cashierReceiptFaker = new Faker<CashierReceipt>()
             .RuleFor(u => u.Id, f => f.Random.Uuid())
             .RuleFor(u => u.CustomerName, f => f.Name.FirstName() + " " + f.Name.LastName())
             .RuleFor(u => u.Products, f => Enumerable.Range(1, 3).Select(x => f.Vehicle.Model()).ToList())
             .RuleFor(u => u.Total, f => f.Random.Decimal(10, 1000));

    private readonly IOutputLogger _logger;

    public ReceiptFileReaderBlock(IOutputLogger logger) => _logger = logger;

    public Receipt ReadReceipt(ReceiptResource cashierReceipt)
    {
        _logger.Log($"Pipeline=Cashier, ResourceId={cashierReceipt.Resource}, Read receipt: {cashierReceipt.Resource}");
        var receipt = _cashierReceiptFaker.Generate();
        receipt.Id = Guid.Parse(cashierReceipt.Resource);
        return receipt;
    }
}
