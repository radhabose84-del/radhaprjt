using Contracts.Dtos.Lookups.Gate;

namespace Contracts.Interfaces.Lookups.Gate
{
    /// <summary>
    /// Cross-module read access to <c>Gate.VehicleMovementRecord</c> rows by id, returning the
    /// display fields other modules need without a cross-schema JOIN. Mirror of <see cref="IGateInwardLookup"/>.
    /// </summary>
    public interface IVehicleMovementRecordLookup
    {
        Task<IReadOnlyList<VehicleMovementRecordLookupDto>> GetByIdsAsync(
            IEnumerable<int> ids, CancellationToken ct = default);
    }
}
