using Contracts.Common;
using MediatR;
using QCManagement.Application.MiscTypeMaster.Dto;

namespace QCManagement.Application.MiscTypeMaster.Queries.GetAllMiscTypeMaster
{
    public class GetAllMiscTypeMasterQuery : IRequest<ApiResponseDTO<List<MiscTypeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
