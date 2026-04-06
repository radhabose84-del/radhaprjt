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
    }
}
