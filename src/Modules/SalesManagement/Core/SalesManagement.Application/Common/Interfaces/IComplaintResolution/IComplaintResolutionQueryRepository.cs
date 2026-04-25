using SalesManagement.Application.ComplaintResolution.Dto;

namespace SalesManagement.Application.Common.Interfaces.IComplaintResolution
{
    public interface IComplaintResolutionQueryRepository
    {
        Task<(List<ResolutionListDto>, int)> GetAllAsync(int pageNumber, int pageSize, string? searchTerm, string? statusFilter);
        Task<ComplaintResolutionFormDataDto?> GetFormDataByComplaintIdAsync(int complaintHeaderId);
        Task<ComplaintResolutionDto?> GetByIdAsync(int id);
        Task<ComplaintResolutionDto?> GetByComplaintHeaderIdAsync(int complaintHeaderId);
        Task<bool> NotFoundAsync(int id);
        Task<bool> ResolutionExistsForComplaintAsync(int complaintHeaderId, int? excludeId = null);
        Task<bool> ComplaintExistsAsync(int complaintHeaderId);
        Task<bool> MiscMasterExistsAsync(int id);

        /// <summary>
        /// Returns true when the given MiscMaster Id is the "Closed" closure status —
        /// MiscTypeCode = 'ClosureStatus' and Code = 'Closed'. Validators use this to block
        /// resolvers from setting ClosureStatus = Closed manually; that state is reserved
        /// for system-driven transitions when the downstream artifact (Credit Note,
        /// Sales Return receipt, Replacement dispatch, etc.) is verified.
        /// </summary>
        Task<bool> IsClosureStatusClosedAsync(int closureStatusId);
    }
}
