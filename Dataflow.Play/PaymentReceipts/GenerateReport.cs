using Dataflow.Play.Simulators;
using System.Threading;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Play.PaymentReceipts;

public class GenerateReport
{
    [Fact]
    public void Run()
    {
        var receiptCreationSimulator = new ReceiptCreationSimulator(1000);
        receiptCreationSimulator.Elapsed += OnCashierReceiptCreated;
        receiptCreationSimulator.Start();

        // Creates the image processing dataflow network and returns the
        // head node of the network.
        ITargetBlock<ReceiptResource> CreateReceiptReportProcessingNetwork()
        {

            var bufferReceipts = new BufferBlock<ReceiptResource>();

            var downloadReceiptFromWeb = new TransformBlock<ReceiptResource, string>(receipt =>
            {
                return "{}";
            });

            var readReceiptFromFile = new TransformBlock<ReceiptResource, string>(receipt =>
            {
                return "{}";

            });

            var addLocation = new TransformBlock<string, string>(receipt =>
            {
                return "{}";

            });

            var bufferProcessedReceipts = new BufferBlock<string>();


            var batchReceiptsForReport = new BatchBlock<string>(10);

            var generateReport = new TransformBlock<string[], string>(receipts =>
            {
                return "{}";
            });

            var saveReport = new ActionBlock<string[]>(receipts =>
            {
            });


            bufferReceipts.LinkTo(ReceiptWebDownloader.CreateBlock(), (receipt) => receipt is OnlineReceiptResource);

            bufferReceipts.LinkTo(readReceiptFromFile, (receipt) => receipt is CashierReceiptResource);
            readReceiptFromFile.LinkTo(addLocation);
            addLocation.LinkTo(bufferProcessedReceipts);
            bufferProcessedReceipts.LinkTo(batchReceiptsForReport);
            batchReceiptsForReport.LinkTo(saveReport);

            var createReportJson = new TransformBlock<IEnumerable<string>, string>(jsons =>
            {
                try
                {
                    return CreateCompositeJson(jsons);
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation by passing null to the next stage
                    // of the network.
                    return null;
                }
            });

            return bufferReceipts;
        }

        var t = CreateReceiptReportProcessingNetwork();
        t.Post(new CashierReceiptResource("localfile"));
    }

    string CreateCompositeJson(IEnumerable<string> jsons)
    {
        return string.Empty;
    }

    static void OnCashierReceiptCreated(object sender, EventArgs e)
    {
        Console.WriteLine("Timer elapsed at: " + DateTime.Now);
    }
}