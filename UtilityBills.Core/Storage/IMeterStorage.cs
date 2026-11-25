using UtilityBills.Core.Meters;

namespace UtilityBills.Core.Storage;

public interface IMeterStorage
{
    List<Meter> Load();
    void Save(IEnumerable<Meter> meters);
}
