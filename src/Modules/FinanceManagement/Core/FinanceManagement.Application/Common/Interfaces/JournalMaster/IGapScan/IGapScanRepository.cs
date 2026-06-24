namespace FinanceManagement.Application.Common.Interfaces.JournalMaster.IGapScan
{
    public sealed class NumberSeriesInfo
    {
        public int SeriesId { get; set; }
        public int VoucherTypeId { get; set; }
        public int FinancialYearId { get; set; }
        public int LastUsedNumber { get; set; }
    }

    public interface IGapScanRepository
    {
        // Series that have issued at least one number.
        Task<IReadOnlyList<NumberSeriesInfo>> GetActiveSeriesAsync(CancellationToken ct);

        // Voucher numbers actually assigned for a (voucher type, fiscal year).
        Task<IReadOnlyList<string>> GetUsedVoucherNumbersAsync(int voucherTypeId, int financialYearId, CancellationToken ct);

        Task AddScanLogAsync(FinanceManagement.Domain.Entities.SequenceGapScanLog log, CancellationToken ct);
    }
}
