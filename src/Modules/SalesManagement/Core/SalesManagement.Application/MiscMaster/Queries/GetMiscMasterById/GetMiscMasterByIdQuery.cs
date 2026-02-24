#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.MiscMaster.Dto;

namespace SalesManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery : IRequest<ApiResponseDTO<MiscMasterDto>>
    {
        public int Id { get; set; }
    }
}
