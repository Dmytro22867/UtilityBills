using System.Text;
using UtilityBills.Meters;

Console.OutputEncoding = System.Text.Encoding.UTF8;

Console.WriteLine("Облік комунальних платежів");

var meters = new List<Meter>
{
    new Meter(1, "Електроенергія (квартира)", 4.32, 1250) { ServiceType = "Електрика" },
    new Meter(2, "Холодна вода (кухня)", 31.20, 410) { ServiceType = "Водопостачання" },
    new Meter(3, "Газ (плита та колонка)", 7.96, 890) { ServiceType = "Газопостачання" }
};

Console.WriteLine($"Лічильників в обліку: {meters.Count}.");

var list = new StringBuilder();
foreach (var meter in meters)
    list.AppendLine($"  {meter}");

Console.Write(list.ToString());

foreach (var meter in meters)
{
    Console.WriteLine();
    Console.WriteLine($"{meter.Name} (попередні показання: {meter.PreviousReading})");
    Console.Write("Введіть поточні показання: ");
    var given = Console.ReadLine() ?? "";
    var bill = meter.CalculateBill(given);

    Console.WriteLine(bill >= 0
        ? $"До сплати: {bill:F2} грн."
        : "Помилка: введено некоректні або менші за попередні показання.");
}
