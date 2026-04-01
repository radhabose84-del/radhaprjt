namespace FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader
{
    public interface IEInvoiceHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.EInvoiceHeader entity);
        Task<int> UpdateAsync(Domain.Entities.EInvoiceHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        /// <summary>
        /// Hard-deletes an EInvoice header and all its detail rows.
        /// Used to clean up orphaned records when IRN generation fails before any external reference (IRN/EWB) is created.
        /// </summary>
        Task HardDeleteWithDetailsAsync(int id, CancellationToken ct);

        /// <summary>
        /// Hard-deletes all EInvoice headers (and their details) for a given InvoiceNo where IrnNumber IS NULL.
        /// Used to clean up incomplete records before recreating from Sales.
        /// </summary>
        Task HardDeleteIncompleteByInvoiceNoAsync(string invoiceNo, CancellationToken ct);

        /// <summary>
        /// Updates only the IRN-related fields after a call to the NIC API.
        /// Called by GenerateIrnCommandHandler after a successful or failed NIC API call.
        /// </summary>
        Task<bool> UpdateIrnDetailsAsync(
            int id,
            string? irn,
            string? ackNo,
            DateTimeOffset? ackDate,
            string? signInvoice,
            string? signQrCode,
            string irnStatus,
            string? errorCode,
            string? errorMessage,
            CancellationToken ct);
        /// <summary>
        /// Updates only the e-Waybill-related fields after a successful NIC e-Waybill API call.
        /// </summary>
        /// <summary>
        /// Updates only IrnStatus, ErrorCode, ErrorMessage — preserves existing IRN data.
        /// </summary>
        Task<bool> UpdateIrnStatusAsync(
            int id, string irnStatus, string? errorCode, string? errorMessage, CancellationToken ct);

        Task<bool> UpdateEwbDetailsAsync(
            int id,
            long? ewbNo,
            string? ewbDate,
            string? ewbValidTill,
            CancellationToken ct);
    }
}
