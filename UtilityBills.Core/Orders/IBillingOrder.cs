using UtilityBills.Core.Meters;

namespace UtilityBills.Core.Orders;

public interface IBillingOrder
{
    string Title { get; }
    IEnumerable<Meter> Arrange(IEnumerable<Meter> meters);
}
