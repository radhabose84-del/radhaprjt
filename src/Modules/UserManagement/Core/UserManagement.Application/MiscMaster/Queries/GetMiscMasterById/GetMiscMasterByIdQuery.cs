using UserManagement.Application.MiscMaster.Queries.GetMiscMaster;
using MediatR;

namespace UserManagement.Application.MiscMaster.Queries.GetMiscMasterById
{
    public class GetMiscMasterByIdQuery  :  IRequest<GetMiscMasterDto>
    { 
         public int Id { get; set; }
        
    }
}