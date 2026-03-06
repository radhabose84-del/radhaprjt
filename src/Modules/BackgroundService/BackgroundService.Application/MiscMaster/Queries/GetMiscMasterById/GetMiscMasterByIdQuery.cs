using BackgroundService.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace BackgroundService.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}