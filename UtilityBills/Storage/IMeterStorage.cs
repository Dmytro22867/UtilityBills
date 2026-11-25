using UtilityBills.Meters;

namespace UtilityBills.Storage;

public interface IMeterStorage
{
    List<Meter> Load();
    void Save(IEnumerable<Meter> meters);
}
