namespace GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord
{
    public interface IVehicleMovementRecordCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VehicleMovementRecord entity, int transactionTypeId);
        Task<int> UpdateAsync(Domain.Entities.VehicleMovementRecord entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);

        // Marks the VMR as exited: StatusId → @exitedStatusId, GateOutTime → UtcNow, GateOutBy → current user.
        // Returns false if the VMR row is not found / already soft-deleted.
        Task<bool> UpdateStatusToExitedAsync(int vmrId, int exitedStatusId, CancellationToken ct);
    }
}
