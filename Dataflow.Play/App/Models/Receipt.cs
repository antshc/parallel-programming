namespace Dataflow.Play.App.Models;

public abstract class Receipt
{
    public Guid Id { get; set; }
    public string CustomerName { get; set; } = "";
    public IEnumerable<string> Products { get; set; }
    public decimal Total { get; set; }
}

public class CashierReceipt : Receipt
{
    public string Location { get; set; } = "";
}

public class OnlineReceipt : Receipt
{
}