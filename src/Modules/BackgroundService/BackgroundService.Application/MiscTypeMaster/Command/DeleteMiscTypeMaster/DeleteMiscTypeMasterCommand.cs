using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using Contracts.Common;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Command.DeleteMiscTypeMaster
{
    public class DeleteMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
          public int Id { get; set; }
        
    }
}