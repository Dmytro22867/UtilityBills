using System.Globalization;

namespace UtilityBills.Meters;

public class Meter
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ServiceType { get; set; } = "Загальна";
    public double Rate { get; set; }
    public double PreviousReading { get; set; }

    public Meter() { }

    public Meter(int id, string name, double rate, double previousReading = 0)
    {
        Id = id;
        Name = name;
        Rate = rate;
        PreviousReading = previousReading;
    }

    public double CalculateBill(string reading)
    {
        var trimmed = reading.Trim();
        if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out var current) &&
            !double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out current))
        {
            return -1;
        }

        var consumed = current - PreviousReading;
        return consumed >= 0 ? Math.Round(consumed * Rate, 2) : -1;
    }

    public override string ToString() =>
        $"#{Id} [{ServiceType}] {Name} (тариф: {Rate} грн, попередні: {PreviousReading})";
}
