using System.Globalization;

namespace UtilityBills.Meters;

public abstract class Meter
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string ServiceType { get; set; } = "Загальна";

    protected Meter() { }

    protected Meter(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public abstract double CalculateBill(string reading);
    public abstract string TariffDetails { get; }
    public virtual string Prompt => "Введіть поточні показання";

    protected static bool TryParseNumber(string text, out double value)
    {
        var trimmed = text.Trim();
        return double.TryParse(trimmed, NumberStyles.Float, CultureInfo.CurrentCulture, out value) ||
               double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    public override string ToString() => $"#{Id} [{ServiceType}] {Name}";
}
