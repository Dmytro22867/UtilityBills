using UtilityBills.Core.Billing;
using UtilityBills.Core.Common;
using UtilityBills.Core.Meters;
using UtilityBills.Core.Orders;
using UtilityBills.Core.Storage;
using Xunit;

namespace UtilityBills.Tests;

public class MeterTests
{
    [Theory]
    [InlineData("420", 312.00)]
    [InlineData("420,5", 327.60)]
    [InlineData("420.5", 327.60)]
    [InlineData("410", 0.00)]
    public void SingleZoneMeter_ValidReading_ReturnsExpectedBill(string input, double expected)
    {
        var meter = new SingleZoneMeter(1, "Вода", 31.20, 410, "м³");

        var actual = meter.CalculateBill(input);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData("409")]
    [InlineData("-5")]
    [InlineData("текст")]
    [InlineData("")]
    public void SingleZoneMeter_InvalidOrLowerReading_ReturnsMinusOne(string input)
    {
        var meter = new SingleZoneMeter(1, "Вода", 31.20, 410, "м³");

        var actual = meter.CalculateBill(input);

        Assert.Equal(-1, actual);
    }

    [Fact]
    public void DualZoneMeter_ValidReadingsAndIndexer_WorkCorrectly()
    {
        var meter = new DualZoneMeter(2, "Світло", 4.32, 2.16, 1200, 600, "День", "Ніч");

        var bill = meter.CalculateBill("1300 650");

        Assert.Equal(540.00, bill);
        Assert.Equal("День", meter[1]);
        Assert.Equal("Ніч", meter[2]);
    }

    [Fact]
    public void StandingChargeMeter_ValidMonths_CalculatesCorrectly()
    {
        var meter = new StandingChargeMeter(3, "Вивіз сміття", 45.50, 2);

        Assert.Equal(182.00, meter.CalculateBill("2"));
        Assert.Equal(-1, meter.CalculateBill("0"));
    }

    [Fact]
    public void MonthlySummary_PlusOperator_AccumulatesValidAndTracksInvalid()
    {
        var summary = new MonthlySummary(0, 0, 0);

        summary += 150.50;
        summary += -1;
        summary += 49.50;

        Assert.Equal(3, summary.ProcessedCount);
        Assert.Equal(2, summary.ValidCount);
        Assert.Equal(1, summary.InvalidCount);
        Assert.Equal(200.00, summary.TotalAmount);
        Assert.Equal(100.00, summary.AverageBill);
    }

    [Fact]
    public void HighestRateFirstOrder_And_CatalogFilter_WorkAsExpected()
    {
        var meters = new List<Meter>
        {
            new SingleZoneMeter(1, "Електрика", 4.32, 100),
            new StandingChargeMeter(2, "Сміття", 45.50, 1),
            new SingleZoneMeter(3, "Вода", 31.20, 50)
        };

        var ordered = new HighestRateFirstOrder().Arrange(meters).ToList();
        var filter = new CatalogFilter<Meter>();
        var singleOnly = filter.FindAll(meters, m => m is SingleZoneMeter);

        Assert.Equal(2, ordered[0].Id);
        Assert.Equal(3, ordered[1].Id);
        Assert.Equal(1, ordered[2].Id);
        Assert.Equal(2, singleOnly.Count);
    }

    [Fact]
    public void JsonMeterStorage_SaveAndLoad_PreservesDerivedTypes()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"meters_test_{Guid.NewGuid()}.json");
        try
        {
            var storage = new JsonMeterStorage(tempFile);
            var original = new List<Meter>
            {
                new SingleZoneMeter(1, "Газ", 7.96, 890, "м³"),
                new DualZoneMeter(2, "Світло", 4.32, 2.16, 100, 50),
                new StandingChargeMeter(3, "Абонплата", 30.00, 3)
            };

            storage.Save(original);
            var loaded = storage.Load();

            Assert.Equal(3, loaded.Count);
            Assert.IsType<SingleZoneMeter>(loaded[0]);
            Assert.IsType<DualZoneMeter>(loaded[1]);
            Assert.IsType<StandingChargeMeter>(loaded[2]);
            Assert.Equal(890, ((SingleZoneMeter)loaded[0]).PreviousReading);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ThreeZoneMeter_ValidReadingsAndJsonSerialization_WorkCorrectly()
    {
        var meter = new ThreeZoneMeter(4, "Тризонний лічильник", 6.48, 4.32, 1.73, 100, 200, 300);

        Assert.Equal("Пік", meter[1]);
        Assert.Equal("Напівпік", meter[2]);
        Assert.Equal("Ніч", meter[3]);
        Assert.Equal(1253.00, meter.CalculateBill("200 300 400"));
        Assert.Equal(-1, meter.CalculateBill("200 300"));

        var tempFile = Path.Combine(Path.GetTempPath(), $"three_zone_{Guid.NewGuid()}.json");
        try
        {
            var storage = new JsonMeterStorage(tempFile);
            storage.Save([meter]);
            var loaded = storage.Load();

            Assert.Single(loaded);
            Assert.IsType<ThreeZoneMeter>(loaded[0]);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}