namespace Dataflow.Play.PaymentReceipts;

public abstract class Receipt
{
    public Guid Id { get; set; }
    public IEnumerable<string> Products { get; set; }
    public decimal Total { get; set; }
}

public class CashierReceipt : Receipt
{
    public string Location { get; set; } = "";
}

public class OnlineReceipt : Receipt
{
    public string CustomerName { get; set; } = "";
}