namespace SalesManagement.Application.Common.Interfaces.IComplaintResolution
{
    public interface IComplaintResolutionCommandRepository
    {
        Task<int> CreateAsync(Domain.Entities.ComplaintResolution entity);
        Task<int> UpdateAsync(Domain.Entities.ComplaintResolution entity);
    }
}
