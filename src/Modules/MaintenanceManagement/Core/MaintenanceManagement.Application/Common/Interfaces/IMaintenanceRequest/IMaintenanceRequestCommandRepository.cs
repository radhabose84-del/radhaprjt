namespace MaintenanceManagement.Application.Common.Interfaces.IMaintenanceRequest
{
    public interface IMaintenanceRequestCommandRepository
    {
          Task<int> CreateAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest);

          /// <summary>Adds entity to DbContext without saving — participates in caller's transaction.</summary>
          Task AddWithoutSaveAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest);

          /// <summary>Commits all pending tracked changes in one atomic SaveChangesAsync call.</summary>
          Task<int> CommitAsync(CancellationToken cancellationToken = default);

            Task<bool> UpdateAsync(MaintenanceManagement.Domain.Entities.MaintenanceRequest maintenanceRequest);

            Task<bool> UpdateStatusAsync(int id);

    }
}