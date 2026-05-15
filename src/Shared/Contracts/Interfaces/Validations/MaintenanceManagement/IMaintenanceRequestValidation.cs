using Contracts.Dtos.Validations.MaintenanceManagement;

namespace Contracts.Interfaces.Validations.MaintenanceManagement
{
    /// <summary>
    /// Cross-module validation contract used by Service PO validators.
    /// Not cached (the name does not end in 'Lookup'), so submit-time correctness is guaranteed.
    /// </summary>
    public interface IMaintenanceRequestValidation
    {
        /// <summary>True if the request exists, is External, IsActive=1, IsDeleted=0,
        /// and current status IN (Open, In-Progress, PartiallyConverted).</summary>
        Task<bool> IsAvailableForServicePoAsync(int requestId, CancellationToken ct = default);

        /// <summary>Minimal projection used for cross-line consistency checks (e.g. vendor match).</summary>
        Task<MaintenanceRequestRefDto?> GetRefAsync(int requestId, CancellationToken ct = default);
    }
}
