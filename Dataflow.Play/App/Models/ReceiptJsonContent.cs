namespace Dataflow.Play.App.Models;

public abstract class ReceiptJsonContent
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = "";
    public IEnumerable<string> Products { get; set; }
    public decimal Total { get; set; }
}

public class CashierReceiptJsonContent : ReceiptJsonContent
{
}

public class OnlineReceiptJsonContent : ReceiptJsonContent
{
}