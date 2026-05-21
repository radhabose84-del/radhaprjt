using GateEntryManagement.Application.VehicleMovementRecord.Dto;

namespace GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord
{
    public interface IVehicleMovementRecordQueryRepository
    {
        Task<(List<VehicleMovementRecordDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<VehicleMovementRecordDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<VehicleMovementRecordAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<List<PendingVehicleDto>> GetPendingVehiclesAsync(string? vehicleMovementId, string? vehicleNumber, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> HasOpenVMRForVehicleAsync(string vehicleNumber);
        Task<bool> MiscMasterExistsAsync(int id);
        Task<bool> TransporterExistsAsync(int transporterId);
        Task<bool> UnitExistsAsync(int unitId);

        // Returns true if the given MiscMaster Id resolves to ReceivingType='Vehicle'.
        // Drives the conditional rule: VehicleNumber is required only when ReceivingType = Vehicle.
        Task<bool> IsVehicleReceivingTypeAsync(int miscId);
    }
}
