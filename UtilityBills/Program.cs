using UtilityBills.Billing;
using UtilityBills.Common;
using UtilityBills.Meters;
using UtilityBills.Orders;
using UtilityBills.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Облік комунальних платежів");

var dataPath = Path.Combine(AppContext.BaseDirectory, "data", "meters.json");
IMeterStorage storage = new JsonMeterStorage(dataPath);

var meters = storage.Load();
if (meters.Count == 0)
{
    meters = new List<Meter>
    {
        new SingleZoneMeter(1, "Холодна вода (кухня)", 31.20, 410, "м³")
            { ServiceType = "Водопостачання" },
        new DualZoneMeter(2, "Електроенергія (день/ніч)", 4.32, 2.16, 1200, 600, "День (Т1)", "Ніч (Т2)")
            { ServiceType = "Електрика" },
        new StandingChargeMeter(3, "Вивіз побутових відходів", 45.50, 2)
            { ServiceType = "Утримання будинку" },
        new SingleZoneMeter(4, "Газ (плита та колонка)", 7.96, 890, "м³")
            { ServiceType = "Газопостачання" }
    };
    storage.Save(meters);
    Console.WriteLine("Створено початковий файл даних meters.json.");
}
else
{
    Console.WriteLine($"Завантажено записів із файлу: {meters.Count}.");
}

var filter = new CatalogFilter<Meter>();
var meteredOnly = filter.FindAll(meters, m => m is SingleZoneMeter or DualZoneMeter);
Console.WriteLine($"Усього послуг: {meters.Count}, з них із лічильниками показань: {meteredOnly.Count}.");

Console.Write("Оберіть порядок перебору (1 - за списком, 2 - від найдорожчого тарифу): ");
var mode = Console.ReadLine()?.Trim();

IBillingOrder order = mode == "2"
    ? new HighestRateFirstOrder()
    : new SequentialOrder();

Console.WriteLine($"Обрано режим: {order.Title}");

var summary = new MonthlySummary(0, 0, 0);
var billedItems = new List<(Meter Meter, double Amount)>();

foreach (var meter in order.Arrange(meters))
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

    if (bill >= 0)
    {
        billedItems.Add((meter, bill));
        Console.WriteLine($"Нараховано: {bill:F2} грн.");
    }
    else
    {
        Console.WriteLine("Помилка: некоректні дані для розрахунку.");
    }
}

storage.Save(meters);

Console.WriteLine();
Console.WriteLine($"Підсумок: опрацьовано {summary.ValidCount} з {summary.ProcessedCount}, " +
                  $"до сплати разом: {summary.TotalAmount:F2} грн (середній платіж: {summary.AverageBill:F2} грн).");
Console.WriteLine($"Дані збережено у: {dataPath}");

if (billedItems.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine("Аналітика за видами послуг (LINQ GroupBy):");
    var byService = billedItems
        .GroupBy(x => x.Meter.ServiceType)
        .Select(g => new { Service = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
        .OrderByDescending(x => x.Total);

    foreach (var group in byService)
        Console.WriteLine($"  • {group.Service}: {group.Total:F2} грн (позицій: {group.Count})");
}
