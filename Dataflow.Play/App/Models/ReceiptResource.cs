namespace Dataflow.Play.App.Models;

public abstract record ReceiptResource(string Resource);

public record CashierReceiptResource(string Resource) : ReceiptResource(Resource);


public record OnlineReceiptResource(string Resource) : ReceiptResource(Resource);
