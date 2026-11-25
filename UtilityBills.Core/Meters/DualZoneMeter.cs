namespace UtilityBills.Core.Meters;

public class DualZoneMeter : Meter
{
    public List<string> ZoneNames { get; set; } = new();
    public double DayRate { get; set; }
    public double NightRate { get; set; }
    public double PreviousDay { get; set; }
    public double PreviousNight { get; set; }

    public string this[int zoneNumber] => ZoneNames[zoneNumber - 1];

    public DualZoneMeter() { }

    public DualZoneMeter(int id, string name, double dayRate, double nightRate)
        : base(id, name)
    {
        DayRate = dayRate;
        NightRate = nightRate;
        ZoneNames.AddRange(["День", "Ніч"]);
    }

    public DualZoneMeter(int id, string name, double dayRate, double nightRate,
        double previousDay, double previousNight, params string[] zones)
        : this(id, name, dayRate, nightRate)
    {
        PreviousDay = previousDay;
        PreviousNight = previousNight;
        if (zones.Length > 0)
        {
            ZoneNames.Clear();
            ZoneNames.AddRange(zones);
        }
    }

    public override double CalculateBill(string reading)
    {
        var parts = reading.Split([';', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2 ||
            !TryParseNumber(parts[0], out var currentDay) ||
            !TryParseNumber(parts[1], out var currentNight))
        {
            return -1;
        }

        var dayConsumed = currentDay - PreviousDay;
        var nightConsumed = currentNight - PreviousNight;
        if (dayConsumed < 0 || nightConsumed < 0)
            return -1;

        return Math.Round(dayConsumed * DayRate + nightConsumed * NightRate, 2);
    }

    public override string TariffDetails =>
        $"Денний: {DayRate:F2} грн (попер. {PreviousDay}), Нічний: {NightRate:F2} грн (попер. {PreviousNight})";

    public override string Prompt => "Показання День і Ніч через пробіл (напр. 1200 650)";
}
