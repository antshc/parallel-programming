using Core.Logger;
using Dataflow.Play.App.Models;
using Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Cashier;
using Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Common;
using Dataflow.Play.App.ReceiptsReportPipeline.Blocks.Online;
using System.Threading.Tasks.Dataflow;

namespace Dataflow.Play.App.ReceiptsReportPipeline;

public class ReportPipelineCreator
{
    public static ITargetBlock<ReceiptResource> Create(IOutputLogger logger)
    {
        var receiptResourceBufferBlock = new BufferBlock<ReceiptResource>();

        var downloadJsonReceiptTransformBlock = new TransformBlock<ReceiptResource, Receipt>(new ReceiptWebDownloaderBlock(logger).ReadReceipt);
        var readFileReceiptTransformBlock = new TransformBlock<ReceiptResource, Receipt>(new ReceiptFileReaderBlock(logger).ReadReceipt);
        var anonymizeCustomerNameTransformBlock = new TransformBlock<Receipt, Receipt>(new AnonymizeCustomerNameBlock(logger).Anonymize);
        var addLocationTransformBlock = new TransformBlock<Receipt, Receipt>(new AddLocationBlock(logger).AddLocation);
        var receiptBufferBlock = new BufferBlock<Receipt>();
        var receiptBatchBlock = new BatchBlock<Receipt>(10);
        var saveReportActionBlock = new ActionBlock<Receipt[]>(new GeneratePdfReportBlock(logger).GeneratePdfReport);

        receiptResourceBufferBlock.LinkTo(downloadJsonReceiptTransformBlock, (receipt) => receipt is OnlineReceiptResource);

        LinkOnlineReceiptPipe(
            downloadJsonReceiptTransformBlock,
            anonymizeCustomerNameTransformBlock,
            receiptBufferBlock);

        receiptResourceBufferBlock.LinkTo(readFileReceiptTransformBlock, (receipt) => receipt is CashierReceiptResource);

        LinkCashierReceiptPipe(
            readFileReceiptTransformBlock,
            anonymizeCustomerNameTransformBlock,
            addLocationTransformBlock,
            receiptBufferBlock);

        receiptBufferBlock.LinkTo(receiptBatchBlock);
        receiptBatchBlock.LinkTo(saveReportActionBlock);

        return receiptResourceBufferBlock;
    }

    private static void LinkCashierReceiptPipe(TransformBlock<ReceiptResource, Receipt> readReceiptFromFile, TransformBlock<Receipt, Receipt> anonymizeCustomerNameBlock, TransformBlock<Receipt, Receipt> addLocation, BufferBlock<Receipt> bufferProcessedReceipts)
    {
        readReceiptFromFile.LinkTo(anonymizeCustomerNameBlock);
        anonymizeCustomerNameBlock.LinkTo(addLocation);
        addLocation.LinkTo(bufferProcessedReceipts);
    }

    private static void LinkOnlineReceiptPipe(TransformBlock<ReceiptResource, Receipt> readReceipt, TransformBlock<Receipt, Receipt> anonymizeCustomerNameBlock, BufferBlock<Receipt> bufferProcessedReceipts)
    {
        readReceipt.LinkTo(anonymizeCustomerNameBlock);
        anonymizeCustomerNameBlock.LinkTo(bufferProcessedReceipts);
    }
}
