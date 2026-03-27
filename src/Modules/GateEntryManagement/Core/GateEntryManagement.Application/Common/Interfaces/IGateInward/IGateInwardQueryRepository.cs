using GateEntryManagement.Application.GateInward.Dto;

namespace GateEntryManagement.Application.Common.Interfaces.IGateInward
{
    public interface IGateInwardQueryRepository
    {
        Task<(List<GateInwardHdrDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<GateInwardHdrDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<GateInwardAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> VehicleMovementRecordExistsAsync(int vmrId);
        Task<bool> UnitExistsAsync(int unitId);
        Task<bool> MiscMasterExistsAsync(int id);
    }
}
