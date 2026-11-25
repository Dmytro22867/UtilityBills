using UtilityBills.Meters;

namespace UtilityBills.Orders;

public interface IBillingOrder
{
    string Title { get; }
    IEnumerable<Meter> Arrange(IEnumerable<Meter> meters);
}
