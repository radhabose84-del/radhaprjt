using SalesManagement.Domain.Entities;

namespace SalesManagement.Application.Common.Interfaces.ITripSheet
{
    public interface ITripSheetCommandRepository
    {
        Task<int> CreateAsync(TripSheetHeader entity, int typeId);
        Task<int> UpdateAsync(TripSheetHeader entity, List<TripSheetDetail> details);
        Task<bool> SoftDeleteAsync(int id, CancellationToken ct);
    }
}
