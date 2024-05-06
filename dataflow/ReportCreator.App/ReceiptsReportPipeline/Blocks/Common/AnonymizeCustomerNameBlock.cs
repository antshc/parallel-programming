using Core.Logger;
using Dataflow.Play.App.Models;

namespace Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Common;

public class AnonymizeCustomerNameBlock
{
    private readonly IOutputLogger _logger;

    public AnonymizeCustomerNameBlock(IOutputLogger logger)
        => _logger = logger;

    public Receipt Anonymize(Receipt receipt)
    {
        var pipeline = receipt is CashierReceipt ? "Cashier" : "Online";
        _logger.Log($"Pipeline={pipeline},ResourceId={receipt.Id}: Anonymize customer name: {receipt.CustomerName}");
        receipt.CustomerName = new string(Enumerable.Range(1, receipt.CustomerName.Length - 1).Select(X => '*').ToArray());
        return receipt;
    }
}
