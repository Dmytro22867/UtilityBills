namespace UtilityBills.Core.Billing;

public record MonthlySummary(int ProcessedCount, int ValidCount, double TotalAmount)
{
    public int InvalidCount => ProcessedCount - ValidCount;
    public double AverageBill => ValidCount == 0 ? 0 : Math.Round(TotalAmount / ValidCount, 2);

    public static MonthlySummary operator +(MonthlySummary s, double billAmount) =>
        billAmount >= 0
            ? new(s.ProcessedCount + 1, s.ValidCount + 1, Math.Round(s.TotalAmount + billAmount, 2))
            : new(s.ProcessedCount + 1, s.ValidCount, s.TotalAmount);
}
