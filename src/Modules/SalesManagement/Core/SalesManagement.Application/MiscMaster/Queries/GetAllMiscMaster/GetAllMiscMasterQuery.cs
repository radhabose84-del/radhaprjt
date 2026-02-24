#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetAllMiscMaster
{
    public class GetAllMiscMasterQuery : IRequest<ApiResponseDTO<List<MiscMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
        public int? MiscTypeId { get; set; }
    }
}
