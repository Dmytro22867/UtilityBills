using Spectre.Console;
using UtilityBills.Core.Billing;
using UtilityBills.Core.Common;
using UtilityBills.Core.Meters;
using UtilityBills.Core.Orders;
using UtilityBills.Core.Storage;

Console.OutputEncoding = System.Text.Encoding.UTF8;

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
}

var lastBilled = new List<(Meter Meter, double Amount)>();

while (true)
{
    AnsiConsole.Clear();
    AnsiConsole.Write(new Panel("[bold yellow]Облік комунальних платежів (UtilityBills)[/]")
        .Border(BoxBorder.Rounded)
        .BorderColor(Color.Cyan1));

    var filter = new CatalogFilter<Meter>();
    var meteredCount = filter.CountMatching(meters, m => m is SingleZoneMeter or DualZoneMeter);
    AnsiConsole.MarkupLine($"[grey]Записів у базі: [white]{meters.Count}[/] (з лічильниками показань: [white]{meteredCount}[/])[/]\n");

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("[green]Оберіть дію:[/]")
            .AddChoices(
                "1. Переглянути всі послуги та тарифи",
                "2. Провести нарахування платежів",
                "3. Додати нову послугу / лічильник",
                "4. Аналітика за видами послуг (LINQ)",
                "0. Вихід"));

    if (choice.StartsWith("0."))
        break;

    if (choice.StartsWith("1."))
    {
        var table = new Table().Border(TableBorder.Rounded);
        table.AddColumn("ID");
        table.AddColumn("Категорія");
        table.AddColumn("Назва послуги");
        table.AddColumn("Умови та тарифи");

        foreach (var m in meters)
            table.AddRow(m.Id.ToString(), m.ServiceType, m.Name, m.TariffDetails);

        AnsiConsole.Write(table);
        AnsiConsole.MarkupLine("\n[grey]Натисніть Enter для повернення в меню...[/]");
        Console.ReadLine();
    }
    else if (choice.StartsWith("2."))
    {
        var orderChoice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Оберіть порядок перебору:")
                .AddChoices("За порядком у списку", "Спочатку найдорожчі за тарифом"));

        IBillingOrder order = orderChoice.StartsWith("Спочатку")
            ? new HighestRateFirstOrder()
            : new SequentialOrder();

        var summary = new MonthlySummary(0, 0, 0);
        lastBilled.Clear();

        foreach (var meter in order.Arrange(meters))
        {
            AnsiConsole.MarkupLine($"\n[bold cyan]{meter}[/] — {meter.TariffDetails}");
            if (meter is DualZoneMeter dual)
            {
                for (var n = 1; n <= dual.ZoneNames.Count; n++)
                    AnsiConsole.MarkupLine($"  [grey]Зона {n}: {dual[n]}[/]");
            }

            var given = AnsiConsole.Ask<string>($"[yellow]{meter.Prompt}[/]:");
            var bill = meter.CalculateBill(given);
            summary += bill;

            if (bill >= 0)
            {
                lastBilled.Add((meter, bill));
                AnsiConsole.MarkupLine($"[green]Нараховано: {bill:F2} грн.[/]");
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Помилка: некоректні дані.[/]");
            }
        }

        storage.Save(meters);
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Panel(
            $"Опрацьовано: [bold]{summary.ValidCount}[/] з {summary.ProcessedCount}\n" +
            $"Разом до сплати: [bold green]{summary.TotalAmount:F2} грн[/]\n" +
            $"Середній платіж: {summary.AverageBill:F2} грн")
            .Header("Підсумкова квитанція")
            .BorderColor(Color.Green));

        AnsiConsole.MarkupLine("\n[grey]Натисніть Enter для повернення в меню...[/]");
        Console.ReadLine();
    }
    else if (choice.StartsWith("3."))
    {
        var type = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Тип обліку:")
                .AddChoices("Однозонний лічильник", "Двозонний лічильник", "Фіксована абонплата"));

        var nextId = meters.Count == 0 ? 1 : meters.Max(m => m.Id) + 1;
        var service = AnsiConsole.Ask<string>("Категорія послуги (напр. Водопостачання):");
        var name = AnsiConsole.Ask<string>("Назва лічильника / послуги:");

        Meter created;
        if (type.StartsWith("Однозонний"))
        {
            var rate = AnsiConsole.Ask<double>("Тариф за одиницю (грн):");
            var prev = AnsiConsole.Ask<double>("Попередні показання:");
            var unit = AnsiConsole.Ask<string>("Одиниця виміру (м³, кВт·год):");
            created = new SingleZoneMeter(nextId, name, rate, prev, unit) { ServiceType = service };
        }
        else if (type.StartsWith("Двозонний"))
        {
            var dayRate = AnsiConsole.Ask<double>("Денний тариф (грн):");
            var nightRate = AnsiConsole.Ask<double>("Нічний тариф (грн):");
            var prevDay = AnsiConsole.Ask<double>("Попередні показання (День):");
            var prevNight = AnsiConsole.Ask<double>("Попередні показання (Ніч):");
            created = new DualZoneMeter(nextId, name, dayRate, nightRate, prevDay, prevNight) { ServiceType = service };
        }
        else
        {
            var fee = AnsiConsole.Ask<double>("Щомісячна абонплата на 1 особу (грн):");
            var residents = AnsiConsole.Ask<int>("Кількість зареєстрованих осіб:");
            created = new StandingChargeMeter(nextId, name, fee, residents) { ServiceType = service };
        }

        meters.Add(created);
        storage.Save(meters);
        AnsiConsole.MarkupLine("[green]Послугу додано та збережено у файл![/]");
        Console.ReadLine();
    }
    else if (choice.StartsWith("4."))
    {
        if (lastBilled.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]Спочатку проведіть нарахування платежів (пункт 2).[/]");
        }
        else
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Вид послуги");
            table.AddColumn("Кількість позицій");
            table.AddColumn("Сума до сплати (грн)");

            var groups = lastBilled
                .GroupBy(x => x.Meter.ServiceType)
                .Select(g => new { Service = g.Key, Total = g.Sum(x => x.Amount), Count = g.Count() })
                .OrderByDescending(x => x.Total);

            foreach (var g in groups)
                table.AddRow(g.Service, g.Count.ToString(), $"{g.Total:F2}");

            AnsiConsole.Write(table);
        }
        AnsiConsole.MarkupLine("\n[grey]Натисніть Enter для повернення в меню...[/]");
        Console.ReadLine();
    }
}