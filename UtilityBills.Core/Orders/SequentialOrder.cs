using UtilityBills.Core.Meters;

namespace UtilityBills.Core.Orders;

public class SequentialOrder : IBillingOrder
{
    public string Title => "За порядком у списку";

    public IEnumerable<Meter> Arrange(IEnumerable<Meter> meters) => meters;
}
