#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.MiscTypeMaster.Dto;

namespace SalesManagement.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery : IRequest<ApiResponseDTO<MiscTypeMasterDto>>
    {
        public int Id { get; set; }
    }
}
