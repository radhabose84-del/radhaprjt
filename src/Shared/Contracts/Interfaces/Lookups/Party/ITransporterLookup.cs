using Contracts.Dtos.Lookups.Party;

namespace Contracts.Interfaces.Lookups.Party
{
    /// <summary>
    /// Cross-module lookup for transporters (parties used as transporters) in the ERP Party Master.
    /// Backs the Freight RFQ transporter selection and FK validation.
    /// </summary>
    public interface ITransporterLookup
    {
        /// <summary>
        /// Searches active, non-deleted transporters by Party Name or Party Code.
        /// </summary>
        Task<IReadOnlyList<TransporterLookupDto>> SearchTransportersAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Returns the transporter if the given party id is an active, non-deleted transporter; otherwise null.
        /// Used for mandatory-transporter FK validation and server-side TransporterName resolution.
        /// </summary>
        Task<TransporterLookupDto?> GetActiveTransporterByIdAsync(int partyId, CancellationToken ct = default);
    }
}
