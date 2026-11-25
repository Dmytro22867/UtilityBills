namespace UtilityBills.Core.Meters;

public class ThreeZoneMeter : Meter
{
    public List<string> ZoneNames { get; set; } = ["Пік", "Напівпік", "Ніч"];
    public double PeakRate { get; set; }
    public double HalfPeakRate { get; set; }
    public double NightRate { get; set; }
    public double PreviousPeak { get; set; }
    public double PreviousHalfPeak { get; set; }
    public double PreviousNight { get; set; }

    public string this[int zoneNumber] => ZoneNames[zoneNumber - 1];

    public ThreeZoneMeter() { }

    public ThreeZoneMeter(int id, string name, double peakRate, double halfPeakRate, double nightRate,
        double previousPeak = 0, double previousHalfPeak = 0, double previousNight = 0)
        : base(id, name)
    {
        PeakRate = peakRate;
        HalfPeakRate = halfPeakRate;
        NightRate = nightRate;
        PreviousPeak = previousPeak;
        PreviousHalfPeak = previousHalfPeak;
        PreviousNight = previousNight;
    }

    public override double CalculateBill(string reading)
    {
        var parts = reading.Split([';', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 ||
            !TryParseNumber(parts[0], out var curPeak) ||
            !TryParseNumber(parts[1], out var curHalfPeak) ||
            !TryParseNumber(parts[2], out var curNight))
        {
            return -1;
        }

        var dPeak = curPeak - PreviousPeak;
        var dHalf = curHalfPeak - PreviousHalfPeak;
        var dNight = curNight - PreviousNight;

        if (dPeak < 0 || dHalf < 0 || dNight < 0)
            return -1;

        return Math.Round(dPeak * PeakRate + dHalf * HalfPeakRate + dNight * NightRate, 2);
    }

    public override string TariffDetails =>
        $"Пік: {PeakRate:F2} (попер. {PreviousPeak}), Напівпік: {HalfPeakRate:F2} (попер. {PreviousHalfPeak}), Ніч: {NightRate:F2} (попер. {PreviousNight})";

    public override string Prompt => "Показання Пік, Напівпік і Ніч через пробіл (напр. 500 800 400)";
}