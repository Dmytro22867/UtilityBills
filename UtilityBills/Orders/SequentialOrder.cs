using UtilityBills.Meters;

namespace UtilityBills.Orders;

public class SequentialOrder : IBillingOrder
{
    public string Title => "За порядком у списку";

    public IEnumerable<Meter> Arrange(IEnumerable<Meter> meters) => meters;
}
