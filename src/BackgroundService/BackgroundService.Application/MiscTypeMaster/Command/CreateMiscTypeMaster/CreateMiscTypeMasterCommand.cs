using BackgroundService.Application.MiscTypeMaster.Queries.GetMiscTypeMaster;
using BackgroundService.Application.Notification.Common.HttpResponse;
using MediatR;

namespace BackgroundService.Application.MiscTypeMaster.Command.CreateMiscTypeMaster
{
    public class CreateMiscTypeMasterCommand  : IRequest<ApiResponseDTO<GetMiscTypeMasterDto>>
    {
      
       public string? MiscTypeCode { get; set; }
       public string? Description { get; set; }
        
    }
}