namespace UtilityBills.Core.Meters;

public class StandingChargeMeter : Meter
{
    public double MonthlyFee { get; set; }
    public int ResidentsCount { get; set; } = 1;

    public StandingChargeMeter() { }

    public StandingChargeMeter(int id, string name, double monthlyFee)
        : base(id, name)
    {
        MonthlyFee = monthlyFee;
    }

    public StandingChargeMeter(int id, string name, double monthlyFee, int residentsCount)
        : this(id, name, monthlyFee)
    {
        ResidentsCount = residentsCount;
    }

    public override double CalculateBill(string reading)
    {
        if (!int.TryParse(reading.Trim(), out var months) || months <= 0)
            return -1;

        return Math.Round(MonthlyFee * ResidentsCount * months, 2);
    }

    public override string TariffDetails =>
        $"Абонплата: {MonthlyFee:F2} грн/міс. (осіб: {ResidentsCount})";

    public override string Prompt => "Кількість місяців для оплати";
}
