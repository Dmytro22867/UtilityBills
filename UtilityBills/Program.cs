using UtilityBills.Billing;
using UtilityBills.Meters;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Облік комунальних платежів");

var meters = new List<Meter>
{
    new SingleZoneMeter(1, "Холодна вода (кухня)", 31.20, 410, "м³")
        { ServiceType = "Водопостачання" },
    new DualZoneMeter(2, "Електроенергія (день/ніч)", 4.32, 2.16, 1200, 600, "День (Т1)", "Ніч (Т2)")
        { ServiceType = "Електрика" },
    new StandingChargeMeter(3, "Вивіз побутових відходів", 45.50, 2)
        { ServiceType = "Утримання будинку" }
};

Console.WriteLine($"Лічильників у списку: {meters.Count}.");

var summary = new MonthlySummary(0, 0, 0);

foreach (var meter in meters)
{
    Console.WriteLine();
    Console.WriteLine($"{meter} — {meter.TariffDetails}");

    if (meter is DualZoneMeter dual)
    {
        for (var n = 1; n <= dual.ZoneNames.Count; n++)
            Console.WriteLine($"  Зона {n}: {dual[n]}");
    }

    Console.Write($"{meter.Prompt}: ");
    var given = Console.ReadLine() ?? "";
    var bill = meter.CalculateBill(given);
    summary += bill;

    Console.WriteLine(bill >= 0
        ? $"Нараховано: {bill:F2} грн."
        : "Помилка: некоректні дані для розрахунку.");
}

Console.WriteLine();
Console.WriteLine($"Підсумок: опрацьовано {summary.ValidCount} з {summary.ProcessedCount}, " +
                  $"до сплати разом: {summary.TotalAmount:F2} грн (середній платіж: {summary.AverageBill:F2} грн).");
