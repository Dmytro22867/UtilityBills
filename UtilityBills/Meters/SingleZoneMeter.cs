namespace UtilityBills.Meters;

public class SingleZoneMeter : Meter
{
    public double Rate { get; set; }
    public double PreviousReading { get; set; }
    public string Unit { get; set; } = "од.";

    public SingleZoneMeter() { }

    public SingleZoneMeter(int id, string name, double rate, double previousReading = 0)
        : base(id, name)
    {
        Rate = rate;
        PreviousReading = previousReading;
    }

    public SingleZoneMeter(int id, string name, double rate, double previousReading, string unit)
        : this(id, name, rate, previousReading)
    {
        Unit = unit;
    }

    public override double CalculateBill(string reading)
    {
        if (!TryParseNumber(reading, out var current))
            return -1;

        var consumed = current - PreviousReading;
        return consumed >= 0 ? Math.Round(consumed * Rate, 2) : -1;
    }

    public override string TariffDetails =>
        $"Тариф: {Rate:F2} грн/{Unit}, попередні: {PreviousReading}";

    public override string Prompt => $"Поточні показання ({Unit})";
}
