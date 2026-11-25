using UtilityBills.Meters;

namespace UtilityBills.Orders;

public class HighestRateFirstOrder : IBillingOrder
{
    public string Title => "Спочатку найдорожчі за тарифом";

    public IEnumerable<Meter> Arrange(IEnumerable<Meter> meters) =>
        meters.OrderByDescending(GetBaseTariff);

    private static double GetBaseTariff(Meter meter) => meter switch
    {
        SingleZoneMeter s => s.Rate,
        DualZoneMeter d => Math.Max(d.DayRate, d.NightRate),
        StandingChargeMeter f => f.MonthlyFee,
        _ => 0
    };
}
