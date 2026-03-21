namespace ProductionManagement.Application.Common.Interfaces.ILotMaster
{
    public interface ILotMasterCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.LotMaster entity);
        Task<int> UpdateAsync(Domain.Entities.LotMaster entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
