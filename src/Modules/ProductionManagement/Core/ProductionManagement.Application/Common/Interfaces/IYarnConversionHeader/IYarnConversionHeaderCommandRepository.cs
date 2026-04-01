namespace ProductionManagement.Application.Common.Interfaces.IYarnConversionHeader
{
    public interface IYarnConversionHeaderCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.YarnConversionHeader entity, int typeId);
        Task<int> UpdateAsync(Domain.Entities.YarnConversionHeader entity);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
