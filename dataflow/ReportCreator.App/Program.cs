using Core.Simulators;
using Core.Logger;
using Dataflow.Play.App.Models;
using Dataflow.Play.App.ReceiptsReportPipeline;
using System.Threading.Tasks.Dataflow;

internal class Program
{
    private static void Main(string[] args)
    {
        var reportCreationPipeline = ReportPipelineCreator.Create(new ConsoleOutputLogger());

        MessageReceiverSimulator cashierReceiptSimulator = CreateCashierResourcesSimulator(reportCreationPipeline);
        MessageReceiverSimulator onlineReceiptSimulator = CreateOnlineResourcesSimulator(reportCreationPipeline);
        cashierReceiptSimulator.Start();
        onlineReceiptSimulator.Start();

        Console.ReadLine();
    }

    private static MessageReceiverSimulator CreateCashierResourcesSimulator(ITargetBlock<ReceiptResource> reportCreationPipeline)
    {
        var receiptCreationSimulator = new MessageReceiverSimulator(1000);
        receiptCreationSimulator.Elapsed += (sender, e) => reportCreationPipeline.Post(new CashierReceiptResource(Guid.NewGuid().ToString()));
        return receiptCreationSimulator;
    }

    private static MessageReceiverSimulator CreateOnlineResourcesSimulator(ITargetBlock<ReceiptResource> reportCreationPipeline)
    {
        var receiptCreationSimulator = new MessageReceiverSimulator(2000);
        receiptCreationSimulator.Elapsed += (sender, e) => reportCreationPipeline.Post(new OnlineReceiptResource(Guid.NewGuid().ToString()));
        return receiptCreationSimulator;
    }
}