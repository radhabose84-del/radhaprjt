namespace FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster
{
    public interface IVoucherTypeMasterCommandRepository
    {
        // Creates the master + allowed account-type rows + (optionally) the initial number-series row for the given fiscal year.
        Task<int> CreateAsync(Domain.Entities.VoucherTypeMaster entity, IEnumerable<int> accountTypeIds, int? initialFinancialYearId);

        // Updates mutable master fields and reconciles the allowed account-type set (code is immutable).
        Task<int> UpdateAsync(Domain.Entities.VoucherTypeMaster entity, IEnumerable<int> accountTypeIds);

        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // Resets a (voucher type, fiscal year) counter to 0, creating the row if it does not exist.
        Task<int> ResetSeriesAsync(int voucherTypeId, int financialYearId);
    }
}
