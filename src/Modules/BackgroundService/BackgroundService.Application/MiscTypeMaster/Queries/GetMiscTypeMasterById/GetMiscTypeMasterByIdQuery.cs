using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMasterById
{
    public class GetMiscTypeMasterByIdQuery  :  IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
        public int Id { get; set; }
        
    }
}