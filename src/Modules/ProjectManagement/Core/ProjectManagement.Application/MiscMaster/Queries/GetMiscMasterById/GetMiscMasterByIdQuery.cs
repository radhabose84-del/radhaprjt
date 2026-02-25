using ProjectManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace ProjectManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}