using Bogus;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Play.PaymentReceipts;

public class ReceiptFileReader
{
    public static TransformBlock<ReceiptResource, string> CreateBlock()
    {
        var cashierReceiptFaker = new Faker<CashierReceipt>()
             .RuleFor(u => u.Id, f => f.Random.Uuid())
             .RuleFor(u => u.Products, f => Enumerable.Range(1, 3).Select(x => f.Vehicle.Model()).ToList())
             .RuleFor(u => u.Total, f => f.Random.Decimal(10, 1000));

        return new TransformBlock<ReceiptResource, string>(receipt =>
        {
            var r = cashierReceiptFaker.Generate();
            var receiptJson = JsonSerializer.Serialize<CashierReceipt>(r);
            return receiptJson;
        });
    }
}
