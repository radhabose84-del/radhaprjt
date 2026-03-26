using GateEntryManagement.Application.GatePass.Dto;

namespace GateEntryManagement.Application.Common.Interfaces.IGatePass
{
    public interface IGatePassQueryRepository
    {
        Task<(List<GatePassHdrDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm);
        Task<GatePassHdrDto?> GetByIdAsync(int id);
        Task<IReadOnlyList<GatePassAutoCompleteDto>> AutocompleteAsync(string term, CancellationToken ct);
        Task<bool> NotFoundAsync(int id);
        Task<bool> VehicleMovementRecordExistsAsync(int vmrId);
        Task<bool> UnitExistsAsync(int unitId);
    }
}
