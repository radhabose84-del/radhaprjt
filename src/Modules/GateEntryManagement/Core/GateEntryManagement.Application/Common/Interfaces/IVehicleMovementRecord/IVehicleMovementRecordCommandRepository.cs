namespace GateEntryManagement.Application.Common.Interfaces.IVehicleMovementRecord
{
    public interface IVehicleMovementRecordCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.VehicleMovementRecord entity);
        Task<int> UpdateAsync(Domain.Entities.VehicleMovementRecord entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
