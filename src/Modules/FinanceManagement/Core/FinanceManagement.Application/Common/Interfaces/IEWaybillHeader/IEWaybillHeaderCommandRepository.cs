namespace FinanceManagement.Application.Common.Interfaces.IEWaybillHeader
{
    public interface IEWaybillHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.EWaybillHeader entity);
        Task<int> UpdateAsync(Domain.Entities.EWaybillHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // Called after a successful NIC e-Waybill generation. Stamps the row with the
        // EWB number returned by NIC plus generated/valid dates, and flips status to Generated.
        Task<bool> UpdateAfterNicSuccessAsync(int id, string ewbNumber,
            DateTimeOffset? generatedDate, DateTimeOffset? validUpto, CancellationToken ct = default);

        // Called when NIC rejects/errors out. Captures the error code and message
        // on the row so operators can see why the EWB didn't generate. Status stays Pending
        // so the operator can fix the data and retry.
        Task<bool> UpdateAfterNicFailureAsync(int id, string? errorCode, string? errorMessage,
            CancellationToken ct = default);
    }
}
