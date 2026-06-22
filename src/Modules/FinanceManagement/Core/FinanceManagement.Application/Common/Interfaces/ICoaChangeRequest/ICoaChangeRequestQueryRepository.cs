using FinanceManagement.Application.CoaChangeRequest.Dto;

namespace FinanceManagement.Application.Common.Interfaces.ICoaChangeRequest
{
    // US-GL02-08B — read side (Dapper). All queries filter IsDeleted = 0.
    public interface ICoaChangeRequestQueryRepository
    {
        Task<(List<CoaChangeRequestDto> Items, int TotalCount)> GetChangeRequestsAsync(
            int companyId, string? status, int pageNumber, int pageSize, CancellationToken ct);

        Task<CoaUnfreezeRequestDto?> GetUnfreezeRequestByIdAsync(int id, CancellationToken ct);

        // AC3 post-freeze change log read-model: committed CoaChangeRequest joined to its CoaUnfreezeRequest.
        Task<List<PostFreezeChangeLogDto>> GetPostFreezeChangeLogAsync(int companyId, CancellationToken ct);

        // Validator support.
        Task<bool> ChangeRequestExistsAsync(int id, int companyId, CancellationToken ct);
        Task<string?> GetChangeRequestStatusAsync(int id, CancellationToken ct);
        Task<bool> UnfreezeRequestExistsAsync(int id, int companyId, CancellationToken ct);
        Task<string?> GetUnfreezeRequestStatusAsync(int id, CancellationToken ct);
    }
}
