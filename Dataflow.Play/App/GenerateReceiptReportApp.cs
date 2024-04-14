using Dataflow.Play.App.Logger;
using Dataflow.Play.App.Models;
using Dataflow.Play.App.ReceiptsReportPipeline;
using Dataflow.Play.Simulators;
using System.Threading.Tasks.Dataflow;
using Xunit.Abstractions;

namespace Dataflow.Play.App;

public class GenerateReceiptReportApp
{
    private readonly ITestOutputHelper output;

    public GenerateReceiptReportApp(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Run()
    {
        var reportCreationPipeline = ReportPipelineCreator.Create(new TestOutputLogger(output));

        ReceiptCreationSimulator cashierReceiptSimulator = CreateCashierResourcesSimulator(reportCreationPipeline);
        ReceiptCreationSimulator onlineReceiptSimulator = CreateOnlineResourcesSimulator(reportCreationPipeline);
        cashierReceiptSimulator.Start();
        onlineReceiptSimulator.Start();

        Task.Delay(100000).Wait();
    }

    private static ReceiptCreationSimulator CreateCashierResourcesSimulator(ITargetBlock<ReceiptResource> reportCreationPipeline)
    {
        var receiptCreationSimulator = new ReceiptCreationSimulator(1000);
        receiptCreationSimulator.Elapsed += (sender, e) => reportCreationPipeline.Post(new CashierReceiptResource(Guid.NewGuid().ToString()));
        return receiptCreationSimulator;
    }

    private static ReceiptCreationSimulator CreateOnlineResourcesSimulator(ITargetBlock<ReceiptResource> reportCreationPipeline)
    {
        var receiptCreationSimulator = new ReceiptCreationSimulator(2000);
        receiptCreationSimulator.Elapsed += (sender, e) => reportCreationPipeline.Post(new OnlineReceiptResource(Guid.NewGuid().ToString()));
        return receiptCreationSimulator;
    }
}