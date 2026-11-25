using System.Text.Encodings.Web;
using System.Text.Json;
using UtilityBills.Meters;

namespace UtilityBills.Storage;

public class JsonMeterStorage : IMeterStorage
{
    private readonly string _filePath;
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    public JsonMeterStorage(string filePath)
    {
        _filePath = filePath;
    }

    public List<Meter> Load()
    {
        if (!File.Exists(_filePath))
            return new List<Meter>();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Meter>>(json, Options) ?? new List<Meter>();
        }
        catch (JsonException)
        {
            return new List<Meter>();
        }
    }

    public void Save(IEnumerable<Meter> meters)
    {
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(meters.ToList(), Options);
        File.WriteAllText(_filePath, json);
    }
}
