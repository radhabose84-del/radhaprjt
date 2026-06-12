using Contracts.Common;
using MediatR;
using PurchaseManagement.Application.MixCodeMaster.Dto;

namespace PurchaseManagement.Application.MixCodeMaster.Queries.GetAllMixCodeMaster
{
    public class GetAllMixCodeMasterQuery : IRequest<ApiResponseDTO<List<MixCodeMasterDto>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string? SearchTerm { get; set; }
    }
}
